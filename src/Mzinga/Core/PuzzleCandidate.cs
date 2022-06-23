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
            return IsOneBestMoveToForceWinPuzzle();
        }

        public bool IsOneBestMoveToForceWinPuzzle()
        {
            var validMoves = Board.GetValidMoves();

            bool bestMoveWon = false;
            foreach (var move in validMoves)
            {
                Board.TrustedPlay(in move, Board.GetMoveString(move));
                bool moveCanWin = TargetColorCanForceWin(MaxDepth - 1);
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

        private bool TargetColorCanForceWin(int depth)
        {
            if (depth == 0 || Board.GameIsOver)
            {
                return TargetColorWins();
            }

            var validMoves = Board.GetValidMoves();

            if (Board.CurrentColor != TargetColor)
            {
                // Every move should force a win for target player
                foreach (var move in validMoves)
                {
                    Board.TrustedPlay(in move, Board.GetMoveString(move));
                    bool moveWonForTargetPlayer = TargetColorCanForceWin(depth - 1);
                    Board.TryUndoLastMove();

                    if (!moveWonForTargetPlayer)
                    {
                        return false;
                    }
                }

                return true;
            }

            // There should be at least one move that forces a win for the target player
            foreach (var move in validMoves)
            {
                Board.TrustedPlay(in move, Board.GetMoveString(move));
                bool moveWonForTargetPlayer = TargetColorCanForceWin(depth - 1);
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
