// 
// GameAITests.cs
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
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Core;
using Mzinga.Core.AI;

namespace Mzinga.CoreTest
{
    [TestClass]
    public class GameAITests
    {
        [TestMethod]
        public void GameAI_NewTest()
        {
            GameAI ai = new GameAI();
            Assert.IsNotNull(ai);
        }

        [TestMethod]
        public void GameAI_FirstMovePerfTest()
        {
            GameBoard gb = new GameBoard();

            int sum = 0;
            int iterations = 3;

            for (int i = 0; i < iterations; i++)
            {
                GameAI ai = GetTestGameAI(TimeSpan.FromSeconds(10.0));
                ai.GetBestMove(gb);

                int totalBoardScoreResults = ai.BestMoveMetrics.BoardScoreTotalResults;
                Trace.WriteLine(string.Format("BoardScoreTotalResults: {0}", totalBoardScoreResults));
                sum += totalBoardScoreResults;
            }

            Trace.WriteLine(string.Format("Average BoardScoreTotalResults: {0}", (int)Math.Round(sum / (double)iterations)));
        }

        private GameAI GetTestGameAI(TimeSpan maxTime)
        {
            GameAI ai = new GameAI(MetricWeightsTests.TestMetricWeights)
            {
                MaxTime = maxTime,
                MaxDepth = GameAI.IterativeDepth,
                AlphaBetaPruning = true,
                TranspositionTable = true
            };

            return ai;
        }
    }
}
