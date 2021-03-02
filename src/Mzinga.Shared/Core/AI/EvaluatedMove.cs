// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

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
            return HashCode.Combine(Move, ScoreAfterMove, Depth);
        }

        public static bool operator ==(EvaluatedMove a, EvaluatedMove b)
        {
            if (a is null)
            {
                return b is null;
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
