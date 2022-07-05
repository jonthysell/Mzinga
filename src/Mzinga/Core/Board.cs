// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mzinga.Core
{
    public class Board
    {
        public readonly GameType GameType;

        public BoardState BoardState { get; private set; } = BoardState.NotStarted;

        public bool GameInProgress => Enums.GameInProgress(BoardState);

        public bool GameIsOver => Enums.GameIsOver(BoardState);

        public int CurrentTurn
        {
            get
            {
                return _currentTurn;
            }
            protected set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                PlayerColor oldColor = CurrentColor;

                _currentTurn = value;

                if (oldColor != CurrentColor)
                {
                    // Turn has changed
                    _zobristHash.ToggleTurn();
                }

                ResetState();
                ResetCaches();
            }
        }
        private int _currentTurn = 0;

        public int CurrentPlayerTurn => 1 + CurrentTurn / 2;

        public PlayerColor CurrentColor => (PlayerColor)(CurrentTurn % (int)PlayerColor.NumPlayerColors);

        public bool CurrentTurnQueenInPlay => PieceInPlay(CurrentColor == PlayerColor.White ? PieceName.wQ : PieceName.bQ);

        public readonly BoardHistory BoardHistory = new BoardHistory();

        public PieceName LastPieceMoved
        {
            get
            {
                return _lastPieceMoved;
            }
            protected set
            {
                PieceName old = _lastPieceMoved;

                _lastPieceMoved = value;

                if (old != value)
                {
                    _zobristHash.ToggleLastMovedPiece(old);
                    _zobristHash.ToggleLastMovedPiece(value);
                }
            }
        }
        private PieceName _lastPieceMoved = PieceName.INVALID;

        public ulong ZobristKey => _zobristHash.Value;

        private readonly Position[] _piecePositions = new Position[(int)PieceName.NumPieceNames];
        private readonly PieceName[,,] _pieceGrid = new PieceName[Position.BoardSize, Position.BoardSize, Position.BoardStackSize];

        private bool _cachedValidPlacementsReady = false;
        private readonly PositionSet _cachedValidPlacements = new PositionSet(32);

        private MoveSet? _cachedValidMoves = null;
        
        private PositionSet? _cachedEnemyQueenNeighbors = null;

        private readonly bool[] _partOfHive = new bool[(int)PieceName.NumPieceNames];
        private readonly Queue<PieceName> _piecesToLookAt = new Queue<PieceName>((int)PieceName.NumPieceNames);

        private readonly ZobristHash _zobristHash = new ZobristHash();

        public static Board ParseGameString(string gameStr, bool trustedPlay = false)
        {
            var split = gameStr.Split(';');

            if (!Enums.TryParse(split[0], out GameType gameType))
            {
                throw new ArgumentException($"Unable to parse '{split[0]}' in GameString.", nameof(gameStr));
            }

            Board board = new Board(gameType);

            for (int i = 3; i < split.Length; i++)
            {
                if (!board.TryParseMove(split[i], out Move move, out string parsedMoveString))
                {
                    throw new ArgumentException($"Unable to parse '{split[i]}' in GameString.", nameof(gameStr));
                }

                if (trustedPlay)
                {
                    board.TrustedPlay(in move, parsedMoveString);
                }
                else if (!board.TryPlayMove(in move, parsedMoveString))
                {
                    throw new ArgumentException($"Unable to play '{split[i]}' in GameString.", nameof(gameStr));
                }
            }

            return board;
        }

        public static bool TryParseGameString(string gameStr, bool trustedPlay, out Board? board)
        {
            try
            {
                board = ParseGameString(gameStr, trustedPlay);
                return true;
            }
            catch (Exception) { }

            board = default;
            return false;
        }

        public Board(GameType gameType = GameType.Base)
        {
            GameType = gameType;

            for (int pn = 0; pn < (int)PieceName.NumPieceNames; pn++)
            {
                _piecePositions[pn] = Position.NullPosition;
            }

            for (int q = 0; q < _pieceGrid.GetLength(0); q++)
            {
                for (int r = 0; r < _pieceGrid.GetLength(1); r++)
                {
                    for (int stack = 0; stack < _pieceGrid.GetLength(2); stack++)
                    {
                        _pieceGrid[q, r, stack] = PieceName.INVALID;
                    }
                }
            }
        }

        public string GetGameString()
        {
            var sb = new StringBuilder();

            sb.Append($"{Enums.GetGameTypeString(GameType)};{BoardState};{CurrentColor}[{CurrentPlayerTurn}]");

            foreach (var item in BoardHistory)
            {
                sb.Append($";{item.MoveString}");
            }

            return sb.ToString();
        }

        internal MoveSet GetValidMoves()
        {
            if (_cachedValidMoves is null)
            {
                _cachedValidMoves = new MoveSet();

                if (GameInProgress)
                {
                    int startPiece = (int)(CurrentColor == PlayerColor.White ? PieceName.wQ : PieceName.bQ);
                    int endPiece = (int)(CurrentColor == PlayerColor.White ? PieceName.bQ : PieceName.NumPieceNames);
                    for (int pn = startPiece; pn < endPiece; pn++)
                    {
                        GetValidMoves((PieceName)pn, _cachedValidMoves);
                    }

                    if (_cachedValidMoves.Count == 0)
                    {
                        _cachedValidMoves.FastAdd(in Move.PassMove);
                    }
                }

#if DEBUG
                _cachedValidMoves.ValidateSet();
#endif
            }
            return _cachedValidMoves;
        }

        public void Play(in Move move, string moveString = "")
        {
            if (move == Move.PassMove)
            {
                Pass();
                return;
            }

            if (GameIsOver)
            {
                throw new InvalidMoveException(move, "You can't play, the game is over.");
            }

            if (!GetValidMoves().Contains(in move))
            {
                if (Enums.GetColor(move.PieceName) != CurrentColor)
                {
                    throw new InvalidMoveException(move, "It's not that player's turn.");
                }

                if (!Enums.PieceNameIsEnabledForGameType(move.PieceName, GameType))
                {
                    throw new InvalidMoveException(move, "That piece is not enabled in this game.");
                }

                if (move.Destination == Position.NullPosition)
                {
                    throw new InvalidMoveException(move, "You can't put a piece back into your hand.");
                }

                if (CurrentPlayerTurn == 1 && Enums.GetBugType(move.PieceName) == BugType.QueenBee)
                {
                    throw new InvalidMoveException(move, "You can't play your Queen Bee on your first turn.");
                }

                if (!CurrentTurnQueenInPlay)
                {
                    if (CurrentPlayerTurn == 4 && Enums.GetBugType(move.PieceName) != BugType.QueenBee)
                    {
                        throw new InvalidMoveException(move, "You must play your Queen Bee on or before your fourth turn.");
                    }
                    else if (PieceInPlay(move.PieceName))
                    {
                        throw new InvalidMoveException(move, "You can't move a piece in play until you've played your Queen Bee.");
                    }
                }

                if (!PlacingPieceInOrder(move.PieceName))
                {
                    throw new InvalidMoveException(move, "When there are multiple pieces of the same bug type, you must play the pieces in order.");
                }

                if (HasPieceAt(move.Destination))
                {
                    throw new InvalidMoveException(move, "You can't move there because a piece already exists at that position.");
                }

                if (PieceInPlay(move.PieceName))
                {
                    if (!PieceIsOnTop(move.PieceName))
                    {
                        throw new InvalidMoveException(move, "You can't move that piece because it has another piece on top of it.");
                    }
                    else if (!CanMoveWithoutBreakingHive(move.PieceName))
                    {
                        throw new InvalidMoveException(move, "You can't move that piece because it will break the hive.");
                    }
                }

                throw new InvalidMoveException(move);
            }

            TrustedPlay(in move, moveString);
        }

        public void Pass()
        {
            if (GameIsOver)
            {
                throw new InvalidMoveException(Move.PassMove, "You can't pass, the game is over.");
            }

            if (!GetValidMoves().Contains(in Move.PassMove))
            {
                throw new InvalidMoveException(Move.PassMove, "You can't pass when you have valid moves.");
            }

            TrustedPlay(in Move.PassMove, Move.PassString);
        }

        public bool TryPlayMove(in Move move, string moveString = "")
        {
            var validMoves = GetValidMoves();

            if (validMoves.Contains(in move))
            {
                TrustedPlay(in move, moveString);
                return true;
            }

            return false;
        }

        public bool TryUndoLastMove()
        {
            if (BoardHistory.Count > 0)
            {
                var lastMove = BoardHistory[^1];

                if (lastMove.Move != Move.PassMove)
                {
                    SetPosition(lastMove.Move.PieceName, in lastMove.Move.Source, true);
                }

                BoardHistory.UndoLast();

                LastPieceMoved = BoardHistory.LastMove?.PieceName ?? PieceName.INVALID;
                CurrentTurn--;

                return true;
            }

            return false;
        }

        public string GetMoveString(in Move move)
        {
            if (move == Move.PassMove)
            {
                return Move.PassString;
            }

            var startPiece = move.PieceName.ToString();

            if (CurrentTurn == 0 && move.Destination == Position.OriginPosition)
            {
                return startPiece;
            }

            var endPiece = "";

            if (move.Destination.Stack > 0)
            {
                PieceName pieceBelow = GetPieceAt(move.Destination.GetBelow());
                endPiece = pieceBelow.ToString();
            }
            else
            {
                SetPosition(move.PieceName, in Position.NullPosition, false);
                for (int dir = 0; dir < (int)Direction.NumDirections; dir++)
                {
                    Position neighborPosition = move.Destination.GetNeighborAt((Direction)dir);
                    PieceName neighbor = GetPieceOnTopAt(in neighborPosition);

                    if (neighbor != PieceName.INVALID)
                    {
                        endPiece = neighbor.ToString();
                        switch (dir)
                        {
                            case 0: // Up
                                endPiece += "\\";
                                break;
                            case 1: // UpRight
                                endPiece = "/" + endPiece;
                                break;
                            case 2: // DownRight
                                endPiece = "-" + endPiece;
                                break;
                            case 3: // Down
                                endPiece = "\\" + endPiece;
                                break;
                            case 4: // DownLeft
                                endPiece += "/";
                                break;
                            case 5: // UpLeft
                                endPiece += "-";
                                break;
                        }
                        break;
                    }
                }
                SetPosition(move.PieceName, in move.Source, false);
            }

            if (endPiece != "")
            {
                return $"{startPiece} {endPiece}";
            }

            throw new ArgumentOutOfRangeException(nameof(move));
        }

        public bool TryGetMoveString(in Move move, out string result)
        {
            try
            {
                result = GetMoveString(in move);
                return true;
            }
            catch (Exception) { }

            result = string.Empty;
            return false;
        }

        public bool TryParseMove(string moveString, out Move result, out string resultString)
        {
            if (Move.TryNormalizeMoveString(moveString, out bool isPass, out PieceName startPiece, out char beforeSeperator, out PieceName endPiece, out char afterSeperator))
            {
                resultString = Move.BuildMoveString(isPass, startPiece, beforeSeperator, endPiece, afterSeperator);

                if (isPass)
                {
                    result = Move.PassMove;
                    return true;
                }

                Position source = _piecePositions[(int)startPiece];

                Position destination = Position.OriginPosition;

                if (endPiece != PieceName.INVALID)
                {
                    Position targetPosition = _piecePositions[(int)endPiece];

                    if (beforeSeperator != '\0')
                    {
                        // Moving piece on the left-hand side of the target piece
                        switch (beforeSeperator)
                        {
                            case '-':
                                destination = targetPosition.GetNeighborAt(Direction.UpLeft).GetBottom();
                                break;
                            case '/':
                                destination = targetPosition.GetNeighborAt(Direction.DownLeft).GetBottom();
                                break;
                            case '\\':
                                destination = targetPosition.GetNeighborAt(Direction.Up).GetBottom();
                                break;
                        }
                    }
                    else if (afterSeperator != '\0')
                    {
                        // Moving piece on the right-hand side of the target piece
                        switch (afterSeperator)
                        {
                            case '-':
                                destination = targetPosition.GetNeighborAt(Direction.DownRight).GetBottom();
                                break;
                            case '/':
                                destination = targetPosition.GetNeighborAt(Direction.UpRight).GetBottom();
                                break;
                            case '\\':
                                destination = targetPosition.GetNeighborAt(Direction.Down).GetBottom();
                                break;
                        }
                    }
                    else
                    {
                        destination = targetPosition.GetAbove();
                    }
                }

                result = new Move(startPiece, source, destination);
                return true;
            }

            result = default;
            resultString = string.Empty;
            return false;
        }

        public bool IsNoisyMove(in Move move)
        {
            if (move == Move.PassMove)
            {
                return false;
            }

            if (_cachedEnemyQueenNeighbors is null)
            {
                _cachedEnemyQueenNeighbors = new PositionSet(8);

                Position enemyQueenPosition = GetPosition(CurrentColor == PlayerColor.White ? PieceName.bQ : PieceName.wQ);

                if (enemyQueenPosition != Position.NullPosition)
                {
                    // Add queen's neighboring positions
                    for (int dir = 0; dir < (int)Direction.NumDirections; dir++)
                    {
                        _cachedEnemyQueenNeighbors.Add(enemyQueenPosition.GetNeighborAt((Direction)dir));
                    }
                }
            }

            return _cachedEnemyQueenNeighbors.Contains(move.Destination) && !_cachedEnemyQueenNeighbors.Contains(GetPosition(move.PieceName));
        }

        // Following the example at https://chessprogramming.wikispaces.com/Perft
        public long CalculatePerft(int depth)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            Task<long?> task = CalculatePerftAsync(depth, cts.Token).AsTask();
            task.Wait();

            return task.Result ?? 0;
        }

        public async ValueTask<long?> CalculatePerftAsync(int depth, CancellationToken token)
        {
            if (depth == 0)
            {
                return 1;
            }

            var validMoves = GetValidMoves();

            if (depth == 1)
            {
                return validMoves.Count;
            }

            long? nodes = null;

            foreach (Move move in validMoves)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                TrustedPlay(in move);

                long? value = await CalculatePerftAsync(depth - 1, token);
                TryUndoLastMove();

                if (!value.HasValue)
                {
                    return null;
                }

                if (!nodes.HasValue)
                {
                    nodes = 0;
                }

                nodes += value;
            }

            return nodes;
        }

        public long ParallelPerft(int depth)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            Task<long?> task = ParallelPerftAsync(depth, cts.Token);
            task.Wait();

            return task.Result ?? 0;
        }

        public async Task<long?> ParallelPerftAsync(int depth, CancellationToken token)
        {
            if (depth == 0)
            {
                return 1;
            }

            MoveSet validMoves = GetValidMoves();

            if (depth == 1)
            {
                return validMoves.Count;
            }

            long nodes = 0;

            await Task.Run(() =>
            {
                var tasks = new Task[validMoves.Count];
                int i = 0;
                foreach (var move in validMoves)
                {
                    tasks[i] = Task.Run(async () =>
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }

                        var clone = Clone();
                        clone.TrustedPlay(in move);

                        long? value = await clone.CalculatePerftAsync(depth - 1, token);

                        if (!value.HasValue)
                        {
                            return;
                        }

                        Interlocked.Add(ref nodes, value.Value);
                    });
                    i++;
                }
                Task.WaitAll(tasks);
            }, token);

            return token.IsCancellationRequested ? null : nodes;
        }

        public BoardMetrics GetBoardMetrics()
        {
            var boardMetrics = new BoardMetrics()
            {
                BoardState = BoardState,
            };

            // Get the metrics for the current turn
            if (GameInProgress)
            {
                var currentValidMoves = GetValidMoves();
                SetCurrentPlayerMetrics(boardMetrics, currentValidMoves);

                // Save off cache objects until return
                PositionSet? enemyQueenNeighbors = _cachedEnemyQueenNeighbors;

                // Spoof going to the next turn to get the opponent's metrics
                CurrentTurn++;

                var nextValidMoves = GetValidMoves();
                SetCurrentPlayerMetrics(boardMetrics, nextValidMoves);
                CurrentTurn--;

                // Returned, so reload saved cached objects
                _cachedEnemyQueenNeighbors = enemyQueenNeighbors;

                _cachedValidMoves = currentValidMoves;
            }

            return boardMetrics;
        }

        private void SetCurrentPlayerMetrics(BoardMetrics boardMetrics, MoveSet moveSet)
        {
            int startPiece = (int)(CurrentColor == PlayerColor.White ? PieceName.wQ : PieceName.bQ);
            int endPiece = (int)(CurrentColor == PlayerColor.White ? PieceName.bQ : PieceName.NumPieceNames);
            for (int pn = startPiece; pn < endPiece; pn++)
            {
                var pieceName = (PieceName)pn;

                if (Enums.PieceNameIsEnabledForGameType(pieceName, GameType))
                {
                    bool pieceInPlay = PieceInPlay(pieceName);
                    if (pieceInPlay)
                    {
                        boardMetrics.PiecesInPlay++;
                        boardMetrics[pieceName].InPlay = 1;
                    }
                    else
                    {
                        boardMetrics.PiecesInHand++;
                        boardMetrics[pieceName].InPlay = 0;
                    }

                    // Move metrics
                    bool isPinned = IsPinned(pieceName, moveSet, out boardMetrics[pieceName].NoisyMoveCount, out boardMetrics[pieceName].QuietMoveCount);

                    boardMetrics[pieceName].IsPinned = isPinned ? 1 : 0;
                    boardMetrics[pieceName].IsCovered = pieceInPlay && !PieceIsOnTop(pieceName) ? 1 : 0;

                    CountNeighbors(pieceName, out boardMetrics[pieceName].FriendlyNeighborCount, out boardMetrics[pieceName].EnemyNeighborCount);
                }
            }
        }

        private bool IsPinned(PieceName pieceName, MoveSet moveSet, out int noisyCount, out int quietCount)
        {
            noisyCount = 0;
            quietCount = 0;

            foreach (var move in moveSet)
            {
                if (move.PieceName == pieceName)
                {
                    if (IsNoisyMove(in move))
                    {
                        noisyCount++;
                    }
                    else
                    {
                        quietCount++;
                    }
                }
            }

            return (noisyCount + quietCount) == 0;
        }

        public Board Clone()
        {
            var board = new Board(GameType);
            foreach (var item in BoardHistory)
            {
                board.TrustedPlay(in item.Move, item.MoveString);
            }

            return board;
        }

        private void GetValidMoves(PieceName pieceName, MoveSet moveSet)
        {
            if (Enums.PieceNameIsEnabledForGameType(pieceName, GameType) && PlacingPieceInOrder(pieceName))
            {
                if (CurrentTurn == 0)
                {
                    // First turn by white
                    if (pieceName != PieceName.wQ)
                    {
                        var move = new Move(pieceName, Position.NullPosition, Position.OriginPosition);
                        moveSet.FastAdd(in move);
                    }
                }
                else if (CurrentTurn == 1)
                {
                    // First turn by black
                    if (pieceName != PieceName.bQ)
                    {
                        for (int dir = 0; dir < (int)Direction.NumDirections; dir++)
                        {
                            var move = new Move(pieceName, Position.NullPosition, Position.OriginNeighbors[dir]);
                            moveSet.FastAdd(in move);
                        }
                    }
                }
                else if (PieceInHand(pieceName))
                {
                    // Piece is in hand
                    if (CurrentPlayerTurn != 4 ||
                         (CurrentPlayerTurn == 4 &&
                          (CurrentTurnQueenInPlay || (!CurrentTurnQueenInPlay && Enums.GetBugType(pieceName) == BugType.QueenBee))))
                    {
                        CalculateValidPlacements();
                        foreach (var placement in _cachedValidPlacements)
                        {
                            var move = new Move(pieceName, Position.NullPosition, placement);
                            moveSet.FastAdd(in move);
                        }
                    }
                }
                else if (pieceName != LastPieceMoved && CurrentTurnQueenInPlay && PieceIsOnTop(pieceName))
                {
                    // Piece is in play and not covered
                    if (CanMoveWithoutBreakingHive(pieceName))
                    {
                        // Look for basic valid moves of played pieces who can move
                        switch (Enums.GetBugType(pieceName))
                        {
                            case BugType.QueenBee:
                                GetValidQueenBeeMoves(pieceName, moveSet);
                                break;
                            case BugType.Spider:
                                GetValidSpiderMoves(pieceName, moveSet);
                                break;
                            case BugType.Beetle:
                                GetValidBeetleMoves(pieceName, moveSet);
                                break;
                            case BugType.Grasshopper:
                                GetValidGrasshopperMoves(pieceName, moveSet);
                                break;
                            case BugType.SoldierAnt:
                                GetValidSoldierAntMoves(pieceName, moveSet);
                                break;
                            case BugType.Mosquito:
                                GetValidMosquitoMoves(pieceName, moveSet, false);
                                break;
                            case BugType.Ladybug:
                                GetValidLadybugMoves(pieceName, moveSet);
                                break;
                            case BugType.Pillbug:
                                MoveSet newMoves = new MoveSet();
                                GetValidPillbugBasicMoves(pieceName, newMoves);
                                GetValidPillbugSpecialMoves(pieceName, newMoves);
                                moveSet.Add(newMoves);
                                break;
                        }
                    }
                    else
                    {
                        // Check for special ability moves
                        switch (Enums.GetBugType(pieceName))
                        {
                            case BugType.Mosquito:
                                GetValidMosquitoMoves(pieceName, moveSet, true);
                                break;
                            case BugType.Pillbug:
                                GetValidPillbugSpecialMoves(pieceName, moveSet);
                                break;
                        }
                    }
                }
            }
        }

        private void CalculateValidPlacements()
        {
            if (!_cachedValidPlacementsReady)
            {
                _cachedValidPlacements.Clear();

                // Loop through pieces of the same color as the current turn
                int startPiece = (int)(CurrentColor == PlayerColor.White ? PieceName.wQ : PieceName.bQ);
                int endPiece = (int)(CurrentColor == PlayerColor.White ? PieceName.bQ : PieceName.NumPieceNames);
                for (int pn = startPiece; pn < endPiece; pn++)
                {
                    var pieceName = (PieceName)pn;

                    if (PieceIsOnTop(pieceName))
                    {
                        var bottomPosition = _piecePositions[pn].GetBottom();
                        for (int dir = 0; dir < (int)Direction.NumDirections; dir++)
                        {
                            var neighbor = bottomPosition.GetNeighborAt((Direction)dir);
                            var neighborPiece = GetPieceOnTopAt(in neighbor);

                            if (neighborPiece != PieceName.INVALID)
                            {
                                // Neighbor has a piece
                                if (Enums.GetColor(neighborPiece) != CurrentColor)
                                {
                                    // Neighbor is the opposite color, skip the following direction
                                    dir++;
                                }
                            }
                            else
                            {
                                // Neighboring position is empty, verify its neighbors are empty or same color

                                var originalPieceDir = (dir + ((int)Direction.NumDirections / 2)) % (int)Direction.NumDirections;

                                bool validPlacement = true;
                                for (int dir2 = 0; dir2 < (int)Direction.NumDirections; dir2++)
                                {
                                    if (dir2 != originalPieceDir)
                                    {
                                        var surroundingPosition = neighbor.GetNeighborAt((Direction)dir2);
                                        var surroundingPiece = GetPieceOnTopAt(in surroundingPosition);
                                        if (surroundingPiece != PieceName.INVALID && Enums.GetColor(surroundingPiece) != CurrentColor)
                                        {
                                            validPlacement = false;
                                            break;
                                        }
                                    }
                                }

                                if (validPlacement)
                                {
                                    _cachedValidPlacements.Add(neighbor);
                                }
                            }
                        }
                    }
                }

                _cachedValidPlacementsReady = true;
            }
        }

        private void GetValidQueenBeeMoves(PieceName pieceName, MoveSet moveSet)
        {
            GetValidSlides(pieceName, moveSet, 1);
        }

        private void GetValidSpiderMoves(PieceName pieceName, MoveSet moveSet)
        {
            GetValidSlides(pieceName, moveSet, 3);
        }

        private void GetValidBeetleMoves(PieceName pieceName, MoveSet moveSet)
        {
            // Look in all directions
            for (int direction = 0; direction < (int)Direction.NumDirections; direction++)
            {
                var newPosition = _piecePositions[(int)pieceName].GetNeighborAt((Direction)direction);

                var topNeighbor = GetPieceOnTopAt(in newPosition);

                // Get positions to left and right or direction we're heading
                var leftOfTarget = Enums.LeftOf((Direction)direction);
                var rightOfTarget = Enums.RightOf((Direction)direction);
                var leftNeighborPosition = _piecePositions[(int)pieceName].GetNeighborAt(leftOfTarget);
                var rightNeighborPosition = _piecePositions[(int)pieceName].GetNeighborAt(rightOfTarget);

                var topLeftNeighbor = GetPieceOnTopAt(in leftNeighborPosition);
                var topRightNeighbor = GetPieceOnTopAt(in rightNeighborPosition);

                // At least one neighbor is present
                uint currentHeight = (uint)(_piecePositions[(int)pieceName].Stack + 1);
                uint destinationHeight = (uint)(topNeighbor != PieceName.INVALID ? _piecePositions[(int)topNeighbor].Stack + 1 : 0);

                uint topLeftNeighborHeight = (uint)(topLeftNeighbor != PieceName.INVALID ? _piecePositions[(int)topLeftNeighbor].Stack + 1 : 0);
                uint topRightNeighborHeight = (uint)(topRightNeighbor != PieceName.INVALID ? _piecePositions[(int)topRightNeighbor].Stack + 1 : 0);

                // "Take-off" beetle
                currentHeight--;

                if (!(currentHeight == 0 && destinationHeight == 0 && topLeftNeighborHeight == 0 && topRightNeighborHeight == 0))
                {
                    // Logic from http://boardgamegeek.com/wiki/page/Hive_FAQ#toc9
                    if (!(destinationHeight < topLeftNeighborHeight && destinationHeight < topRightNeighborHeight && currentHeight < topLeftNeighborHeight && currentHeight < topRightNeighborHeight))
                    {
                        var targetMove = new Move(pieceName, _piecePositions[(int)pieceName], new Position(newPosition.Q, newPosition.R, (int)destinationHeight));
                        moveSet.FastAdd(in targetMove);
                    }
                }
            }
        }

        private void GetValidGrasshopperMoves(PieceName pieceName, MoveSet moveSet)
        {
            var startingPosition = _piecePositions[(int)pieceName];

            for (int dir = 0; dir < (int)Direction.NumDirections; dir++)
            {
                var landingPosition = startingPosition.GetNeighborAt((Direction)dir);

                int distance = 0;
                while (HasPieceAt(in landingPosition))
                {
                    // Jump one more in the same direction
                    landingPosition = landingPosition.GetNeighborAt((Direction)dir);
                    distance++;
                }

                if (distance > 0)
                {
                    // Can only move if there's at least one piece in the way
                    var move = new Move(pieceName, startingPosition, landingPosition);
                    moveSet.FastAdd(in move);
                }
            }
        }

        private void GetValidSoldierAntMoves(PieceName pieceName, MoveSet moveSet)
        {
            GetValidSlides(pieceName, moveSet);
        }

        private void GetValidMosquitoMoves(PieceName pieceName, MoveSet moveSet, bool specialAbilityOnly)
        {
            var position = GetPosition(pieceName);

            if (position.Stack > 0 && !specialAbilityOnly)
            {
                // Mosquito on top acts like a beetle
                GetValidBeetleMoves(pieceName, moveSet);
                return;
            }

            var bugTypesEvaluated = new bool[(int)BugType.NumBugTypes];

            for (int dir = 0; dir < (int)Direction.NumDirections; dir++)
            {
                var neighborPosition = position.GetNeighborAt((Direction)dir);
                var neighborPieceName = GetPieceOnTopAt(in neighborPosition);

                var neighborBugType = Enums.GetBugType(neighborPieceName);

                if (neighborPieceName != PieceName.INVALID && !bugTypesEvaluated[(int)neighborBugType])
                {
                    var newMoves = new MoveSet();
                    if (specialAbilityOnly)
                    {
                        if (neighborBugType == BugType.Pillbug)
                        {
                            GetValidPillbugSpecialMoves(pieceName, newMoves);
                        }
                    }
                    else
                    {
                        switch (neighborBugType)
                        {
                            case BugType.QueenBee:
                                GetValidQueenBeeMoves(pieceName, newMoves);
                                break;
                            case BugType.Spider:
                                GetValidSpiderMoves(pieceName, newMoves);
                                break;
                            case BugType.Beetle:
                                GetValidBeetleMoves(pieceName, newMoves);
                                break;
                            case BugType.Grasshopper:
                                GetValidGrasshopperMoves(pieceName, newMoves);
                                break;
                            case BugType.SoldierAnt:
                                GetValidSoldierAntMoves(pieceName, newMoves);
                                break;
                            case BugType.Ladybug:
                                GetValidLadybugMoves(pieceName, newMoves);
                                break;
                            case BugType.Pillbug:
                                GetValidPillbugBasicMoves(pieceName, newMoves);
                                GetValidPillbugSpecialMoves(pieceName, newMoves);
                                break;
                        }
                    }

                    moveSet.Add(newMoves);

                    bugTypesEvaluated[(int)neighborBugType] = true;
                }
            }
        }

        private void GetValidLadybugMoves(PieceName pieceName, MoveSet moveSet)
        {
            var startingPosition = GetPosition(pieceName);

            var firstMoves = new MoveSet();
            GetValidBeetleMoves(pieceName, firstMoves);

            foreach (var firstMove in firstMoves)
            {
                if (firstMove.Destination.Stack > 0)
                {
                    SetPosition(pieceName, in firstMove.Destination, false);

                    var secondMoves = new MoveSet();
                    GetValidBeetleMoves(pieceName, secondMoves);

                    foreach (var secondMove in secondMoves)
                    {
                        if (secondMove.Destination.Stack > 0)
                        {
                            SetPosition(pieceName, in secondMove.Destination, false);

                            var thirdMoves = new MoveSet();
                            GetValidBeetleMoves(pieceName, thirdMoves);

                            foreach (var thirdMove in thirdMoves)
                            {
                                if (thirdMove.Destination.Stack == 0 && thirdMove.Destination != startingPosition)
                                {
                                    var finalMove = new Move(pieceName, startingPosition, thirdMove.Destination);
                                    moveSet.Add(in finalMove);
                                }
                            }

                            SetPosition(pieceName, in firstMove.Destination, false);
                        }
                    }

                    SetPosition(pieceName, in startingPosition, false);
                }
            }
        }

        private void GetValidPillbugBasicMoves(PieceName pieceName, MoveSet moveSet)
        {
            GetValidSlides(pieceName, moveSet, 1);
        }

        private void GetValidPillbugSpecialMoves(PieceName pieceName, MoveSet moveSet)
        {
            var position = GetPosition(pieceName);
            var positionAboveTargetPiece = position.GetAbove();

            for (int dir = 0; dir < (int)Direction.NumDirections; dir++)
            {
                var neighborPosition = position.GetNeighborAt((Direction)dir);
                var neighborPieceName = GetPieceAt(in neighborPosition);

                if (neighborPieceName != PieceName.INVALID && neighborPieceName != LastPieceMoved &&
                    !HasPieceAt(in neighborPosition, Direction.Above) && CanMoveWithoutBreakingHive(neighborPieceName))
                {
                    // Piece can be moved
                    var firstMove = new Move(neighborPieceName, neighborPosition, positionAboveTargetPiece);
                    var firstMoves = new MoveSet();
                    GetValidBeetleMoves(neighborPieceName, firstMoves);
                    if (firstMoves.Contains(in firstMove))
                    {
                        // Piece can be moved on top
                        SetPosition(neighborPieceName, in positionAboveTargetPiece, false);

                        var secondMoves = new MoveSet();
                        GetValidBeetleMoves(neighborPieceName, secondMoves);

                        foreach (var secondMove in secondMoves)
                        {
                            if (secondMove.Destination.Stack == 0 && secondMove.Destination != neighborPosition)
                            {
                                var finalMove = new Move(neighborPieceName, neighborPosition, secondMove.Destination);
                                moveSet.Add(in finalMove);
                            }
                        }

                        SetPosition(neighborPieceName, in neighborPosition, false);
                    }
                }
            }
        }

        private void GetValidSlides(PieceName pieceName, MoveSet moveSet, int fixedRange = 0)
        {
            var startingPosition = GetPosition(pieceName);
            SetPosition(pieceName, in Position.NullPosition, false);

            if (fixedRange > 0)
            {
                GetValidSlides(pieceName, moveSet, in startingPosition, in startingPosition, in startingPosition, fixedRange, fixedRange == 1);
            }
            else
            {
                GetValidSlides(pieceName, moveSet, in startingPosition, in startingPosition, in startingPosition);
            }

            SetPosition(pieceName, in startingPosition, false);
        }

        private void GetValidSlides(PieceName pieceName, MoveSet moveSet, in Position startingPosition, in Position lastPosition, in Position currentPosition)
        {
            for (int slideDirection = 0; slideDirection < (int)Direction.NumDirections; slideDirection++)
            {
                var slidePosition = currentPosition.GetNeighborAt((Direction)slideDirection);

                if (slidePosition != lastPosition && slidePosition != startingPosition && !HasPieceAt(in slidePosition))
                {
                    // Slide position is open
                    if (HasPieceAt(in currentPosition, Enums.RightOf((Direction)slideDirection)) != HasPieceAt(in currentPosition, Enums.LeftOf((Direction)slideDirection)))
                    {
                        // Can slide into slide position
                        var move = new Move(pieceName, startingPosition, slidePosition);
                        if (moveSet.Add(in move))
                        {
                            GetValidSlides(pieceName, moveSet, in startingPosition, in currentPosition, in slidePosition);
                        }
                    }
                }
            }
        }

        private void GetValidSlides(PieceName pieceName, MoveSet moveSet, in Position startingPosition, in Position lastPosition, in Position currentPosition, int remainingSlides, bool fastAdd)
        {
            if (remainingSlides == 0)
            {
                var move = new Move(pieceName, startingPosition, currentPosition);
                if (fastAdd)
                {
                    moveSet.FastAdd(in move);
                }
                else
                {
                    moveSet.Add(in move);
                }
            }
            else
            {
                for (int slideDirection = 0; slideDirection < (int)Direction.NumDirections; slideDirection++)
                {
                    var slidePosition = currentPosition.GetNeighborAt((Direction)slideDirection);
                    if (slidePosition != lastPosition && slidePosition != startingPosition && !HasPieceAt(in slidePosition))
                    {
                        // Slide position is open
                        if (HasPieceAt(in currentPosition, Enums.RightOf((Direction)slideDirection)) != HasPieceAt(in currentPosition, Enums.LeftOf((Direction)slideDirection)))
                        {
                            // Can slide into slide position
                            GetValidSlides(pieceName, moveSet, in startingPosition, in currentPosition, in slidePosition, remainingSlides - 1, fastAdd);
                        }
                    }
                }
            }
        }

        internal void TrustedPlay(in Move move, string moveStr = "")
        {
            BoardHistory.Add(in move, moveStr);

            if (move != Move.PassMove)
            {
                SetPosition(move.PieceName, in move.Destination, true);
            }

            CurrentTurn++;
            LastPieceMoved = move.PieceName;
        }

        internal bool PlacingPieceInOrder(PieceName pieceName)
        {
            if (_piecePositions[(int)pieceName].Stack < 0)
            {
                switch (pieceName)
                {
                    case PieceName.wS2:
                    case PieceName.wB2:
                    case PieceName.wG2:
                    case PieceName.wG3:
                    case PieceName.wA2:
                    case PieceName.wA3:
                    case PieceName.bS2:
                    case PieceName.bB2:
                    case PieceName.bG2:
                    case PieceName.bG3:
                    case PieceName.bA2:
                    case PieceName.bA3:
                        return _piecePositions[(int)pieceName - 1].Stack >= 0;
                }
            }

            return true;
        }

        public ref Position GetPosition(PieceName pieceName)
        {
            return ref _piecePositions[(int)pieceName];
        }

        internal void SetPosition(PieceName pieceName, in Position position, bool updateZobrist)
        {
            var oldPosition = GetPosition(pieceName);

            if (oldPosition.Stack >= 0)
            {
                if (updateZobrist)
                {
                    _zobristHash.TogglePiece(pieceName, in oldPosition);
                }

                _pieceGrid[(Position.BoardSize / 2) + oldPosition.Q, (Position.BoardSize / 2) + oldPosition.R, oldPosition.Stack] = PieceName.INVALID;
            }

            _piecePositions[(int)pieceName] = position;

            if (position.Stack >= 0)
            {
                if (updateZobrist)
                {
                    _zobristHash.TogglePiece(pieceName, in position);
                }

                _pieceGrid[(Position.BoardSize / 2) + position.Q, (Position.BoardSize / 2) + position.R, position.Stack] = pieceName;
            }
        }

        private PieceName GetPieceAt(in Position position)
        {
            return _pieceGrid[(Position.BoardSize / 2) + position.Q, (Position.BoardSize / 2) + position.R, position.Stack];
        }

        private PieceName GetPieceAt(in Position position, Direction direction)
        {
            return _pieceGrid[(Position.BoardSize / 2) + position.Q + Position.NeighborDeltas[(int)direction, 0], (Position.BoardSize / 2) + position.R + Position.NeighborDeltas[(int)direction, 1], position.Stack + Position.NeighborDeltas[(int)direction, 2]];
        }

        public PieceName GetPieceOnTopAt(in Position position)
        {
            var topPieceName = PieceName.INVALID;

            for (int stack = 0; stack < Position.BoardStackSize; stack++)
            {
                var pieceName = _pieceGrid[(Position.BoardSize / 2) + position.Q, (Position.BoardSize / 2) + position.R, stack];
                if (pieceName == PieceName.INVALID)
                {
                    break;
                }
                topPieceName = pieceName;
            }

            return topPieceName;
        }

        private bool HasPieceAt(in Position position)
        {
            return GetPieceAt(in position) != PieceName.INVALID;
        }

        private bool HasPieceAt(in Position position, Direction direction)
        {
            return GetPieceAt(in position, direction) != PieceName.INVALID;
        }

        public bool PieceInHand(PieceName pieceName)
        {
            return _piecePositions[(int)pieceName].Stack < 0;
        }

        public bool PieceInPlay(PieceName pieceName)
        {
            return _piecePositions[(int)pieceName].Stack >= 0;
        }

        private bool PieceIsOnTop(PieceName pieceName)
        {
            return PieceInPlay(pieceName) && !HasPieceAt(in _piecePositions[(int)pieceName], Direction.Above);
        }

        internal bool CanMoveWithoutBreakingHive(PieceName pieceName)
        {
            int pieceIndex = (int)pieceName;
            if (_piecePositions[pieceIndex].Stack == 0)
            {
                // Try gaps heurestic
                int gaps = 0;
                bool? lastHasPiece = null;
                for (int dir = 0; dir < (int)Direction.NumDirections; dir++)
                {
                    bool hasPiece = HasPieceAt(in _piecePositions[pieceIndex], (Direction)dir);
                    if (lastHasPiece.HasValue && lastHasPiece.Value != hasPiece)
                    {
                        gaps++;
                        if (gaps > 2)
                        {
                            break;
                        }
                    }
                    lastHasPiece = hasPiece;
                }

                if (gaps <= 2)
                {
                    return true;
                }

                var startingPosition = _piecePositions[pieceIndex];

                // Temporarily remove piece from board
                SetPosition(pieceName, in Position.NullPosition, false);

                // Determine if the hive is broken
                bool isOneHive = IsOneHive();

                // Return piece to the board
                SetPosition(pieceName, in startingPosition, false);

                return isOneHive;
            }
            return true;
        }

        internal bool IsOneHive()
        {
            int piecesVisited = 0;

            // Find a piece on the board to start checking
            var startingPiece = PieceName.INVALID;
            for (int pn = 0; pn < (int)PieceName.NumPieceNames; pn++)
            {
                if (PieceInHand((PieceName)pn))
                {
                    _partOfHive[pn] = true;
                    piecesVisited++;
                }
                else
                {
                    _partOfHive[pn] = false;
                    if (startingPiece == PieceName.INVALID && _piecePositions[pn].Stack == 0)
                    {
                        // Save off a starting piece on the bottom
                        startingPiece = (PieceName)pn;
                        _partOfHive[pn] = true;
                        piecesVisited++;
                    }
                }
            }

            // There is at least one piece on the board
            if (startingPiece != PieceName.INVALID && piecesVisited < (int)PieceName.NumPieceNames)
            {
                _piecesToLookAt.Enqueue(startingPiece);

                while (_piecesToLookAt.Count > 0)
                {
                    var currentPiece = _piecesToLookAt.Dequeue();

                    var currentPosition = _piecePositions[(int)currentPiece];

                    // Check all pieces at this stack level
                    for (int dir = 0; dir < (int)Direction.NumDirections; dir++)
                    {
                        var neighborPiece = GetPieceAt(in currentPosition, (Direction)dir);
                        if (neighborPiece != PieceName.INVALID && !_partOfHive[(int)neighborPiece])
                        {
                            _piecesToLookAt.Enqueue(neighborPiece);
                            _partOfHive[(int)neighborPiece] = true;
                            piecesVisited++;
                        }
                    }

                    // Check for all pieces above this one
                    var pieceAbove = GetPieceAt(in currentPosition, Direction.Above);
                    while (PieceName.INVALID != pieceAbove)
                    {
                        _partOfHive[(int)pieceAbove] = true;
                        piecesVisited++;
                        pieceAbove = GetPieceAt(in _piecePositions[(int)pieceAbove], Direction.Above);
                    }
                }
            }

            return piecesVisited == (int)PieceName.NumPieceNames;
        }

        private int CountNeighbors(PieceName pieceName)
        {
            return CountNeighbors(pieceName, out _, out _);
        }

        private int CountNeighbors(PieceName pieceName, out int friendlyCount, out int enemyCount)
        {
            friendlyCount = 0;
            enemyCount = 0;

            if (PieceInPlay(pieceName))
            {
                var pieceColor = Enums.GetColor(pieceName);

                for (int dir = 0; dir < (int)Direction.NumDirections; dir++)
                {
                    var neighbor = GetPieceAt(in _piecePositions[(int)pieceName], (Direction)dir);
                    if (neighbor != PieceName.INVALID)
                    {
                        if (pieceColor == Enums.GetColor(neighbor))
                        {
                            friendlyCount++;
                        }
                        else
                        {
                            enemyCount++;
                        }
                    }
                }
            }

            return friendlyCount + enemyCount;
        }

        private void ResetState()
        {
            bool whiteQueenSurrounded = CountNeighbors(PieceName.wQ) == 6;
            bool blackQueenSurrounded = CountNeighbors(PieceName.bQ) == 6;

            if (whiteQueenSurrounded && blackQueenSurrounded)
            {
                BoardState = BoardState.Draw;
            }
            else if (whiteQueenSurrounded)
            {
                BoardState = BoardState.BlackWins;
            }
            else if (blackQueenSurrounded)
            {
                BoardState = BoardState.WhiteWins;
            }
            else
            {
                BoardState = CurrentTurn == 0 ? BoardState.NotStarted : BoardState.InProgress;
            }
        }

        private void ResetCaches()
        {
            _cachedValidPlacementsReady = false;
            _cachedValidMoves = null;
            _cachedEnemyQueenNeighbors = null;
        }
    }

    public class InvalidMoveException : Exception
    {
        public readonly Move Move;

        public InvalidMoveException(Move move) : this(move, "You can't move that piece there.") { }

        public InvalidMoveException(Move move, string message) : base(message)
        {
            Move = move;
        }
    }
}
