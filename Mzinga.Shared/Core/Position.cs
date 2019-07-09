// 
// Position.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017, 2018, 2019 Jon Thysell <http://jonthysell.com>
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

namespace Mzinga.Core
{
    public class Position : IEquatable<Position>
    {
        public static Position Origin
        {
            get
            {
                return _origin ?? (_origin = new Position(0, 0, 0));
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

        private static Dictionary<Position, Position[]> _sharedCache = new Dictionary<Position, Position[]>();

        public Position(int x, int y, int z, uint stack)
        {
            if (x + y + z != 0)
            {
                throw new ArgumentOutOfRangeException();
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
                throw new ArgumentNullException("position");
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
            return CacheLookup(index, out bool createdNew);
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
                throw new ArgumentOutOfRangeException("count");
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

            throw new ArgumentOutOfRangeException("positionString");
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
            int hash = 17;
            hash = hash * 31 + Q;
            hash = hash * 31 + R;
            hash = hash * 31 + (int)Stack;
            return hash;
        }

        public static bool operator ==(Position a, Position b)
        {
            if (ReferenceEquals(a, null))
            {
                return ReferenceEquals(b, null);
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
