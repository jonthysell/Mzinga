// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using Mzinga.Core;

namespace Mzinga.SharedUX
{
    static class BoardExtensions
    {
        public static IEnumerable<PieceName> GetPiecesInPlay(this Board board)
        {
            for (int pn = 0; pn < (int)PieceName.NumPieceNames; pn++)
            {
                var pieceName = (PieceName)pn;
                if (board.PieceInPlay(pieceName) && Enums.PieceNameIsEnabledForGameType(pieceName, board.GameType))
                {
                    yield return pieceName;
                }
            }
        }

        public static IEnumerable<PieceName> GetWhiteHand(this Board board)
        {
            for (int pn = (int)PieceName.wQ; pn < (int)PieceName.bQ; pn++)
            {
                var pieceName = (PieceName)pn;
                if (board.PieceInHand(pieceName) && Enums.PieceNameIsEnabledForGameType(pieceName, board.GameType))
                {
                    yield return pieceName;
                }
            }
        }

        public static IEnumerable<PieceName> GetBlackHand(this Board board)
        {
            for (int pn = (int)PieceName.bQ; pn < (int)PieceName.NumPieceNames; pn++)
            {
                var pieceName = (PieceName)pn;
                if (board.PieceInHand(pieceName) && Enums.PieceNameIsEnabledForGameType(pieceName, board.GameType))
                {
                    yield return pieceName;
                }
            }
        }

        public static int GetHeight(this Board board)
        {
            bool pieceInPlay = false;
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            foreach (PieceName pieceName in board.GetPiecesInPlay())
            {
                pieceInPlay = true;
                Position pos = board.GetPosition(pieceName);

                minY = Math.Min(minY, 1 - pos.Q - pos.R);
                maxY = Math.Max(maxY, 1- pos.Q - pos.R);
            }

            return pieceInPlay ? (maxY - minY) : 0;
        }

        public static int GetWidth(this Board board)
        {
            bool pieceInPlay = false;
            int minX = int.MaxValue;
            int maxX = int.MinValue;

            foreach (PieceName pieceName in board.GetPiecesInPlay())
            {
                pieceInPlay = true;
                Position pos = board.GetPosition(pieceName);

                minX = Math.Min(minX, pos.Q);
                maxX = Math.Max(maxX, pos.Q);
            }

            return pieceInPlay ? (maxX - minX) : 0;
        }
    }
}
