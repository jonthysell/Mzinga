// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Mzinga.Core.AI
{
    static class ListExtensions
    {
        public static IEnumerable<T> GetEnumerableByOrderType<T>(this List<T> items, OrderType orderType)
        {
            int i = orderType == OrderType.SkipOffset && items.Count > 1 ? 1 : 0;

            int count = 0;
            while (i < items.Count)
            {
                yield return items[i];

                count++;

                i += orderType == OrderType.Default ? 1 : 2;

                if (count < items.Count && i >= items.Count)
                {
                    i = orderType == OrderType.SkipOffset ? 0 : 1;
                }
            }
        }
    }

    public enum OrderType
    {
        Default = 0,
        Skip, // Starts at 0
        SkipOffset // Starts at 1
    }
}
