// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

namespace Mzinga.Core
{
    public class Piece : PiecePositionBase
    {
        public bool InPlay
        {
            get
            {
                return (null != Position);
            }
        }

        public bool InHand
        {
            get
            {
                return (null == Position);
            }
        }

        public Piece PieceAbove { get; internal set; } = null;

        public Piece PieceBelow { get; internal set; } = null;

        public Piece(PieceName pieceName, Position position = null)
        {
            Init(pieceName, position);
        }

        public Piece(string pieceString)
        {
            if (string.IsNullOrWhiteSpace(pieceString))
            {
                throw new ArgumentNullException(nameof(pieceString));
            }

            Parse(pieceString, out PieceName pieceName, out Position position);

            Init(pieceName, position);
        }

        private void Init(PieceName pieceName, Position position)
        {
            if (pieceName == PieceName.INVALID)
            {
                throw new ArgumentOutOfRangeException(nameof(pieceName));
            }

            PieceName = pieceName;
            Position = position;
        }

        internal void Move(Position newPosition)
        {
            Position = newPosition;
        }
    }
}
