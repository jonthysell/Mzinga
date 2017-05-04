// 
// FixedCache.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2017 Jon Thysell <http://jonthysell.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;

namespace Mzinga.Core.AI
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

        public CacheMetrics Metrics { get; private set; } = new CacheMetrics();

        private Dictionary<TKey, TEntry> _dict;
        private Queue<TKey> _queue;

        private FixedCacheReplaceEntryPredicate<TEntry> _replaceEntryPredicate;

        public FixedCache(int capacity = DefaultCapacity, FixedCacheReplaceEntryPredicate<TEntry> replaceEntryPredicate = null)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException("capacity");
            }

            Capacity = capacity;

            _replaceEntryPredicate = replaceEntryPredicate;

            _dict = new Dictionary<TKey, TEntry>(Capacity);
            _queue = new Queue<TKey>(Capacity);
        }

        public void Store(TKey key, TEntry newEntry)
        {
            TEntry existingEntry;
            if (!_dict.TryGetValue(key, out existingEntry))
            {
                // New entry

                if (Count == Capacity)
                {
                    // Make space
                    _dict.Remove(_queue.Dequeue());
                }

                // Add
                _dict.Add(key, newEntry);
                _queue.Enqueue(key);

                Metrics.Stores++;
            }
            else
            {
                // Existing entry
                if (null == _replaceEntryPredicate || _replaceEntryPredicate(existingEntry, newEntry))
                {
                    int count = _queue.Count;
                    while (count > 0)
                    {
                        TKey k = _queue.Dequeue();
                        if (!k.Equals(key))
                        {
                            _queue.Enqueue(k);
                        }
                        count--;
                    }

                    // Replace
                    _dict[key] = newEntry;
                    _queue.Enqueue(key);

                    Metrics.Updates++;
                }
            }
        }

        public bool TryLookup(TKey key, out TEntry entry)
        {
            if (_dict.TryGetValue(key, out entry))
            {
                Metrics.Hits++;
                return true;
            }

            Metrics.Misses++;

            entry = default(TEntry);
            return false;
        }

        public void Clear()
        {
            _dict.Clear();
            _queue.Clear();
            Metrics.Reset();
        }

        public override string ToString()
        {
            return string.Format("U: {0}/{1} ({2:P2}) {3}", Count, Capacity, Usage, Metrics);
        }

        private const int DefaultCapacity = 1024;
    }
}
