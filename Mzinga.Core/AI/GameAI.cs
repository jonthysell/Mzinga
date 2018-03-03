// 
// GameAI.cs
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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

        public event BestMoveFoundEventHandler BestMoveFound;

        private TranspositionTable _transpositionTable;

        private FixedCache<long, double> _cachedBoardScores = new FixedCache<long, double>(DefaultBoardScoresCacheSize);
        private const int DefaultBoardScoresCacheSize = 516240; // perft(5)

        private const int QuiescentSearchMaxDepth = 12; // To prevent runaway stack overflows

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

        public Move GetBestMove(GameBoard gameBoard, int maxDepth, int maxHelperThreads)
        {
            return GetBestMove(gameBoard, maxDepth, TimeSpan.MaxValue, maxHelperThreads);
        }

        public Move GetBestMove(GameBoard gameBoard, TimeSpan maxTime, int maxHelperThreads)
        {
            return GetBestMove(gameBoard, int.MaxValue, maxTime, maxHelperThreads);
        }

        private Move GetBestMove(GameBoard gameBoard, int maxDepth, TimeSpan maxTime, int maxHelperThreads)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            if (maxTime < TimeSpan.MaxValue)
            {
                cts.CancelAfter(maxTime);
            }

            Task<Move> task = GetBestMoveAsync(gameBoard, maxDepth, maxTime, maxHelperThreads, cts.Token);
            task.Wait();

            return task.Result;
        }

        public async Task<Move> GetBestMoveAsync(GameBoard gameBoard, int maxHelperThreads, CancellationToken token)
        {
            return await GetBestMoveAsync(gameBoard, int.MaxValue, TimeSpan.MaxValue, maxHelperThreads, token);
        }

        public async Task<Move> GetBestMoveAsync(GameBoard gameBoard, int maxDepth, int maxHelperThreads, CancellationToken token)
        {
            return await GetBestMoveAsync(gameBoard, maxDepth, TimeSpan.MaxValue, maxHelperThreads, token);
        }

        public async Task<Move> GetBestMoveAsync(GameBoard gameBoard, TimeSpan maxTime, int maxHelperThreads, CancellationToken token)
        {
            return await GetBestMoveAsync(gameBoard, int.MaxValue, maxTime, maxHelperThreads, token);
        }

        private async Task<Move> GetBestMoveAsync(GameBoard gameBoard, int maxDepth, TimeSpan maxTime, int maxHelperThreads, CancellationToken token)
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

            if (maxHelperThreads < 0)
            {
                throw new ArgumentOutOfRangeException("maxHelperThreads");
            }

            if (gameBoard.GameIsOver)
            {
                throw new Exception("Game is over.");
            }

            BestMoveMetrics = new BestMoveMetrics(maxDepth, maxTime, maxHelperThreads);

            BestMoveMetrics.Start();

            EvaluatedMoveCollection evaluatedMoves = await EvaluateMovesAsync(gameBoard, token);

            if (evaluatedMoves.Count == 0)
            {
                return null;
            }

            foreach (EvaluatedMove evaluatedMove in evaluatedMoves)
            {
                BestMoveMetrics.IncrementMoves(evaluatedMove.Depth);
            }

            BestMoveMetrics.End();

            // Make sure at least one move is reported
            OnBestMoveFound(evaluatedMoves.BestMove);

            return evaluatedMoves.BestMove.Move;
        }

        private async Task<EvaluatedMoveCollection> EvaluateMovesAsync(GameBoard gameBoard, CancellationToken token)
        {
            MoveSet validMoves = gameBoard.GetValidMoves();

            EvaluatedMoveCollection movesToEvaluate = new EvaluatedMoveCollection();

            foreach (Move move in validMoves)
            {
                movesToEvaluate.Add(new EvaluatedMove(move));
            }

            // Try to get cached best move if available
            long key = gameBoard.ZobristKey;
            TranspositionTableEntry tEntry;
            if (_transpositionTable.TryLookup(key, out tEntry) && null != tEntry.BestMove)
            {
                movesToEvaluate.Update(new EvaluatedMove(tEntry.BestMove, tEntry.Value, tEntry.Depth));
                OnBestMoveFound(movesToEvaluate.BestMove);

                if (movesToEvaluate.BestMove.ScoreAfterMove == double.PositiveInfinity || movesToEvaluate.BestMove.ScoreAfterMove == double.NegativeInfinity)
                {
                    // The best move ends the game, stop searching
                    return movesToEvaluate;
                }
            }

            if (movesToEvaluate.Count <= 1 || BestMoveMetrics.MaxSearchDepth == 0)
            {
                // No need to search
                return movesToEvaluate;
            }

            // Iterative search
            int depth = 1 + Math.Max(0, movesToEvaluate.BestMove.Depth);
            while (depth <= BestMoveMetrics.MaxSearchDepth)
            {
                // Start LazySMP helper threads
                CancellationTokenSource helperCTS = new CancellationTokenSource();
                Task[] helperThreads = StartHelperThreads(gameBoard, depth, BestMoveMetrics.MaxHelperThreads, helperCTS);

                // "Re-sort" moves to evaluate based on the next iteration
                movesToEvaluate = await EvaluateMovesToDepthAsync(gameBoard, depth, movesToEvaluate, token);

                // End LazySMP helper threads
                EndHelperThreads(helperThreads, helperCTS);

                // Fire BestMoveFound for current depth
                OnBestMoveFound(movesToEvaluate.BestMove);

                if (movesToEvaluate.BestMove.ScoreAfterMove == double.PositiveInfinity || movesToEvaluate.BestMove.ScoreAfterMove == double.NegativeInfinity)
                {
                    // The best move ends the game, stop searching
                    break;
                }

                if (token.IsCancellationRequested)
                {
                    // Cancelled, stop searching
                    break;
                }

                depth = 1 + Math.Max(depth, movesToEvaluate.BestMove.Depth);
            }

            return movesToEvaluate;
        }

        private async Task<EvaluatedMoveCollection> EvaluateMovesToDepthAsync(GameBoard gameBoard, int depth, IEnumerable<EvaluatedMove> movesToEvaluate, CancellationToken token)
        {
            double alpha = double.NegativeInfinity;
            double beta = double.PositiveInfinity;

            int color = gameBoard.CurrentTurnColor == Color.White ? 1 : -1;

            double alphaOriginal = alpha;

            double bestValue = double.NegativeInfinity;

            EvaluatedMoveCollection evaluatedMoves = new EvaluatedMoveCollection();

            foreach (EvaluatedMove moveToEvaluate in movesToEvaluate)
            {
                if (token.IsCancellationRequested)
                {
                    // Cancel
                    return new EvaluatedMoveCollection(movesToEvaluate);
                }

                gameBoard.TrustedPlay(moveToEvaluate.Move);
                double? value = -1 * await NegaMaxSearchAsync(gameBoard, depth - 1, -beta, -alpha, -color, token);
                gameBoard.UndoLastMove();

                if (!value.HasValue)
                {
                    // Cancel occurred during evaluation
                    return new EvaluatedMoveCollection(movesToEvaluate);
                }

                EvaluatedMove evaluatedMove = new EvaluatedMove(moveToEvaluate.Move, value.Value, depth);
                evaluatedMoves.Add(evaluatedMove);

                bestValue = Math.Max(bestValue, value.Value);
                alpha = Math.Max(alpha, value.Value);

                if (alpha >= beta)
                {
                    // A winning move has been found, since beta is always infinity in this function
                    break;
                }
            }

            long key = gameBoard.ZobristKey;

            TranspositionTableEntry tEntry = new TranspositionTableEntry();

            if (bestValue <= alphaOriginal)
            {
                tEntry.Type = TranspositionTableEntryType.UpperBound;
            }
            else
            {
                tEntry.Type = bestValue >= beta ? TranspositionTableEntryType.LowerBound : TranspositionTableEntryType.Exact;
                tEntry.BestMove = evaluatedMoves.BestMove.Move;
            }

            tEntry.Value = bestValue;
            tEntry.Depth = depth;

            _transpositionTable.Store(key, tEntry);

            return evaluatedMoves;
        }

        private void OnBestMoveFound(EvaluatedMove evaluatedMove)
        {
            if (null != BestMoveFound && evaluatedMove != BestMoveMetrics.BestMove)
            {
                BestMoveFoundEventArgs args = new BestMoveFoundEventArgs(evaluatedMove.Move, evaluatedMove.Depth, evaluatedMove.ScoreAfterMove);
                BestMoveFound.Invoke(this, args);
            }
            BestMoveMetrics.BestMove = evaluatedMove;
        }

        #endregion

        #region Threading support

        private Task[] StartHelperThreads(GameBoard gameBoard, int depth, int threads, CancellationTokenSource tokenSource)
        {
            Task[] helperThreads = null;

            if (depth > 1 && threads > 0)
            {
                helperThreads = new Task[threads];
                int color = gameBoard.CurrentTurnColor == Color.White ? 1 : -1;

                for (int i = 0; i < helperThreads.Length; i++)
                {
                    GameBoard clone = gameBoard.Clone();
                    helperThreads[i] = Task.Factory.StartNew(async () =>
                    {
                        await NegaMaxSearchAsync(clone, depth + i % 2, double.NegativeInfinity, double.PositiveInfinity, color, tokenSource.Token);
                    });
                }
            }

            return helperThreads;
        }

        private void EndHelperThreads(Task[] helperThreads, CancellationTokenSource tokenSource)
        {
            if (null != helperThreads)
            {
                tokenSource.Cancel();
                Task.WaitAll(helperThreads);
            }
        }

        #endregion

        #region NegaMax Search

        private async Task<double?> NegaMaxSearchAsync(GameBoard gameBoard, int depth, double alpha, double beta, int color, CancellationToken token)
        {
            double alphaOriginal = alpha;

            long key = gameBoard.ZobristKey;

            TranspositionTableEntry tEntry;
            if (_transpositionTable.TryLookup(key, out tEntry) && tEntry.Depth >= depth)
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

            if (depth == 0 || gameBoard.GameIsOver)
            {
                return await QuiescenceSearchAsync(gameBoard, QuiescentSearchMaxDepth, alpha, beta, color, token);
            }

            double? bestValue = null;
            Move bestMove = tEntry?.BestMove;

            List<Move> moves = new List<Move>(gameBoard.GetValidMoves().Shuffle());

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
                if (token.IsCancellationRequested)
                {
                    return null;
                }

                gameBoard.TrustedPlay(move);
                double? value = -1 * await NegaMaxSearchAsync(gameBoard, depth - 1, -beta, -alpha, -color, token);
                gameBoard.UndoLastMove();

                if (!value.HasValue)
                {
                    return null;
                }

                if (!bestValue.HasValue || value >= bestValue)
                {
                    bestValue = value;
                    bestMove = move;
                }

                alpha = Math.Max(alpha, bestValue.Value);

                if (alpha >= beta)
                {
                    break;
                }
            }

            if (bestValue.HasValue)
            {
                tEntry = new TranspositionTableEntry();

                if (bestValue <= alphaOriginal)
                {
                    tEntry.Type = TranspositionTableEntryType.UpperBound;
                }
                else
                {
                    tEntry.Type = bestValue >= beta ? TranspositionTableEntryType.LowerBound : TranspositionTableEntryType.Exact;
                    tEntry.BestMove = bestMove;
                }

                tEntry.Value = bestValue.Value;
                tEntry.Depth = depth;

                _transpositionTable.Store(key, tEntry);
            }

            return bestValue;
        }

        #endregion

        #region QuiescenceSearch

        private async Task<double?> QuiescenceSearchAsync(GameBoard gameBoard, int depth, double alpha, double beta, int color, CancellationToken token)
        {
            double bestValue = color * CalculateBoardScore(gameBoard);

            alpha = Math.Max(alpha, bestValue);

            if (alpha >= beta || depth == 0 || gameBoard.GameIsOver)
            {
                return bestValue;
            }

            foreach (Move move in gameBoard.GetValidMoves())
            {
                if (gameBoard.IsNoisyMove(move))
                {
                    if (token.IsCancellationRequested)
                    {
                        return null;
                    }

                    gameBoard.TrustedPlay(move);
                    double? value = -1 * await QuiescenceSearchAsync(gameBoard, depth - 1, -beta, -alpha, -color, token);
                    gameBoard.UndoLastMove();

                    if (!value.HasValue)
                    {
                        return null;
                    }

                    bestValue = Math.Max(bestValue, value.Value);

                    alpha = Math.Max(alpha, bestValue);

                    if (alpha >= beta)
                    {
                        break;
                    }
                }
            }

            return bestValue;
        }

        #endregion

        #region Board Scores

        private double CalculateBoardScore(GameBoard gameBoard)
        {
            // Always score from white's point of view

            if (gameBoard.BoardState == BoardState.WhiteWins)
            {
                return double.PositiveInfinity;
            }
            else if (gameBoard.BoardState == BoardState.BlackWins)
            {
                return double.NegativeInfinity;
            }
            else if (gameBoard.BoardState == BoardState.Draw)
            {
                return 0.0;
            }

            long key = gameBoard.ZobristKey;

            double score;
            if (_cachedBoardScores.TryLookup(key, out score))
            {
                return score;
            }

            BoardMetrics boardMetrics = gameBoard.GetBoardMetrics();

            score = CalculateBoardScore(boardMetrics);

            _cachedBoardScores.Store(key, score);

            return score;
        }

        private double CalculateBoardScore(BoardMetrics boardMetrics)
        {
            double score = 0;

            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                BugType bugType = EnumUtils.GetBugType(pieceName);

                double colorValue = EnumUtils.GetColor(pieceName) == Color.White ? 1.0 : -1.0;

                score += colorValue * MetricWeights.Get(bugType, BugTypeWeight.InPlayWeight) * boardMetrics[pieceName].InPlay;
                score += colorValue * MetricWeights.Get(bugType, BugTypeWeight.IsPinnedWeight) * boardMetrics[pieceName].IsPinned;
                score += colorValue * MetricWeights.Get(bugType, BugTypeWeight.IsCoveredWeight) * boardMetrics[pieceName].IsCovered;
                score += colorValue * MetricWeights.Get(bugType, BugTypeWeight.NoisyMoveWeight) * boardMetrics[pieceName].NoisyMoveCount;
                score += colorValue * MetricWeights.Get(bugType, BugTypeWeight.QuietMoveWeight) * boardMetrics[pieceName].QuietMoveCount;
                score += colorValue * MetricWeights.Get(bugType, BugTypeWeight.FriendlyNeighborWeight) * boardMetrics[pieceName].FriendlyNeighborCount;
                score += colorValue * MetricWeights.Get(bugType, BugTypeWeight.EnemyNeighborWeight) * boardMetrics[pieceName].EnemyNeighborCount;
            }

            return score;
        }

        #endregion
    }

    public delegate void BestMoveFoundEventHandler(object sender, BestMoveFoundEventArgs args);

    public class BestMoveFoundEventArgs : EventArgs
    {
        public Move Move { get; private set; }
        public int Depth { get; private set; }
        public double Score { get; private set; }

        public BestMoveFoundEventArgs(Move move, int depth, double score)
        {
            Move = move;
            Depth = depth;
            Score = score;
        }
    }
}
