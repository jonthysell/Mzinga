// 
// BestMoveMetrics.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017, 2018 Jon Thysell <http://jonthysell.com>
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
using System.Diagnostics;

namespace Mzinga.Core.AI
{
    public class BestMoveMetrics
    {
        public int MaxSearchDepth { get; private set;  }

        public TimeSpan MaxSearchTime { get; private set; }

        public int MaxHelperThreads { get; private set; }

        public int MovesEvaluated { get; private set; } = 0;

        public int MinDepth { get; private set; } = 0;

        public int MaxDepth { get; private set; } = 0;

        public double AverageDepth { get; private set; } = 0.0;

        public TimeSpan ElapsedTime
        {
            get
            {
                return _stopwatch.Elapsed;
            }
        }

        internal EvaluatedMove BestMove { get; set; } = null;

        private Stopwatch _stopwatch = new Stopwatch();

        public BestMoveMetrics(int maxSearchDepth, TimeSpan maxSearchTime, int maxHelperThreads)
        {
            MaxSearchDepth = maxSearchDepth;
            MaxSearchTime = maxSearchTime;
            MaxHelperThreads = maxHelperThreads;
        }

        public void Start()
        {
            _stopwatch.Start();
        }

        public void End()
        {
            _stopwatch.Stop();
        }

        public void IncrementMoves(int depth)
        {
            if (depth < 0)
            {
                throw new ArgumentOutOfRangeException("depth");
            }

            MinDepth = Math.Min(MinDepth, depth);
            MaxDepth = Math.Max(MaxDepth, depth);

            double totalDepth = AverageDepth * MovesEvaluated;
            totalDepth += depth;

            MovesEvaluated++;

            AverageDepth = totalDepth / MovesEvaluated;
        }
    }
}
