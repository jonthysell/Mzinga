// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mzinga.Core
{
    public class FastSet<T> : IReadOnlyCollection<T> where T: struct
    {
        private readonly List<T> _items = new List<T>(32);

        public int Count => _items.Count;

        public bool Contains(in T item)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (_items[i].Equals(item))
                {
                    return true;
                }
            }
            return false;
        }
        
        internal bool Add(in T item)
        {
            if (Contains(in item))
            {
                return false;
            }

            _items.Add(item);
            return true;
        }

        internal void FastAdd(in T item)
        {
            _items.Add(item);
        }

        internal void Clear()
        {
            _items.Clear();
        }

        internal void ValidateSet()
        {
            var set = _items.ToHashSet();
            if (set.Count != Count)
            {
                throw new Exception("FastSet contains duplicates.");
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}