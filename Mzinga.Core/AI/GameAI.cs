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
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _maxDepth = value;
            }
        }
        private int _maxDepth;

        public GameAI(MetricWeights metricWeights, int maxDepth = 0) : base(metricWeights)
        {
            MaxDepth = maxDepth;
        }

        protected override EvaluatedMoveCollection EvaluateMoves(GameBoard gameBoard)
        {
            EvaluatedMoveCollection evaluatedMoves = new EvaluatedMoveCollection();

            MoveSet validMoves = gameBoard.GetValidMoves();

            if (validMoves.Count == 1)
            {
                evaluatedMoves.Add(new EvaluatedMove(validMoves[0], 0));
                return evaluatedMoves;
            }

            Color maxColor = gameBoard.CurrentTurnColor;

            double alpha = Double.NegativeInfinity;
            double beta = Double.PositiveInfinity;

            foreach (Move validMove in validMoves)
            {
                EvaluatedMove evaluatedMove;

                if (MaxDepth == 0)
                {
                    evaluatedMove = new EvaluatedMove(validMove, 0);
                }
                else
                {
                    gameBoard.TrustedPlay(validMove);

                    double scoreAfterMove = EvaluateBoardAfterMove(gameBoard, maxColor, false, 1, alpha, beta);
                    gameBoard.UndoLastMove();

                    evaluatedMove = new EvaluatedMove(validMove, scoreAfterMove);

                    alpha = Math.Max(alpha, scoreAfterMove);
                }

                evaluatedMoves.Add(evaluatedMove);
            }

            return evaluatedMoves;
        }

        private double EvaluateBoardAfterMove(GameBoard gameBoard, Color maxColor, bool maxPlayer, int depth, double alpha, double beta)
        {
            if (null == gameBoard)
            {
                throw new ArgumentNullException("gameBoard");
            }

            // Leaf, search no more
            if (depth == MaxDepth)
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

                double scoreAfterMove = EvaluateBoardAfterMove(gameBoard, maxColor, !maxPlayer, depth + 1, alpha, beta);
                
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

                if (beta <= alpha)
                {
                    break;
                }
            }

            return score;
        }
    }
}
