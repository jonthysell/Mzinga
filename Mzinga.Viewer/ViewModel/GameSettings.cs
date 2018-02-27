// 
// GameSettings.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017, 2018 Jon Thysell <http://jonthysell.com>
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

using Mzinga.Core;

namespace Mzinga.Viewer.ViewModel
{
    public class GameSettings
    {
        public PlayerType WhitePlayerType { get; set; } = PlayerType.Human;

        public PlayerType BlackPlayerType { get; set; } = PlayerType.EngineAI;

        public ExpansionPieces ExpansionPieces { get; set; } = ExpansionPieces.None;

        public BestMoveType BestMoveType
        {
            get
            {
                return _bestMoveType;
            }
            set
            {
                if (_bestMoveType != value)
                {
                    _bestMoveType = value;
                    if (value == BestMoveType.MaxDepth)
                    {
                        BestMoveMaxTime = null;
                        BestMoveMaxDepth = DefaultMaxDepth;
                    }
                    else
                    {
                        BestMoveMaxDepth = null;
                        BestMoveMaxTime = DefaultMaxTime;
                    }
                }
            }
        }
        private BestMoveType _bestMoveType = BestMoveType.MaxTime;

        public int? BestMoveMaxDepth
        {
            get
            {
                return _bestMoveMaxDepth;
            }
            set
            {
                if (value.HasValue && value.Value < 0)
                {
                    value = null;
                }
                _bestMoveMaxDepth = value;
            }
        }
        private int? _bestMoveMaxDepth = null;

        public TimeSpan? BestMoveMaxTime
        {
            get
            {
                return _bestMoveMaxTime;
            }
            set
            {
                if (value.HasValue && value.Value < TimeSpan.Zero)
                {
                    value = null;
                }
                _bestMoveMaxTime = value;
            }
        }
        private TimeSpan? _bestMoveMaxTime = DefaultMaxTime;

        public GameSettings() { }

        public GameSettings Clone()
        {
            GameSettings clone = new GameSettings();

            clone.WhitePlayerType = WhitePlayerType;
            clone.BlackPlayerType = BlackPlayerType;

            clone.ExpansionPieces = ExpansionPieces;

            clone.BestMoveType = BestMoveType;

            clone.BestMoveMaxDepth = BestMoveMaxDepth;
            clone.BestMoveMaxTime = BestMoveMaxTime;

            return clone;
        }

        private static int DefaultMaxDepth= 2;
        private static TimeSpan DefaultMaxTime = TimeSpan.FromSeconds(5.0);
    }

    public enum PlayerType
    {
        Human = 0,
        EngineAI
    }

    public enum BestMoveType
    {
        MaxDepth = 0,
        MaxTime
    }
}
