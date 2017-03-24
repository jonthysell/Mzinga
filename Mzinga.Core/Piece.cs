// 
// Piece.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017 Jon Thysell <http://jonthysell.com>
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

        public Piece(PieceName pieceName, Position position = null)
        {
            Init(pieceName, position);
        }

        public Piece(string pieceString)
        {
            if (string.IsNullOrWhiteSpace(pieceString))
            {
                throw new ArgumentNullException("pieceString");
            }

            PieceName pieceName;
            Position position;

            Parse(pieceString, out pieceName, out position);

            Init(pieceName, position);
        }

        private void Init(PieceName pieceName, Position position)
        {
            if (pieceName == PieceName.INVALID)
            {
                throw new ArgumentOutOfRangeException("pieceName");
            }

            PieceName = pieceName;
            Position = position;
        }

        internal void Move(Position newPosition)
        {
            Position = newPosition;
        }

        internal void Shift(int deltaX, int deltaY, int deltaZ, int deltaStack = 0)
        {
            Position = Position.GetShifted(deltaX, deltaY, deltaZ, deltaStack);
        }

        internal void RotateRight()
        {
            Position = Position.GetRotatedRight();
        }

        internal void RotateLeft()
        {
            Position = Position.GetRotatedLeft();
        }
    }
}
