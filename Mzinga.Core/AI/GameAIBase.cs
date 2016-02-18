// 
// GameAIBase.cs
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
    public abstract class GameAIBase : IGameAI
    {
        public MetricWeights MetricWeights { get; private set; }

        public TimeSpan MaxTime
        {
            get
            {
                return _maxTime;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _maxTime = value;
            }
        }
        private TimeSpan _maxTime;

        public bool HasTimeLeft
        {
            get
            {
                if (StartTime.HasValue)
                {
                    return (DateTime.Now - StartTime.Value) < MaxTime;
                }

                return false;
            }
        }

        public DateTime? StartTime { get; protected set; }

        protected Random Random;

        public GameAIBase()
        {
            MetricWeights = new MetricWeights();
            Random = new Random();
            MaxTime = TimeSpan.Zero;
        }

        public Move GetBestMove(GameBoard gameBoard)
        {
            if (null == gameBoard)
            {
                throw new ArgumentNullException("gameBoard");
            }

            EvaluatedMoveCollection evaluatedMoves = EvaluateMoves(gameBoard);

            if (evaluatedMoves.Count == 0)
            {
                return null;
            }

            List<EvaluatedMove> bestMoves = new List<EvaluatedMove>(evaluatedMoves.GetBestMoves());

            int randIndex = Random.Next(bestMoves.Count);
            return bestMoves[randIndex].Move;
        }

        protected abstract EvaluatedMoveCollection EvaluateMoves(GameBoard gameBoard);

        protected double CalculateBoardScore(GameBoard gameBoard, Color maxColor)
        {
            if (null == gameBoard)
            {
                throw new ArgumentNullException("gameBoard");
            }

            Color minColor = (Color)(1 - (int)maxColor);

            if ((maxColor == Color.White && gameBoard.BoardState == BoardState.WhiteWins) ||
                (maxColor == Color.Black && gameBoard.BoardState == BoardState.BlackWins))
            {
                return Double.PositiveInfinity;
            }
            else if ((maxColor == Color.White && gameBoard.BoardState == BoardState.BlackWins) ||
                     (maxColor == Color.Black && gameBoard.BoardState == BoardState.WhiteWins))
            {
                return Double.NegativeInfinity;
            }
            else if (gameBoard.BoardState == BoardState.Draw)
            {
                return MetricWeights.DrawScore;
            }

            BoardMetrics boardMetrics = gameBoard.GetBoardMetrics();

            double score = 0;

            // Add max player scores
            score += MetricWeights.Get(Player.Maximizing, PlayerWeight.ValidMoveWeight) * boardMetrics[maxColor].ValidMoveCount;
            score += MetricWeights.Get(Player.Maximizing, PlayerWeight.ValidPlacementWeight) * boardMetrics[maxColor].ValidPlacementCount;
            score += MetricWeights.Get(Player.Maximizing, PlayerWeight.ValidMovementWeight) * boardMetrics[maxColor].ValidMovementCount;

            score += MetricWeights.Get(Player.Maximizing, PlayerWeight.InHandWeight) * boardMetrics[maxColor].PiecesInHandCount;
            score += MetricWeights.Get(Player.Maximizing, PlayerWeight.InPlayWeight) * boardMetrics[maxColor].PiecesInPlayCount;

            score += MetricWeights.Get(Player.Maximizing, PlayerWeight.IsPinnedWeight) * boardMetrics[maxColor].PiecesPinnedCount;

            // Add min player scores
            score += MetricWeights.Get(Player.Minimizing, PlayerWeight.ValidMoveWeight) * boardMetrics[minColor].ValidMoveCount;
            score += MetricWeights.Get(Player.Minimizing, PlayerWeight.ValidPlacementWeight) * boardMetrics[minColor].ValidPlacementCount;
            score += MetricWeights.Get(Player.Minimizing, PlayerWeight.ValidMovementWeight) * boardMetrics[minColor].ValidMovementCount;

            score += MetricWeights.Get(Player.Minimizing, PlayerWeight.InHandWeight) * boardMetrics[minColor].PiecesInHandCount;
            score += MetricWeights.Get(Player.Minimizing, PlayerWeight.InPlayWeight) * boardMetrics[minColor].PiecesInPlayCount;

            score += MetricWeights.Get(Player.Minimizing, PlayerWeight.IsPinnedWeight) * boardMetrics[minColor].PiecesPinnedCount;

            // Add max player piece scores
            foreach (PieceName pieceName in boardMetrics[maxColor].PieceNames)
            {
                BugType bugType = EnumUtils.GetBugType(pieceName);
                score += CalculatePieceScore(Player.Maximizing, boardMetrics[maxColor][pieceName]);
            }

            // Add min player piece scores
            foreach (PieceName pieceName in boardMetrics[minColor].PieceNames)
            {
                BugType bugType = EnumUtils.GetBugType(pieceName);
                score += CalculatePieceScore(Player.Minimizing, boardMetrics[minColor][pieceName]);
            }

            return score;
        }

        private double CalculatePieceScore(Player player, PieceMetrics metrics)
        {
            BugType bugType = EnumUtils.GetBugType(metrics.PieceName);

            double score = 0.0;

            score += MetricWeights.Get(player, bugType, BugTypeWeight.ValidMoveWeight) * metrics.ValidMoveCount;
            score += MetricWeights.Get(player, bugType, BugTypeWeight.ValidPlacementWeight) * metrics.ValidPlacementCount;
            score += MetricWeights.Get(player, bugType, BugTypeWeight.ValidMovementWeight) * metrics.ValidMovementCount;

            score += MetricWeights.Get(player, bugType, BugTypeWeight.NeighborWeight) * metrics.NeighborCount;

            score += MetricWeights.Get(player, bugType, BugTypeWeight.InHandWeight) * metrics.InHand;
            score += MetricWeights.Get(player, bugType, BugTypeWeight.InPlayWeight) * metrics.InPlay;

            score += MetricWeights.Get(player, bugType, BugTypeWeight.IsPinnedWeight) * metrics.IsPinned;

            return score;
        }
    }
}
