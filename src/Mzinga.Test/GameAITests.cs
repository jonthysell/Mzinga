// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

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
                var gb = new Board();
                GameAI ai = GetTestGameAI(gb.GameType);

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

            var original = GetBoardOnFifthTurn();

            for (int i = 0; i < iterations; i++)
            {
                var gb = original.Clone();
                GameAI ai = GetTestGameAI(gb.GameType);

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
            var gb = new Board();
            GameAI ai = GetTestGameAI(gb.GameType);

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
            var gb = GetBoardOnFifthTurn();
            GameAI ai = GetTestGameAI(gb.GameType);

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

        private static Board GetBoardOnFifthTurn()
        {
            var gb = new Board();
            GameAI ai = GetTestGameAI(gb.GameType);

            while (gb.CurrentPlayerTurn < 5)
            {
                gb.TryPlayMove(ai.GetBestMove(gb, 1, TestMaxHelperThreads), "");
            }

            return gb;
        }

        public static GameAI GetTestGameAI(GameType gameType)
        {
            return TestUtils.DefaultGameEngineConfig.GetGameAI(gameType);
        }

        private class GameAIBestMoveTestCase : ITestCase
        {
            public Board Board;
            public int MaxDepth;

            public Move ExpectedBestMove;
            public Move ActualBestMove;

            public void Execute()
            {
                GameAI ai = GetTestGameAI(Board.GameType);
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

                Assert.IsTrue(Board.TryParseGameString(vals[0], false, out Board));
                MaxDepth = int.Parse(vals[1]);
                Assert.IsTrue(Board.TryParseMove(vals[2], out ExpectedBestMove, out string _));
            }
        }

        private static readonly int TestMaxHelperThreads = 0;
        private static readonly int PerfTestMaxHelperThreads = 0;
    }
}
