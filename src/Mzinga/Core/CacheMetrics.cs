// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Threading;

namespace Mzinga.Core
{
    class CacheMetrics
    {
        public int Hits => _hits;
        private volatile int _hits = 0;

        public int Misses => _misses;
        private volatile int _misses = 0;

        public int Stores => _stores;
        private volatile int _stores = 0;

        public int Updates => _updates;
        private volatile int _updates = 0;

        public double HitRatio => Hits / (double)Math.Max(Hits + Misses, 1);

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
