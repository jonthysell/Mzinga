// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

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
                throw new ArgumentOutOfRangeException(nameof(pieceName));
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
            if (a is null)
            {
                return b is null;
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
