// 
// TurnMetrics.cs
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
using System.Collections.Generic;

namespace Mzinga.Core
{
    public class TurnMetrics
    {
        public Color TurnColor { get; private set; }

        public IDictionary<PieceName, PieceMetrics> PieceMetrics
        {
            get
            {
                return _pieceMetrics;
            }
        }
        private Dictionary<PieceName, PieceMetrics> _pieceMetrics;

        public int TotalMoveCount
        {
            get
            {
                int count = 0;

                foreach (PieceMetrics pieceMetrics in PieceMetrics.Values)
                {
                    count += pieceMetrics.MoveCount;
                }

                return count;
            }
        }

        public TurnMetrics(Color turnColor)
        {
            TurnColor = turnColor;

            _pieceMetrics = new Dictionary<PieceName,PieceMetrics>();

            IEnumerable<PieceName> pieceNames = TurnColor == Color.White ? EnumUtils.WhitePieceNames : EnumUtils.BlackPieceNames;
            foreach (PieceName pieceName in pieceNames)
            {
                _pieceMetrics.Add(pieceName, new PieceMetrics(pieceName));
            }
        }

        public void CopyFrom(TurnMetrics turnMetrics)
        {
            if (null == turnMetrics)
            {
                throw new ArgumentNullException("turnMetrics");
            }

            if (TurnColor != turnMetrics.TurnColor)
            {
                throw new ArgumentOutOfRangeException("turnMetrics");
            }

            foreach (PieceName pieceName in _pieceMetrics.Keys)
            {
                _pieceMetrics[pieceName].CopyFrom(turnMetrics.PieceMetrics[pieceName]);
            }
        }
    }
}
