// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Mzinga.Core
{
    class ZobristHash
    {
        public ulong Value { get; private set; }

        private static ulong _next = 1;
        private static readonly ulong _hashPartByTurnColor;
        private static readonly ulong[] _hashPartByLastMovedPiece = new ulong[(int)PieceName.NumPieceNames];
        private static readonly ulong[,,,] _hashPartByPosition = new ulong[(int)PieceName.NumPieceNames, Position.BoardSize, Position.BoardSize, Position.BoardStackSize + 1];

        public ZobristHash()
        {
            Value = EmptyBoard;
        }

        public void TogglePiece(PieceName pieceName, Position position)
        {
            Value ^= _hashPartByPosition[(int)pieceName, (Position.BoardSize / 2) + position.Q, (Position.BoardSize / 2) + position.R, position.Stack + 1];
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

            for (int pn = 0; pn < _hashPartByPosition.GetLength(0); pn++)
            {
                for (int q = 0; q < _hashPartByPosition.GetLength(1); q++)
                {
                    for (int r = 0; r < _hashPartByPosition.GetLength(2); r++)
                    {
                        for (int s = 0; s < _hashPartByPosition.GetLength(3); s++)
                        {
                            _hashPartByPosition[pn, q, r, s] = Rand64();
                        }
                    }
                }
            }
        }

        private static ulong Rand64()
        {
            _next = _next * 1103515245 + 12345;
            return _next;
        }

        public const long EmptyBoard = 0;
    }
}
