// 
// PieceMetrics.cs
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

namespace Mzinga.Core
{
    public class PieceMetrics
    {
        public bool IsInPlay { get; internal set; } = false;

        public int InPlay
        {
            get
            {
                return IsInPlay ? 1 : 0;
            }
        }

        public int ValidMoveCount
        {
            get
            {
                return _validMoveCount;
            }
            internal set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _validMoveCount = value;
            }
        }
        private int _validMoveCount = 0;

        public int IsPinned
        {
            get
            {
                return ValidMoveCount == 0 ? 1 : 0;
            }
        }

        public int NeighborCount
        {
            get
            {
                return _neighborCount;
            }
            internal set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _neighborCount = value;
            }
        }
        private int _neighborCount = 0;

        public PieceMetrics() { }
    }
}
