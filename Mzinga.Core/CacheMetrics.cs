// 
// CacheMetrics.cs
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

namespace Mzinga.Core
{
    public class CacheMetrics
    {
        public int Hits { get; set; } = 0;
        public int Misses { get; set; } = 0;

        public int Stores { get; set; } = 0;
        public int Updates { get; set; } = 0;

        public double HitRatio
        {
            get
            {
                return Hits / (double)Math.Max(Hits + Misses, 1);
            }
        }

        public CacheMetrics() { }

        public void Reset()
        {
            Hits = 0;
            Misses = 0;
            Stores = 0;
            Updates = 0;
        }

        public override string ToString()
        {
            return string.Format("H: {0} M: {1} HR: {2:P2}", Hits, Misses, HitRatio);
        }
    }
}
