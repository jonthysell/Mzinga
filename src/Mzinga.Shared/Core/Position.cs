﻿// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Mzinga.Core
{
    public class Position : IEquatable<Position>
    {
        public static Position Origin
        {
            get
            {
                return _origin ??= new Position(0, 0, 0);
            }
        }
        private static Position _origin;

        public readonly int Q;
        public readonly int R;

        public readonly uint Stack;

        public int X => Q;
        public int Y => 0 - Q - R;
        public int Z => R;

        private Position[] _localCache = null;

        private static readonly Dictionary<Position, Position[]> _sharedCache = new Dictionary<Position, Position[]>();

        public Position(int x, int y, int z, uint stack)
        {
            if (x + y + z != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }

            Q = x;
            R = z;

            Stack = stack;
        }

        public Position(int q, int r, uint stack)
        {
            Q = q;
            R = r;
            Stack = stack;
        }

        public bool IsTouching(Position position)
        {
            if (null == position)
            {
                throw new ArgumentNullException(nameof(position));
            }

            for (int i = 0; i < EnumUtils.NumDirections; i++)
            {
                if (NeighborAt(i) == position)
                {
                    return true;
                }
            }

            return false;
        }

        public Position NeighborAt(Direction direction)
        {
            return NeighborAt((int)direction);
        }

        public Position NeighborAt(int direction)
        {
            return CacheLookup(direction % EnumUtils.NumDirections);
        }

        public Position GetAbove()
        {
            return CacheLookup(EnumUtils.NumDirections);
        }

        public Position GetBelow()
        {
            return CacheLookup(EnumUtils.NumDirections + 1);
        }

        private Position CacheLookup(int index)
        {
            return CacheLookup(index, out _);
        }

        private Position CacheLookup(int index, out bool createdNew)
        {
            if (null == _localCache)
            {
                if (!_sharedCache.TryGetValue(this, out _localCache))
                {
                    _localCache = (_sharedCache[this] = new Position[EnumUtils.NumDirections + 2]);
                }
            }

            createdNew = false;

            if (null == _localCache[index])
            {
                if (index < EnumUtils.NumDirections)
                {
                    _localCache[index] = new Position(Q + _neighborDeltas[index][0], R + _neighborDeltas[index][2], 0);
                    createdNew = true;
                }
                else if (index == EnumUtils.NumDirections) // Above
                {
                    _localCache[index] = new Position(Q, R, Stack + 1);
                    createdNew = true;
                }
                else if (index == EnumUtils.NumDirections + 1 && Stack > 0) // Below
                {
                    _localCache[index] = new Position(Q, R, Stack - 1);
                    createdNew = true;
                }
            }

            return _localCache[index];
        }

        public static IEnumerable<Position> GetUniquePositions(int count, uint maxStack = MaxStack)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            Queue<Position> positions = new Queue<Position>();
            positions.Enqueue(Origin);

            HashSet<Position> result = new HashSet<Position>();

            while (positions.Count > 0)
            {
                Position pos = positions.Dequeue();

                for (int i = 0; i < EnumUtils.NumDirections + 2; i++)
                {
                    if (result.Count < count)
                    {
                        Position neighbor = pos.CacheLookup(i);
                        if (null != neighbor && neighbor.Stack < maxStack && result.Add(neighbor))
                        {
                            positions.Enqueue(neighbor);
                        }
                    }
                }
            }

            return result;
        }

        public static Position Parse(string positionString)
        {
            if (TryParse(positionString, out Position position))
            {
                return position;
            }

            throw new ArgumentOutOfRangeException(nameof(positionString));
        }

        public static bool TryParse(string positionString, out Position position)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(positionString))
                {
                    position = null;
                    return true;
                }

                positionString = positionString.Trim();

                string[] split = positionString.Split(new char[] { PositionStringSeparator }, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length == 2)
                {
                    int q = int.Parse(split[0]);
                    int r = int.Parse(split[1]);

                    position = new Position(q, r, 0);
                    return true;
                }
                else if (split.Length >= 3)
                {
                    int x = int.Parse(split[0]);
                    int y = int.Parse(split[1]);
                    int z = int.Parse(split[2]);
                    uint stack = split.Length > 3 ? uint.Parse(split[3]) : 0;

                    position = new Position(x, y, z, stack);
                    return true;
                }
            }
            catch (Exception) { }

            position = null;
            return false;
        }

        public bool Equals(Position pos)
        {
            if (null == pos)
            {
                return false;
            }

            return Q == pos.Q && R == pos.R && Stack == pos.Stack;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Position);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Q, R, Stack);
        }

        public static bool operator ==(Position a, Position b)
        {
            if (a is null)
            {
                return b is null;
            }

            return a.Equals(b);
        }

        public static bool operator !=(Position a, Position b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            if (Stack > 0)
            {
                return string.Format("{1}{0}{2}{0}{3}{0}{4}", PositionStringSeparator, X, Y, Z, Stack);
            }

            return string.Format("{1}{0}{2}{0}{3}", PositionStringSeparator, X, Y, Z);
        }
        
        private static readonly int[][] _neighborDeltas = new int[][]
        {
            new int[] { 0, 1, -1 },
            new int[] { 1, 0, -1 },
            new int[] { 1, -1, 0 },
            new int[] { 0, -1, 1 },
            new int[] { -1, 0, 1 },
            new int[] { -1, 1, 0 },
        };

        public const uint MaxStack = 5;

        public const char PositionStringSeparator = ',';
    }
}
