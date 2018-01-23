// 
// GameAITests.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2017, 2018 Jon Thysell <http://jonthysell.com>
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
        [TestCategory("Performance")]
        public void GameAI_FirstTurnBestMovePerfTest()
        {
            TimeSpan sum = TimeSpan.Zero;
            int iterations = 100;

            for (int i = 0; i < iterations; i++)
            {
                GameBoard gb = new GameBoard();
                GameAI ai = GetTestGameAI();

                Stopwatch sw = Stopwatch.StartNew();
                Move m = ai.GetBestMove(gb, 2);
                sw.Stop();

                sum += sw.Elapsed;
            }

            Trace.WriteLine(string.Format("Average Ticks: {0}", sum.Ticks / iterations));
        }

        [TestMethod]
        [TestCategory("Performance")]
        public void GameAI_FifthTurnBestMovePerfTest()
        {
            TimeSpan sum = TimeSpan.Zero;
            int iterations = 100;

            GameBoard original = new GameBoard();
            GameAI originalAI = GetTestGameAI();

            while (original.CurrentPlayerTurn < 5)
            {
                original.Play(originalAI.GetBestMove(original, 1));
            }

            for (int i = 0; i < iterations; i++)
            {
                GameBoard gb = original.Clone();
                GameAI ai = GetTestGameAI();

                Stopwatch sw = Stopwatch.StartNew();
                Move m = ai.GetBestMove(gb, 2);
                sw.Stop();

                sum += sw.Elapsed;
            }

            Trace.WriteLine(string.Format("Average Ticks: {0}", sum.Ticks / iterations));
        }

        private GameAI GetTestGameAI()
        {
            return new GameAI(MetricWeightsTests.TestMetricWeights);
        }
    }

    static partial class GameBoardExtensions
    {
        public static GameBoard Clone(this GameBoard original)
        {
            GameBoard clone = new GameBoard();
            foreach (BoardHistoryItem item in original.BoardHistory)
            {
                clone.Play(item.Move);
            }
            return clone;
        }
    }
}
