// 
// BoardExtensions.cs
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

using System;

using Mzinga.Core;

namespace Mzinga.Viewer.ViewModel
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
