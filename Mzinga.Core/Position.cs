// 
// Position.cs
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
using System.Collections.Generic;
using System.Linq;

namespace Mzinga.Core
{
    public class Position : IEquatable<Position>, IComparable<Position>
    {
        public static Position Origin
        {
            get
            {
                return _origin ?? (_origin = new Position(0, 0, 0, 0));
            }
        }
        private static Position _origin;

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }

        public int Stack { get; private set; }

        public int Q
        {
            get
            {
                return X;
            }
        }

        public int R
        {
            get
            {
                return Z;
            }
        }

        public IEnumerable<Position> Neighbors
        {
            get
            {
                for (int i = 0; i < _neighborDeltas.Length; i++)
                {
                    yield return NeighborAt(i);
                }
            }
        }

        public Position(int x, int y, int z, int stack)
        {
            if (x + y + z != 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (stack < 0)
            {
                throw new ArgumentOutOfRangeException("stack");
            }

            X = x;
            Y = y;
            Z = z;

            Stack = stack;
        }

        public Position(int q, int r, int stack)
        {
            if (stack < 0)
            {
                throw new ArgumentOutOfRangeException("stack");
            }

            X = q;
            Z = r;
            Y = 0 - q - r;

            Stack = stack;
        }

        public bool IsTouching(Position position)
        {
            if (null == position)
            {
                throw new ArgumentNullException("position");
            }

            return Neighbors.Contains(position);
        }

        public Position NeighborAt(Direction direction, int deltaStack = 0)
        {
            int dirIndex = (int)direction;
            return NeighborAt(dirIndex, deltaStack);
        }

        public Position NeighborAt(Direction[] directions, int deltaStack = 0)
        {
            if (null == directions)
            {
                throw new ArgumentNullException("directions");
            }

            Position neighbor = this;
            for (int i = 0; i < directions.Length; i++)
            {
                int dirIndex = (int)directions[i];
                neighbor = neighbor.GetShifted(_neighborDeltas[dirIndex][0], _neighborDeltas[dirIndex][1], _neighborDeltas[dirIndex][2], deltaStack);
            }

            return neighbor;
        }

        private Position NeighborAt(int dirIndex, int deltaStack = 0)
        {
            return GetShifted(_neighborDeltas[dirIndex][0], _neighborDeltas[dirIndex][1], _neighborDeltas[dirIndex][2], deltaStack);
        }

        public Position GetShifted(int deltaX, int deltaY, int deltaZ, int deltaStack = 0)
        {
            return new Position(X + deltaX, Y + deltaY, Z + deltaZ, Stack + deltaStack);
        }

        public Position GetRotatedRight()
        {
            return new Position(-Z, -X, -Y, Stack);
        }

        public Position GetRotatedLeft()
        {
            return new Position(-Y, -Z, -X, Stack);
        }

        public static Position FromCursor(double cursorX, double cursorY, double hexRadius)
        {
            if (hexRadius < 0)
            {
                throw new ArgumentOutOfRangeException("hexRadius");
            }
            else if (double.IsInfinity(cursorX) || double.IsInfinity(cursorY) || hexRadius == 0) // No hexes on board
            {
                return Origin;
            }

            // Convert cursor to axial
            double q = (cursorX * (2.0 / 3.0)) / hexRadius;
            double r = ((-cursorX / 3.0) + (Math.Sqrt(3.0) / 3.0) * cursorY) / hexRadius;

            // Convert axial to cube
            double x = q;
            double z = r;
            double y = -x - z;

            // Round cube
            double rx = Math.Round(x);
            double ry = Math.Round(y);
            double rz = Math.Round(z);

            double xdiff = Math.Abs(rx - x);
            double ydiff = Math.Abs(ry - y);
            double zdiff = Math.Abs(rz - z);

            if (xdiff > ydiff && xdiff > zdiff)
            {
                rx = -ry - rz;
            }
            else if (ydiff > zdiff)
            {
                ry = -rx - rz;
            }
            else
            {
                rz = -rx - ry;
            }

            return new Position((int)rx, (int)ry, (int)rz, 0);
        }

        public Position Clone()
        {
            return new Position(X, Y, Z, Stack);
        }

        public static Position Parse(string positionString)
        {
            Position position;
            if (TryParse(positionString, out position))
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
                    int stack = split.Length > 3 ? int.Parse(split[3]) : 0;

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
            hash = hash * 31 + Stack;
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

        public int CompareTo(Position position)
        {
            if (null == position)
            {
                throw new ArgumentNullException("position");
            }

            int xCompare = X.CompareTo(position.X);

            if (xCompare != 0)
            {
                return xCompare;
            }

            int yCompare = Y.CompareTo(position.Y);

            if (yCompare != 0)
            {
                return yCompare;
            }

            int zCompare = Z.CompareTo(position.Z);

            if (zCompare != 0)
            {
                return zCompare;
            }

            return Stack.CompareTo(position.Stack);
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

        public const char PositionStringSeparator = ',';
    }
}
