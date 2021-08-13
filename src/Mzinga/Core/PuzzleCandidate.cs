// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

namespace Mzinga.Core
{
    public class PuzzleCandidate
    {
        public readonly Board Board;
        public readonly PlayerColor TargetColor;

        public readonly Move BestMove;
        public readonly int MaxDepth;

        public PuzzleCandidate(Board board, Move bestMove, int maxDepth)
        {
            Board = board;
            TargetColor = board.CurrentColor;
            BestMove = bestMove;
            MaxDepth = maxDepth >= 1 ? maxDepth : throw new ArgumentOutOfRangeException(nameof(maxDepth));
        }

        public static bool IsPuzzleCandidate(int depth, double score)
        {
            return depth >= 1 && double.IsPositiveInfinity(score);
        }

        public bool IsPuzzle()
        {
            var validMoves = Board.GetValidMoves();

            bool bestMoveWon = false;
            foreach (var move in validMoves)
            {
                Board.TrustedPlay(move, Board.GetMoveString(move));
                bool moveCanWin = TargetColorCanWin(MaxDepth - 1);
                Board.TryUndoLastMove();

                if (move == BestMove)
                {
                    bestMoveWon = moveCanWin;
                }
                else if (move != BestMove && moveCanWin)
                {
                    return false;
                }
            }

            return bestMoveWon;
        }

        private bool TargetColorCanWin(int depth)
        {
            if (depth == 0)
            {
                return TargetColorWins();
            }

            var validMoves = Board.GetValidMoves();
            foreach (var move in validMoves)
            {
                Board.TrustedPlay(move, Board.GetMoveString(move));
                bool moveWonForTargetPlayer = TargetColorCanWin(depth - 1);
                Board.TryUndoLastMove();

                if (moveWonForTargetPlayer)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TargetColorWins()
        {
            return Board.BoardState == (TargetColor == PlayerColor.White ? BoardState.WhiteWins : BoardState.BlackWins);
        }
    }
}
