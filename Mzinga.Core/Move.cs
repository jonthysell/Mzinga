// 
// Move.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015 Jon Thysell <http://jonthysell.com>
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
using System.Text;
using System.Text.RegularExpressions;

namespace Mzinga.Core
{
    public class Move : PiecePositionBase, IComparable
    {
        public static Move Pass
        {
            get
            {
                return new Move();
            }
        }

        public bool IsPass
        {
            get
            {
                return (this.CompareTo(Move.Pass) == 0);
            }
        }

        private Move()
        {
            PieceName = PieceName.INVALID;
            Position = null;
        }

        public Move(PieceName pieceName, Position position) : this()
        {
            Init(pieceName, position);
        }

        public Move(string moveString) : this()
        {
            if (!moveString.Equals(PassString, StringComparison.CurrentCultureIgnoreCase))
            {
                PieceName pieceName;
                Position position;

                Parse(moveString, out pieceName, out position);

                Init(pieceName, position);
            }
        }

        private void Init(PieceName pieceName, Position position)
        {
            if (pieceName == PieceName.INVALID)
            {
                throw new ArgumentOutOfRangeException("pieceName");
            }

            if (null == position)
            {
                throw new ArgumentNullException("position");
            }

            PieceName = pieceName;
            Position = position;
        }

        public int CompareTo(object obj)
        {
            if (null == obj)
            {
                throw new ArgumentNullException("obj");
            }

            Move move = obj as Move;
            if (null == move)
            {
                throw new ArgumentException();
            }

            int pieceCompare = ((int)PieceName).CompareTo((int)move.PieceName);

            if (pieceCompare != 0)
            {
                return pieceCompare;
            }

            if (null == Position && null == move.Position)
            {
                return 0;
            }
            else if (null == Position && null != move.Position)
            {
                return -1;
            }
            else if (null != Position && null == move.Position)
            {
                return 1;
            }

            return Position.CompareTo(move.Position);
        }

        public override string ToString()
        {
            if (IsPass)
            {
                return PassString;
            }

            return base.ToString();
        }

        public const string PassString = "PASS";
    }
}
