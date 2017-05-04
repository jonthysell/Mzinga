// 
// BestMoveMetrics.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017 Jon Thysell <http://jonthysell.com>
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
        public int MaxSearchDepth { get; private set;  }

        public TimeSpan MaxSearchTime{ get; private set; }

        public int MovesEvaluated { get; private set; }

        public int MinDepth { get; private set; }

        public int MaxDepth { get; private set; }

        public double AverageDepth { get; private set; }

        public DateTime? StartTime { get; private set; }

        public DateTime? EndTime { get; private set; }

        public TimeSpan ElapsedTime
        {
            get
            {
                if (StartTime.HasValue)
                {
                    if (EndTime.HasValue)
                    {
                        return EndTime.Value - StartTime.Value;
                    }
                    return DateTime.Now - StartTime.Value;
                }

                return TimeSpan.Zero;
            }
        }

        public bool HasTimeLeft
        {
            get
            {
                return ElapsedTime < MaxSearchTime;
            }
        }

        public int AlphaBetaCuts { get; set; }

        public int BoardScoreConstantResults { get; set; }

        public int BoardScoreCalculatedResults { get; set; }

        public int BoardScoreTotalResults
        {
            get
            {
                return BoardScoreConstantResults + BoardScoreCalculatedResults;
            }
        }

        public CacheMetrics TranspositionTableMetrics { get; private set; }

        public BestMoveMetrics(int maxSearchDepth, TimeSpan maxSearchTime)
        {
            MaxSearchDepth = maxSearchDepth;
            MaxSearchTime = maxSearchTime;

            MovesEvaluated = 0;

            MinDepth = int.MaxValue;
            MaxDepth = int.MinValue;
            AverageDepth = 0.0;

            AlphaBetaCuts = 0;
            BoardScoreConstantResults = 0;

            TranspositionTableMetrics = new CacheMetrics();
        }

        public void Start()
        {
            if (StartTime.HasValue)
            {
                throw new InvalidOperationException("Cannot call Start twice!");
            }
            StartTime = DateTime.Now;
        }

        public void End()
        {
            if (EndTime.HasValue)
            {
                throw new InvalidOperationException("Cannot call End twice!");
            }
            EndTime = DateTime.Now;
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
