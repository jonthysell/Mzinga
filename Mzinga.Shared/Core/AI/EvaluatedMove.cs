// 
// EvaluatedMove.cs
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

namespace Mzinga.Core.AI
{
    internal class EvaluatedMove : IEquatable<EvaluatedMove>, IComparable<EvaluatedMove>
    {
        public Move Move { get; private set; }

        public double ScoreAfterMove { get; private set; }

        public int Depth { get; private set; }

        public EvaluatedMove(Move move, double scoreAfterMove = UnevaluatedMoveScore, int depth = 0)
        {
            Move = move;
            ScoreAfterMove = scoreAfterMove;
            Depth = depth;
        }

        public int CompareTo(EvaluatedMove evaluatedMove)
        {
            return evaluatedMove.ScoreAfterMove.CompareTo(ScoreAfterMove);
        }

        public bool Equals(EvaluatedMove evaluatedMove)
        {
            if (null == evaluatedMove)
            {
                return false;
            }

            return Depth == evaluatedMove.Depth && ScoreAfterMove == evaluatedMove.ScoreAfterMove && Move == evaluatedMove.Move;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EvaluatedMove);
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 31 + Move.GetHashCode();
            hash = hash * 31 + ScoreAfterMove.GetHashCode();
            hash = hash * 31 + Depth;

            return hash;
        }

        public static bool operator ==(EvaluatedMove a, EvaluatedMove b)
        {
            if (ReferenceEquals(a, null))
            {
                return ReferenceEquals(b, null);
            }

            return a.Equals(b);
        }

        public static bool operator !=(EvaluatedMove a, EvaluatedMove b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return string.Format("{1}{0}{2}{0}{3}", EvaluatedMoveStringSeparator, Move, Depth, ScoreAfterMove);
        }

        public const char EvaluatedMoveStringSeparator = ';';

        private const double UnevaluatedMoveScore = double.MinValue;
    }
}
