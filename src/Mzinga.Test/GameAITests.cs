// 
// GameAITests.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2017, 2018, 2019, 2021 Jon Thysell <http://jonthysell.com>
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

namespace Mzinga.Test
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
        public void GameAI_FirstTurnBestMoveByDepthPerfTest()
        {
            TimeSpan sum = TimeSpan.Zero;
            int iterations = 100;

            for (int i = 0; i < iterations; i++)
            {
                GameBoard gb = new GameBoard();
                GameAI ai = GetTestGameAI(gb.ExpansionPieces);

                Stopwatch sw = Stopwatch.StartNew();
                _ = ai.GetBestMove(gb, 2, PerfTestMaxHelperThreads);
                sw.Stop();

                sum += sw.Elapsed;
            }

            Trace.WriteLine(string.Format("Average Ticks: {0}", sum.Ticks / iterations));
        }

        [TestMethod]
        [TestCategory("Performance")]
        public void GameAI_FifthTurnBestMoveByDepthPerfTest()
        {
            TimeSpan sum = TimeSpan.Zero;
            int iterations = 100;

            GameBoard original = GetBoardOnFifthTurn();

            for (int i = 0; i < iterations; i++)
            {
                GameBoard gb = original.Clone();
                GameAI ai = GetTestGameAI(gb.ExpansionPieces);

                Stopwatch sw = Stopwatch.StartNew();
                _ = ai.GetBestMove(gb, 2, PerfTestMaxHelperThreads);
                sw.Stop();

                sum += sw.Elapsed;
            }

            Trace.WriteLine(string.Format("Average Ticks: {0}", sum.Ticks / iterations));
        }

        [TestMethod]
        [TestCategory("Performance")]
        public void GameAI_FirstTurnBestMoveByTimePerfTest()
        {
            GameBoard gb = new GameBoard();
            GameAI ai = GetTestGameAI(gb.ExpansionPieces);

            int maxDepth = 0;

            ai.BestMoveFound += (sender, e) =>
            {
                maxDepth = Math.Max(maxDepth, e.Depth);
            };

            Move m = ai.GetBestMove(gb, TimeSpan.FromSeconds(5), PerfTestMaxHelperThreads);

            Trace.WriteLine(string.Format("Max Depth: {0}", maxDepth));
        }

        [TestMethod]
        [TestCategory("Performance")]
        public void GameAI_FifthTurnBestMoveByTimePerfTest()
        {
            GameBoard gb = GetBoardOnFifthTurn();
            GameAI ai = GetTestGameAI(gb.ExpansionPieces);

            int maxDepth = 0;

            ai.BestMoveFound += (sender, e) =>
            {
                maxDepth = Math.Max(maxDepth, e.Depth);
            };

            Move m = ai.GetBestMove(gb, TimeSpan.FromSeconds(5), PerfTestMaxHelperThreads);

            Trace.WriteLine(string.Format("Max Depth: {0}", maxDepth));
        }

        [TestMethod]
        public void GameAI_WinningMoveIsBestMoveTest()
        {
            TestUtils.LoadAndExecuteTestCases<GameAIBestMoveTestCase>("GameAI_WinningMoveIsBestMoveTest.csv");
        }

        [TestMethod]
        public void GameAI_BlockWinningMoveIsBestMoveTest()
        {
            TestUtils.LoadAndExecuteTestCases<GameAIBestMoveTestCase>("GameAI_BlockWinningMoveIsBestMoveTest.csv");
        }

        private static GameBoard GetBoardOnFifthTurn()
        {
            GameBoard gb = new GameBoard();
            GameAI ai = GetTestGameAI(gb.ExpansionPieces);

            while (gb.CurrentPlayerTurn < 5)
            {
                gb.Play(ai.GetBestMove(gb, 1, TestMaxHelperThreads));
            }

            return gb;
        }

        public static GameAI GetTestGameAI(ExpansionPieces expansionPieces)
        {
            return TestUtils.DefaultGameEngineConfig.GetGameAI(expansionPieces);
        }

        private class GameAIBestMoveTestCase : ITestCase
        {
            public GameBoard Board;
            public int MaxDepth;

            public Move ExpectedBestMove;
            public Move ActualBestMove;

            public void Execute()
            {
                GameAI ai = GetTestGameAI(Board.ExpansionPieces);
                ActualBestMove = ai.GetBestMove(Board, MaxDepth, TestMaxHelperThreads);
                Assert.AreEqual(ExpectedBestMove, ActualBestMove);
            }

            public void Parse(string s)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    throw new ArgumentNullException(nameof(s));
                }

                s = s.Trim();

                string[] vals = s.Split('\t');

                Board = new GameBoard(vals[0]);
                MaxDepth = int.Parse(vals[1]);
                ExpectedBestMove = new Move(vals[2]);
            }
        }

        private static readonly int TestMaxHelperThreads = 0;
        private static readonly int PerfTestMaxHelperThreads = 0;
    }
}
