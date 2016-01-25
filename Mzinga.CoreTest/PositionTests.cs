// 
// PositionTests.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016 Jon Thysell <http://jonthysell.com>
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
    public class PositionTests
    {
        [TestMethod]
        public void Position_NewXYZTest()
        {
            foreach (int[] coordinate in _validCoordinates)
            {
                Position p = new Position(coordinate[0], coordinate[1], coordinate[2], coordinate[3]);
                Assert.IsNotNull(p);

                Assert.AreEqual(coordinate[0], p.X);
                Assert.AreEqual(coordinate[1], p.Y);
                Assert.AreEqual(coordinate[2], p.Z);
                Assert.AreEqual(coordinate[3], p.Stack);

                Assert.AreEqual(coordinate[0], p.Q);
                Assert.AreEqual(coordinate[2], p.R);
            }
        }

        [TestMethod]
        public void Position_NewQRTest()
        {
            foreach (int[] coordinate in _validCoordinates)
            {
                Position p = new Position(coordinate[0], coordinate[2], coordinate[3]);
                Assert.IsNotNull(p);

                Assert.AreEqual(coordinate[0], p.Q);
                Assert.AreEqual(coordinate[2], p.R);
                Assert.AreEqual(coordinate[3], p.Stack);

                Assert.AreEqual(coordinate[0], p.X);
                Assert.AreEqual(coordinate[1], p.Y);
                Assert.AreEqual(0 - coordinate[0] - coordinate[2], p.Y);
                Assert.AreEqual(coordinate[2], p.Z);
            }
        }

        [TestMethod]
        public void Position_InvalidNewXYZTest()
        {
            foreach (int[] coordinate in _invalidXYZCoordinates)
            {
                TestUtils.ExceptionThrown<ArgumentOutOfRangeException>(() =>
                {
                    Position p = new Position(coordinate[0], coordinate[1], coordinate[2], coordinate[3]);
                });
            }
        }

        [TestMethod]
        public void Position_InvalidNewQRTest()
        {
            foreach (int[] coordinate in _invalidQRCoordinates)
            {
                TestUtils.ExceptionThrown<ArgumentOutOfRangeException>(() =>
                {
                    Position p = new Position(coordinate[0], coordinate[1], coordinate[2]);
                });
            }
        }

        [TestMethod]
        public void Position_OriginTest()
        {
            Assert.IsNotNull(Position.Origin);

            Assert.AreEqual(0, Position.Origin.X);
            Assert.AreEqual(0, Position.Origin.Y);
            Assert.AreEqual(0, Position.Origin.Z);
            Assert.AreEqual(0, Position.Origin.Stack);

            Assert.AreEqual(0, Position.Origin.Q);
            Assert.AreEqual(0, Position.Origin.R);
        }

        [TestMethod]
        public void Position_NeighborsTest()
        {
            Position position = Position.Origin;

            List<Position> expectedNeighbors = new List<Position>(GetOriginNeighbors().Values);

            List<Position> actualNeighbors = new List<Position>(position.Neighbors);

            TestUtils.EqualChildren<Position>(expectedNeighbors, actualNeighbors);
        }

        [TestMethod]
        public void Position_NeighborAtTest()
        {
            Position position = Position.Origin;

            Dictionary<Direction, Position> expectedNeighbors = GetOriginNeighbors();

            foreach (Direction direction in expectedNeighbors.Keys)
            {
                Assert.AreEqual(expectedNeighbors[direction], position.NeighborAt(direction));
            }
        }

        [TestMethod]
        public void Position_IsTouchingTrueTest()
        {
            Position position = Position.Origin;

            List<Position> expectedNeighbors = new List<Position>(GetOriginNeighbors().Values);

            foreach (Position neighbor in expectedNeighbors)
            {
                Assert.IsTrue(position.IsTouching(neighbor));
            }
        }

        [TestMethod]
        public void Position_IsTouchingFalseTest()
        {
            Position position = Position.Origin;

            Position[] nonNeighbors = new Position[]
            {
                new Position(0, 2, -2, 0),
                new Position(2, 0, -2, 0),
                new Position(2, -2, 0, 0),
                new Position(0, -2, 2, 0),
                new Position(-2, 0, 2, 0),
                new Position(-2, 2, 0, 0),
            };

            foreach (Position nonNeighbor in nonNeighbors)
            {
                Assert.IsFalse(position.IsTouching(nonNeighbor));
            }
        }

        [TestMethod]
        public void Position_GetShiftedTest()
        {
            Position position = Position.Origin;

            int[] deltas = new int[]
            {
                -1,
                0,
                1
            };

            foreach (int delta in deltas)
            {
                Assert.AreEqual(delta, position.GetShifted(delta, 0, -delta, 0).X);
                Assert.AreEqual(delta, position.GetShifted(delta, -delta, 0, 0).X);
                Assert.AreEqual(delta, position.GetShifted(-delta, delta, 0, 0).Y);
                Assert.AreEqual(delta, position.GetShifted(0, delta, -delta, 0).Y);
                Assert.AreEqual(delta, position.GetShifted(-delta, 0, delta, 0).Z);
                Assert.AreEqual(delta, position.GetShifted(0, -delta, delta, 0).Z);
                
                if (delta >= 0)
                {
                    Assert.AreEqual(delta, position.GetShifted(0, 0, 0, delta).Stack);
                }

            }
        }

        [TestMethod]
        public void Position_CloneTest()
        {
            foreach (int[] coordinate in _validCoordinates)
            {
                Position p = new Position(coordinate[0], coordinate[1], coordinate[2], coordinate[3]);
                Assert.IsNotNull(p);

                Position clone = p.Clone();

                Assert.AreEqual(p, clone);
                Assert.AreNotSame(p, clone);
            }
        }

        private Dictionary<Direction, Position> GetOriginNeighbors()
        {
            Dictionary<Direction, Position> originNeighbors = new Dictionary<Direction, Position>();

            originNeighbors.Add(Direction.Up, new Position(0, 1, -1, 0));
            originNeighbors.Add(Direction.UpRight, new Position(1, 0, -1, 0));
            originNeighbors.Add(Direction.DownRight, new Position(1, -1, 0, 0));
            originNeighbors.Add(Direction.Down, new Position(0, -1, 1, 0));
            originNeighbors.Add(Direction.DownLeft, new Position(-1, 0, 1, 0));
            originNeighbors.Add(Direction.UpLeft, new Position(-1, 1, 0, 0));

            return originNeighbors;
        }

        private int[][] _validCoordinates = new int[][]
        {
            new int[] { 0, 0, 0, 0},
            new int[] { 1, 0, -1, 0},
            new int[] { -1, 0, 1, 0},
            new int[] { 1, -1, 0, 0},
            new int[] { -1, 1, 0, 0},
            new int[] { 0, 1, -1, 0},
            new int[] { 0, -1, 1, 0},
            new int[] { 0, 0, 0, 1},
            new int[] { 1, 0, -1, 1},
            new int[] { -1, 0, 1, 1},
            new int[] { 1, -1, 0, 1},
            new int[] { -1, 1, 0, 1},
            new int[] { 0, 1, -1, 1},
            new int[] { 0, -1, 1, 1},
            new int[] { 0, 0, 0, Int32.MaxValue},
            new int[] { Int32.MaxValue, 0, -Int32.MaxValue, Int32.MaxValue},
            new int[] { -Int32.MaxValue, 0, Int32.MaxValue, Int32.MaxValue},
            new int[] { Int32.MaxValue, -Int32.MaxValue, 0, Int32.MaxValue},
            new int[] { -Int32.MaxValue, Int32.MaxValue, 0, Int32.MaxValue},
            new int[] { 0, Int32.MaxValue, -Int32.MaxValue, Int32.MaxValue},
            new int[] { 0, -Int32.MaxValue, Int32.MaxValue, Int32.MaxValue},
        };

        private int[][] _invalidXYZCoordinates = new int[][]
        {
            new int[] { 0, 0, 0, -1},
            new int[] { 0, 0, 0, -Int32.MaxValue},
            new int[] { 1, 0, 0, 0},
            new int[] { 0, 1, 0, 0},
            new int[] { 0, 0, 1, 0},
            new int[] { 1, 1, 0, 0},
            new int[] { 0, 1, 1, 0},
            new int[] { 1, 0, 1, 0},
            new int[] { 1, 1, 1, 0},
            new int[] { -1, 0, 0, 0},
            new int[] { 0, -1, 0, 0},
            new int[] { 0, 0, -1, 0},
            new int[] { -1, -1, 0, 0},
            new int[] { 0, -1, -1, 0},
            new int[] { -1, 0, -1, 0},
            new int[] { -1, -1, -1, 0},
            new int[] { Int32.MaxValue, 0, 0, 0},
            new int[] { 0, Int32.MaxValue, 0, 0},
            new int[] { 0, 0, Int32.MaxValue, 0},
            new int[] { Int32.MaxValue, Int32.MaxValue, 0, 0},
            new int[] { 0, Int32.MaxValue, Int32.MaxValue, 0},
            new int[] { Int32.MaxValue, 0, Int32.MaxValue, 0},
            new int[] { Int32.MaxValue, Int32.MaxValue, Int32.MaxValue, 0},
            new int[] { -Int32.MaxValue, 0, 0, 0},
            new int[] { 0, -Int32.MaxValue, 0, 0},
            new int[] { 0, 0, -Int32.MaxValue, 0},
            new int[] { -Int32.MaxValue, -Int32.MaxValue, 0, 0},
            new int[] { 0, -Int32.MaxValue, -Int32.MaxValue, 0},
            new int[] { -Int32.MaxValue, 0, -Int32.MaxValue, 0},
            new int[] { -Int32.MaxValue, -Int32.MaxValue, -Int32.MaxValue, 0},
        };

        private int[][] _invalidQRCoordinates = new int[][]
        {
            new int[] { 0, 0, -1},
            new int[] { 0, 0, -Int32.MaxValue},
        };
    }
}
