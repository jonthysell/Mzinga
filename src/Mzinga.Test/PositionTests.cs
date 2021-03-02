// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Core;

namespace Mzinga.Test
{
    [TestClass]
    public class PositionTests
    {
        [TestMethod]
        public void Position_NewXYZTest()
        {
            foreach (int[] coordinate in _validXYZCoordinates)
            {
                Position p = new Position(coordinate[0], coordinate[1], coordinate[2], (uint)coordinate[3]);
                Assert.IsNotNull(p);

                Assert.AreEqual(coordinate[0], p.X);
                Assert.AreEqual(coordinate[1], p.Y);
                Assert.AreEqual(coordinate[2], p.Z);
                Assert.AreEqual((uint)coordinate[3], p.Stack);

                Assert.AreEqual(coordinate[0], p.Q);
                Assert.AreEqual(coordinate[2], p.R);
            }
        }

        [TestMethod]
        public void Position_NewQRTest()
        {
            foreach (int[] coordinate in _validQRCoordinates)
            {
                Position p = new Position(coordinate[0], coordinate[1], (uint)coordinate[2]);
                Assert.IsNotNull(p);

                Assert.AreEqual(coordinate[0], p.Q);
                Assert.AreEqual(coordinate[1], p.R);
                Assert.AreEqual((uint)coordinate[2], p.Stack);

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
                    Position p = new Position(coordinate[0], coordinate[1], coordinate[2], (uint)coordinate[3]);
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
            Assert.AreEqual((uint)0, Position.Origin.Stack);

            Assert.AreEqual(0, Position.Origin.Q);
            Assert.AreEqual(0, Position.Origin.R);
        }

        [TestMethod]
        public void Position_NeighborAtIndexTest()
        {
            Position position = Position.Origin;

            List<Position> expectedNeighbors = new List<Position>(GetOriginNeighbors().Values);

            List<Position> actualNeighbors = new List<Position>();

            for (int direction = 0; direction < EnumUtils.NumDirections; direction++)
            {
                actualNeighbors.Add(position.NeighborAt(direction));
            }

            TestUtils.AssertHaveEqualChildren(expectedNeighbors, actualNeighbors);
        }

        [TestMethod]
        public void Position_NeighborAtDirectionTest()
        {
            Position position = Position.Origin;

            Dictionary<Direction, Position> expectedNeighbors = GetOriginNeighbors();

            foreach (Direction direction in expectedNeighbors.Keys)
            {
                Assert.AreEqual(expectedNeighbors[direction], position.NeighborAt(direction));
            }
        }

        [TestMethod]
        public void Position_GetAboveTest()
        {
            Position position = Position.Origin;

            Position expectedAbove = new Position(0, 0, 0, 1);

            Assert.AreEqual(expectedAbove, position.GetAbove());
        }

        [TestMethod]
        public void Position_GetBelowTest()
        {
            Position position = new Position(0, 0, 0, 1);

            Position expectedBelow = new Position(0, 0, 0, 0);

            Assert.AreEqual(expectedBelow, position.GetBelow());
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
        public void Position_ParseXYZTest()
        {
            int[][] coordinates = _validXYZCoordinates;
            string[] positionStrings = GetPositionStrings(coordinates);

            for (int i = 0; i < positionStrings.Length; i++)
            {
                Position position = new Position(coordinates[i][0], coordinates[i][1], coordinates[i][2], (uint)coordinates[i][3]);
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
                Position position = new Position(coordinates[i][0], coordinates[i][1], coordinates[i][2], (uint)coordinates[i][3]);
                Assert.IsNotNull(position);

                Assert.IsTrue(Position.TryParse(positionStrings[i], out Position parsedPosition));
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

                Assert.IsTrue(Position.TryParse(positionStrings[i], out Position parsedPosition));
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
                _ = Position.Origin;
                Assert.IsTrue(Position.TryParse(positionStrings[i], out Position parsedPosition));
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
        public void Position_InvalidTryParseTest()
        {
            string[] positionStrings = _invalidPositionStrings;

            for (int i = 0; i < positionStrings.Length; i++)
            {
                _ = Position.Origin;
                Assert.IsFalse(Position.TryParse(positionStrings[i], out Position parsedPosition));
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
                _ = Position.Origin;
                Assert.IsFalse(Position.TryParse(positionStrings[i], out Position parsedPosition));
                Assert.IsNull(parsedPosition);
            }
        }

        [TestMethod]
        public void Position_EqualityTest()
        {
            foreach (int[] coordinate in _validXYZCoordinates)
            {
                Position p1 = new Position(coordinate[0], coordinate[1], coordinate[2], (uint)coordinate[3]);
                Position p2 = new Position(coordinate[0], coordinate[1], coordinate[2], (uint)coordinate[3]);

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
                Position p1 = new Position(coordinates[i - 1][0], coordinates[i - 1][1], coordinates[i - 1][2], (uint)coordinates[i - 1][3]);
                Position p2 = new Position(coordinates[i][0], coordinates[i][1], coordinates[i][2], (uint)coordinates[i][3]);

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
        public void Position_ToStringXYZTest()
        {
            foreach (int[] coordinate in _validXYZCoordinates)
            {
                Position p = new Position(coordinate[0], coordinate[1], coordinate[2], (uint)coordinate[3]);
                Assert.IsNotNull(p);

                if (coordinate[3] == 0)
                {
                    Assert.AreEqual(string.Format("{1}{0}{2}{0}{3}", Position.PositionStringSeparator, coordinate[0], coordinate[1], coordinate[2]), p.ToString());
                }
                else
                {
                    Assert.AreEqual(string.Format("{1}{0}{2}{0}{3}{0}{4}", Position.PositionStringSeparator, coordinate[0], coordinate[1], coordinate[2], coordinate[3]), p.ToString());
                }
            }
        }

        [TestMethod]
        public void Position_ToStringQRTest()
        {
            foreach (int[] coordinate in _validQRCoordinates)
            {
                Position p = new Position(coordinate[0], coordinate[1], (uint)coordinate[2]);
                Assert.IsNotNull(p);

                int y = 0 - coordinate[0] - coordinate[1];

                if (coordinate[2] == 0)
                {
                    Assert.AreEqual(string.Format("{1}{0}{2}{0}{3}", Position.PositionStringSeparator, coordinate[0], y, coordinate[1]), p.ToString());
                }
                else
                {
                    Assert.AreEqual(string.Format("{1}{0}{2}{0}{3}{0}{4}", Position.PositionStringSeparator, coordinate[0], y, coordinate[1], coordinate[2]), p.ToString());
                }
            }
        }

        private static Dictionary<Direction, Position> GetOriginNeighbors()
        {
            Dictionary<Direction, Position> originNeighbors = new Dictionary<Direction, Position>
            {
                { Direction.Up, new Position(0, 1, -1, 0) },
                { Direction.UpRight, new Position(1, 0, -1, 0) },
                { Direction.DownRight, new Position(1, -1, 0, 0) },
                { Direction.Down, new Position(0, -1, 1, 0) },
                { Direction.DownLeft, new Position(-1, 0, 1, 0) },
                { Direction.UpLeft, new Position(-1, 1, 0, 0) }
            };

            return originNeighbors;
        }

        private static string[] GetPositionStrings(int[][] coordinates, bool skipStack = false)
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

        private static void AssertPositionsAreEqual(Position expected, Position actual)
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

        private readonly int[][] _validXYZCoordinates = new int[][]
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
            new int[] { 0, 0, 0, int.MaxValue},
            new int[] { int.MaxValue, 0, -int.MaxValue, int.MaxValue},
            new int[] { -int.MaxValue, 0, int.MaxValue, int.MaxValue},
            new int[] { int.MaxValue, -int.MaxValue, 0, int.MaxValue},
            new int[] { -int.MaxValue, int.MaxValue, 0, int.MaxValue},
            new int[] { 0, int.MaxValue, -int.MaxValue, int.MaxValue},
            new int[] { 0, -int.MaxValue, int.MaxValue, int.MaxValue},
        };

        private readonly int[][] _validQRCoordinates = new int[][]
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
            new int[] { 0, 0, int.MaxValue},
            new int[] { int.MaxValue, -int.MaxValue, int.MaxValue},
            new int[] { -int.MaxValue, int.MaxValue, int.MaxValue},
            new int[] { int.MaxValue, 0, int.MaxValue},
            new int[] { -int.MaxValue, 0, int.MaxValue},
            new int[] { 0, -int.MaxValue, int.MaxValue},
            new int[] { 0, int.MaxValue, int.MaxValue},
        };

        private readonly int[][] _invalidXYZCoordinates = new int[][]
        {
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
            new int[] { int.MaxValue, 0, 0, 0},
            new int[] { 0, int.MaxValue, 0, 0},
            new int[] { 0, 0, int.MaxValue, 0},
            new int[] { int.MaxValue, int.MaxValue, 0, 0},
            new int[] { 0, int.MaxValue, int.MaxValue, 0},
            new int[] { int.MaxValue, 0, int.MaxValue, 0},
            new int[] { int.MaxValue, int.MaxValue, int.MaxValue, 0},
            new int[] { -int.MaxValue, 0, 0, 0},
            new int[] { 0, -int.MaxValue, 0, 0},
            new int[] { 0, 0, -int.MaxValue, 0},
            new int[] { -int.MaxValue, -int.MaxValue, 0, 0},
            new int[] { 0, -int.MaxValue, -int.MaxValue, 0},
            new int[] { -int.MaxValue, 0, -int.MaxValue, 0},
            new int[] { -int.MaxValue, -int.MaxValue, -int.MaxValue, 0},
        };

        private readonly string[] _invalidPositionStrings = new string[]
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
