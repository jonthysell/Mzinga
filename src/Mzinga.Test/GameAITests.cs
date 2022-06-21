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
        [TestCategory("Performance")]
        public void GameAI_DifferentHelperThreadsTest()
        {
            var gb = Board.ParseGameString(@"Base;InProgress;Black[17];wA1;bA1 wA1-;wA2 -wA1;bA2 bA1\;wQ \wA1;bQ bA2/;wA2 /bA2;bA3 \bQ;wA3 wA2\;bA3 wA3-;wG1 /wA2;bG1 bQ/;wQ -wA1;bA3 wA3\;wG1 bG1/;bS1 -bG1;wS1 /wA2;bS1 \wQ;wS1 bA3\;bG2 bG1\;wG2 -wA2;bS2 -bS1;wG2 bA2\;bG2 bS2\;wA2 \bS2;bB1 /bG2;wA2 -bS2;bB1 bG2\;wA2 wG2\;bS2 \wA1;wA2 \bA1;bB1 wQ\;wA2 bS2/");

            for (int maxHelpers = 0; maxHelpers < Environment.ProcessorCount / 2; maxHelpers++)
            {
                GameAI ai = GetTestGameAI(gb.GameType);
                Stopwatch sw = Stopwatch.StartNew();
                var bestMove = ai.GetBestMove(gb, TimeSpan.FromSeconds(5), maxHelpers);
                sw.Stop();
                Trace.WriteLine($"{maxHelpers}: {sw.ElapsedMilliseconds}ms {gb.GetMoveString(bestMove)}");
            }
        }

        [TestMethod]
        public void GameAI_CanSolveOneBestMoveToForceWinPuzzleTest()
        {
            TestUtils.LoadAndExecuteTestCases<GameAIBestMoveTestCase>("PuzzleCandidate_IsOneBestMoveToForceWinPuzzleTest.csv");
        }

        [TestMethod]
        public void GameAI_TreeStrapImprovesBoardScoreForOneBestMoveToForceWinPuzzleTest()
        {
            TestUtils.LoadAndExecuteTestCases<GameAITreeStrapTestCase>("PuzzleCandidate_IsOneBestMoveToForceWinPuzzleTest.csv");
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

        private class GameAIBestMoveTestCase : GameAIBestMoveTestCaseBase
        {
            public Move ActualBestMove;

            public override void Execute()
            {
                GameAI ai = GetTestGameAI(Board.GameType);
                ActualBestMove = ai.GetBestMove(Board, MaxDepth, TestMaxHelperThreads);
                Assert.AreEqual(ExpectedBestMove, ActualBestMove, $"Expected: {Board.GetMoveString(ExpectedBestMove)}, Actual: {Board.GetMoveString(ActualBestMove)}.");
            }
        }

        private class GameAITreeStrapTestCase : GameAIBestMoveTestCaseBase
        {
            public override void Execute()
            {
                GameAI ai = GetTestGameAI(Board.GameType);

                double colorValue = Board.CurrentColor == PlayerColor.White ? 1.0 : -1.0;

                Board.TrustedPlay(ExpectedBestMove);
                var clone = Board.Clone();

                double startScore = colorValue * ai.CalculateBoardScore(Board);

                ai.TreeStrap(Board, MaxDepth, TestMaxHelperThreads);
                ai.ResetCaches();

                double endScore = colorValue * ai.CalculateBoardScore(clone);

                Assert.IsTrue(endScore >= startScore);
            }
        }

        private abstract class GameAIBestMoveTestCaseBase : ITestCase
        {
            public Board Board;
            public int MaxDepth;

            public Move ExpectedBestMove;

            public abstract void Execute();

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
