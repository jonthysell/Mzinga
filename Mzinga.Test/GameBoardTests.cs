// 
// GameBoardTests.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017, 2018, 2019 Jon Thysell <http://jonthysell.com>
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

namespace Mzinga.Test
{
    [TestClass]
    [DeploymentItem("TestCases\\GameBoardTests")]
    public class GameBoardTests
    {
        [TestMethod]
        public void GameBoard_NewTest()
        {
            GameBoard b = new GameBoard();
            Assert.IsNotNull(b);
        }

        [TestMethod]
        public void GameBoard_NewBoardValidMovesTest()
        {
            GameBoard b = new GameBoard();
            Assert.IsNotNull(b);

            int numBaseBugsWithoutQueen = EnumUtils.NumBugTypes - 4;

            MoveSet validMoves = b.GetValidMoves();
            Assert.IsNotNull(validMoves);
            Assert.AreEqual(numBaseBugsWithoutQueen, validMoves.Count);

            b.Play(new Move(PieceName.WhiteSpider1, Position.Origin));

            validMoves = b.GetValidMoves();
            Assert.IsNotNull(validMoves);
            Assert.AreEqual(numBaseBugsWithoutQueen * EnumUtils.NumDirections, validMoves.Count);
        }

        [TestMethod]
        public void GameBoard_ValidMovesTest()
        {
            GameBoard b = new GameBoard();
            Assert.IsNotNull(b);

            b.Play(new Move(PieceName.WhiteSpider1, Position.Origin));
            b.Play(new Move(PieceName.BlackSpider1, Position.Origin.NeighborAt(Direction.Up)));

            MoveSet validMoves = b.GetValidMoves();
            Assert.IsNotNull(validMoves);
        }

        [TestMethod]
        public void GameBoard_QueenMustPlayByFourthMoveValidMovesTest()
        {
            GameBoard b = new GameBoard();
            Assert.IsNotNull(b);

            // Turn 1
            b.Play(new Move(PieceName.WhiteSpider1, Position.Origin));
            b.Play(new Move(PieceName.BlackSpider1, Position.Origin.NeighborAt(Direction.Up)));

            // Turn 2
            b.Play(new Move(PieceName.WhiteSpider2, Position.Origin.NeighborAt(Direction.Down)));
            b.Play(new Move(PieceName.BlackSpider2, Position.Origin.NeighborAt(StraightLine(Direction.Up, 2))));

            // Turn 3
            b.Play(new Move(PieceName.WhiteSoldierAnt1, Position.Origin.NeighborAt(StraightLine(Direction.Down, 2))));
            b.Play(new Move(PieceName.BlackSoldierAnt1, Position.Origin.NeighborAt(StraightLine(Direction.Up, 3))));

            // Turn 4
            MoveSet validMoves = b.GetValidMoves();
            Assert.IsNotNull(validMoves);
            Assert.AreEqual(7, validMoves.Count);

            foreach (Move move in validMoves)
            {
                Assert.AreEqual(PieceName.WhiteQueenBee, move.PieceName);
            }
        }

        [TestMethod]
        public void GameBoard_CanCommitSuicideTest()
        {
            GameBoard b = new GameBoard();
            Assert.IsNotNull(b);

            // Turn 1
            b.Play(new Move(PieceName.WhiteSpider1, Position.Origin));
            b.Play(new Move(PieceName.BlackSpider1, Position.Origin.NeighborAt(Direction.Up)));

            // Turn 2
            b.Play(new Move(PieceName.WhiteQueenBee, Position.Origin.NeighborAt(Direction.Down)));
            b.Play(new Move(PieceName.BlackQueenBee, Position.Origin.NeighborAt(StraightLine(Direction.Up, 2))));

            // Turn 3
            b.Play(new Move(PieceName.WhiteSpider2, b.GetPiecePosition(PieceName.WhiteQueenBee).NeighborAt(Direction.UpLeft)));
            b.Play(new Move(PieceName.BlackSpider2, Position.Origin.NeighborAt(StraightLine(Direction.Up, 3))));

            // Turn 4
            b.Play(new Move(PieceName.WhiteSoldierAnt1, b.GetPiecePosition(PieceName.WhiteQueenBee).NeighborAt(Direction.UpRight)));
            b.Play(new Move(PieceName.BlackSoldierAnt1, Position.Origin.NeighborAt(StraightLine(Direction.Up, 4))));

            // Turn 5
            b.Play(new Move(PieceName.WhiteSoldierAnt2, b.GetPiecePosition(PieceName.WhiteQueenBee).NeighborAt(Direction.DownLeft)));
            b.Play(new Move(PieceName.BlackSoldierAnt2, Position.Origin.NeighborAt(StraightLine(Direction.Up, 5))));

            // Turn 6
            b.Play(new Move(PieceName.WhiteSoldierAnt3, b.GetPiecePosition(PieceName.WhiteQueenBee).NeighborAt(Direction.DownRight)));
            b.Play(new Move(PieceName.BlackSoldierAnt3, Position.Origin.NeighborAt(StraightLine(Direction.Up, 6))));

            // Turn 7
            b.Play(new Move(PieceName.WhiteBeetle1, b.GetPiecePosition(PieceName.WhiteQueenBee).NeighborAt(Direction.Down)));

            Assert.AreEqual(BoardState.BlackWins, b.BoardState);

            MoveSet validMoves = b.GetValidMoves();
            Assert.IsNotNull(validMoves);
            Assert.AreEqual(0, validMoves.Count);
        }

        [TestMethod]
        public void GameBoard_ValidMovesForQueenBeeTest()
        {
            TestUtils.LoadAndExecuteTestCases<GameBoardValidMoveTestCase>("GameBoard_ValidMovesForQueenBeeTest.csv");
        }

        [TestMethod]
        public void GameBoard_ValidMovesForSpiderTest()
        {
            TestUtils.LoadAndExecuteTestCases<GameBoardValidMoveTestCase>("GameBoard_ValidMovesForSpiderTest.csv");
        }

        [TestMethod]
        public void GameBoard_ValidMovesForBeetleTest()
        {
            TestUtils.LoadAndExecuteTestCases<GameBoardValidMoveTestCase>("GameBoard_ValidMovesForBeetleTest.csv");
        }

        [TestMethod]
        public void GameBoard_ValidMovesForGrasshopperTest()
        {
            TestUtils.LoadAndExecuteTestCases<GameBoardValidMoveTestCase>("GameBoard_ValidMovesForGrasshopperTest.csv");
        }

        [TestMethod]
        public void GameBoard_ValidMovesForSoldierAntTest()
        {
            TestUtils.LoadAndExecuteTestCases<GameBoardValidMoveTestCase>("GameBoard_ValidMovesForSoldierAntTest.csv");
        }

        [TestMethod]
        public void GameBoard_ValidMovesForMosquitoTest()
        {
            TestUtils.LoadAndExecuteTestCases<GameBoardValidMoveTestCase>("GameBoard_ValidMovesForMosquitoTest.csv");
        }

        [TestMethod]
        public void GameBoard_ValidMovesForLadybugTest()
        {
            TestUtils.LoadAndExecuteTestCases<GameBoardValidMoveTestCase>("GameBoard_ValidMovesForLadybugTest.csv");
        }

        [TestMethod]
        public void GameBoard_ValidMovesForPillbugTest()
        {
            TestUtils.LoadAndExecuteTestCases<GameBoardValidMoveTestCase>("GameBoard_ValidMovesForPillbugTest.csv");
        }

        [TestMethod]
        public void GameBoard_InvalidMovesByRuleTest()
        {
            TestUtils.LoadAndExecuteTestCases<GameBoardInvalidMoveTestCase>("GameBoard_InvalidMovesByRuleTest.csv");
        }

        [TestMethod]
        [TestCategory("Performance")]
        public void GameBoard_NewGamePerftTest()
        {
            long[] expectedNodes = new long[]
            {
                1, 4, 96, 1440, // Confirmed
                21600, 516240, 12219480, // Unconfirmed
                //181641900, 2657392800, 99375893436 // Unconfirmed and too long to test
            };

            for (int depth = 0; depth < expectedNodes.Length; depth++)
            {
                GameBoard gameBoard = new GameBoard();

                Stopwatch sw = Stopwatch.StartNew();
                long actualNodes = gameBoard.CalculatePerft(depth);
                sw.Stop();

                Assert.AreEqual(expectedNodes[depth], actualNodes, string.Format("Failed at depth {0}.", depth));
                Trace.WriteLine(string.Format("{0,-9} = {1,16:#,##0} in {2,16:#,##0} ms. {3,8:#,##0.0} KN/s", string.Format("perft({0})", depth), actualNodes, sw.ElapsedMilliseconds, Math.Round(actualNodes / (double)sw.ElapsedMilliseconds, 1)));
            }
        }

        private static Direction[] StraightLine(Direction direction, int length)
        {
            if (length <= 0)
            {
                throw new ArgumentNullException(nameof(length));
            }

            Direction[] line = new Direction[length];

            for (int i = 0; i < line.Length; i++)
            {
                line[i] = direction;
            }

            return line;
        }

        private class GameBoardValidMoveTestCase : ITestCase
        {
            public GameBoard Board;

            public string[] ValidMoveStrings;
            public Move[] ValidMoves;

            public void Execute()
            {
                Trace.TraceInformation($"Current Board: {Board.ToGameString()}");
                for (int i = 0; i < ValidMoveStrings.Length; i++)
                {
                    Trace.TraceInformation($"Playing: {ValidMoveStrings[i]}");
                    Board.Play(ValidMoves[i]);
                    Board.UndoLastMove();
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

                Board = GameBoard.ParseGameString(vals[0]);

                ValidMoveStrings = vals[1].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                ValidMoves = new Move[ValidMoveStrings.Length];

                for (int i = 0; i < ValidMoveStrings.Length; i++)
                {
                    ValidMoves[i] = NotationUtils.ParseMoveString(Board, ValidMoveStrings[i]);
                }
            }
        }

        private class GameBoardInvalidMoveTestCase : ITestCase
        {
            public GameBoard Board;
            
            public string InvalidMoveString;
            public Move InvalidMove;

            public void Execute()
            {
                try
                {
                    Trace.TraceInformation($"Current Board: {Board.ToGameString()}");
                    Trace.TraceInformation($"Playing: {InvalidMoveString}");
                    Board.Play(InvalidMove);
                    Assert.Fail();
                }
                catch (InvalidMoveException ex)
                {
                    Trace.TraceInformation($"Invalid move reason: {ex.Message}");
                    Assert.AreEqual(InvalidMove, ex.Move);
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

                Board = GameBoard.ParseGameString(vals[0]);
                InvalidMoveString = vals[1];
                InvalidMove = NotationUtils.ParseMoveString(Board, vals[1]);
            }
        }
    }

    static partial class PositionExtensions
    {
        public static Position NeighborAt(this Position pos, Direction[] directions)
        {
            if (null == directions)
            {
                throw new ArgumentNullException(nameof(directions));
            }

            Position neighbor = pos;
            for (int i = 0; i < directions.Length; i++)
            {
                neighbor = neighbor.NeighborAt(directions[i]);
            }

            return neighbor;
        }
    }
}
