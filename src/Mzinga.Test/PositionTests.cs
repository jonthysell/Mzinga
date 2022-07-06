// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Core;

namespace Mzinga.Test
{
    [TestClass]
    public class PositionTests
    {
        [TestMethod]
        public void Position_NewTest()
        {
            foreach (int[] coordinate in _validCoordinates)
            {
                Position p = new Position(coordinate[0], coordinate[1], coordinate[2]);
                Assert.IsNotNull(p);

                Assert.AreEqual(coordinate[0], p.Q);
                Assert.AreEqual(coordinate[1], p.R);
                Assert.AreEqual(coordinate[2], p.Stack);
            }
        }

        [TestMethod]
        public void Position_OriginTest()
        {
            Assert.IsNotNull(Position.OriginPosition);

            Assert.AreEqual(0, Position.OriginPosition.Q);
            Assert.AreEqual(0, Position.OriginPosition.R);
            Assert.AreEqual(0, Position.OriginPosition.Stack);
        }

        [TestMethod]
        public void Position_NeighborAtIndexTest()
        {
            Position position = Position.OriginPosition;

            List<Position> expectedNeighbors = new List<Position>(GetOriginNeighbors().Values);

            List<Position> actualNeighbors = new List<Position>();

            for (int direction = 0; direction < (int)Direction.NumDirections; direction++)
            {
                actualNeighbors.Add(position.GetNeighborAt((Direction)direction));
            }

            TestUtils.AssertHaveEqualChildren(expectedNeighbors, actualNeighbors);
        }

        [TestMethod]
        public void Position_NeighborAtDirectionTest()
        {
            Position position = Position.OriginPosition;

            Dictionary<Direction, Position> expectedNeighbors = GetOriginNeighbors();

            foreach (Direction direction in expectedNeighbors.Keys)
            {
                Assert.AreEqual(expectedNeighbors[direction], position.GetNeighborAt(direction));
            }
        }

        [TestMethod]
        public void Position_GetAboveTest()
        {
            Position position = Position.OriginPosition;

            Position expectedAbove = new Position(0, 0, 1);

            Assert.AreEqual(expectedAbove, position.GetAbove());
        }

        [TestMethod]
        public void Position_GetBelowTest()
        {
            Position position = new Position(0, 0, 1);

            Position expectedBelow = new Position(0, 0, 0);

            Assert.AreEqual(expectedBelow, position.GetBelow());
        }

        [TestMethod]
        public void Position_EqualityTest()
        {
            foreach (int[] coordinate in _validCoordinates)
            {
                Position p1 = new Position(coordinate[0], coordinate[1], coordinate[2]);
                Position p2 = new Position(coordinate[0], coordinate[1], coordinate[2]);

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
            int[][] coordinates = _validCoordinates;

            for (int i = 1; i < coordinates.Length; i++)
            {
                Position p1 = new Position(coordinates[i - 1][0], coordinates[i - 1][1], coordinates[i - 1][2]);
                Position p2 = new Position(coordinates[i][0], coordinates[i][1], coordinates[i][2]);

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

        private static Dictionary<Direction, Position> GetOriginNeighbors()
        {
            Dictionary<Direction, Position> originNeighbors = new Dictionary<Direction, Position>
            {
                { Direction.Up, new Position(0, -1, 0) },
                { Direction.UpRight, new Position(1, -1, 0) },
                { Direction.DownRight, new Position(1, 0, 0) },
                { Direction.Down, new Position(0, 1, 0) },
                { Direction.DownLeft, new Position(-1, 1, 0) },
                { Direction.UpLeft, new Position(-1, 0, 0) }
            };

            return originNeighbors;
        }

        private readonly int[][] _validCoordinates = new int[][]
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
    }
}
