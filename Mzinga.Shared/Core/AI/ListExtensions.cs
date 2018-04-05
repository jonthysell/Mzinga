// 
// ListExtensions.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2018 Jon Thysell <http://jonthysell.com>
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
