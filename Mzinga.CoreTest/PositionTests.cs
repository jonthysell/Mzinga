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
            foreach (int[] coordinate in _validXYZCoordinates)
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
            foreach (int[] coordinate in _validQRCoordinates)
            {
                Position p = new Position(coordinate[0], coordinate[1], coordinate[2]);
                Assert.IsNotNull(p);

                Assert.AreEqual(coordinate[0], p.Q);
                Assert.AreEqual(coordinate[1], p.R);
                Assert.AreEqual(coordinate[2], p.Stack);

                Assert.AreEqual(coordinate[0], p.X);
                Assert.AreEqual(0 - coordinate[0] - coordinate[1], p.Y);
                Assert.AreEqual(coordinate[1], p.Z);
            }
        }

        [TestMethod]
        public void Position_InvalidNewXYZTest()
        {
            foreach (int[] coordinate in _invalidXYZCoordinates)
            {
                TestUtils.AssertExceptionThrown<ArgumentOutOfRangeException>(() =>
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
                TestUtils.AssertExceptionThrown<ArgumentOutOfRangeException>(() =>
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

            TestUtils.AssertHaveEqualChildren<Position>(expectedNeighbors, actualNeighbors);
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
            foreach (int[] coordinate in _validXYZCoordinates)
            {
                Position p = new Position(coordinate[0], coordinate[1], coordinate[2], coordinate[3]);
                Assert.IsNotNull(p);

                Position clone = p.Clone();

                AssertPositionsAreEqual(p, clone);
                Assert.AreNotSame(p, clone);
            }
        }

        [TestMethod]
        public void Position_ParseXYZTest()
        {
            int[][] coordinates = _validXYZCoordinates;
            string[] positionStrings = GetPositionStrings(coordinates);

            for (int i = 0; i < positionStrings.Length; i++)
            {
                Position position = new Position(coordinates[i][0], coordinates[i][1], coordinates[i][2], coordinates[i][3]);
                Assert.IsNotNull(position);

                Position parsedPosition = Position.Parse(positionStrings[i]);
                Assert.IsNotNull(parsedPosition);

                AssertPositionsAreEqual(position, parsedPosition);
            }
        }

        [TestMethod]
        public void Position_ParseQRTest()
        {
            int[][] coordinates = _validQRCoordinates;
            string[] positionStrings = GetPositionStrings(coordinates, true);

            for (int i = 0; i < positionStrings.Length; i++)
            {
                Position position = new Position(coordinates[i][0], coordinates[i][1], 0);
                Assert.IsNotNull(position);

                Position parsedPosition = Position.Parse(positionStrings[i]);
                Assert.IsNotNull(parsedPosition);

                AssertPositionsAreEqual(position, parsedPosition);
            }
        }

        [TestMethod]
        public void Position_NullOrWhiteSpaceParseTest()
        {
            string[] positionStrings = TestUtils.NullOrWhiteSpaceStrings;

            for (int i = 0; i < positionStrings.Length; i++)
            {
                Position parsedPosition = Position.Parse(positionStrings[i]);
                Assert.IsNull(parsedPosition);
            }
        }

        [TestMethod]
        public void Position_TryParseXYZTest()
        {
            int[][] coordinates = _validXYZCoordinates;
            string[] positionStrings = GetPositionStrings(coordinates);

            for (int i = 0; i < positionStrings.Length; i++)
            {
                Position position = new Position(coordinates[i][0], coordinates[i][1], coordinates[i][2], coordinates[i][3]);
                Assert.IsNotNull(position);

                Position parsedPosition = null;
                Assert.IsTrue(Position.TryParse(positionStrings[i], out parsedPosition));
                Assert.IsNotNull(parsedPosition);

                AssertPositionsAreEqual(position, parsedPosition);
            }
        }

        [TestMethod]
        public void Position_TryParseQRTest()
        {
            int[][] coordinates = _validQRCoordinates;
            string[] positionStrings = GetPositionStrings(coordinates, true);

            for (int i = 0; i < positionStrings.Length; i++)
            {
                Position position = new Position(coordinates[i][0], coordinates[i][1], 0);
                Assert.IsNotNull(position);

                Position parsedPosition = null;
                Assert.IsTrue(Position.TryParse(positionStrings[i], out parsedPosition));
                Assert.IsNotNull(parsedPosition);

                AssertPositionsAreEqual(position, parsedPosition);
            }
        }

        [TestMethod]
        public void Position_NullOrWhiteSpaceTryParseTest()
        {
            string[] positionStrings = TestUtils.NullOrWhiteSpaceStrings;

            for (int i = 0; i < positionStrings.Length; i++)
            {
                Position parsedPosition = Position.Origin;
                Assert.IsTrue(Position.TryParse(positionStrings[i], out parsedPosition));
                Assert.IsNull(parsedPosition);
            }
        }

        [TestMethod]
        public void Position_InvalidParseTest()
        {
            string[] positionStrings = _invalidPositionStrings;

            for (int i = 0; i < positionStrings.Length; i++)
            {
                TestUtils.AssertExceptionThrown<ArgumentOutOfRangeException>(() =>
                {
                    Position parsedPosition = Position.Parse(positionStrings[i]);
                });
            }
        }

        [TestMethod]
        public void Position_InvalidXYZParseTest()
        {
            int[][] coordinates = _invalidXYZCoordinates;
            string[] positionStrings = GetPositionStrings(coordinates);

            for (int i = 0; i < coordinates.Length; i++)
            {
                TestUtils.AssertExceptionThrown<ArgumentOutOfRangeException>(() =>
                {
                    Position parsedPosition = Position.Parse(positionStrings[i]);
                });
            }
        }

        [TestMethod]
        public void Position_InvalidQRParseTest()
        {
            int[][] coordinates = _invalidQRCoordinates;
            string[] positionStrings = GetPositionStrings(coordinates);

            for (int i = 0; i < coordinates.Length; i++)
            {
                TestUtils.AssertExceptionThrown<ArgumentOutOfRangeException>(() =>
                {
                    Position parsedPosition = Position.Parse(positionStrings[i]);
                });
            }
        }

        [TestMethod]
        public void Position_InvalidTryParseTest()
        {
            string[] positionStrings = _invalidPositionStrings;

            for (int i = 0; i < positionStrings.Length; i++)
            {
                Position parsedPosition = Position.Origin;
                Assert.IsFalse(Position.TryParse(positionStrings[i], out parsedPosition));
                Assert.IsNull(parsedPosition);
            }
        }

        [TestMethod]
        public void Position_InvalidXYZTryParseTest()
        {
            int[][] coordinates = _invalidXYZCoordinates;
            string[] positionStrings = GetPositionStrings(coordinates);

            for (int i = 0; i < coordinates.Length; i++)
            {
                Position parsedPosition = Position.Origin;
                Assert.IsFalse(Position.TryParse(positionStrings[i], out parsedPosition));
                Assert.IsNull(parsedPosition);
            }
        }

        [TestMethod]
        public void Position_InvalidQRTryParseTest()
        {
            int[][] coordinates = _invalidQRCoordinates;
            string[] positionStrings = GetPositionStrings(coordinates);

            for (int i = 0; i < coordinates.Length; i++)
            {
                Position parsedPosition = Position.Origin;
                Assert.IsFalse(Position.TryParse(positionStrings[i], out parsedPosition));
                Assert.IsNull(parsedPosition);
            }
        }

        [TestMethod]
        public void Position_EqualityTest()
        {
            foreach (int[] coordinate in _validXYZCoordinates)
            {
                Position p1 = new Position(coordinate[0], coordinate[1], coordinate[2], coordinate[3]);
                Position p2 = new Position(coordinate[0], coordinate[1], coordinate[2], coordinate[3]);

                Assert.AreEqual(p1, p2);
                Assert.AreEqual(p2, p1);

                Assert.IsTrue(p1.Equals(p2));
                Assert.IsTrue(p2.Equals(p1));

                Assert.IsTrue(p1 == p2);
                Assert.IsTrue(p2 == p1);

                Assert.IsFalse(p1 != p2);
                Assert.IsFalse(p2 != p1);
            }
        }

        [TestMethod]
        public void Position_InequalityTest()
        {
            int[][] coordinates = _validXYZCoordinates;

            for (int i = 1; i < coordinates.Length; i++)
            {
                Position p1 = new Position(coordinates[i - 1][0], coordinates[i - 1][1], coordinates[i - 1][2], coordinates[i - 1][3]);
                Position p2 = new Position(coordinates[i][0], coordinates[i][1], coordinates[i][2], coordinates[i][3]);

                Assert.AreNotEqual(p1, p2);
                Assert.AreNotEqual(p2, p1);

                Assert.IsFalse(p1.Equals(p2));
                Assert.IsFalse(p2.Equals(p1));

                Assert.IsFalse(p1 == p2);
                Assert.IsFalse(p2 == p1);

                Assert.IsTrue(p1 != p2);
                Assert.IsTrue(p2 != p1);
            }
        }

        [TestMethod]
        public void Position_NullEqualityTest()
        {
            Position p = null;

            Assert.AreEqual(p, null);
            Assert.AreEqual(null, p);

            Assert.IsTrue(p == null);
            Assert.IsTrue(null == p);

            Assert.IsFalse(p != null);
            Assert.IsFalse(null != p);
        }

        [TestMethod]
        public void Position_NullInequalityTest()
        {
            Position p = Position.Origin;

            Assert.AreNotEqual(p, null);
            Assert.AreNotEqual(null, p);

            Assert.IsFalse(p.Equals(null));

            Assert.IsFalse(p == null);
            Assert.IsFalse(null == p);

            Assert.IsTrue(p != null);
            Assert.IsTrue(null != p);
        }

        [TestMethod]
        public void Position_CompareToEqualsTest()
        {
            foreach (int[] coordinate in _validXYZCoordinates)
            {
                Position p1 = new Position(coordinate[0], coordinate[1], coordinate[2], coordinate[3]);
                Position p2 = new Position(coordinate[0], coordinate[1], coordinate[2], coordinate[3]);

                TestUtils.AssertCompareToEqualTo<Position>(p1, p2);
                TestUtils.AssertCompareToEqualTo<Position>(p2, p1);
            }
        }

        [TestMethod]
        public void Position_CompareToNotEqualsTest()
        {
            Position[] sortedPositions = new Position[]
            {
                new Position(-1, 0, 1, 0),
                new Position(-1, 0, 1, 1),
                new Position(-1, 1, 0, 0),
                new Position(-1, 1, 0, 1),
                new Position(0, 0, 0, 0),
                new Position(0, 0, 0, 1),
                new Position(1, -1, 0, 0),
                new Position(1, -1, 0, 1),
                new Position(1, 0, -1, 0),
                new Position(1, 0, -1, 1),
            };

            for (int i = 1; i < sortedPositions.Length; i++)
            {
                TestUtils.AssertCompareToLessThan<Position>(sortedPositions[i - 1], sortedPositions[i]);
                TestUtils.AssertCompareToGreaterThan<Position>(sortedPositions[i], sortedPositions[i-1]);
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

        private string[] GetPositionStrings(int[][] coordinates, bool skipStack = false)
        {
            string[] positionStrings = new string[coordinates.Length];

            for (int i = 0; i < coordinates.Length; i++)
            {
                string positionString = "";

                int maxCoordinate = coordinates[i].Length;

                if (skipStack)
                {
                    maxCoordinate--;
                }

                for (int j = 0; j < maxCoordinate; j++)
                {
                    positionString += coordinates[i][j].ToString() + Position.PositionStringSeparator.ToString();
                }

                positionStrings[i] = positionString.TrimEnd(Position.PositionStringSeparator);
            }

            return positionStrings;
        }

        private void AssertPositionsAreEqual(Position expected, Position actual)
        {
            Assert.IsNotNull(expected);
            Assert.IsNotNull(actual);

            Assert.AreEqual(expected, actual);

            Assert.AreEqual(expected.X, actual.X);
            Assert.AreEqual(expected.Y, actual.Y);
            Assert.AreEqual(expected.Z, actual.Z);
            Assert.AreEqual(expected.Stack, actual.Stack);

            Assert.AreEqual(expected.Q, actual.Q);
            Assert.AreEqual(expected.R, actual.R);
        }

        private int[][] _validXYZCoordinates = new int[][]
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

        private int[][] _validQRCoordinates = new int[][]
        {
            new int[] { 0, 0, 0},
            new int[] { 1, -1, 0},
            new int[] { -1, 1, 0},
            new int[] { 1, 0, 0},
            new int[] { -1, 0, 0},
            new int[] { 0, -1, 0},
            new int[] { 0, 1, 0},
            new int[] { 0, 0, 1},
            new int[] { 1, -1, 1},
            new int[] { -1, 1, 1},
            new int[] { 1, 0, 1},
            new int[] { -1, 0, 1},
            new int[] { 0, -1, 1},
            new int[] { 0, 1, 1},
            new int[] { 0, 0, Int32.MaxValue},
            new int[] { Int32.MaxValue, -Int32.MaxValue, Int32.MaxValue},
            new int[] { -Int32.MaxValue, Int32.MaxValue, Int32.MaxValue},
            new int[] { Int32.MaxValue, 0, Int32.MaxValue},
            new int[] { -Int32.MaxValue, 0, Int32.MaxValue},
            new int[] { 0, -Int32.MaxValue, Int32.MaxValue},
            new int[] { 0, Int32.MaxValue, Int32.MaxValue},
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

        private string[] _invalidPositionStrings = new string[]
        {
            "0",
            "a",
            "a,0",
            "0,a",
            "a,a",
            "a,0,0",
            "0,a,0",
            "0,0,a",
            "a,a,0",
            "0,a,a",
            "a,a,a",
            "a,0,0,0",
            "0,a,0,0",
            "0,0,a,0",
            "0,0,0,a",
            "a,a,0,0",
            "a,0,a,0",
            "a,0,0,a",
            "a,a,a,0",
            "a,a,0,a",
            "a,a,a,a",
            "0,a,a,0",
            "0,a,0,a",
            "0,0,a,a",
            "0,a,a,a",
        };
    }
}
