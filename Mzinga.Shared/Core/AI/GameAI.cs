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
        public event EventHandler<BestMoveFoundEventArgs> BestMoveFound;

        public MetricWeights StartMetricWeights { get; private set; }
        public MetricWeights EndMetricWeights { get; private set; }

        private int _maxBranchingFactor = MaxMaxBranchingFactor; // To prevent search explosion

        public const int MaxMaxBranchingFactor = 500;

        private TranspositionTable _transpositionTable;

        private FixedCache<ulong, double> _cachedBoardScores = new FixedCache<ulong, double>(DefaultBoardScoresCacheSize);
        private const int DefaultBoardScoresCacheSize = 516240; // perft(5)

        private const int QuiescentSearchMaxDepth = 12; // To prevent runaway stack overflows

        public GameAI()
        {
            StartMetricWeights = new MetricWeights();
            EndMetricWeights = new MetricWeights();

            _transpositionTable = new TranspositionTable();
        }

        public GameAI(GameAIConfig config)
        {
            if (null == config)
            {
                throw new ArgumentNullException("config");
            }

            StartMetricWeights = config.StartMetricWeights?.Clone() ?? new MetricWeights();
            EndMetricWeights = config.EndMetricWeights?.Clone() ?? new MetricWeights();

            if (config.TranspositionTableSizeMB.HasValue)
            {
                if (config.TranspositionTableSizeMB.Value <= 0)
                {
                    throw new ArgumentOutOfRangeException("config.TranspositionTableSizeMB");
                }

                _transpositionTable = new TranspositionTable(config.TranspositionTableSizeMB.Value * 1024 * 1024);
            }
            else
            {
                _transpositionTable = new TranspositionTable();
            }

            if (config.MaxBranchingFactor.HasValue)
            {
                if (config.MaxBranchingFactor.Value <= 0)
                {
                    throw new ArgumentOutOfRangeException("config.MaxBranchingFactor");
                }

                _maxBranchingFactor = config.MaxBranchingFactor.Value;
            }
        }

        public void ResetCaches()
        {
            _transpositionTable.Clear();
            _cachedBoardScores.Clear();
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

            BestMoveParams bestMoveParams = new BestMoveParams()
            {
                MaxSearchDepth = maxDepth,
                MaxSearchTime = maxTime,
                MaxHelperThreads = maxHelperThreads,
            };

            EvaluatedMoveCollection evaluatedMoves = await EvaluateMovesAsync(gameBoard, bestMoveParams, token);

            if (evaluatedMoves.Count == 0)
            {
                throw new Exception("No moves after evaluation!");
            }

            // Make sure at least one move is reported
            OnBestMoveFound(bestMoveParams, evaluatedMoves.BestMove);

            return bestMoveParams.BestMove.Move;
        }

        private async Task<EvaluatedMoveCollection> EvaluateMovesAsync(GameBoard gameBoard, BestMoveParams bestMoveParams, CancellationToken token)
        {
            EvaluatedMoveCollection movesToEvaluate = new EvaluatedMoveCollection();

            EvaluatedMove bestMove = null;

            // Try to get cached best move if available
            ulong key = gameBoard.ZobristKey;
            TranspositionTableEntry tEntry;
            if (_transpositionTable.TryLookup(key, out tEntry) && null != tEntry.BestMove)
            {
                bestMove = new EvaluatedMove(tEntry.BestMove, tEntry.Value, tEntry.Depth);
                OnBestMoveFound(bestMoveParams, bestMove);
            }

            if (null != bestMove && double.IsPositiveInfinity(bestMove.ScoreAfterMove))
            {
                // Winning move, don't search
                movesToEvaluate.Add(bestMove);
                return movesToEvaluate;
            }

            List<EvaluatedMove> validMoves = GetPreSortedValidMoves(gameBoard, bestMove);
            movesToEvaluate.Add(validMoves, false);

            if (movesToEvaluate.Count <= 1 || bestMoveParams.MaxSearchDepth == 0)
            {
                // No need to search
                return movesToEvaluate;
            }

            // Iterative search
            int depth = 1 + Math.Max(0, movesToEvaluate.BestMove.Depth);
            while (depth <= bestMoveParams.MaxSearchDepth)
            {
                // Start LazySMP helper threads
                CancellationTokenSource helperCTS = new CancellationTokenSource();
                Task[] helperThreads = StartHelperThreads(gameBoard, depth, bestMoveParams.MaxHelperThreads, helperCTS);

                // "Re-sort" moves to evaluate based on the next iteration
                movesToEvaluate = await EvaluateMovesToDepthAsync(gameBoard, depth, movesToEvaluate, token);

                // End LazySMP helper threads
                EndHelperThreads(helperThreads, helperCTS);

                // Fire BestMoveFound for current depth
                OnBestMoveFound(bestMoveParams, movesToEvaluate.BestMove);

                if (double.IsInfinity(movesToEvaluate.BestMove.ScoreAfterMove))
                {
                    // The best move ends the game, stop searching
                    break;
                }

                // Prune game-losing moves if possible
                movesToEvaluate.PruneGameLosingMoves();

                if (movesToEvaluate.Count <= 1)
                {
                    // Only one move, no reason to keep looking
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
                    return new EvaluatedMoveCollection(movesToEvaluate, false);
                }

                gameBoard.TrustedPlay(moveToEvaluate.Move);

                double? value = -1 * await PrincipalVariationSearchAsync(gameBoard, depth - 1, -beta, -alpha, -color, token);

                gameBoard.UndoLastMove();

                if (!value.HasValue)
                {
                    // Cancel occurred during evaluation
                    return new EvaluatedMoveCollection(movesToEvaluate, false);
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

            ulong key = gameBoard.ZobristKey;

            TranspositionTableEntry tEntry = new TranspositionTableEntry();

            if (bestValue <= alphaOriginal)
            {
                // Losing move since alphaOriginal os negative infinity in this function
                tEntry.Type = TranspositionTableEntryType.UpperBound;
            }
            else
            {
                // Move is a lower bound winning move if bestValue >= beta (always infinity in this function), otherwise it's exact
                tEntry.Type = bestValue >= beta ? TranspositionTableEntryType.LowerBound : TranspositionTableEntryType.Exact;
                tEntry.BestMove = evaluatedMoves.BestMove.Move;
            }

            tEntry.Value = bestValue;
            tEntry.Depth = depth;

            _transpositionTable.Store(key, tEntry);

            return evaluatedMoves;
        }

        private void OnBestMoveFound(BestMoveParams bestMoveParams, EvaluatedMove evaluatedMove)
        {
            if (null == evaluatedMove)
            {
                throw new ArgumentNullException("evaluatedMove");
            }

            if (evaluatedMove != bestMoveParams.BestMove)
            {
                BestMoveFound?.Invoke(this, new BestMoveFoundEventArgs(evaluatedMove.Move, evaluatedMove.Depth, evaluatedMove.ScoreAfterMove));
                bestMoveParams.BestMove = evaluatedMove;
            }
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
                        await PrincipalVariationSearchAsync(clone, depth + i % 2, double.NegativeInfinity, double.PositiveInfinity, color, tokenSource.Token);
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

        #region Principal Variation Search

        private async Task<double?> PrincipalVariationSearchAsync(GameBoard gameBoard, int depth, double alpha, double beta, int color, CancellationToken token)
        {
            double alphaOriginal = alpha;

            ulong key = gameBoard.ZobristKey;

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

            List<Move> moves = GetPreSortedValidMoves(gameBoard, bestMove);

            bool firstMove = true;

            foreach (Move move in moves)
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }

                double? value = null;

                gameBoard.TrustedPlay(move);

                if (firstMove)
                {
                    // Full window search
                    value = -1 * await PrincipalVariationSearchAsync(gameBoard, depth - 1, -beta, -alpha, -color, token);
                    firstMove = false;
                }
                else
                {
                    // Null window search
                    value = -1 * await PrincipalVariationSearchAsync(gameBoard, depth - 1, -alpha - double.Epsilon, -alpha, -color, token);
                    if (value.HasValue && value > alpha && value < beta)
                    {
                        // Research with full window
                        value = -1 * await PrincipalVariationSearchAsync(gameBoard, depth - 1, -beta, -alpha, -color, token);
                    }
                }

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

        #region Pre-Sorted Moves

        private List<EvaluatedMove> GetPreSortedValidMoves(GameBoard gameBoard, EvaluatedMove bestMove)
        {
            List<Move> validMoves = GetPreSortedValidMoves(gameBoard, bestMove?.Move);

            List<EvaluatedMove> evaluatedMoves = new List<EvaluatedMove>(validMoves.Count);
            foreach (Move move in validMoves)
            {
                evaluatedMoves.Add(move == bestMove?.Move ? bestMove : new EvaluatedMove(move));
            }

            return evaluatedMoves;
        }

        private List<Move> GetPreSortedValidMoves(GameBoard gameBoard, Move bestMove)
        {
            List<Move> validMoves = new List<Move>(gameBoard.GetValidMoves());

            validMoves.Sort((a, b) => { return PreSortMoves(a, b, gameBoard, bestMove); });

            if (validMoves.Count > _maxBranchingFactor)
            {
                // Too many moves, reduce branching factor
                validMoves.RemoveRange(_maxBranchingFactor, validMoves.Count - _maxBranchingFactor);
            }

            return validMoves;
        }

        private static int PreSortMoves(Move a, Move b, GameBoard gameBoard, Move bestMove)
        {
            // Put the best move from a previous search first
            if (null != bestMove)
            {
                if (a == bestMove)
                {
                    return -1;
                }
                else if (b == bestMove)
                {
                    return 1;
                }
            }

            // Put noisy moves first
            if (gameBoard.IsNoisyMove(a) && !gameBoard.IsNoisyMove(b))
            {
                return -1;
            }
            else if (gameBoard.IsNoisyMove(b) && !gameBoard.IsNoisyMove(a))
            {
                return 1;
            }

            return 0;
        }

        #endregion

        #region Quiescence Search

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

            ulong key = gameBoard.ZobristKey;

            double score;
            if (_cachedBoardScores.TryLookup(key, out score))
            {
                return score;
            }

            BoardMetrics boardMetrics = gameBoard.GetBoardMetrics();

            score = CalculateBoardScore(boardMetrics, StartMetricWeights, EndMetricWeights);

            _cachedBoardScores.Store(key, score);

            return score;
        }

        private static double CalculateBoardScore(BoardMetrics boardMetrics, MetricWeights startMetricWeights, MetricWeights endMetricWeights)
        {
            double endScore = CalculateBoardScore(boardMetrics, endMetricWeights);

            if (boardMetrics.PiecesInHand == 0)
            {
                // In "end-game", no need to blend
                return endScore;
            }
            else
            {
                // Pieces still in hand, blend start and end scores
                double startScore = CalculateBoardScore(boardMetrics, startMetricWeights);

                double startRatio = boardMetrics.PiecesInHand / (double)(boardMetrics.PiecesInHand + boardMetrics.PiecesInPlay);
                double endRatio = 1 - startRatio;

                return (startRatio * startScore) + (endRatio * endScore);
            }
        }

        private static double CalculateBoardScore(BoardMetrics boardMetrics, MetricWeights metricWeights)
        {
            double score = 0;

            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                BugType bugType = EnumUtils.GetBugType(pieceName);

                double colorValue = EnumUtils.GetColor(pieceName) == Color.White ? 1.0 : -1.0;

                score += colorValue * metricWeights.Get(bugType, BugTypeWeight.InPlayWeight) * boardMetrics[pieceName].InPlay;
                score += colorValue * metricWeights.Get(bugType, BugTypeWeight.IsPinnedWeight) * boardMetrics[pieceName].IsPinned;
                score += colorValue * metricWeights.Get(bugType, BugTypeWeight.IsCoveredWeight) * boardMetrics[pieceName].IsCovered;
                score += colorValue * metricWeights.Get(bugType, BugTypeWeight.NoisyMoveWeight) * boardMetrics[pieceName].NoisyMoveCount;
                score += colorValue * metricWeights.Get(bugType, BugTypeWeight.QuietMoveWeight) * boardMetrics[pieceName].QuietMoveCount;
                score += colorValue * metricWeights.Get(bugType, BugTypeWeight.FriendlyNeighborWeight) * boardMetrics[pieceName].FriendlyNeighborCount;
                score += colorValue * metricWeights.Get(bugType, BugTypeWeight.EnemyNeighborWeight) * boardMetrics[pieceName].EnemyNeighborCount;
            }

            return score;
        }

        #endregion

        #region TreeStrap

        // TreeStrap algorithms taken from http://papers.nips.cc/paper/3722-bootstrapping-from-game-tree-search.pdf

        public async Task TreeStrapAsync(GameBoard gameBoard, int maxDepth, int maxHelperThreads, CancellationToken token)
        {
           await TreeStrapAsync(gameBoard, maxDepth, TimeSpan.MaxValue, maxHelperThreads, token);
        }

        public async Task TreeStrapAsync(GameBoard gameBoard, TimeSpan maxTime, int maxHelperThreads, CancellationToken token)
        {
            await TreeStrapAsync(gameBoard, int.MaxValue, maxTime, maxHelperThreads, token);
        }

        private async Task TreeStrapAsync(GameBoard gameBoard, int maxDepth, TimeSpan maxTime, int maxHelperThreads, CancellationToken token)
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

            // Make sure we have a clean state
            ResetCaches();

            List<ulong> boardKeys = new List<ulong>();

            while (gameBoard.GameInProgress)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                boardKeys.Add(gameBoard.ZobristKey);

                if (boardKeys.Count >= 6)
                {
                    int lastIndex = boardKeys.Count - 1;
                    if (boardKeys[lastIndex] == boardKeys[lastIndex - 4] && boardKeys[lastIndex - 1] == boardKeys[lastIndex - 5])
                    {
                        // We're in a loop, exit now
                        break;
                    }
                }

                CancellationTokenSource searchCancelationToken = new CancellationTokenSource();

                if (maxTime < TimeSpan.MaxValue)
                {
                    searchCancelationToken.CancelAfter(maxTime);
                }

                // Get best move
                Move bestMove = await GetBestMoveAsync(gameBoard, maxDepth, maxTime, maxHelperThreads, searchCancelationToken.Token);

                // Get delta metric weights
                MetricWeights deltaStart;
                MetricWeights deltaEnd;

                HashSet<ulong> visitedKeys = new HashSet<ulong>();
                DeltaFromTransTable(gameBoard, visitedKeys, token, out deltaStart, out deltaEnd);

                // Update metric weights with delta
                StartMetricWeights.Add(deltaStart);
                EndMetricWeights.Add(deltaEnd);

                // Play best move and reset caches
                gameBoard.TrustedPlay(bestMove);
                ResetCaches();
            }
        }

        private void DeltaFromTransTable(GameBoard gameBoard, HashSet<ulong> visitedKeys, CancellationToken token, out MetricWeights deltaStart, out MetricWeights deltaEnd)
        {
            MetricWeights ds = new MetricWeights();
            MetricWeights de = new MetricWeights();

            ulong key = gameBoard.ZobristKey;
            bool newKey = visitedKeys.Add(key);

            TranspositionTableEntry tEntry;
            if (newKey && _transpositionTable.TryLookup(key, out tEntry) && tEntry.Depth > 1 && !token.IsCancellationRequested)
            {
                double colorValue = gameBoard.CurrentTurnColor == Color.White ? 1.0 : -1.0;

                double boardScore = colorValue * TruncateBounds(CalculateBoardScore(gameBoard));

                double storedValue = TruncateBounds(tEntry.Value);

                BoardMetrics boardMetrics = gameBoard.GetBoardMetrics();

                MetricWeights startGradient = GetGradient(boardMetrics);
                MetricWeights endGradient = startGradient.Clone();

                double scaleFactor = TreeStrapStepConstant * (storedValue - boardScore);

                double startRatio = boardMetrics.PiecesInHand / (double)(boardMetrics.PiecesInHand + boardMetrics.PiecesInPlay);
                double endRatio = 1 - startRatio;

                startGradient.Scale(scaleFactor * startRatio * colorValue);
                endGradient.Scale(scaleFactor * endRatio * colorValue);

                if ((tEntry.Type == TranspositionTableEntryType.LowerBound || tEntry.Type == TranspositionTableEntryType.Exact) && storedValue > boardScore)
                {
                    ds.Add(startGradient);
                    de.Add(endGradient);
                }

                if ((tEntry.Type == TranspositionTableEntryType.UpperBound || tEntry.Type == TranspositionTableEntryType.Exact) && storedValue < boardScore)
                {
                    ds.Add(startGradient);
                    de.Add(endGradient);
                }

                foreach (Move move in gameBoard.GetValidMoves())
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    gameBoard.TrustedPlay(move);
                    MetricWeights ds1;
                    MetricWeights de1;
                    DeltaFromTransTable(gameBoard, visitedKeys, token, out ds1, out de1);
                    ds.Add(ds1);
                    de.Add(de1);
                    gameBoard.UndoLastMove();
                }
            }

            deltaStart = ds;
            deltaEnd = de;
        }

        private MetricWeights GetGradient(BoardMetrics boardMetrics)
        {
            MetricWeights gradient = new MetricWeights();

            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                BugType bugType = EnumUtils.GetBugType(pieceName);

                double colorValue = EnumUtils.GetColor(pieceName) == Color.White ? 1.0 : -1.0;

                gradient.Set(bugType, BugTypeWeight.InPlayWeight, gradient.Get(bugType, BugTypeWeight.InPlayWeight) + colorValue * boardMetrics[pieceName].InPlay);
                gradient.Set(bugType, BugTypeWeight.IsPinnedWeight, gradient.Get(bugType, BugTypeWeight.IsPinnedWeight) + colorValue * boardMetrics[pieceName].IsPinned);
                gradient.Set(bugType, BugTypeWeight.IsCoveredWeight, gradient.Get(bugType, BugTypeWeight.IsCoveredWeight) + colorValue * boardMetrics[pieceName].IsCovered);
                gradient.Set(bugType, BugTypeWeight.NoisyMoveWeight, gradient.Get(bugType, BugTypeWeight.NoisyMoveWeight) + colorValue * boardMetrics[pieceName].NoisyMoveCount);
                gradient.Set(bugType, BugTypeWeight.QuietMoveWeight, gradient.Get(bugType, BugTypeWeight.QuietMoveWeight) + colorValue * boardMetrics[pieceName].QuietMoveCount);
                gradient.Set(bugType, BugTypeWeight.FriendlyNeighborWeight, gradient.Get(bugType, BugTypeWeight.FriendlyNeighborWeight) + colorValue * boardMetrics[pieceName].FriendlyNeighborCount);
                gradient.Set(bugType, BugTypeWeight.EnemyNeighborWeight, gradient.Get(bugType, BugTypeWeight.EnemyNeighborWeight) + colorValue * boardMetrics[pieceName].EnemyNeighborCount);
            }

            return gradient;
        }

        private double TruncateBounds(double value)
        {
            if (double.IsPositiveInfinity(value))
            {
                return TreeStrapInfinity;
            }
            else if (double.IsNegativeInfinity(value))
            {
                return -TreeStrapInfinity;
            }

            return Math.Max(-0.99 * TreeStrapInfinity, Math.Min(0.99 * TreeStrapInfinity, value));
        }

        private const double TreeStrapStepConstant = 1e-6;
        private const double TreeStrapInfinity = 1e6;

        #endregion

        #region BestMoveParams

        private class BestMoveParams
        {
            public int MaxSearchDepth;
            public TimeSpan MaxSearchTime;
            public int MaxHelperThreads;
            public EvaluatedMove BestMove = null;
        }

        #endregion
    }

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
