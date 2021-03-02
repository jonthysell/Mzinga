// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Core;

namespace Mzinga.Test
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
            _ = new Piece(PieceName.INVALID);
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

        private static void VerifyPieceProperties(Piece actualPiece, PieceName expectedPieceName, Position expectedPosition)
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
