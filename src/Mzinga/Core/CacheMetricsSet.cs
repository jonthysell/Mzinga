// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Mzinga.Core
{
    class CacheMetricsSet
    {
        public CacheMetrics this[string name]
        {
            get
            {
                return GetCacheMetrics(name);
            }
        }

        private readonly Dictionary<string, CacheMetrics> _cacheMetrics = new Dictionary<string, CacheMetrics>();

        public CacheMetricsSet()
        {
            Reset();
        }

        public void Reset()
        {
            _cacheMetrics.Clear();
        }

        private CacheMetrics GetCacheMetrics(string name)
        {
            if (!_cacheMetrics.TryGetValue(name, out CacheMetrics? cm))
            {
                cm = new CacheMetrics();
                _cacheMetrics.Add(name, cm);
            }
            return cm;
        }
    }
}
