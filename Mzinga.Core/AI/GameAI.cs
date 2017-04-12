// 
// GameAI.cs
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
using System.Collections.Generic;

namespace Mzinga.Core.AI
{
    public class GameAI
    {
        public MetricWeights MetricWeights { get; private set; }

        public TimeSpan? MaxTime
        {
            get
            {
                return _maxTime;
            }
            set
            {
                if (value.HasValue && value.Value < TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _maxTime = value;
            }
        }
        private TimeSpan? _maxTime;

        public DateTime? StartTime { get; protected set; }

        public TimeSpan? ElapsedTime
        {
            get
            {
                if (StartTime.HasValue)
                {
                    return (DateTime.Now - StartTime.Value);
                }

                return null;
            }
        }

        public bool HasTimeLeft
        {
            get
            {
                if (!MaxTime.HasValue)
                {
                    return true;
                }

                TimeSpan? elapsedTime = ElapsedTime;
                if (elapsedTime.HasValue)
                {
                    return elapsedTime.Value < MaxTime.Value;
                }

                return false;
            }
        }

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

        public bool TranspositionTable { get; set; }

        public BestMoveMetrics BestMoveMetrics
        {
            get
            {
                return _bestMoveMetrics;
            }
            private set
            {
                if (null != value)
                {
                    _bestMoveMetricsHistory.AddLast(value);
                }
                _bestMoveMetrics = value;
            }
        }
        private BestMoveMetrics _bestMoveMetrics = null;

        public IEnumerable<BestMoveMetrics> BestMoveMetricsHistory
        {
            get
            {
                return _bestMoveMetricsHistory;
            }
        }
        private LinkedList<BestMoveMetrics> _bestMoveMetricsHistory;

        private Dictionary<string, double>[] _cachedBoardScores;

        private Random _random;

        public GameAI()
        {
            MetricWeights = new MetricWeights();            

            MaxTime = null;

            MaxDepth = 0;
            AlphaBetaPruning = false;

            TranspositionTable = false;

            ResetCaches();

            _random = new Random();
        }

        public GameAI(MetricWeights metricWeights) : this()
        {
            if (null == metricWeights)
            {
                throw new ArgumentNullException("metricWeights");
            }

            MetricWeights = metricWeights;
        }

        public void ResetCaches()
        {
            _cachedBoardScores = new Dictionary<string, double>[]
            {
                new Dictionary<string, double>(),
                new Dictionary<string, double>()
            };

            BestMoveMetrics = null;
            _bestMoveMetricsHistory = new LinkedList<BestMoveMetrics>();
        }

        #region Move Evaluation

        public Move GetBestMove(GameBoard gameBoard)
        {
            if (null == gameBoard)
            {
                throw new ArgumentNullException("gameBoard");
            }

            StartTime = DateTime.Now;

            BestMoveMetrics = new BestMoveMetrics();

            EvaluatedMoveCollection evaluatedMoves = EvaluateMoves(gameBoard);

            if (evaluatedMoves.Count == 0)
            {
                return null;
            }

            foreach (EvaluatedMove evaluatedMove in evaluatedMoves)
            {
                BestMoveMetrics.IncrementMoves(evaluatedMove.Depth);
            }

            BestMoveMetrics.ElapsedTime = ElapsedTime.Value;

            StartTime = null;

            List<EvaluatedMove> bestMoves = new List<EvaluatedMove>(evaluatedMoves.GetBestMoves());

            int randIndex = _random.Next(bestMoves.Count);
            return bestMoves[randIndex].Move;
        }

        private EvaluatedMoveCollection EvaluateMoves(GameBoard gameBoard)
        {
            if (null == gameBoard)
            {
                throw new ArgumentNullException("gameBoard");
            }

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

            // Non-iterative search
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

            double alpha = double.NegativeInfinity;
            double beta = double.PositiveInfinity;

            foreach (EvaluatedMove moveToEvaluate in movesToEvaluate)
            {
                if (!HasTimeLeft)
                {
                    break;
                }

                gameBoard.TrustedPlay(moveToEvaluate.Move);
                double? scoreAfterMove = EvaluateBoardAfterMove(gameBoard, maxColor, false, maxDepth, 1, alpha, beta);
                gameBoard.UndoLastMove();

                if (!scoreAfterMove.HasValue)
                {
                    // Time-out occurred during evaluation, so don't save it
                    break;
                }

                EvaluatedMove evaluatedMove = new EvaluatedMove(moveToEvaluate.Move, scoreAfterMove.Value, maxDepth);
                evaluatedMoves.Add(evaluatedMove);

                alpha = Math.Max(alpha, scoreAfterMove.Value);

                if (AlphaBetaPruning && beta <= alpha)
                {
                    BestMoveMetrics.AlphaBetaCuts++;
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

        private double? EvaluateBoardAfterMove(GameBoard gameBoard, Color maxColor, bool maxPlayer, int maxDepth, int depth, double alpha, double beta)
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

            double score = maxPlayer ? double.NegativeInfinity : double.PositiveInfinity;

            foreach (Move validMove in validMoves)
            {
                if (!HasTimeLeft)
                {
                    return null;
                }

                gameBoard.TrustedPlay(validMove);
                double? scoreAfterMove = EvaluateBoardAfterMove(gameBoard, maxColor, !maxPlayer, maxDepth, depth + 1, alpha, beta);
                gameBoard.UndoLastMove();

                if (!scoreAfterMove.HasValue)
                {
                    return null;
                }

                if (maxPlayer)
                {
                    score = Math.Max(score, scoreAfterMove.Value);
                    alpha = Math.Max(alpha, score);
                }
                else
                {
                    score = Math.Min(score, scoreAfterMove.Value);
                    beta = Math.Min(beta, score);
                }

                if (AlphaBetaPruning && beta <= alpha)
                {
                    BestMoveMetrics.AlphaBetaCuts++;
                    break;
                }
            }

            return score;
        }

        #endregion

        #region Board Scores

        private double CalculateBoardScore(GameBoard gameBoard, Color maxColor)
        {
            if (null == gameBoard)
            {
                throw new ArgumentNullException("gameBoard");
            }

            if ((maxColor == Color.White && gameBoard.BoardState == BoardState.WhiteWins) ||
                (maxColor == Color.Black && gameBoard.BoardState == BoardState.BlackWins))
            {
                BestMoveMetrics.BoardScoreConstantResults++;
                return double.PositiveInfinity;
            }
            else if ((maxColor == Color.White && gameBoard.BoardState == BoardState.BlackWins) ||
                     (maxColor == Color.Black && gameBoard.BoardState == BoardState.WhiteWins))
            {
                BestMoveMetrics.BoardScoreConstantResults++;
                return double.NegativeInfinity;
            }
            else if (gameBoard.BoardState == BoardState.Draw)
            {
                BestMoveMetrics.BoardScoreConstantResults++;
                return MetricWeights.DrawScore;
            }

            string key = null;

            if (TranspositionTable)
            {
                key = gameBoard.GetTranspositionKey();

                double score;
                if (_cachedBoardScores[(int)maxColor].TryGetValue(key, out score))
                {
                    BestMoveMetrics.TranspositionTableMetrics.Hits++;
                    return score;
                }
            }

            BoardMetrics boardMetrics = gameBoard.GetBoardMetrics();

            double maxScore = CalculateBoardScore(boardMetrics, maxColor);

            if (TranspositionTable)
            {
                _cachedBoardScores[(int)maxColor].Add(key, maxScore);

                Color minColor = (Color)(1 - (int)maxColor);
                double minScore = CalculateBoardScore(boardMetrics, minColor);
                _cachedBoardScores[(int)minColor].Add(key, minScore);
            }

            BestMoveMetrics.TranspositionTableMetrics.Misses++;
            return maxScore;
        }

        private double CalculateBoardScore(BoardMetrics boardMetrics, Color maxColor)
        {
            Color minColor = (Color)(1 - (int)maxColor);

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

        #endregion

        public const int IterativeDepth = -1;
    }
}
