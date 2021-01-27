// 
// Move.cs
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

namespace Mzinga.Core
{
    public class Move : PiecePositionBase, IEquatable<Move>
    {
        public static readonly Move Pass = new Move();

        public bool IsPass
        {
            get
            {
                return (this == Pass);
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
            if (string.IsNullOrWhiteSpace(moveString))
            {
                throw new ArgumentNullException(nameof(moveString));
            }

            if (!moveString.Equals(PassString, StringComparison.CurrentCultureIgnoreCase))
            {

                Parse(moveString, out PieceName pieceName, out Position position);

                Init(pieceName, position);
            }
        }

        private void Init(PieceName pieceName, Position position)
        {
            if (pieceName == PieceName.INVALID)
            {
                throw new ArgumentOutOfRangeException("pieceName");
            }

            PieceName = pieceName;
            Position = position ?? throw new ArgumentNullException(nameof(position));
        }

        public bool Equals(Move move)
        {
            if (null == move)
            {
                return false;
            }

            return PieceName == move.PieceName && Position == move.Position;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Move);
        }

        public override int GetHashCode()
        {
            int hash = 17;

            if (PieceName != PieceName.INVALID)
            {
                hash = hash * 31 + (int)PieceName;
                hash = hash * 31 + Position.GetHashCode();
            }

            return hash;
        }

        public static bool operator ==(Move a, Move b)
        {
            if (ReferenceEquals(a, null))
            {
                return ReferenceEquals(b, null);
            }

            return a.Equals(b);
        }

        public static bool operator !=(Move a, Move b)
        {
            return !(a == b);
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
