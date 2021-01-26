// 
// MoveSetTests.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017, 2019 Jon Thysell <http://jonthysell.com>
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

namespace Mzinga.Test
{
    [TestClass]
    public class MoveSetTests
    {
        [TestMethod]
        public void MoveSet_NewTest()
        {
            MoveSet ms = new MoveSet();
            Assert.IsNotNull(ms);
            Assert.AreEqual(0, ms.Count);
        }

        [TestMethod]
        public void MoveSet_NewMoveSetStringTest()
        {
            MoveSet ms = new MoveSet(_validMovesSortedString);
            Assert.IsNotNull(ms);

            Assert.AreEqual(_validMovesSorted.Length, ms.Count);
            foreach (Move move in _validMovesSorted)
            {
                Assert.IsTrue(ms.Contains(move));
            }
        }

        [TestMethod]
        public void MoveSet_NullOrWhiteSpaceNewMoveSetStringTest()
        {
            string[] moveSetStrings = TestUtils.NullOrWhiteSpaceStrings;

            for (int i = 0; i < moveSetStrings.Length; i++)
            {
                TestUtils.AssertExceptionThrown<ArgumentNullException>(() =>
                {
                    MoveSet ms = new MoveSet(moveSetStrings[i]);
                });
            }
        }

        [TestMethod]
        public void MoveSet_AddSingleTest()
        {
            MoveSet ms = new MoveSet();
            Assert.IsNotNull(ms);

            Assert.IsTrue(ms.Add(Move.Pass));
            Assert.AreEqual(1, ms.Count);
            Assert.IsTrue(ms.Contains(Move.Pass));
        }

        [TestMethod]
        public void MoveSet_AddMultipleTest()
        {
            Move[] movesToAdd = _validMovesSorted;

            MoveSet ms = new MoveSet();
            Assert.IsNotNull(ms);

            int count = 0;
            Assert.AreEqual(count, ms.Count);

            foreach (Move move in movesToAdd)
            {
                Assert.IsTrue(ms.Add(move));
                count++;
                Assert.AreEqual(count, ms.Count);
                Assert.IsTrue(ms.Contains(move));
            }   
        }

        [TestMethod]
        public void MoveSet_AddSingleDuplicateTest()
        {
            MoveSet ms = new MoveSet();
            Assert.IsNotNull(ms);

            Assert.IsTrue(ms.Add(Move.Pass));
            Assert.AreEqual(1, ms.Count);
            Assert.IsTrue(ms.Contains(Move.Pass));

            Assert.IsFalse(ms.Add(Move.Pass));
            Assert.AreEqual(1, ms.Count);
            Assert.IsTrue(ms.Contains(Move.Pass));
        }

        [TestMethod]
        public void MoveSet_AddMultipleDuplicateTest()
        {
            Move[] movesToAdd = _validMovesSorted;

            MoveSet ms = new MoveSet();
            Assert.IsNotNull(ms);

            int count = 0;
            Assert.AreEqual(count, ms.Count);

            foreach (Move move in movesToAdd)
            {
                Assert.IsTrue(ms.Add(move));
                count++;
                Assert.AreEqual(count, ms.Count);
                Assert.IsTrue(ms.Contains(move));
            }

            foreach (Move move in movesToAdd)
            {
                Assert.IsFalse(ms.Add(move));
                Assert.AreEqual(count, ms.Count);
                Assert.IsTrue(ms.Contains(move));
            }
        }

        [TestMethod]
        public void MoveSet_AddSingleByEnumerableTest()
        {
            Move[] movesToAdd = new Move[]
            {
                Move.Pass
            };

            MoveSet ms = new MoveSet();
            Assert.IsNotNull(ms);

            Assert.AreEqual(0, ms.Count);
            ms.Add(movesToAdd);
            Assert.AreEqual(movesToAdd.Length, ms.Count);

            foreach (Move move in movesToAdd)
            {
                Assert.IsTrue(ms.Contains(move));
            }
        }

        [TestMethod]
        public void MoveSet_AddMultipleByEnumerableTest()
        {
            Move[] movesToAdd = _validMovesSorted;

            MoveSet ms = new MoveSet();
            Assert.IsNotNull(ms);

            Assert.AreEqual(0, ms.Count);
            ms.Add(movesToAdd);
            Assert.AreEqual(movesToAdd.Length, ms.Count);

            foreach (Move move in movesToAdd)
            {
                Assert.IsTrue(ms.Contains(move));
            }
        }

        [TestMethod]
        public void MoveSet_AddSingleDuplicateByEnumerableTest()
        {
            Move[] movesToAdd = new Move[]
            {
                Move.Pass
            };

            MoveSet ms = new MoveSet();
            Assert.IsNotNull(ms);

            Assert.AreEqual(0, ms.Count);
            ms.Add(movesToAdd);
            Assert.AreEqual(movesToAdd.Length, ms.Count);

            foreach (Move move in movesToAdd)
            {
                Assert.IsTrue(ms.Contains(move));
            }

            ms.Add(movesToAdd);
            Assert.AreEqual(movesToAdd.Length, ms.Count);

            foreach (Move move in movesToAdd)
            {
                Assert.IsTrue(ms.Contains(move));
            }
        }

        [TestMethod]
        public void MoveSet_AddMultipleDuplicateByEnumerableTest()
        {
            Move[] movesToAdd = _validMovesSorted;

            MoveSet ms = new MoveSet();
            Assert.IsNotNull(ms);

            Assert.AreEqual(0, ms.Count);
            ms.Add(movesToAdd);
            Assert.AreEqual(movesToAdd.Length, ms.Count);

            foreach (Move move in movesToAdd)
            {
                Assert.IsTrue(ms.Contains(move));
            }

            ms.Add(movesToAdd);
            Assert.AreEqual(movesToAdd.Length, ms.Count);

            foreach (Move move in movesToAdd)
            {
                Assert.IsTrue(ms.Contains(move));
            }
        }

        [TestMethod]
        public void MoveSet_RemoveSingleTest()
        {
            Move validMove = Move.Pass;

            MoveSet ms = new MoveSet();
            Assert.IsNotNull(ms);

            Assert.AreEqual(0, ms.Count);
            Assert.IsTrue(ms.Add(validMove));
            Assert.AreEqual(1, ms.Count);
            Assert.IsTrue(ms.Contains(validMove));

            Assert.IsTrue(ms.Remove(validMove));
            Assert.AreEqual(0, ms.Count);
            Assert.IsFalse(ms.Contains(validMove));
        }

        [TestMethod]
        public void MoveSet_RemoveMultipleTest()
        {
            Move[] validMoves = _validMovesSorted;

            MoveSet ms = new MoveSet();
            Assert.IsNotNull(ms);

            int count = 0;
            Assert.AreEqual(count, ms.Count);

            foreach (Move move in validMoves)
            {
                Assert.IsTrue(ms.Add(move));
                count++;
                Assert.AreEqual(count, ms.Count);
                Assert.IsTrue(ms.Contains(move));
            }

            foreach (Move move in validMoves)
            {
                Assert.IsTrue(ms.Remove(move));
                count--;
                Assert.AreEqual(count, ms.Count);
                Assert.IsFalse(ms.Contains(move));
            }
        }

        [TestMethod]
        public void MoveSet_RemoveSingleByEnumerableTest()
        {
            Move validMove = Move.Pass;

            MoveSet ms = new MoveSet();
            Assert.IsNotNull(ms);

            Assert.AreEqual(0, ms.Count);
            Assert.IsTrue(ms.Add(validMove));
            Assert.AreEqual(1, ms.Count);
            Assert.IsTrue(ms.Contains(validMove));

            Assert.IsTrue(ms.Remove(validMove));
            Assert.AreEqual(0, ms.Count);
            Assert.IsFalse(ms.Contains(validMove));
        }

        [TestMethod]
        public void MoveSet_RemoveMultipleByEnumerableTest()
        {
            Move[] validMoves = _validMovesSorted;

            MoveSet ms = new MoveSet();
            Assert.IsNotNull(ms);

            Assert.AreEqual(0, ms.Count);
            ms.Add(validMoves);
            Assert.AreEqual(validMoves.Length, ms.Count);

            foreach (Move move in validMoves)
            {
                Assert.IsTrue(ms.Contains(move));
            }

            ms.Remove(validMoves);
            Assert.AreEqual(0, ms.Count);

            foreach (Move move in validMoves)
            {
                Assert.IsFalse(ms.Contains(move));
            }
        }

        [TestMethod]
        public void MoveSet_ToStringTest()
        {
            MoveSet ms = new MoveSet();
            Assert.IsNotNull(ms);

            ms.Add(_validMovesSorted);
            Assert.AreEqual(_validMovesSortedString, ms.ToString());
        }

        [TestMethod]
        public void MoveSet_EmptyToStringTest()
        {
            MoveSet ms = new MoveSet();
            Assert.IsNotNull(ms);

            Assert.AreEqual("", ms.ToString());
        }

        private static Move[] _validMovesSorted = new Move[]
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

        private static string _validMovesSortedString = @"PASS;WQ[-1,0,1];WQ[-1,1,0];WQ[0,0,0];WQ[1,-1,0];WQ[1,0,-1];WA3[-1,0,1];WA3[-1,1,0];WA3[0,0,0];WA3[1,-1,0];WA3[1,0,-1];BQ[-1,0,1];BQ[-1,1,0];BQ[0,0,0];BQ[1,-1,0];BQ[1,0,-1];BA3[-1,0,1];BA3[-1,1,0];BA3[0,0,0];BA3[1,-1,0];BA3[1,0,-1]";
    }
}
