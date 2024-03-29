﻿// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

namespace Mzinga.Core.AI
{
    public readonly struct EvaluatedMove : IEquatable<EvaluatedMove>, IComparable<EvaluatedMove>
    {
        public readonly Move Move;

        public readonly double ScoreAfterMove;

        public readonly int Depth;

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
            return Depth == evaluatedMove.Depth && ScoreAfterMove == evaluatedMove.ScoreAfterMove && Move == evaluatedMove.Move;
        }

        public override bool Equals(object? obj)
        {
            return obj is EvaluatedMove move && this == move;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Move, ScoreAfterMove, Depth);
        }

        public static bool operator ==(EvaluatedMove a, EvaluatedMove b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(EvaluatedMove a, EvaluatedMove b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return string.Join(EvaluatedMoveStringSeparator, Move, Depth, ScoreAfterMove);
        }

        public const char EvaluatedMoveStringSeparator = ';';

        private const double UnevaluatedMoveScore = double.MinValue;
    }
}
