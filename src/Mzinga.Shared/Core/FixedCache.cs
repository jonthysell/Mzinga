// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Mzinga.Core
{
    public delegate bool FixedCacheReplaceEntryPredicate<TValue>(TValue existingEntry, TValue newEntry);

    public class FixedCache<TKey, TEntry> where TKey : IEquatable<TKey>, IComparable<TKey>
    {
        public int Count
        {
            get
            {
                return _dict.Count;
            }
        }

        public int Capacity { get; private set; }

        public double Usage
        {
            get
            {
                return Count / (double)Capacity;
            }
        }

        public IEnumerable<TKey> Keys
        {
            get
            {
                return _dict.Keys;
            }
        }

#if DEBUG
        public CacheMetrics Metrics { get; private set; } = new CacheMetrics();
#endif

        private readonly Dictionary<TKey, FixedCacheEntry<TKey, TEntry>> _dict;
        private readonly LinkedList<TKey> _list;

        private readonly FixedCacheReplaceEntryPredicate<TEntry> _replaceEntryPredicate;

        private readonly object _storeLock = new object();

        public FixedCache(int capacity = DefaultCapacity, FixedCacheReplaceEntryPredicate<TEntry> replaceEntryPredicate = null)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            Capacity = capacity;

            _replaceEntryPredicate = replaceEntryPredicate;

            _dict = new Dictionary<TKey, FixedCacheEntry<TKey, TEntry>>(Capacity);
            _list = new LinkedList<TKey>();
        }

        public void Store(TKey key, TEntry newEntry)
        {
            lock (_storeLock)
            {
                if (!_dict.TryGetValue(key, out FixedCacheEntry<TKey, TEntry> existingEntry))
                {
                    // New entry
                    if (Count == Capacity)
                    {
                        // Make space
                        TKey first = _list.First.Value;
                        _dict.Remove(first);
                        _list.RemoveFirst();
                    }

                    // Add
                    StoreInternal(key, newEntry);
#if DEBUG
                    Metrics.Store();
#endif
                }
                else
                {
                    // Existing entry
                    if (null == _replaceEntryPredicate || _replaceEntryPredicate(existingEntry.Entry, newEntry))
                    {
                        // Replace
                        _list.Remove(existingEntry.ListNode);

                        StoreInternal(key, newEntry);
#if DEBUG
                        Metrics.Update();
#endif
                    }
                }
            }
        }

        private void StoreInternal(TKey key, TEntry newEntry)
        {
            LinkedListNode<TKey> listNode = _list.AddLast(key);

            FixedCacheEntry<TKey, TEntry> wrappedEntry = new FixedCacheEntry<TKey, TEntry>
            {
                ListNode = listNode,
                Entry = newEntry,
            };

            _dict[key] = wrappedEntry;
        }

        public bool TryLookup(TKey key, out TEntry entry)
        {
            if (_dict.TryGetValue(key, out FixedCacheEntry<TKey, TEntry> wrappedEntry))
            {
                entry = wrappedEntry.Entry;
#if DEBUG
                Metrics.Hit();
#endif
                return true;
            }
#if DEBUG
            Metrics.Miss();
#endif

            entry = default;
            return false;
        }

        public void Clear()
        {
            _dict.Clear();
            _list.Clear();
#if DEBUG
            Metrics.Reset();
#endif
        }

        public override string ToString()
        {
#if DEBUG
            return string.Format("U: {0}/{1} ({2:P2}) {3}", Count, Capacity, Usage, Metrics);
#else
            return string.Format("U: {0}/{1} ({2:P2})", Count, Capacity, Usage);
#endif
        }

        private const int DefaultCapacity = 1024;

        private class FixedCacheEntry<TK, TE> where TK : IEquatable<TK>, IComparable<TK>
        {
            public LinkedListNode<TK> ListNode;
            public TE Entry;
        }
    }
}
