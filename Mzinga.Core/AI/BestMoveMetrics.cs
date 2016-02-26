// 
// BestMoveMetrics.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016 Jon Thysell <http://jonthysell.com>
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

namespace Mzinga.Core.AI
{
    public class BestMoveMetrics
    {
        public int MovesEvaluated { get; private set; }

        public int MinDepth { get; private set; }

        public int MaxDepth { get; private set; }

        public double AverageDepth { get; private set; }

        public TimeSpan ElapsedTime { get; set; }

        public int AlphaBetaCuts { get; set; }

        public int CachedBoardScoreHits { get; set; }

        public int CachedBoardMetricHits { get; set; }

        public int BoardScoresCalculated { get; set; }

        public int BoardMetricsCalculated { get; set; }

        public BestMoveMetrics()
        {
            MovesEvaluated = 0;

            MinDepth = Int32.MaxValue;
            MaxDepth = Int32.MinValue;
            AverageDepth = 0.0;

            ElapsedTime = TimeSpan.Zero;

            AlphaBetaCuts = 0;
            CachedBoardScoreHits = 0;
            CachedBoardMetricHits = 0;
            BoardScoresCalculated = 0;
            BoardMetricsCalculated = 0;
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

            AverageDepth = totalDepth / (double)MovesEvaluated;
        }
    }
}
