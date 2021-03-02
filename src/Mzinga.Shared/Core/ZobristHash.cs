// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Mzinga.Core
{
    public class ZobristHash
    {
        public ulong Value { get; private set; }

        private static ulong _next = 1;
        private static readonly ulong _hashPartByTurnColor = 0;
        private static readonly ulong[] _hashPartByLastMovedPiece = new ulong[EnumUtils.NumPieceNames];
        private static readonly Dictionary<Position, ulong>[] _hashPartByPosition = new Dictionary<Position, ulong>[EnumUtils.NumPieceNames];

        public ZobristHash()
        {
            Value = EmptyBoard;
        }

        public void TogglePiece(PieceName pieceName, Position position)
        {
            Value ^= _hashPartByPosition[(int)pieceName][position];
        }

        public void ToggleLastMovedPiece(PieceName pieceName)
        {
            if (pieceName != PieceName.INVALID)
            {
                Value ^= _hashPartByLastMovedPiece[(int)pieceName];
            }
        }

        public void ToggleTurn()
        {
            Value ^= _hashPartByTurnColor;
        }

        static ZobristHash()
        {
            _next = 1;
            _hashPartByTurnColor = Rand64();

            for (int i = 0; i < _hashPartByLastMovedPiece.Length; i++)
            {
                _hashPartByLastMovedPiece[i] = Rand64();
            }

            IEnumerable<Position> uniquePositions = Position.GetUniquePositions(NumUniquePositions);

            for (int i = 0; i < _hashPartByPosition.Length; i++)
            {
                _hashPartByPosition[i] = new Dictionary<Position, ulong>();

                foreach (Position pos in uniquePositions)
                {
                    _hashPartByPosition[i].Add(pos, Rand64());
                }
            }
        }

        private static ulong Rand64()
        {
            _next = _next * 1103515245 + 12345;
            return _next;
        }

        public const long EmptyBoard = 0;

        private const int NumUniquePositions = (int)Position.MaxStack * EnumUtils.NumPieceNames * EnumUtils.NumPieceNames;
    }
}
