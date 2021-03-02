// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using Mzinga.Core;

namespace Mzinga.SharedUX
{
    static class BoardExtensions
    {
        public static int GetHeight(this Board board)
        {
            bool pieceInPlay = false;
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            foreach (PieceName pieceName in board.PiecesInPlay)
            {
                pieceInPlay = true;
                Position pos = board.GetPiecePosition(pieceName);

                minY = Math.Min(minY, pos.Y);
                maxY = Math.Max(maxY, pos.Y);
            }

            return pieceInPlay ? (maxY - minY) : 0;
        }

        public static int GetWidth(this Board board)
        {
            bool pieceInPlay = false;
            int minX = int.MaxValue;
            int maxX = int.MinValue;

            foreach (PieceName pieceName in board.PiecesInPlay)
            {
                pieceInPlay = true;
                Position pos = board.GetPiecePosition(pieceName);

                minX = Math.Min(minX, pos.X);
                maxX = Math.Max(maxX, pos.X);
            }

            return pieceInPlay ? (maxX - minX) : 0;
        }
    }
}
