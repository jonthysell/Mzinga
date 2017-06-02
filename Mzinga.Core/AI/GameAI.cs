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
        public MetricWeights MetricWeights { get; private set; } = new MetricWeights();

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
        private LinkedList<BestMoveMetrics> _bestMoveMetricsHistory = new LinkedList<BestMoveMetrics>();

        private TranspositionTable _transpositionTable;

        private FixedCache<string, double> _cachedBoardScores = new FixedCache<string, double>(DefaultBoardScoresCacheSize);
        private const int DefaultBoardScoresCacheSize = 32768;

        public GameAI()
        {
            _transpositionTable = new TranspositionTable();
        }

        public GameAI(MetricWeights metricWeights)
        {
            if (null == metricWeights)
            {
                throw new ArgumentNullException("metricWeights");
            }

            MetricWeights.CopyFrom(metricWeights);
            _transpositionTable = new TranspositionTable();
        }

        public GameAI(int transpositionTableSizeMB)
        {
            if (transpositionTableSizeMB <= 0)
            {
                throw new ArgumentOutOfRangeException("transpositionTableSizeMB");
            }

            _transpositionTable = new TranspositionTable(transpositionTableSizeMB * 1024 * 1024);
        }

        public GameAI(MetricWeights metricWeights, int transpositionTableSizeMB)
        {
            if (null == metricWeights)
            {
                throw new ArgumentNullException("metricWeights");
            }
            
            if (transpositionTableSizeMB <= 0)
            {
                throw new ArgumentOutOfRangeException("transpositionTableSizeMB");
            }

            MetricWeights.CopyFrom(metricWeights);
            _transpositionTable = new TranspositionTable(transpositionTableSizeMB * 1024 * 1024);
        }

        public void ResetCaches()
        {
            BestMoveMetrics = null;
            _bestMoveMetricsHistory.Clear();
            _transpositionTable.Clear();
        }

        #region Move Evaluation

        public Move GetBestMove(GameBoard gameBoard, int maxDepth)
        {
            return GetBestMove(gameBoard, maxDepth, TimeSpan.MaxValue);
        }

        public Move GetBestMove(GameBoard gameBoard, TimeSpan maxTime)
        {
            return GetBestMove(gameBoard, int.MaxValue, maxTime);
        }

        private Move GetBestMove(GameBoard gameBoard, int maxDepth, TimeSpan maxTime)
        {
            if (null == gameBoard)
            {
                throw new ArgumentNullException("gameBoard");
            }

            if (maxDepth < 0)
            {
                throw new ArgumentOutOfRangeException("maxDepth");
            }

            if (maxTime < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException("maxTime");
            }

            BestMoveMetrics = new BestMoveMetrics(maxDepth, maxTime);

            BestMoveMetrics.Start();

            EvaluatedMoveCollection evaluatedMoves = EvaluateMoves(gameBoard);

            if (evaluatedMoves.Count == 0)
            {
                return null;
            }

            foreach (EvaluatedMove evaluatedMove in evaluatedMoves)
            {
                BestMoveMetrics.IncrementMoves(evaluatedMove.Depth);
            }

            BestMoveMetrics.End();

            return evaluatedMoves.BestMove.Move;
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

            if (movesToEvaluate.Count <= 1 || BestMoveMetrics.MaxSearchDepth == 0)
            {
                // No choices, don't bother evaluating
                return movesToEvaluate;
            }

            // Iterative search
            int depth = 1;
            while (depth <= BestMoveMetrics.MaxSearchDepth)
            {
                // "Re-sort" moves to evaluate based on the next iteration
                movesToEvaluate = EvaluateMovesToDepth(gameBoard, depth, movesToEvaluate);

                if (movesToEvaluate.BestMove.ScoreAfterMove == double.PositiveInfinity || movesToEvaluate.BestMove.ScoreAfterMove == double.NegativeInfinity)
                {
                    // The best move ends the game, stop searching
                    break;
                }

                if (!BestMoveMetrics.HasTimeLeft)
                {
                    // Out of time, stop searching
                    break;
                }

                depth = 1 + Math.Max(depth, movesToEvaluate.BestMove.Depth);
            }

            return movesToEvaluate;
        }

        private EvaluatedMoveCollection EvaluateMovesToDepth(GameBoard gameBoard, int depth, EvaluatedMoveCollection movesToEvaluate)
        {
            if (null == gameBoard)
            {
                throw new ArgumentNullException("gameBoard");
            }

            if (depth <= 0)
            {
                throw new ArgumentOutOfRangeException("depth");
            }

            if (null == movesToEvaluate || movesToEvaluate.Count == 0)
            {
                throw new ArgumentNullException("movesToEvaluate");
            }

            double alpha = double.NegativeInfinity;
            double beta = double.PositiveInfinity;

            int color = gameBoard.CurrentTurnColor == Color.White ? 1 : -1;

            double alphaOriginal = alpha;

            string key = GetColoredTranspositionKey(gameBoard);

            TranspositionTableEntry tEntry;
            if (!_transpositionTable.TryLookup(key, out tEntry))
            {
                BestMoveMetrics.TranspositionTableMetrics.Misses++;
            }
            else
            {
                BestMoveMetrics.TranspositionTableMetrics.Hits++;

                if (tEntry.Depth >= depth)
                {
                    if (tEntry.Type == TranspositionTableEntryType.LowerBound)
                    {
                        alpha = Math.Max(alpha, tEntry.Value);
                    }
                    else if (tEntry.Type == TranspositionTableEntryType.UpperBound)
                    {
                        beta = Math.Min(beta, tEntry.Value);
                    }

                    if (tEntry.Type == TranspositionTableEntryType.Exact || alpha >= beta)
                    {
                        if (null != tEntry.BestMove)
                        {
                            // This should only be hit once by EvaluateMoves since it now skips calling the next depths
                            movesToEvaluate.Update(new EvaluatedMove(new Move(tEntry.BestMove), tEntry.Value, tEntry.Depth));
                            return movesToEvaluate;
                        }
                    }
                }
            }

            EvaluatedMoveCollection evaluatedMoves = new EvaluatedMoveCollection();

            double bestValue = double.NegativeInfinity;

            foreach (EvaluatedMove moveToEvaluate in movesToEvaluate)
            {
                if (!BestMoveMetrics.HasTimeLeft)
                {
                    // Time-out
                    return movesToEvaluate;
                }

                gameBoard.TrustedPlay(moveToEvaluate.Move);
                double? value = -1 * NegaMaxSearch(gameBoard, depth - 1, -beta, -alpha, -color);
                gameBoard.UndoLastMove();

                if (!value.HasValue)
                {
                    // Time-out occurred during evaluation
                    return movesToEvaluate;
                }

                EvaluatedMove evaluatedMove = new EvaluatedMove(moveToEvaluate.Move, value.Value, depth);
                evaluatedMoves.Add(evaluatedMove);

                bestValue = Math.Max(bestValue, value.Value);
                alpha = Math.Max(alpha, value.Value);

                if (alpha >= beta)
                {
                    BestMoveMetrics.AlphaBetaCuts++;
                    break;
                }
            }

            tEntry = new TranspositionTableEntry();

            if (bestValue <= alphaOriginal)
            {
                tEntry.Type = TranspositionTableEntryType.UpperBound;
            }
            else
            {
                tEntry.Type = bestValue >= beta ? TranspositionTableEntryType.LowerBound : TranspositionTableEntryType.Exact;
                tEntry.BestMove = evaluatedMoves.BestMove.Move.ToString();
            }

            tEntry.Value = bestValue;
            tEntry.Depth = depth;

            _transpositionTable.Store(key, tEntry);

            return evaluatedMoves;
        }

        private double? NegaMaxSearch(GameBoard gameBoard, int depth, double alpha, double beta, int color)
        {
            double alphaOriginal = alpha;

            string key = GetColoredTranspositionKey(gameBoard);

            TranspositionTableEntry tEntry;
            if (!_transpositionTable.TryLookup(key, out tEntry))
            {
                BestMoveMetrics.TranspositionTableMetrics.Misses++;
            }
            else
            {
                BestMoveMetrics.TranspositionTableMetrics.Hits++;

                if (tEntry.Depth >= depth)
                {
                    if (tEntry.Type == TranspositionTableEntryType.Exact)
                    {
                        return tEntry.Value;
                    }
                    else if (tEntry.Type == TranspositionTableEntryType.LowerBound)
                    {
                        alpha = Math.Max(alpha, tEntry.Value);
                    }
                    else if (tEntry.Type == TranspositionTableEntryType.UpperBound)
                    {
                        beta = Math.Min(beta, tEntry.Value);
                    }

                    if (alpha >= beta)
                    {
                        return tEntry.Value;
                    }
                }
            }

            if (depth == 0 || gameBoard.GameIsOver)
            {
                return color * CalculateBoardScore(gameBoard);
            }

            double bestValue = double.NegativeInfinity;
            Move bestMove = null != tEntry?.BestMove ? new Move(tEntry.BestMove) : null;

            List<Move> moves = new List<Move>(gameBoard.GetValidMoves());

            if (null != bestMove)
            {
                // Put the best move from a previous search first
                int bestIndex = moves.IndexOf(bestMove);
                if (bestIndex > 0)
                {
                    moves[bestIndex] = moves[0];
                    moves[0] = bestMove;
                }
            }

            foreach (Move move in moves)
            {
                if (!BestMoveMetrics.HasTimeLeft)
                {
                    return null;
                }

                gameBoard.TrustedPlay(move);
                double? value = -1 * NegaMaxSearch(gameBoard, depth - 1, -beta, -alpha, -color);
                gameBoard.UndoLastMove();

                if (!value.HasValue)
                {
                    return null;
                }

                if (value >= bestValue)
                {
                    bestValue = value.Value;
                    bestMove = move;
                }

                alpha = Math.Max(alpha, bestValue);

                if (alpha >= beta)
                {
                    BestMoveMetrics.AlphaBetaCuts++;
                    break;
                }
            }

            tEntry = new TranspositionTableEntry();

            if (bestValue <= alphaOriginal)
            {
                tEntry.Type = TranspositionTableEntryType.UpperBound;
            }
            else
            {
                tEntry.Type = bestValue >= beta ? TranspositionTableEntryType.LowerBound : TranspositionTableEntryType.Exact;
                tEntry.BestMove = bestMove?.ToString();
            }

            tEntry.Value = bestValue;
            tEntry.Depth = depth;

            _transpositionTable.Store(key, tEntry);

            return bestValue;
        }

        #endregion

        #region Board Scores

        private double CalculateBoardScore(GameBoard gameBoard)
        {
            if (null == gameBoard)
            {
                throw new ArgumentNullException("gameBoard");
            }

            if (gameBoard.BoardState == BoardState.WhiteWins)
            {
                BestMoveMetrics.BoardScoreConstantResults++;
                return double.PositiveInfinity;
            }
            else if (gameBoard.BoardState == BoardState.BlackWins)
            {
                BestMoveMetrics.BoardScoreConstantResults++;
                return double.NegativeInfinity;
            }
            else if (gameBoard.BoardState == BoardState.Draw)
            {
                BestMoveMetrics.BoardScoreConstantResults++;
                return MetricWeights.DrawScore;
            }

            string key = gameBoard.TranspositionKey;

            double score;
            if (_cachedBoardScores.TryLookup(key, out score))
            {
                return score;
            }

            BoardMetrics boardMetrics = gameBoard.GetBoardMetrics();

            score = (boardMetrics[Color.White].ValidMoveCount - boardMetrics[Color.Black].ValidMoveCount);

            //score = CalculateBoardScore(boardMetrics, maxColor);
            BestMoveMetrics.BoardScoreCalculatedResults++;

            _cachedBoardScores.Store(key, score);

            return score;
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

        private string GetColoredTranspositionKey(GameBoard gameBoard)
        {
            return string.Format("{0};{1}", gameBoard.CurrentTurnColor.ToString()[0], gameBoard.TranspositionKey);
        }
    }
}
