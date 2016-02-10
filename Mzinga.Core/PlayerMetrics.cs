// 
// PlayerMetrics.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016 Jon Thysell <http://jonthysell.com>
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mzinga.Core
{
    public class PlayerMetrics
    {
        public Color PlayerColor { get; private set; }

        public int ValidMoveCount { get; set; }

        public int ValidPlacementCount { get; set; }

        public int ValidMovementCount { get; set; }

        public int PieceCount { get; private set; }

        public int PiecesInPlayCount { get; set; }

        public int PiecesInHandCount { get; set; }

        public int PiecesPinnedCount { get; set; }

        public IEnumerable<PieceName> PieceNames
        {
            get
            {
                return _pieceMetrics.Keys.AsEnumerable();
            }
        }

        public PieceMetrics this[PieceName pieceName]
        {
            get
            {
                return Get(pieceName);
            }
        }

        private Dictionary<PieceName, PieceMetrics> _pieceMetrics;

        public PlayerMetrics(Color playerColor)
        {
            PlayerColor = playerColor;

            _pieceMetrics = new Dictionary<PieceName,PieceMetrics>();

            IEnumerable<PieceName> pieceNames = PlayerColor == Color.White ? EnumUtils.WhitePieceNames : EnumUtils.BlackPieceNames;
            foreach (PieceName pieceName in pieceNames)
            {
                _pieceMetrics.Add(pieceName, new PieceMetrics(pieceName));
            }

            ValidMoveCount = 0;
            ValidPlacementCount = 0;
            ValidMovementCount = 0;
            PieceCount = _pieceMetrics.Count;
            PiecesInPlayCount = 0;
            PiecesInHandCount = 0;
            PiecesPinnedCount = 0;
        }

        public PieceMetrics Get(PieceName pieceName)
        {
            if (pieceName == PieceName.INVALID)
            {
                throw new ArgumentOutOfRangeException("pieceName");
            }

            if (!_pieceMetrics.Keys.Contains(pieceName))
            {
                throw new KeyNotFoundException("pieceName");
            }

            return _pieceMetrics[pieceName];
        }

        public void CopyFrom(PlayerMetrics playerMetrics)
        {
            if (null == playerMetrics)
            {
                throw new ArgumentNullException("playerMetrics");
            }

            if (PlayerColor != playerMetrics.PlayerColor)
            {
                throw new ArgumentOutOfRangeException("playerMetrics");
            }

            foreach (PieceName pieceName in PieceNames)
            {
                _pieceMetrics[pieceName].CopyFrom(playerMetrics[pieceName]);
            }
        }
    }
}
