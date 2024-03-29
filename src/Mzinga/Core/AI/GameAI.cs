﻿// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mzinga.Core.AI
{
    public class GameAI
    {
        public event EventHandler<BestMoveFoundEventArgs>? BestMoveFound = null;

        public readonly MetricWeights StartMetricWeights;
        public readonly MetricWeights EndMetricWeights;

        public readonly TranspositionTable TranspositionTable;

        private readonly int _maxBranchingFactor;

        private readonly int _quiescentSearchMaxDepth; // To prevent runaway stack overflows

        private readonly bool _useNullAspirationWindow;

        private readonly FixedCache<ulong, double> _cachedBoardScores = new FixedCache<ulong, double>(BoardScoreCacheSize);
        private static readonly int BoardScoreCacheSize = 1024 * 1024 / FixedCache<ulong, double>.EstimateSizeInBytes(sizeof(ulong), sizeof(double)); // 1MB

        private readonly FixedCache<ulong, MoveSet> _cachedValidMoves = new FixedCache<ulong, MoveSet>();
        private readonly FixedCache<ulong, EvaluatedMoveCollection> _cachedSortedMoves = new FixedCache<ulong, EvaluatedMoveCollection>();

        public GameAI(GameAIConfig config)
        {
            StartMetricWeights = config.StartMetricWeights?.Clone() ?? new MetricWeights();
            EndMetricWeights = config.EndMetricWeights?.Clone() ?? new MetricWeights();
            
            _maxBranchingFactor = config.MaxBranchingFactor ?? GameAIConfig.DefaultMaxBranchingFactor;
            _quiescentSearchMaxDepth = config.QuiescentSearchMaxDepth ?? GameAIConfig.DefaultQuiescentSearchMaxDepth;

            if (config.TranspositionTableSizeMB.HasValue)
            {
                TranspositionTable = new TranspositionTable(config.TranspositionTableSizeMB.Value);
            }
            else
            {
                TranspositionTable = new TranspositionTable(GameAIConfig.DefaultTranspositionTableSizeMB);
            }

            _useNullAspirationWindow = config.UseNullAspirationWindow ?? GameAIConfig.DefaultUseNullAspirationWindow;

            ResetCaches();
        }

        public void ResetCaches()
        {
            TranspositionTable.Clear();
            _cachedBoardScores.Clear();
            _cachedValidMoves.Clear();
        }

        #region Move Evaluation

        public Move GetBestMove(Board board, int maxDepth, int maxHelperThreads)
        {
            return GetBestMove(board, maxDepth, TimeSpan.MaxValue, maxHelperThreads);
        }

        public Move GetBestMove(Board board, TimeSpan maxTime, int maxHelperThreads)
        {
            return GetBestMove(board, int.MaxValue, maxTime, maxHelperThreads);
        }

        private Move GetBestMove(Board board, int maxDepth, TimeSpan maxTime, int maxHelperThreads)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            if (maxTime < TimeSpan.MaxValue)
            {
                cts.CancelAfter(maxTime);
            }

            Task<Move> task = GetBestMoveAsync(board, maxDepth, maxTime, maxHelperThreads, cts.Token).AsTask();
            task.Wait();

            return task.Result;
        }

        public async ValueTask<Move> GetBestMoveAsync(Board board, int maxHelperThreads, CancellationToken token)
        {
            return await GetBestMoveAsync(board, int.MaxValue, TimeSpan.MaxValue, maxHelperThreads, token);
        }

        public async ValueTask<Move> GetBestMoveAsync(Board board, int maxDepth, int maxHelperThreads, CancellationToken token)
        {
            return await GetBestMoveAsync(board, maxDepth, TimeSpan.MaxValue, maxHelperThreads, token);
        }

        public async ValueTask<Move> GetBestMoveAsync(Board board, TimeSpan maxTime, int maxHelperThreads, CancellationToken token)
        {
            return await GetBestMoveAsync(board, int.MaxValue, maxTime, maxHelperThreads, token);
        }

        private async ValueTask<Move> GetBestMoveAsync(Board board, int maxDepth, TimeSpan maxTime, int maxHelperThreads, CancellationToken token)
        {
            if (maxDepth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDepth));
            }

            if (maxTime < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(maxTime));
            }

            if (maxHelperThreads < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHelperThreads));
            }

            if (board.GameIsOver)
            {
                throw new Exception("Game is over.");
            }

            BestMoveParams bestMoveParams = new BestMoveParams()
            {
                MaxSearchDepth = maxDepth,
                MaxSearchTime = maxTime,
                MaxHelperThreads = maxHelperThreads,
            };

            EvaluatedMoveCollection evaluatedMoves = await EvaluateMovesAsync(board, bestMoveParams, token);

            EvaluatedMove bestMove = evaluatedMoves.BestMove ?? throw new Exception("No moves after evaluation!");

            // Make sure at least one move is reported
            OnBestMoveFound(board, bestMoveParams, bestMove);

            return bestMove.Move;
        }

        private async ValueTask<EvaluatedMoveCollection> EvaluateMovesAsync(Board board, BestMoveParams bestMoveParams, CancellationToken token)
        {
            EvaluatedMoveCollection movesToEvaluate = new EvaluatedMoveCollection();

            EvaluatedMove? bestMove = null;

            // Try to get cached best move if available
            ulong key = board.ZobristKey;
            if (TranspositionTable.TryLookup(key, out TranspositionTableEntry? tEntry) && tEntry.BestMove.HasValue)
            {
                bestMove = new EvaluatedMove(tEntry.BestMove.Value, tEntry.Value, tEntry.Depth);
                OnBestMoveFound(board, bestMoveParams, bestMove.Value);
            }

            if (bestMove.HasValue && double.IsPositiveInfinity(bestMove.Value.ScoreAfterMove))
            {
                // Winning move, don't search
                movesToEvaluate.Add(bestMove.Value);
                return movesToEvaluate;
            }

            var validMoves = GetSortedValidMoves(board, bestMove);
            movesToEvaluate.Add(validMoves, false);

            if (movesToEvaluate.Count <= 1 || bestMoveParams.MaxSearchDepth == 0)
            {
                // No need to search
                return movesToEvaluate;
            }

            // Iterative search
            int depth = movesToEvaluate.BestMove.HasValue ? 1 + Math.Max(0, movesToEvaluate.BestMove.Value.Depth) : 1;
            while (depth <= bestMoveParams.MaxSearchDepth)
            {
                // Start LazySMP helper threads
                CancellationTokenSource helperCTS = new CancellationTokenSource();
                Task[]? helperThreads = StartHelperThreads(board, depth, bestMoveParams.MaxSearchDepth, bestMoveParams.MaxHelperThreads, helperCTS);

                // "Re-sort" moves to evaluate based on the next iteration
                movesToEvaluate = await EvaluateMovesToDepthAsync(board, depth, movesToEvaluate, OrderType.Default, depth == 1, token);

                // End LazySMP helper threads
                EndHelperThreads(helperThreads, helperCTS);

                // Fire BestMoveFound for current depth
                if (movesToEvaluate.BestMove.HasValue)
                {
                    OnBestMoveFound(board, bestMoveParams, movesToEvaluate.BestMove.Value);

                    if (double.IsInfinity(movesToEvaluate.BestMove.Value.ScoreAfterMove))
                    {
                        // The best move ends the game, stop searching
                        break;
                    }
                }

                // Prune game-losing moves if possible
                movesToEvaluate.PruneGameLosingMoves();

                // Save this sorted move list for later
                _cachedSortedMoves.Store(key, movesToEvaluate);

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

                depth = 1 + (movesToEvaluate.BestMove.HasValue ? Math.Max(depth, movesToEvaluate.BestMove.Value.Depth) : depth);
            }

            return movesToEvaluate;
        }

        private async ValueTask<EvaluatedMoveCollection> EvaluateMovesToDepthAsync(Board board, int depth, IReadOnlyList<EvaluatedMove> movesToEvaluate, OrderType orderType, bool forceFullWindow, CancellationToken token)
        {
            double alpha = double.NegativeInfinity;
            double beta = double.PositiveInfinity;

            int color = board.CurrentColor == PlayerColor.White ? 1 : -1;

            double alphaOriginal = alpha;

            double? bestValue = null;

            EvaluatedMoveCollection evaluatedMoves = new EvaluatedMoveCollection();

            bool fullWindowSearch = true;

            foreach (EvaluatedMove moveToEvaluate in movesToEvaluate.GetEnumerableByOrderType(orderType))
            {
                if (token.IsCancellationRequested)
                {
                    // Cancel
                    return new EvaluatedMoveCollection(movesToEvaluate, false);
                }

                board.TrustedPlay(in moveToEvaluate.Move);

                double? value = null;

                if (fullWindowSearch || forceFullWindow || !_useNullAspirationWindow)
                {
                    // Full window search
                    value = -1 * await PrincipalVariationSearchAsync(board, depth - 1, -beta, -alpha, -color, orderType, token);
                }
                else
                {
                    // Null window search
                    value = -1 * await PrincipalVariationSearchAsync(board, depth - 1, -alpha - double.Epsilon, -alpha, -color, orderType, token);
                    if (value.HasValue && value > alpha && value < beta)
                    {
                        // Research with full window
                        value = -1 * await PrincipalVariationSearchAsync(board, depth - 1, -beta, -alpha, -color, orderType, token);
                    }
                }

                board.TryUndoLastMove();

                if (!value.HasValue)
                {
                    // Cancel occurred during evaluation
                    return new EvaluatedMoveCollection(movesToEvaluate, false);
                }

                EvaluatedMove evaluatedMove = new EvaluatedMove(moveToEvaluate.Move, value.Value, depth);
                evaluatedMoves.Add(evaluatedMove);

                if (!bestValue.HasValue || value > bestValue)
                {
                    bestValue = value;
                }

                if (value.Value > alpha)
                {
                    alpha = value.Value;
                    fullWindowSearch = false;
                }

                if (value.Value >= beta)
                {
                    // A winning move has been found, since beta is always infinity in this function
                    break;
                }
            }

            // If we made it through all the moves, store to TT

            ulong key = board.ZobristKey;

            TranspositionTableEntry tEntry = new TranspositionTableEntry();

            if (bestValue <= alphaOriginal)
            {
                // Losing move since alphaOriginal is negative infinity in this function
                tEntry.Type = TranspositionTableEntryType.UpperBound;
            }
            else
            {
                // Move is a lower bound winning move if bestValue >= beta (always infinity in this function), otherwise it's exact
                tEntry.Type = bestValue >= beta ? TranspositionTableEntryType.LowerBound : TranspositionTableEntryType.Exact;
                tEntry.BestMove = evaluatedMoves.BestMove?.Move;
#if DEBUG
                tEntry.BestMoveString = tEntry.BestMove.HasValue ? board.GetMoveString(tEntry.BestMove.Value) : null;
#endif
            }

            tEntry.Value = bestValue ?? double.NaN;
            tEntry.Depth = depth;

            TranspositionTable.Store(key, tEntry);

            return evaluatedMoves;
        }

        private void OnBestMoveFound(Board board, BestMoveParams bestMoveParams, EvaluatedMove evaluatedMove)
        {
            if (evaluatedMove != bestMoveParams.BestMove)
            {
                if (BestMoveFound is not null)
                {
                    BestMoveFound.Invoke(this, new BestMoveFoundEventArgs(evaluatedMove.Move, evaluatedMove.Depth, evaluatedMove.ScoreAfterMove, GetPrincipalVariationFromTranspositionTable(board, evaluatedMove.Depth)));
                }
                bestMoveParams.BestMove = evaluatedMove;
            }
        }

        #endregion

        #region Threading support

        private Task[]? StartHelperThreads(Board board, int depth, int maxDepth, int threads, CancellationTokenSource tokenSource)
        {
            Task[]? helperThreads = null;

            // Only start helpers on later iterative depths past the first
            if (depth > 1 && threads > 0)
            {
                helperThreads = new Task[threads];
                int color = board.CurrentColor == PlayerColor.White ? 1 : -1;

                for (int i = 0; i < helperThreads.Length; i++)
                {
                    Board clone = board.Clone();
                    int helperDepth = Math.Min(maxDepth, depth + ((i + 1)/ 3));
                    OrderType orderType = (OrderType)(2 - (i % 3));
                    helperThreads[i] = Task.Run(async () =>
                    {
                        await PrincipalVariationSearchAsync(clone, helperDepth, double.NegativeInfinity, double.PositiveInfinity, color, orderType, tokenSource.Token);
                    });
                }
            }

            return helperThreads;
        }

        private static void EndHelperThreads(Task[]? helperThreads, CancellationTokenSource tokenSource)
        {
            if (helperThreads is not null)
            {
                tokenSource.Cancel();
                Task.WaitAll(helperThreads);
            }
        }

        #endregion

        #region Principal Variation Search

        private async ValueTask<double?> PrincipalVariationSearchAsync(Board board, int depth, double alpha, double beta, int color, OrderType orderType, CancellationToken token)
        {
            double alphaOriginal = alpha;

            ulong key = board.ZobristKey;

            if (TranspositionTable.TryLookup(key, out TranspositionTableEntry? tEntry) && tEntry.Depth >= depth)
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

            if (depth == 0 || board.GameIsOver)
            {
                return await QuiescenceSearchAsync(board, _quiescentSearchMaxDepth, alpha, beta, color, token);
            }

            double? bestValue = null;
            Move? bestMove = tEntry?.BestMove;

            var moves = GetSortedValidMoves(board, bestMove.HasValue ? new EvaluatedMove(bestMove.Value, alpha, tEntry!.Depth) : null);

            bool fullWindowSearch = true;

            foreach (var move in moves.GetEnumerableByOrderType(orderType))
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }

                double? value = null;

                board.TrustedPlay(in move.Move);

                if (fullWindowSearch || !_useNullAspirationWindow)
                {
                    // Full window search
                    value = -1 * await PrincipalVariationSearchAsync(board, depth - 1, -beta, -alpha, -color, orderType, token);
                }
                else
                {
                    // Null window search
                    value = -1 * await PrincipalVariationSearchAsync(board, depth - 1, -alpha - double.Epsilon, -alpha, -color, orderType, token);
                    if (value.HasValue && value > alpha && value < beta)
                    {
                        // Research with full window
                        value = -1 * await PrincipalVariationSearchAsync(board, depth - 1, -beta, -alpha, -color, orderType, token);
                    }
                }

                board.TryUndoLastMove();

                if (!value.HasValue)
                {
                    return null;
                }

                if (!bestValue.HasValue || value > bestValue)
                {
                    bestValue = value;
                    bestMove = move.Move;
                }

                if (value.Value > alpha)
                {
                    alpha = value.Value;
                    fullWindowSearch = false;
                }

                if (value.Value >= beta)
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
#if DEBUG
                    tEntry.BestMoveString = tEntry.BestMove.HasValue ? board.GetMoveString(tEntry.BestMove.Value) : null;
#endif
                }

                tEntry.Value = bestValue.Value;
                tEntry.Depth = depth;

                TranspositionTable.Store(key, tEntry);
            }

            return bestValue;
        }

        public IReadOnlyList<Move> GetPrincipalVariationFromTranspositionTable(Board board, int maxDepth)
        {
            List<Move> moves = new List<Move>();

            if (maxDepth > 0)
            {
                Board clone = board.Clone();

                while (clone.GameInProgress && moves.Count < maxDepth)
                {
                    ulong key = clone.ZobristKey;

                    if (!TranspositionTable.TryLookup(key, out TranspositionTableEntry? tEntry) || tEntry is null || !tEntry.BestMove.HasValue)
                    {
                        break;
                    }

                    var move = tEntry.BestMove.Value;
                    moves.Add(move);
                    clone.TrustedPlay(in move);
                }
            }

            return moves;
        }

        #endregion

        #region Getting Moves

        private MoveSet GetValidMoves(Board board)
        {
            ulong key = board.ZobristKey;

            if (_cachedValidMoves.TryLookup(key, out var moves))
            {
                return moves;
            }

            moves = board.GetValidMoves();
            _cachedValidMoves.Store(key, moves);

            return moves;
        }

        private EvaluatedMoveCollection GetSortedValidMoves(Board board, EvaluatedMove? bestMove)
        {
            ulong key = board.ZobristKey;

            if (_cachedSortedMoves.TryLookup(key, out var moves))
            {
                return moves;
            }

            moves = GetPreSortedValidMoves(board, bestMove);
            _cachedSortedMoves.Store(key, moves);

            return moves;
        }

        private EvaluatedMoveCollection GetPreSortedValidMoves(Board board, EvaluatedMove? bestMove)
        {
            var validMoves = new List<Move>(GetValidMoves(board));

            validMoves.Sort((a, b) => { return PreSortMoves(a, b, board, bestMove?.Move); });

            if (validMoves.Count > _maxBranchingFactor)
            {
                validMoves.RemoveRange(_maxBranchingFactor, validMoves.Count - _maxBranchingFactor);
            }

            if (bestMove is null)
            {
                // Just return the pre-sorted result
                return new EvaluatedMoveCollection(validMoves);
            }

            EvaluatedMoveCollection evaluatedMoves = new EvaluatedMoveCollection(validMoves.Count);

            // Add evaluated best move as first item
            if (bestMove.HasValue)
            {
                evaluatedMoves.Add(bestMove.Value);
            }

            // Add remaining moves after the first move
            for (int i = 1; i < validMoves.Count; i++)
            {
                evaluatedMoves.Add(validMoves[i]);
            }

            return evaluatedMoves;
        }

        private static int PreSortMoves(in Move a, in Move b, Board board, Move? bestMove)
        {
            // Put the best move from a previous search first
            if (bestMove is not null)
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
            if (board.IsNoisyMove(in a) && !board.IsNoisyMove(in b))
            {
                return -1;
            }
            else if (board.IsNoisyMove(in b) && !board.IsNoisyMove(in a))
            {
                return 1;
            }

            return 0;
        }

        #endregion

        #region Quiescence Search

        private async ValueTask<double?> QuiescenceSearchAsync(Board board, int depth, double alpha, double beta, int color, CancellationToken token)
        {
            double bestValue = color * CalculateBoardScore(board);

            alpha = Math.Max(alpha, bestValue);

            if (alpha >= beta || depth == 0 || board.GameIsOver)
            {
                return bestValue;
            }

            foreach (Move move in GetValidMoves(board))
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }

                if (board.IsNoisyMove(in move))
                {
                    board.TrustedPlay(in move);
                    double? value = -1 * await QuiescenceSearchAsync(board, depth - 1, -beta, -alpha, -color, token);
                    board.TryUndoLastMove();

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

        internal double CalculateBoardScore(Board board)
        {
            // Always score from white's point of view

            if (board.BoardState == BoardState.WhiteWins)
            {
                return double.PositiveInfinity;
            }
            else if (board.BoardState == BoardState.BlackWins)
            {
                return double.NegativeInfinity;
            }
            else if (board.BoardState == BoardState.Draw || board.BoardState == BoardState.NotStarted)
            {
                return 0.0;
            }

            ulong key = board.ZobristKey;

            if (_cachedBoardScores.TryLookup(key, out double score))
            {
                return score;
            }

            BoardMetrics boardMetrics = board.GetBoardMetrics();

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

            for (int pn = 0; pn < (int)PieceName.NumPieceNames; pn++)
            {
                var pieceName = (PieceName)pn;
                BugType bugType = Enums.GetBugType(pieceName);

                double colorValue = Enums.GetColor(pieceName) == PlayerColor.White ? 1.0 : -1.0;

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

        public void TreeStrap(Board board, TimeSpan maxTime, int maxHelperThreads)
        {
            TreeStrap(board, int.MaxValue, maxTime, maxHelperThreads);
        }

        public void TreeStrap(Board board, int maxDepth, int maxHelperThreads)
        {
            TreeStrap(board, maxDepth, TimeSpan.MaxValue, maxHelperThreads);
        }

        private void TreeStrap(Board board, int maxDepth, TimeSpan maxTime, int maxHelperThreads)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            if (maxTime < TimeSpan.MaxValue)
            {
                cts.CancelAfter(maxTime);
            }

            Task task = TreeStrapAsync(board, maxDepth, maxTime, maxHelperThreads, cts.Token).AsTask();
            task.Wait();
        }

        public async ValueTask TreeStrapAsync(Board board, int maxDepth, int maxHelperThreads, CancellationToken token)
        {
           await TreeStrapAsync(board, maxDepth, TimeSpan.MaxValue, maxHelperThreads, token);
        }

        public async ValueTask TreeStrapAsync(Board board, TimeSpan maxTime, int maxHelperThreads, CancellationToken token)
        {
            await TreeStrapAsync(board, int.MaxValue, maxTime, maxHelperThreads, token);
        }

        private async ValueTask TreeStrapAsync(Board board, int maxDepth, TimeSpan maxTime, int maxHelperThreads, CancellationToken token)
        {
            if (maxDepth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDepth));
            }

            if (maxTime < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(maxTime));
            }

            if (maxHelperThreads < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHelperThreads));
            }

            // Make sure we have a clean state
            ResetCaches();

            List<ulong> boardKeys = new List<ulong>();

            while (board.GameInProgress)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                boardKeys.Add(board.ZobristKey);

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
                Move bestMove = await GetBestMoveAsync(board, maxDepth, maxTime, maxHelperThreads, searchCancelationToken.Token);

                HashSet<ulong> visitedKeys = new HashSet<ulong>();
                DeltaFromTransTable(board, visitedKeys, token, out MetricWeights deltaStart, out MetricWeights deltaEnd);

                // Update metric weights with delta
                StartMetricWeights.Add(deltaStart);
                EndMetricWeights.Add(deltaEnd);

                // Play best move and reset caches
                board.TrustedPlay(in bestMove, board.GetMoveString(bestMove));
                ResetCaches();
            }
        }

        private void DeltaFromTransTable(Board board, HashSet<ulong> visitedKeys, CancellationToken token, out MetricWeights deltaStart, out MetricWeights deltaEnd)
        {
            MetricWeights ds = new MetricWeights();
            MetricWeights de = new MetricWeights();

            ulong key = board.ZobristKey;
            bool newKey = visitedKeys.Add(key);

            if (newKey && TranspositionTable.TryLookup(key, out TranspositionTableEntry? tEntry) && tEntry.Depth > 1 && !token.IsCancellationRequested)
            {
                double colorValue = board.CurrentColor == PlayerColor.White ? 1.0 : -1.0;

                double boardScore = colorValue * TruncateBounds(CalculateBoardScore(board));

                double storedValue = TruncateBounds(tEntry.Value);

                BoardMetrics boardMetrics = board.GetBoardMetrics();

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

                foreach (Move move in GetValidMoves(board))
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    board.TrustedPlay(in move);
                    DeltaFromTransTable(board, visitedKeys, token, out MetricWeights ds1, out MetricWeights de1);
                    ds.Add(ds1);
                    de.Add(de1);
                    board.TryUndoLastMove();
                }
            }

            deltaStart = ds;
            deltaEnd = de;
        }

        private static MetricWeights GetGradient(BoardMetrics boardMetrics)
        {
            MetricWeights gradient = new MetricWeights();

            for (int pn = 0; pn < (int)PieceName.NumPieceNames; pn++)
            {
                var pieceName = (PieceName)pn;

                BugType bugType = Enums.GetBugType(pieceName);

                double colorValue = Enums.GetColor(pieceName) == PlayerColor.White ? 1.0 : -1.0;

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

        private static double TruncateBounds(double value)
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
            public EvaluatedMove? BestMove = null;
        }

        #endregion
    }

    public class BestMoveFoundEventArgs : EventArgs
    {
        public readonly Move Move;
        public readonly int Depth;
        public readonly double Score;
        public readonly IReadOnlyList<Move> PrincipalVariation;

        public BestMoveFoundEventArgs(Move move, int depth, double score, IReadOnlyList<Move> principalVariation)
        {
            Move = move;
            Depth = depth;
            Score = score;
            PrincipalVariation = principalVariation;
        }
    }
}
