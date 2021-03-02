// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Mzinga.Core
{
    public class CacheMetricsSet
    {
        public CacheMetrics this[string name]
        {
            get
            {
                return GetCacheMetrics(name);
            }
        }

        private Dictionary<string, CacheMetrics> _cacheMetrics;

        public CacheMetricsSet()
        {
            Reset();
        }

        public void Reset()
        {
            _cacheMetrics = new Dictionary<string, CacheMetrics>();
        }

        private CacheMetrics GetCacheMetrics(string name)
        {
            if (!_cacheMetrics.TryGetValue(name, out CacheMetrics cm))
            {
                cm = new CacheMetrics();
                _cacheMetrics.Add(name, cm);
            }
            return cm;
        }
    }
}
