// 
// GameAI.cs
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
using System.Collections.Generic;
using System.Linq;

using Mzinga.Core;

namespace Mzinga.Core.AI
{
    public class GameAI : GameAIBase
    {
        public int MaxDepth
        {
            get
            {
                return _maxDepth;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _maxDepth = value;
            }
        }
        private int _maxDepth;

        public bool AlphaBetaPruning { get; set; }

        public GameAI() : base()
        {
            MaxDepth = 0;
            AlphaBetaPruning = false;
        }

        protected override EvaluatedMoveCollection EvaluateMoves(GameBoard gameBoard)
        {
            if (null == gameBoard)
            {
                throw new ArgumentNullException("gameBoard");
            }

            StartTime = DateTime.Now;

            MoveSet validMoves = gameBoard.GetValidMoves();

            EvaluatedMoveCollection movesToEvaluate = new EvaluatedMoveCollection();

            foreach (Move move in validMoves)
            {
                movesToEvaluate.Add(new EvaluatedMove(move));
            }

            // No choices, don't bother evaluating
            if (movesToEvaluate.Count <= 1 || MaxDepth == 0)
            {
                return movesToEvaluate;
            }

            // Non-iterative search (can't bound on time)
            if (MaxDepth > 0)
            {
                return EvaluateMovesToDepth(gameBoard, movesToEvaluate, MaxDepth);
            }

            // Iterative search
            int depth = 0;
            while (HasTimeLeft)
            {
                depth++;

                // "Re-sort" moves to evaluate based on the next iteration
                movesToEvaluate = EvaluateMovesToDepth(gameBoard, movesToEvaluate, depth);
            }

            StartTime = null;

            return movesToEvaluate;
        }

        private EvaluatedMoveCollection EvaluateMovesToDepth(GameBoard gameBoard, EvaluatedMoveCollection movesToEvaluate, int maxDepth)
        {
            if (null == gameBoard)
            {
                throw new ArgumentNullException("gameBoard");
            }

            if (null == movesToEvaluate)
            {
                throw new ArgumentNullException("movesToEvaluate");
            }

            if (maxDepth <= 0)
            {
                throw new ArgumentOutOfRangeException("maxDepth");
            }

            EvaluatedMoveCollection evaluatedMoves = new EvaluatedMoveCollection();

            Color maxColor = gameBoard.CurrentTurnColor;

            double alpha = Double.NegativeInfinity;
            double beta = Double.PositiveInfinity;

            foreach (EvaluatedMove moveToEvaluate in movesToEvaluate)
            {
                if (!HasTimeLeft)
                {
                    break;
                }

                gameBoard.TrustedPlay(moveToEvaluate.Move);
                double scoreAfterMove = EvaluateBoardAfterMove(gameBoard, maxColor, false, maxDepth, 1, alpha, beta);
                gameBoard.UndoLastMove();

                EvaluatedMove evaluatedMove = new EvaluatedMove(moveToEvaluate.Move, scoreAfterMove, maxDepth);
                evaluatedMoves.Add(evaluatedMove);

                alpha = Math.Max(alpha, scoreAfterMove);

                if (AlphaBetaPruning && beta <= alpha)
                {
                    break;
                }                
            }

            // We must have cut-off early, add the remaining moves to the end (possibly for the next iteration)
            if (evaluatedMoves.Count < movesToEvaluate.Count)
            {
                for (int i = evaluatedMoves.Count; i < movesToEvaluate.Count; i++)
                {
                    evaluatedMoves.Add(movesToEvaluate[i]);
                }
            }

            return evaluatedMoves;
        }

        private double EvaluateBoardAfterMove(GameBoard gameBoard, Color maxColor, bool maxPlayer, int maxDepth, int depth, double alpha, double beta)
        {
            if (null == gameBoard)
            {
                throw new ArgumentNullException("gameBoard");
            }

            // Leaf, search no more
            if (depth == maxDepth)
            {
                return CalculateBoardScore(gameBoard, maxColor);
            }

            MoveSet validMoves = gameBoard.GetValidMoves();

            // Dead-end, game-over
            if (validMoves.Count == 0)
            {
                return CalculateBoardScore(gameBoard, maxColor);
            }

            double score = maxPlayer ? Double.NegativeInfinity : Double.PositiveInfinity;

            foreach (Move validMove in validMoves)
            {
                gameBoard.TrustedPlay(validMove);

                double scoreAfterMove = EvaluateBoardAfterMove(gameBoard, maxColor, !maxPlayer, maxDepth, depth + 1, alpha, beta);
                
                gameBoard.UndoLastMove();

                if (maxPlayer)
                {
                    score = Math.Max(score, scoreAfterMove);
                    alpha = Math.Max(alpha, score);
                }
                else
                {
                    score = Math.Min(score, scoreAfterMove);
                    beta = Math.Min(beta, score);
                }

                if (AlphaBetaPruning && beta <= alpha)
                {
                    break;
                }
            }

            return score;
        }

        public const int IterativeDepth = -1;
    }
}
