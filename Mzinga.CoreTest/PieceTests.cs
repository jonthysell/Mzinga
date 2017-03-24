// 
// PieceTests.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017 Jon Thysell <http://jonthysell.com>
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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Core;

namespace Mzinga.CoreTest
{
    [TestClass]
    public class PieceTests
    {
        [TestMethod]
        public void Piece_NewInHandTest()
        {
            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                Piece p = new Piece(pieceName);
                VerifyPieceProperties(p, pieceName, null);
            }
        }

        [TestMethod]
        public void Piece_NewInPlayTest()
        {
            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                Piece p = new Piece(pieceName, Position.Origin);
                VerifyPieceProperties(p, pieceName, Position.Origin);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Piece_InvalidNewTest()
        {
            Piece p = new Piece(PieceName.INVALID);
        }

        [TestMethod]
        public void Piece_NewInHandPieceStringTest()
        {
            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                string pieceString = string.Format("{0}[]", EnumUtils.GetShortName(pieceName));

                Piece p = new Piece(pieceString);
                VerifyPieceProperties(p, pieceName, null);
            }
        }

        [TestMethod]
        public void Piece_NewInPlayPieceStringTest()
        {
            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                string pieceString = string.Format("{0}[{1}]", EnumUtils.GetShortName(pieceName), Position.Origin);

                Piece p = new Piece(pieceString);
                VerifyPieceProperties(p, pieceName, Position.Origin);
            }
        }

        [TestMethod]
        public void Piece_NullOrWhiteSpaceNewPieceStringTest()
        {
            string[] pieceStrings = TestUtils.NullOrWhiteSpaceStrings;

            for (int i = 0; i < pieceStrings.Length; i++)
            {
                TestUtils.AssertExceptionThrown<ArgumentNullException>(() =>
                {
                    Piece p = new Piece(pieceStrings[i]);
                });
            }
        }

        [TestMethod]
        public void Piece_InvalidNewPieceStringTest()
        {
            string[] pieceStrings = new string[]
            {
                "test",
                "test[0,0,0]",
                "WQ[test]",
            };

            for (int i = 0; i < pieceStrings.Length; i++)
            {
                TestUtils.AssertExceptionThrown<ArgumentException>(() =>
                {
                    Piece p = new Piece(pieceStrings[i]);
                });
            }
        }

        [TestMethod]
        public void Piece_ToStringInHandTest()
        {
            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                string pieceString = string.Format("{0}[]", EnumUtils.GetShortName(pieceName));

                Piece p = new Piece(pieceString);
                Assert.IsNotNull(p);

                Assert.AreEqual(pieceString, p.ToString());
            }
        }

        [TestMethod]
        public void Piece_ToStringInPlayTest()
        {
            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                string pieceString = string.Format("{0}[{1}]", EnumUtils.GetShortName(pieceName), Position.Origin);

                Piece p = new Piece(pieceString);
                Assert.IsNotNull(p);

                Assert.AreEqual(pieceString, p.ToString());
            }
        }

        private void VerifyPieceProperties(Piece actualPiece, PieceName expectedPieceName, Position expectedPosition)
        {
            Assert.IsNotNull(actualPiece);

            Assert.AreEqual(expectedPieceName, actualPiece.PieceName);
            Assert.AreEqual(EnumUtils.GetColor(expectedPieceName), actualPiece.Color);
            Assert.AreEqual(EnumUtils.GetBugType(expectedPieceName), actualPiece.BugType);

            if (null == expectedPosition)
            {
                Assert.IsNull(actualPiece.Position);

                Assert.IsTrue(actualPiece.InHand);
                Assert.IsFalse(actualPiece.InPlay);
            }
            else
            {
                Assert.AreEqual(expectedPosition, actualPiece.Position);

                Assert.IsFalse(actualPiece.InHand);
                Assert.IsTrue(actualPiece.InPlay);
            }
        }
    }
}
