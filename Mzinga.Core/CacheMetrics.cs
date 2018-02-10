// 
// CacheMetrics.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2017, 2018 Jon Thysell <http://jonthysell.com>
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
using System.Threading;

namespace Mzinga.Core
{
    public class CacheMetrics
    {
        public int Hits
        {
            get
            {
                return _hits;
            }
        }
        private int _hits = 0;

        public int Misses
        {
            get
            {
                return _misses;
            }
        }
        private int _misses = 0;

        public int Stores
        {
            get
            {
                return _stores;
            }
        }
        private int _stores = 0;

        public int Updates
        {
            get
            {
                return _updates;
            }
        }
        private int _updates = 0;

        public double HitRatio
        {
            get
            {
                return Hits / (double)Math.Max(Hits + Misses, 1);
            }
        }

        public CacheMetrics() { }

        public void Hit()
        {
            Interlocked.Increment(ref _hits);
        }

        public void Miss()
        {
            Interlocked.Increment(ref _misses);
        }

        public void Store()
        {
            Interlocked.Increment(ref _stores);
        }

        public void Update()
        {
            Interlocked.Increment(ref _updates);
        }

        public void Reset()
        {
            Interlocked.Exchange(ref _hits, 0);
            Interlocked.Exchange(ref _misses, 0);
            Interlocked.Exchange(ref _stores, 0);
            Interlocked.Exchange(ref _updates, 0);
        }

        public override string ToString()
        {
            return string.Format("H: {0} M: {1} HR: {2:P2}", Hits, Misses, HitRatio);
        }
    }
}
