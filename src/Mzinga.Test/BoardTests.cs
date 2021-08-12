// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Core;

namespace Mzinga.Test
{
    [TestClass]
    public class BoardTests
    {
        [TestMethod]
        public void Board_NewTest()
        {
            var b = new Board();
            Assert.IsNotNull(b);
        }

        [TestMethod]
        public void Board_CanMoveWithoutBreakingHive_InHandTest()
        {
            Board b = new Board();

            VerifyCanMoveWithoutBreakingHive(b, PieceName.wS1, true);
        }

        [TestMethod]
        public void Board_CanMoveWithoutBreakingHive_OnlyPieceInPlayTest()
        {
            Board b = Board.ParseGameString("Base;InProgress;Black[1];wS1");

            VerifyCanMoveWithoutBreakingHive(b, PieceName.wS1, true);
        }

        [TestMethod]
        public void Board_CanMoveWithoutBreakingHive_ClosedCircleTest()
        {
            Board b = Board.ParseGameString(@"Base;InProgress;Black[5];wS1;bS1 wS1-;wQ \wS1;bQ bS1/;wG1 wQ/;bA1 bQ\;wG1 /wQ;bA1 \bQ;wG1 -bA1");

            for (int pn = 0; pn < (int)PieceName.NumPieceNames; pn++)
            {
                var pieceName = (PieceName)pn;
                if (b.PieceInPlay(pieceName))
                {
                    VerifyCanMoveWithoutBreakingHive(b, pieceName, true);
                }
            }
        }

        [TestMethod]
        public void Board_CanMoveWithoutBreakingHive_OpenCircleTest()
        {
            Board b = Board.ParseGameString(@"Base;InProgress;Black[3];wS1;bS1 wS1-;wQ \wS1;bQ bS1/;wG1 wQ/");

            for (int pn = 0; pn < (int)PieceName.NumPieceNames; pn++)
            {
                var pieceName = (PieceName)pn;
                if (b.PieceInPlay(pieceName))
                {
                    VerifyCanMoveWithoutBreakingHive(b, pieceName, pieceName == PieceName.wG1 || pieceName == PieceName.bQ);
                }
            }
        }

        [TestMethod]
        public void Board_IsOneHive_NewGameTest()
        {
            Board b = new Board();
            Assert.IsTrue(b.IsOneHive());
        }

        [TestMethod]
        public void Board_IsOneHive_OnePieceTest()
        {
            Board b = Board.ParseGameString("Base;InProgress;Black[1];wS1");
            Assert.IsTrue(b.IsOneHive());
        }

        [TestMethod]
        public void Board_IsOneHive_ClosedCircleTest()
        {
            Board b = Board.ParseGameString(@"Base;InProgress;Black[5];wS1;bS1 wS1-;wQ \wS1;bQ bS1/;wG1 wQ/;bA1 bQ\;wG1 /wQ;bA1 \bQ;wG1 -bA1");
            Assert.IsTrue(b.IsOneHive());
        }

        [TestMethod]
        public void Board_IsOneHive_OpenCircleTest()
        {
            Board b = Board.ParseGameString(@"Base;InProgress;Black[3];wS1;bS1 wS1-;wQ \wS1;bQ bS1/;wG1 wQ/");
            Assert.IsTrue(b.IsOneHive());
        }

        [TestMethod]
        public void Board_IsOneHive_TwoHivesTest()
        {
            Board b = Board.ParseGameString(@"Base;InProgress;Black[3];wS1;bS1 wS1-;wQ \wS1;bQ bS1/;wG1 wQ/");
            Assert.IsTrue(b.IsOneHive());

            b.SetPosition(PieceName.wS1, Position.NullPosition, false);
            Assert.IsFalse(b.IsOneHive());
        }

        [TestMethod]
        public void Board_NewBoardValidMovesTest()
        {
            var b = new Board();
            Assert.IsNotNull(b);

            int numBaseBugsWithoutQueen = (int)BugType.NumBugTypes - 4;

            MoveSet validMoves = b.GetValidMoves();
            Assert.IsNotNull(validMoves);
            Assert.AreEqual(numBaseBugsWithoutQueen, validMoves.Count);

            b.Play(new Move(PieceName.wS1, Position.NullPosition, Position.OriginPosition));

            validMoves = b.GetValidMoves();
            Assert.IsNotNull(validMoves);
            Assert.AreEqual(numBaseBugsWithoutQueen * (int)Direction.NumDirections, validMoves.Count);
        }

        [TestMethod]
        public void Board_ValidMovesTest()
        {
            var b = new Board();
            Assert.IsNotNull(b);

            b.Play(new Move(PieceName.wS1, Position.NullPosition, Position.OriginPosition));
            b.Play(new Move(PieceName.bS1, Position.NullPosition, Position.OriginPosition.GetNeighborAt(Direction.Up)));

            MoveSet validMoves = b.GetValidMoves();
            Assert.IsNotNull(validMoves);
        }

        [TestMethod]
        public void Board_QueenMustPlayByFourthMoveValidMovesTest()
        {
            var b = new Board();
            Assert.IsNotNull(b);

            // Turn 1
            b.Play(new Move(PieceName.wS1, Position.NullPosition, Position.OriginPosition));
            b.Play(new Move(PieceName.bS1, Position.NullPosition, Position.OriginPosition.GetNeighborAt(Direction.Up)));

            // Turn 2
            b.Play(new Move(PieceName.wS2, Position.NullPosition, Position.OriginPosition.GetNeighborAt(Direction.Down)));
            b.Play(new Move(PieceName.bS2, Position.NullPosition, StraightLine(Position.OriginPosition, Direction.Up, 2)));

            // Turn 3
            b.Play(new Move(PieceName.wA1, Position.NullPosition, StraightLine(Position.OriginPosition, Direction.Down, 2)));
            b.Play(new Move(PieceName.bA1, Position.NullPosition, StraightLine(Position.OriginPosition, Direction.Up, 3)));

            // Turn 4
            MoveSet validMoves = b.GetValidMoves();
            Assert.IsNotNull(validMoves);
            Assert.AreEqual(7, validMoves.Count);

            foreach (Move move in validMoves)
            {
                Assert.AreEqual(PieceName.wQ, move.PieceName);
            }
        }

        [TestMethod]
        public void Board_CanCommitSuicideTest()
        {
            var b = new Board();
            Assert.IsNotNull(b);

            // Turn 1
            b.Play(new Move(PieceName.wS1, Position.NullPosition, Position.OriginPosition));
            b.Play(new Move(PieceName.bS1, Position.NullPosition, Position.OriginPosition.GetNeighborAt(Direction.Up)));

            // Turn 2
            b.Play(new Move(PieceName.wQ, Position.NullPosition, Position.OriginPosition.GetNeighborAt(Direction.Down)));
            b.Play(new Move(PieceName.bQ, Position.NullPosition, StraightLine(Position.OriginPosition, Direction.Up, 2)));

            // Turn 3
            b.Play(new Move(PieceName.wS2, Position.NullPosition, b.GetPosition(PieceName.wQ).GetNeighborAt(Direction.UpLeft)));
            b.Play(new Move(PieceName.bS2, Position.NullPosition, StraightLine(Position.OriginPosition, Direction.Up, 3)));

            // Turn 4
            b.Play(new Move(PieceName.wA1, Position.NullPosition, b.GetPosition(PieceName.wQ).GetNeighborAt(Direction.UpRight)));
            b.Play(new Move(PieceName.bA1, Position.NullPosition, StraightLine(Position.OriginPosition, Direction.Up, 4)));

            // Turn 5
            b.Play(new Move(PieceName.wA2, Position.NullPosition, b.GetPosition(PieceName.wQ).GetNeighborAt(Direction.DownLeft)));
            b.Play(new Move(PieceName.bA2, Position.NullPosition, StraightLine(Position.OriginPosition, Direction.Up, 5)));

            // Turn 6
            b.Play(new Move(PieceName.wA3, Position.NullPosition, b.GetPosition(PieceName.wQ).GetNeighborAt(Direction.DownRight)));
            b.Play(new Move(PieceName.bA3, Position.NullPosition, StraightLine(Position.OriginPosition, Direction.Up, 6)));

            // Turn 7
            b.Play(new Move(PieceName.wB1, Position.NullPosition, b.GetPosition(PieceName.wQ).GetNeighborAt(Direction.Down)));

            Assert.AreEqual(BoardState.BlackWins, b.BoardState);

            MoveSet validMoves = b.GetValidMoves();
            Assert.IsNotNull(validMoves);
            Assert.AreEqual(0, validMoves.Count);
        }

        [TestMethod]
        public void Board_ValidMovesForQueenBeeTest()
        {
            TestUtils.LoadAndExecuteTestCases<BoardValidMoveTestCase>("Board_ValidMovesForQueenBeeTest.csv");
        }

        [TestMethod]
        public void Board_ValidMovesForSpiderTest()
        {
            TestUtils.LoadAndExecuteTestCases<BoardValidMoveTestCase>("Board_ValidMovesForSpiderTest.csv");
        }

        [TestMethod]
        public void Board_ValidMovesForBeetleTest()
        {
            TestUtils.LoadAndExecuteTestCases<BoardValidMoveTestCase>("Board_ValidMovesForBeetleTest.csv");
        }

        [TestMethod]
        public void Board_ValidMovesForGrasshopperTest()
        {
            TestUtils.LoadAndExecuteTestCases<BoardValidMoveTestCase>("Board_ValidMovesForGrasshopperTest.csv");
        }

        [TestMethod]
        public void Board_ValidMovesForSoldierAntTest()
        {
            TestUtils.LoadAndExecuteTestCases<BoardValidMoveTestCase>("Board_ValidMovesForSoldierAntTest.csv");
        }

        [TestMethod]
        public void Board_ValidMovesForMosquitoTest()
        {
            TestUtils.LoadAndExecuteTestCases<BoardValidMoveTestCase>("Board_ValidMovesForMosquitoTest.csv");
        }

        [TestMethod]
        public void Board_ValidMovesForLadybugTest()
        {
            TestUtils.LoadAndExecuteTestCases<BoardValidMoveTestCase>("Board_ValidMovesForLadybugTest.csv");
        }

        [TestMethod]
        public void Board_ValidMovesForPillbugTest()
        {
            TestUtils.LoadAndExecuteTestCases<BoardValidMoveTestCase>("Board_ValidMovesForPillbugTest.csv");
        }

        [TestMethod]
        public void Board_InvalidMovesByRuleTest()
        {
            TestUtils.LoadAndExecuteTestCases<BoardInvalidMoveTestCase>("Board_InvalidMovesByRuleTest.csv");
        }

        [TestMethod]
        public void Board_Base_PerftTest()
        {
            TestUtils.LoadAndExecuteTestCases<BoardPerftTestCase>("Board_Base_PerftTest.csv");
        }

        [TestMethod]
        public void Board_BaseM_PerftTest()
        {
            TestUtils.LoadAndExecuteTestCases<BoardPerftTestCase>("Board_BaseM_PerftTest.csv");
        }

        [TestMethod]
        public void Board_BaseL_PerftTest()
        {
            TestUtils.LoadAndExecuteTestCases<BoardPerftTestCase>("Board_BaseL_PerftTest.csv");
        }

        [TestMethod]
        public void Board_BaseP_PerftTest()
        {
            TestUtils.LoadAndExecuteTestCases<BoardPerftTestCase>("Board_BaseP_PerftTest.csv");
        }

        [TestMethod]
        public void Board_BaseML_PerftTest()
        {
            TestUtils.LoadAndExecuteTestCases<BoardPerftTestCase>("Board_BaseML_PerftTest.csv");
        }

        [TestMethod]
        public void Board_BaseMP_PerftTest()
        {
            TestUtils.LoadAndExecuteTestCases<BoardPerftTestCase>("Board_BaseMP_PerftTest.csv");
        }

        [TestMethod]
        public void Board_BaseLP_PerftTest()
        {
            TestUtils.LoadAndExecuteTestCases<BoardPerftTestCase>("Board_BaseLP_PerftTest.csv");
        }

        [TestMethod]
        public void Board_BaseMLP_PerftTest()
        {
            TestUtils.LoadAndExecuteTestCases<BoardPerftTestCase>("Board_BaseMLP_PerftTest.csv");
        }

        [TestMethod]
        [TestCategory("Performance")]
        public void Board_NewGamePerftTest()
        {
            long[] expectedNodes = new long[]
            {
                1, 4, 96, 1440, // Confirmed
                21600, 516240, 12219480, // Unconfirmed
            };

            for (int depth = 0; depth < expectedNodes.Length; depth++)
            {
                Board board = new Board();

                Stopwatch sw = Stopwatch.StartNew();
                long actualNodes = board.CalculatePerft(depth);
                sw.Stop();

                Assert.AreEqual(expectedNodes[depth], actualNodes, string.Format("Failed at depth {0}.", depth));
                Trace.WriteLine(string.Format("{0,-9} = {1,16:#,##0} in {2,16:#,##0} ms. {3,8:#,##0.0} KN/s", string.Format("perft({0})", depth), actualNodes, sw.ElapsedMilliseconds, Math.Round(actualNodes / (double)sw.ElapsedMilliseconds, 1)));
            }
        }

        private static void VerifyCanMoveWithoutBreakingHive(Board board, PieceName pieceName, bool canMoveExpected)
        {
            bool canMoveActual = board.CanMoveWithoutBreakingHive(pieceName);
            Assert.AreEqual(canMoveExpected, canMoveActual);
        }

        private static Position StraightLine(Position start, Direction direction, int length)
        {
            if (length <= 0)
            {
                throw new ArgumentNullException(nameof(length));
            }

            var pos = start;

            for (int i = 0; i < length; i++)
            {
                pos = pos.GetNeighborAt(direction);
            }

            return pos;
        }

        private class BoardValidMoveTestCase : ITestCase
        {
            public Board Board;

            public string[] ValidMoveStrings;
            public Move[] ValidMoves;

            public void Execute()
            {
                Trace.TraceInformation($"Current Board: {Board.GetGameString()}");
                for (int i = 0; i < ValidMoveStrings.Length; i++)
                {
                    Trace.TraceInformation($"Playing: {ValidMoveStrings[i]}");
                    Board.Play(ValidMoves[i]);
                    _ = Board.TryUndoLastMove();
                }
            }

            public void Parse(string s)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    throw new ArgumentNullException(nameof(s));
                }

                s = s.Trim();

                string[] vals = s.Split('\t');

                Board = Board.ParseGameString(vals[0]);

                ValidMoveStrings = vals[1].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                ValidMoves = new Move[ValidMoveStrings.Length];

                for (int i = 0; i < ValidMoveStrings.Length; i++)
                {
                    _ = Board.TryParseMove(ValidMoveStrings[i], out ValidMoves[i], out ValidMoveStrings[i]);
                }
            }
        }

        private class BoardInvalidMoveTestCase : ITestCase
        {
            public Board Board;
            
            public string InvalidMoveString;
            public Move InvalidMove;

            public void Execute()
            {
                try
                {
                    Trace.TraceInformation($"Current Board: {Board.GetGameString()}");
                    Trace.TraceInformation($"Playing: {InvalidMoveString}");
                    Board.Play(InvalidMove);
                    Assert.Fail();
                }
                catch (InvalidMoveException ex)
                {
                    Trace.TraceInformation($"Invalid move reason: {ex.Message}");
                    Assert.AreEqual(InvalidMove, ex.Move, $"Expected: {Board.GetMoveString(InvalidMove)}, Actual: {Board.GetMoveString(ex.Move)}.");
                }
            }

            public void Parse(string s)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    throw new ArgumentNullException(nameof(s));
                }

                s = s.Trim();

                string[] vals = s.Split('\t');

                Board = Board.ParseGameString(vals[0]);
                _ = Board.TryParseMove(vals[1], out InvalidMove, out InvalidMoveString);
            }
        }

        private class BoardPerftTestCase : ITestCase
        {
            public Board Board;

            public int Depth;
            public long NodeCount;

            public const int MaxDepth = 6;

            public void Execute()
            {
                Trace.TraceInformation($"Current Board: {Board.GetGameString()}");
                if (Depth > MaxDepth)
                {
                    Trace.TraceInformation($"Skipping slow test (Depth = {Depth})");
                    return;
                }
                var actualNodeCount = Board.ParallelPerft(Depth);
                Assert.AreEqual(NodeCount, actualNodeCount);
            }

            public void Parse(string s)
            {
                if (string.IsNullOrWhiteSpace(s))
                {
                    throw new ArgumentNullException(nameof(s));
                }

                s = s.Trim();

                string[] vals = s.Split('\t');

                Board = Board.ParseGameString(vals[0]);

                Depth = int.Parse(vals[1]);
                NodeCount = long.Parse(vals[2]);
            }
        }
    }
}
