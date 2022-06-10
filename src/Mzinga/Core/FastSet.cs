// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mzinga.Core
{
    public class FastSet<T> : IReadOnlyCollection<T>
    {
        private readonly List<T> _items = new List<T>(32);

        public int Count => _items.Count;

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }
        
        internal bool Add(T item)
        {
            if (_items.Contains(item))
            {
                return false;
            }

            _items.Add(item);
            return true;
        }

        internal void FastAdd(T item)
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