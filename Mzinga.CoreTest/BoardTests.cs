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

            foreach (Piece p in b.PiecesInPlay)
            {
                VerifyCanMoveWithoutBreakingHive(b, p.PieceName, true);
            }
        }

        [TestMethod]
        public void Board_CanMoveWithoutBreakingHive_OpenCircleTest()
        {
            MockBoard b = new MockBoard("InProgress;Black[3];WQ[-1,0,1];WS1[0,0,0];WG1[-2,1,1];BQ[-1,2,-1];BS1[0,1,-1]");

            foreach (Piece p in b.PiecesInPlay)
            {
                VerifyCanMoveWithoutBreakingHive(b, p.PieceName, p.PieceName == PieceName.WhiteGrasshopper1 || p.PieceName == PieceName.BlackQueenBee);
            }
        }

        private void VerifyCanMoveWithoutBreakingHive(MockBoard board, PieceName pieceName, bool canMoveExpected)
        {
            Assert.IsNotNull(board);

            Piece piece = board.GetPiece(pieceName);
            Assert.IsNotNull(piece);

            bool canMoveActual = board.CanMoveWithoutBreakingHive(piece);
            Assert.AreEqual(canMoveExpected, canMoveActual);
        }
    }

    public class MockBoard : Board
    {
        public MockBoard() : base() { }

        public MockBoard(string boardString) : base(boardString) { }

        public new bool CanMoveWithoutBreakingHive(Piece targetPiece)
        {
            return base.CanMoveWithoutBreakingHive(targetPiece);
        }
    }
}
