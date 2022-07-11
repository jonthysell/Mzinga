// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Core;
using Mzinga.Core.AI;
using Mzinga.Engine;

namespace Mzinga.Test
{
    [TestClass]
    public class GameAITests
    {
        [TestMethod]
        public void GameAI_NewTest()
        {
            GameAI ai = new GameAI(new GameAIConfig());
            Assert.IsNotNull(ai);
        }

        [TestMethod]
        [TestCategory("Performance")]
        public void GameAI_FirstTurnBestMoveByDepthPerfTest()
        {
            TimeSpan sum = TimeSpan.Zero;
            int iterations = 100;

            var config = EngineConfig.GetDefaultEngineConfig();

            for (int i = 0; i < iterations; i++)
            {
                var gb = new Board();
                GameAI ai = config.GetGameAI(gb.GameType);

                Stopwatch sw = Stopwatch.StartNew();
                _ = ai.GetBestMove(gb, 2, config.MaxHelperThreads);
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

            var config = EngineConfig.GetDefaultEngineConfig();

            var original = GetBoardOnFifthTurn();

            for (int i = 0; i < iterations; i++)
            {
                var gb = original.Clone();
                GameAI ai = config.GetGameAI(gb.GameType);

                Stopwatch sw = Stopwatch.StartNew();
                _ = ai.GetBestMove(gb, 2, config.MaxHelperThreads);
                sw.Stop();

                sum += sw.Elapsed;
            }

            Trace.WriteLine(string.Format("Average Ticks: {0}", sum.Ticks / iterations));
        }

        [TestMethod]
        [TestCategory("Performance")]
        public void GameAI_FirstTurnBestMoveByTimePerfTest()
        {
            var config = EngineConfig.GetDefaultEngineConfig();

            var gb = new Board();
            GameAI ai = config.GetGameAI(gb.GameType);

            int maxDepth = 0;

            ai.BestMoveFound += (sender, e) =>
            {
                maxDepth = Math.Max(maxDepth, e.Depth);
            };

            Move m = ai.GetBestMove(gb, TimeSpan.FromSeconds(5), config.MaxHelperThreads);

            Trace.WriteLine(string.Format("Max Depth: {0}", maxDepth));
        }

        [TestMethod]
        [TestCategory("Performance")]
        public void GameAI_FifthTurnBestMoveByTimePerfTest()
        {
            var config = EngineConfig.GetDefaultEngineConfig();

            var gb = GetBoardOnFifthTurn();
            GameAI ai = config.GetGameAI(gb.GameType);

            int maxDepth = 0;

            ai.BestMoveFound += (sender, e) =>
            {
                maxDepth = Math.Max(maxDepth, e.Depth);
            };

            Move m = ai.GetBestMove(gb, TimeSpan.FromSeconds(5), config.MaxHelperThreads);

            Trace.WriteLine(string.Format("Max Depth: {0}", maxDepth));
        }

        [TestMethod]
        [TestCategory("Performance")]
        public void GameAI_DifferentHelperThreadsTest()
        {
            var config = EngineConfig.GetDefaultEngineConfig();

            var gb = Board.ParseGameString(@"Base;InProgress;Black[17];wA1;bA1 wA1-;wA2 -wA1;bA2 bA1\;wQ \wA1;bQ bA2/;wA2 /bA2;bA3 \bQ;wA3 wA2\;bA3 wA3-;wG1 /wA2;bG1 bQ/;wQ -wA1;bA3 wA3\;wG1 bG1/;bS1 -bG1;wS1 /wA2;bS1 \wQ;wS1 bA3\;bG2 bG1\;wG2 -wA2;bS2 -bS1;wG2 bA2\;bG2 bS2\;wA2 \bS2;bB1 /bG2;wA2 -bS2;bB1 bG2\;wA2 wG2\;bS2 \wA1;wA2 \bA1;bB1 wQ\;wA2 bS2/");

            for (int maxHelpers = 0; maxHelpers < Environment.ProcessorCount / 2; maxHelpers++)
            {
                GameAI ai = config.GetGameAI(gb.GameType);
                Stopwatch sw = Stopwatch.StartNew();
                var bestMove = ai.GetBestMove(gb, TimeSpan.FromSeconds(5), maxHelpers);
                sw.Stop();
                Trace.WriteLine($"{maxHelpers}: {sw.ElapsedMilliseconds}ms {gb.GetMoveString(bestMove)}");
            }
        }

        [DataTestMethod]
        [DynamicData(nameof(GetEngineConfigOptions), DynamicDataSourceType.Method)]
        public void GameAI_CanSolveOneBestMoveToForceWinPuzzleTest(int maxHelperThreads, int quiescentSearchMaxDepth)
        {
            TestUtils.LoadAndExecuteTestCases<GameAIBestMoveTestCase>("PuzzleCandidate_IsOneBestMoveToForceWinPuzzleTest.csv", maxHelperThreads, quiescentSearchMaxDepth);
        }

        [DataTestMethod]
        [DynamicData(nameof(GetEngineConfigOptions), DynamicDataSourceType.Method)]
        public void GameAI_TreeStrapAICanSolveOneBestMoveToForceWinPuzzleTest(int maxHelperThreads, int quiescentSearchMaxDepth)
        {
            TestUtils.LoadAndExecuteTestCases<GameAITreeStrapTestCase>("PuzzleCandidate_IsOneBestMoveToForceWinPuzzleTest.csv", maxHelperThreads, quiescentSearchMaxDepth);
        }

        [DataTestMethod]
        [DynamicData(nameof(GetEngineConfigOptions), DynamicDataSourceType.Method)]
        public void GameAI_BlockWinningMoveIsBestMoveTest(int maxHelperThreads, int quiescentSearchMaxDepth)
        {
            TestUtils.LoadAndExecuteTestCases<GameAIBestMoveTestCase>("GameAI_BlockWinningMoveIsBestMoveTest.csv", maxHelperThreads, quiescentSearchMaxDepth);
        }

        private static IEnumerable<object[]> GetEngineConfigOptions()
        {
            var maxHelperThreads = new int?[] {  EngineConfig.MinMaxHelperThreads, /*EngineConfig.DefaultMaxHelperThreads*/ };
            var quiescentSearchMaxDepth = new int?[] { GameAIConfig.MinQuiescentSearchMaxDepth, GameAIConfig.DefaultQuiescentSearchMaxDepth };

            foreach (var mht in maxHelperThreads)
            {
                foreach (var qsmd in quiescentSearchMaxDepth)
                {
                    yield return new object[] { mht, qsmd };
                }
            }
        }

        private static Board GetBoardOnFifthTurn()
        {
            var gb = new Board();

            var config = EngineConfig.GetDefaultEngineConfig();

            GameAI ai = config.GetGameAI(gb.GameType);

            while (gb.CurrentPlayerTurn < 5)
            {
                gb.TryPlayMove(ai.GetBestMove(gb, 1, config.MaxHelperThreads), "");
            }

            return gb;
        }

        private static EventHandler<BestMoveFoundEventArgs> GetLogBestMoveFoundEventHandler(Board board)
        {
            return (sender, args) =>
            {
                var sb = new StringBuilder();
                sb.AppendJoin(';', board.GetMoveString(args.Move), args.Depth, args.Score.ToString("0.00"));

                if (args.PrincipalVariation.Count > 0)
                {
                    Board clone = board.Clone();
                    for (int i = 0; i < args.PrincipalVariation.Count; i++)
                    {
                        var move = args.PrincipalVariation[i];
                        sb.Append(';');
                        sb.Append(clone.GetMoveString(in move));
                        clone.TrustedPlay(in move);
                    }
                }

                Trace.TraceInformation(sb.ToString());
            };
        }

        private class GameAIBestMoveTestCase : GameAIBestMoveTestCaseBase
        {
            public override void Execute()
            {
                var config = EngineConfig.GetDefaultEngineConfig();
                config._maxHelperThreads= TestArgs?[0] as int?;
                config.QuiescentSearchMaxDepth = TestArgs?[1] as int?;

                GameAI ai = config.GetGameAI(Board.GameType);
#if DEBUG
                ai.BestMoveFound += GetLogBestMoveFoundEventHandler(Board);
#endif

                ActualBestMove = ai.GetBestMove(Board, MaxDepth, config.MaxHelperThreads);
                Assert.AreEqual(ExpectedBestMove, ActualBestMove, $"Expected: {Board.GetMoveString(ExpectedBestMove)}, Actual: {Board.GetMoveString(ActualBestMove)}.");
            }
        }

        private class GameAITreeStrapTestCase : GameAIBestMoveTestCaseBase
        {
            public override void Execute()
            {
                var config = EngineConfig.GetDefaultEngineConfig();
                config._maxHelperThreads= TestArgs?[0] as int?;
                config.QuiescentSearchMaxDepth = TestArgs?[1] as int?;

                // Use TreeStrap (on clone since it doesn't undo moves) to improve the metrics
                GameAI ai = config.GetGameAI(Board.GameType);
                ai.TreeStrap(Board.Clone(), MaxDepth, config.MaxHelperThreads);
                ai.ResetCaches();

                // Confirm best move hasn't changed
#if DEBUG
                ai.BestMoveFound += GetLogBestMoveFoundEventHandler(Board);
#endif
                ActualBestMove = ai.GetBestMove(Board, MaxDepth, config.MaxHelperThreads);
                Assert.AreEqual(ExpectedBestMove, ActualBestMove, $"Expected: {Board.GetMoveString(ExpectedBestMove)}, Actual: {Board.GetMoveString(ActualBestMove)}.");
            }
        }

        private abstract class GameAIBestMoveTestCaseBase : ITestCase
        {
            public object[] TestArgs { get; set; }

            public Board Board;
            public int MaxDepth;

            public Move ExpectedBestMove;
            public Move ActualBestMove;

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
    }
}
