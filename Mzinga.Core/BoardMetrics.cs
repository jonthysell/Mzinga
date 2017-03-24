// 
// BoardMetrics.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017 Jon Thysell <http://jonthysell.com>
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

using System.Collections.Generic;
using System.Linq;

namespace Mzinga.Core
{
    public class BoardMetrics
    {
        public BoardState BoardState { get; private set; }

        public int ValidMoveCount
        {
            get
            {
                return _playerMetrics.Values.Sum((playerMetrics) => { return playerMetrics.ValidMoveCount; });
            }
        }

        public int ValidPlacementCount
        {
            get
            {
                return _playerMetrics.Values.Sum((playerMetrics) => { return playerMetrics.ValidPlacementCount; });
            }
        }

        public int ValidMovementCount
        {
            get
            {
                return _playerMetrics.Values.Sum((playerMetrics) => { return playerMetrics.ValidMovementCount; });
            }
        }

        public int PieceCount
        {
            get
            {
                return _playerMetrics.Values.Sum((playerMetrics) => { return playerMetrics.PieceCount; });
            }
        }

        public int PiecesInPlayCount
        {
            get
            {
                return _playerMetrics.Values.Sum((playerMetrics) => { return playerMetrics.PiecesInPlayCount; });
            }
        }

        public int PiecesInHandCount
        {
            get
            {
                return _playerMetrics.Values.Sum((playerMetrics) => { return playerMetrics.PiecesInHandCount; });
            }
        }

        public int PiecesPinnedCount
        {
            get
            {
                return _playerMetrics.Values.Sum((playerMetrics) => { return playerMetrics.PiecesPinnedCount; });
            }
        }

        public PlayerMetrics this[Color playerColor]
        {
            get
            {
                return Get(playerColor);
            }
        }

        private Dictionary<Color, PlayerMetrics> _playerMetrics;

        public BoardMetrics(BoardState boardState)
        {
            BoardState = boardState;

            _playerMetrics = new Dictionary<Color, PlayerMetrics>();
            _playerMetrics.Add(Color.White, new PlayerMetrics(Color.White));
            _playerMetrics.Add(Color.Black, new PlayerMetrics(Color.Black));
        }

        public PlayerMetrics Get(Color playerColor)
        {
            return _playerMetrics[playerColor];
        }
    }
}
