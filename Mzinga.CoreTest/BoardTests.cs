// 
// BoardTests.cs
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
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Core;

namespace Mzinga.CoreTest
{
    [TestClass]
    public class BoardTests
    {
        [TestMethod]
        public void Board_NewTest()
        {
            Board b = new Board();
            Assert.IsNotNull(b);
        }

        [TestMethod]
        public void Board_CanMoveWithoutBreakingHive_InHandTest()
        {
            MockBoard b = new MockBoard();

            VerifyCanMoveWithoutBreakingHive(b, PieceName.WhiteSpider1, true);
        }

        [TestMethod]
        public void Board_CanMoveWithoutBreakingHive_OnlyPieceInPlayTest()
        {
            MockBoard b = new MockBoard("InProgress;Black[1];WS1[0,0,0]");

            VerifyCanMoveWithoutBreakingHive(b, PieceName.WhiteSpider1, true);
        }

        [TestMethod]
        public void Board_CanMoveWithoutBreakingHive_ClosedCircleTest()
        {
            MockBoard b = new MockBoard("InProgress;Black[4];WQ[-1,0,1];WS1[0,0,0];WG1[-2,1,1];BQ[-1,2,-1];BS1[0,1,-1];BG1[-2,2,0]");

            foreach (PieceName pieceName in b.PiecesInPlay)
            {
                VerifyCanMoveWithoutBreakingHive(b, pieceName, true);
            }
        }

        [TestMethod]
        public void Board_CanMoveWithoutBreakingHive_OpenCircleTest()
        {
            MockBoard b = new MockBoard("InProgress;Black[3];WQ[-1,0,1];WS1[0,0,0];WG1[-2,1,1];BQ[-1,2,-1];BS1[0,1,-1]");

            foreach (PieceName pieceName in b.PiecesInPlay)
            {
                VerifyCanMoveWithoutBreakingHive(b, pieceName, pieceName == PieceName.WhiteGrasshopper1 || pieceName == PieceName.BlackQueenBee);
            }
        }

        [TestMethod]
        public void Board_IsOneHive_NewGameTest()
        {
            MockBoard b = new MockBoard();
            Assert.IsTrue(b.IsOneHive());
        }

        [TestMethod]
        public void Board_IsOneHive_OnePieceTest()
        {
            MockBoard b = new MockBoard("InProgress;Black[1];WS1[0,0,0]");
            Assert.IsTrue(b.IsOneHive());
        }

        [TestMethod]
        public void Board_IsOneHive_ClosedCircleTest()
        {
            MockBoard b = new MockBoard("InProgress;Black[4];WQ[-1,0,1];WS1[0,0,0];WG1[-2,1,1];BQ[-1,2,-1];BS1[0,1,-1];BG1[-2,2,0]");
            Assert.IsTrue(b.IsOneHive());
        }

        [TestMethod]
        public void Board_IsOneHive_OpenCircleTest()
        {
            MockBoard b = new MockBoard("InProgress;Black[3];WQ[-1,0,1];WS1[0,0,0];WG1[-2,1,1];BQ[-1,2,-1];BS1[0,1,-1]");
            Assert.IsTrue(b.IsOneHive());
        }

        [TestMethod]
        public void Board_IsOneHive_TwoHivesTest()
        {
            MockBoard b = new MockBoard("InProgress;Black[3];WQ[-1,0,1];WS1[0,0,0];WG1[-2,1,1];BQ[-1,2,-1];BS1[0,1,-1]");
            Assert.IsTrue(b.IsOneHive());

            b.MovePiece(b.GetPiece(PieceName.WhiteSpider1), null);
            Assert.IsFalse(b.IsOneHive());
        }

        [TestMethod]
        public void Board_GetValidMoves_BeetleStackTest()
        {
            VerifyValidMoves("InProgress;White[5];WQ[1,-1,0];WS1[0,0,0];WB1[0,0,0,1];BQ[1,1,-2];BS1[0,1,-1];BB1[0,1,-1,1]",
                "WQ[1,0,-1];WQ[0,-1,1];WS2[2,-1,-1];WS2[2,-2,0];WS2[1,-2,1];WS2[0,-1,1];WS2[-1,0,1];WB1[0,1,-1,2];WB1[1,0,-1];WB1[1,-1,0,1];WB1[0,-1,1];WB1[-1,0,1];WB1[-1,1,0];WB2[2,-1,-1];WB2[2,-2,0];WB2[1,-2,1];WB2[0,-1,1];WB2[-1,0,1];WG1[2,-1,-1];WG1[2,-2,0];WG1[1,-2,1];WG1[0,-1,1];WG1[-1,0,1];WA1[2,-1,-1];WA1[2,-2,0];WA1[1,-2,1];WA1[0,-1,1];WA1[-1,0,1]");
        }

        [TestMethod]
        [TestCategory("Performance")]
        public void Board_ValidMoves_PerfTest()
        {
            TimeSpan sum = TimeSpan.Zero;
            int iterations = 10000;

            for (int i = 0; i < iterations; i++)
            {
                Board b = new Board("InProgress;White[13];WQ[1,0,-1];WS1[0,0,0];WS2[2,-1,-1];WB1[0,1,-1];WB2[2,-2,0];WG1[3,-1,-2];WG2[4,-2,-2];WG3[5,-2,-3];WA1[3,0,-3];WA2[6,-2,-4];WA3[5,-3,-2];BQ[-2,1,1];BS1[-1,1,0];BS2[-3,2,1];BB1[-1,0,1];BB2[-3,3,0];BG1[-4,2,2];BG2[-5,3,2];BG3[-6,3,3];BA1[-4,1,3];BA2[-7,3,4];BA3[-6,4,2]");

                Stopwatch sw = Stopwatch.StartNew();
                MoveSet moves = b.GetValidMoves();
                sw.Stop();

                sum += sw.Elapsed;
            }

            Trace.WriteLine(string.Format("Average Ticks: {0}", sum.Ticks / iterations));
        }

        private void VerifyCanMoveWithoutBreakingHive(MockBoard board, PieceName pieceName, bool canMoveExpected)
        {
            Assert.IsNotNull(board);

            Piece piece = board.GetPiece(pieceName);
            Assert.IsNotNull(piece);

            bool canMoveActual = board.CanMoveWithoutBreakingHive(piece);
            Assert.AreEqual(canMoveExpected, canMoveActual);
        }

        private void VerifyValidMoves(string boardString, string expectedMovesString)
        {
            Board board = new Board(boardString);
            Assert.IsNotNull(board);

            MoveSet expectedMoves = new MoveSet(expectedMovesString);
            Assert.IsNotNull(expectedMoves);

            VerifyValidMoves(board, expectedMoves);
        }

        private void VerifyValidMoves(Board board, MoveSet expectedMoves)
        {
            Assert.IsNotNull(board);
            Assert.IsNotNull(expectedMoves);

            MoveSet actualMoves = board.GetValidMoves();
            TestUtils.AssertHaveEqualChildren(expectedMoves, actualMoves);
        }
    }

    public class MockBoard : Board
    {
        public MockBoard() : base() { }

        public MockBoard(string boardString) : base(boardString) { }

        public new Piece GetPiece(PieceName pieceName)
        {
            return base.GetPiece(pieceName);
        }

        public new bool CanMoveWithoutBreakingHive(Piece targetPiece)
        {
            return base.CanMoveWithoutBreakingHive(targetPiece);
        }

        public new void MovePiece(Piece piece, Position newPosition)
        {
            base.MovePiece(piece, newPosition);
        }
    }
}
