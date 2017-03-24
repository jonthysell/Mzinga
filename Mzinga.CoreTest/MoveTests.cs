// 
// MoveTests.cs
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
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Core;

namespace Mzinga.CoreTest
{
    [TestClass]
    public class MoveTests
    {
        [TestMethod]
        public void Move_NewTest()
        {
            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                Move m = new Move(pieceName, Position.Origin);
                VerifyMoveProperties(m, pieceName, Position.Origin);
            }
        }

        [TestMethod]
        public void Move_NewPassTest()
        {
            Move pass = new Move(PieceName.INVALID, null);
            VerifyMoveProperties(pass, PieceName.INVALID, null);

            Assert.AreEqual(Move.Pass, pass);
        }

        [TestMethod]
        public void Move_PassTest()
        {
            Move pass = Move.Pass;
            VerifyMoveProperties(pass, PieceName.INVALID, null);

            Assert.AreEqual(Move.Pass, pass);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Move_InvalidPieceNameNewTest()
        {
            Move m = new Move(PieceName.INVALID, Position.Origin);
        }

        [TestMethod]
        public void Move_NullPositionNewTest()
        {
            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                TestUtils.AssertExceptionThrown<ArgumentNullException>(() =>
                {
                    Move m = new Move(pieceName, null);
                });
            }
        }

        [TestMethod]
        public void Move_NewMoveStringTest()
        {
            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                string moveString = string.Format("{0}[{1}]", EnumUtils.GetShortName(pieceName), Position.Origin);
                Move m = new Move(moveString);
                VerifyMoveProperties(m, pieceName, Position.Origin);
            }
        }

        [TestMethod]
        public void Move_NewPassMoveStringTest()
        {
            Move pass = new Move(Move.PassString);
            VerifyMoveProperties(pass, PieceName.INVALID, null);

            Assert.AreEqual(Move.Pass, pass);
        }

        [TestMethod]
        public void Move_NullOrWhiteSpaceNewMoveStringTest()
        {
            string[] moveStrings = TestUtils.NullOrWhiteSpaceStrings;

            for (int i = 0; i < moveStrings.Length; i++)
            {
                TestUtils.AssertExceptionThrown<ArgumentNullException>(() =>
                {
                    Move p = new Move(moveStrings[i]);
                });
            }
        }

        [TestMethod]
        public void Move_InvalidNewMoveStringTest()
        {
            string[] moveStrings = new string[]
            {
                "test",
                "test[0,0,0]",
                "WQ[test]",
            };

            for (int i = 0; i < moveStrings.Length; i++)
            {
                TestUtils.AssertExceptionThrown<ArgumentException>(() =>
                {
                    Move m = new Move(moveStrings[i]);
                });
            }
        }

        [TestMethod]
        public void Move_EqualityTest()
        {
            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                Move m1 = new Move(pieceName, Position.Origin);
                Move m2 = new Move(pieceName, Position.Origin);

                Assert.AreEqual(m1, m2);
                Assert.AreEqual(m2, m1);

                Assert.IsTrue(m1.Equals(m2));
                Assert.IsTrue(m2.Equals(m1));

                Assert.IsTrue(m1 == m2);
                Assert.IsTrue(m2 == m1);

                Assert.IsFalse(m1 != m2);
                Assert.IsFalse(m2 != m1);
            }
        }

        [TestMethod]
        public void Move_InequalityTest()
        {
            List<PieceName> pieceNames = new List<PieceName>(EnumUtils.PieceNames);

            for (int i = 1; i < pieceNames.Count; i++)
            {
                Move m1 = new Move(pieceNames[i], Position.Origin);
                Move m2 = new Move(pieceNames[i-1], Position.Origin);

                Assert.AreNotEqual(m1, m2);
                Assert.AreNotEqual(m2, m1);

                Assert.IsFalse(m1.Equals(m2));
                Assert.IsFalse(m2.Equals(m1));

                Assert.IsFalse(m1 == m2);
                Assert.IsFalse(m2 == m1);

                Assert.IsTrue(m1 != m2);
                Assert.IsTrue(m2 != m1);
            }
        }

        [TestMethod]
        public void Move_NullEqualityTest()
        {
            Move m = null;

            Assert.AreEqual(m, null);
            Assert.AreEqual(null, m);

            Assert.IsTrue(m == null);
            Assert.IsTrue(null == m);

            Assert.IsFalse(m != null);
            Assert.IsFalse(null != m);
        }

        [TestMethod]
        public void Move_NullInequalityTest()
        {
            Move m = Move.Pass;

            Assert.AreNotEqual(m, null);
            Assert.AreNotEqual(null, m);

            Assert.IsFalse(m.Equals(null));

            Assert.IsFalse(m == null);
            Assert.IsFalse(null == m);

            Assert.IsTrue(m != null);
            Assert.IsTrue(null != m);
        }

        [TestMethod]
        public void Move_CompareToEqualsTest()
        {
            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                Move m1 = new Move(pieceName, Position.Origin);
                Move m2 = new Move(pieceName, Position.Origin);

                TestUtils.AssertCompareToEqualTo<Move>(m1, m2);
                TestUtils.AssertCompareToEqualTo<Move>(m2, m1);
            }
        }

        [TestMethod]
        public void Move_CompareToNotEqualsTest()
        {
            Move[] sortedMoves = new Move[]
            {
                Move.Pass,
                new Move(PieceName.WhiteQueenBee, new Position(-1, 0, 1, 0)),
                new Move(PieceName.WhiteQueenBee, new Position(-1, 1, 0, 0)),
                new Move(PieceName.WhiteQueenBee, new Position(0, 0, 0, 0)),
                new Move(PieceName.WhiteQueenBee, new Position(1, -1, 0, 0)),
                new Move(PieceName.WhiteQueenBee, new Position(1, 0, -1, 0)),
                new Move(PieceName.WhiteSoldierAnt3, new Position(-1, 0, 1, 0)),
                new Move(PieceName.WhiteSoldierAnt3, new Position(-1, 1, 0, 0)),
                new Move(PieceName.WhiteSoldierAnt3, new Position(0, 0, 0, 0)),
                new Move(PieceName.WhiteSoldierAnt3, new Position(1, -1, 0, 0)),
                new Move(PieceName.WhiteSoldierAnt3, new Position(1, 0, -1, 0)),
                new Move(PieceName.BlackQueenBee, new Position(-1, 0, 1, 0)),
                new Move(PieceName.BlackQueenBee, new Position(-1, 1, 0, 0)),
                new Move(PieceName.BlackQueenBee, new Position(0, 0, 0, 0)),
                new Move(PieceName.BlackQueenBee, new Position(1, -1, 0, 0)),
                new Move(PieceName.BlackQueenBee, new Position(1, 0, -1, 0)),
                new Move(PieceName.BlackSoldierAnt3, new Position(-1, 0, 1, 0)),
                new Move(PieceName.BlackSoldierAnt3, new Position(-1, 1, 0, 0)),
                new Move(PieceName.BlackSoldierAnt3, new Position(0, 0, 0, 0)),
                new Move(PieceName.BlackSoldierAnt3, new Position(1, -1, 0, 0)),
                new Move(PieceName.BlackSoldierAnt3, new Position(1, 0, -1, 0)),
            };

            for (int i = 1; i < sortedMoves.Length; i++)
            {
                TestUtils.AssertCompareToLessThan<Move>(sortedMoves[i - 1], sortedMoves[i]);
                TestUtils.AssertCompareToGreaterThan<Move>(sortedMoves[i], sortedMoves[i - 1]);
            }
        }

        [TestMethod]
        public void Move_ToStringTest()
        {
            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                string moveString = string.Format("{0}[{1}]", EnumUtils.GetShortName(pieceName), Position.Origin);

                Move m = new Move(moveString);
                Assert.IsNotNull(m);

                Assert.AreEqual(moveString, m.ToString());
            }
        }

        [TestMethod]
        public void Move_ToStringPassTest()
        {
            string moveString = Move.PassString;

            Move pass = new Move(moveString);
            Assert.IsNotNull(pass);

            Assert.AreEqual(moveString, pass.ToString());
        }

        [TestMethod]
        public void Move_PassToStringTest()
        {
            Move pass = Move.Pass;
            Assert.IsNotNull(pass);

            Assert.AreEqual(Move.PassString, pass.ToString());
        }

        private void VerifyMoveProperties(Move actualMove, PieceName expectedPieceName, Position expectedPosition)
        {
            Assert.IsNotNull(actualMove);

            Assert.AreEqual(expectedPieceName, actualMove.PieceName);

            if (expectedPieceName != PieceName.INVALID)
            {
                Assert.AreEqual(EnumUtils.GetColor(expectedPieceName), actualMove.Color);
                Assert.AreEqual(EnumUtils.GetBugType(expectedPieceName), actualMove.BugType);
            }

            Assert.AreEqual(expectedPosition, actualMove.Position);
        }
    }
}
