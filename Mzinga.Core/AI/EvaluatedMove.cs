// 
// EvaluatedMove.cs
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

using System;

namespace Mzinga.Core.AI
{
    public class EvaluatedMove : IComparable<EvaluatedMove>
    {
        public Move Move { get; private set; }

        public double ScoreAfterMove { get; private set; }

        public int Depth { get; private set; }

        public EvaluatedMove(Move move, double scoreAfterMove = UnevaluatedMoveScore, int depth = 0)
        {
            if (null == move)
            {
                throw new ArgumentNullException("move");
            }

            Move = move;
            ScoreAfterMove = scoreAfterMove;
            Depth = depth;
        }

        public int CompareTo(EvaluatedMove evaluatedMove)
        {
            if (null == evaluatedMove)
            {
                throw new ArgumentNullException("evaluatedMove");
            }

            return ScoreAfterMove.CompareTo(evaluatedMove.ScoreAfterMove);
        }

        public override string ToString()
        {
            return string.Format("{1}{0}{2}{0}{3}", EvaluatedMoveStringSeparator, Move, Depth, ScoreAfterMove);
        }

        public const char EvaluatedMoveStringSeparator = ';';

        private const double UnevaluatedMoveScore = double.MinValue;
    }
}
