// 
// ZobristHash.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2018 Jon Thysell <http://jonthysell.com>
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

namespace Mzinga.Core
{
    public class ZobristHash
    {
        public long Value { get; private set; }

        private static long _next = 1;
        private static long _hashPartByTurnColor = 0;
        private static Dictionary<Position, long>[] _hashPartByPosition = new Dictionary<Position, long>[EnumUtils.NumPieceNames];

        public ZobristHash()
        {
            Value = EmptyBoard;
        }

        public void TogglePiece(PieceName pieceName, Position position)
        {
            Value ^= _hashPartByPosition[(int)pieceName][position];
        }

        public void ToggleTurn()
        {
            Value ^= _hashPartByTurnColor;
        }

        static ZobristHash()
        {
            _next = 1;
            _hashPartByTurnColor = Rand64();

            IEnumerable<Position> uniquePositions = Position.GetUniquePositions(NumUniquePositions);

            for (int i = 0; i < _hashPartByPosition.Length; i++)
            {
                _hashPartByPosition[i] = new Dictionary<Position, long>();

                foreach (Position pos in uniquePositions)
                {
                    _hashPartByPosition[i].Add(pos, Rand64());
                }
            }
        }

        private static long Rand64()
        {
            _next = _next * 1103515245 + 12345;
            return _next;
        }

        public const long EmptyBoard = 0;

        private const int NumUniquePositions = Position.MaxStack * EnumUtils.NumPieceNames * EnumUtils.NumPieceNames;
    }
}
