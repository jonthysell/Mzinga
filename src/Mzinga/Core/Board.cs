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
                // Only update when Pillbug is enabled
                if (Enums.BugTypeIsEnabledForGameType(BugType.Pillbug, GameType))
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
        }
        private PieceName _lastPieceMoved = PieceName.INVALID;

        public ulong ZobristKey => _zobristHash.Value;

        private readonly Position[] m_piecePositions = new Position[(int)PieceName.NumPieceNames];
        private readonly PieceName[,,] m_pieceGrid = new PieceName[Position.BoardSize, Position.BoardSize, Position.BoardStackSize];

        private bool m_cachedValidPlacementsReady = false;
        private readonly PositionSet m_cachedValidPlacements = new PositionSet();

        private readonly PositionSet m_visitedPositions = new PositionSet();
        
        private PositionSet? _cachedEnemyQueenNeighbors = null;

        private readonly bool[] m_partOfHive = new bool[(int)PieceName.NumPieceNames];
        private readonly Queue<PieceName> m_piecesToLookAt = new Queue<PieceName>((int)PieceName.NumPieceNames);

        private readonly ZobristHash _zobristHash = new ZobristHash();

        public static Board ParseGameString(string gameStr)
        {
            var split = gameStr.Split(';');

            GameType gameType = Enum.Parse<GameType>(split[0].Replace("+", ""));

            Board board = new Board(gameType);

            for (int i = 3; i < split.Length; i++)
            {
                if (!board.TryParseMove(split[i], out Move move, out string parsedMoveString) || !board.TryPlayMove(move, parsedMoveString))
                {
                    throw new ArgumentException("Unable to parse GameString.", nameof(gameStr));
                }
            }

            return board;
        }

        public static bool TryParseGameString(string gameStr, out Board? board)
        {
            try
            {
                board = ParseGameString(gameStr);
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
                m_piecePositions[pn] = Position.NullPosition;
            }

            for (int q = 0; q < m_pieceGrid.GetLength(0); q++)
            {
                for (int r = 0; r < m_pieceGrid.GetLength(1); r++)
                {
                    for (int stack = 0; stack < m_pieceGrid.GetLength(2); stack++)
                    {
                        m_pieceGrid[q, r, stack] = PieceName.INVALID;
                    }
                }
            }
        }

        public string GetGameString()
        {
            var sb = new StringBuilder();

            sb.Append($"{GameType};{BoardState};{CurrentColor}[{CurrentPlayerTurn}]");

            foreach (var item in BoardHistory)
            {
                sb.Append($";{item.MoveString}");
            }

            return sb.ToString();
        }

        public MoveSet GetValidMoves()
        {
            var validMoves = new MoveSet();

            if (GameInProgress)
            {
                for (int pn = (int)(CurrentColor == PlayerColor.White ? PieceName.wQ : PieceName.bQ); pn < (int)(CurrentColor == PlayerColor.White ? PieceName.bQ : PieceName.NumPieceNames); pn++)
                {
                    GetValidMoves((PieceName)pn, validMoves);
                }

                if (validMoves.Count == 0)
                {
                    validMoves.Add(Move.PassMove);
                }
            }

            return validMoves;
        }

        public void Play(Move move, string moveString = "")
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

            if (!GetValidMoves().Contains(move))
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

            TrustedPlay(move, moveString);
        }

        public void Pass()
        {
            Move pass = Move.PassMove;

            if (GameIsOver)
            {
                throw new InvalidMoveException(pass, "You can't pass, the game is over.");
            }

            if (!GetValidMoves().Contains(Move.PassMove))
            {
                throw new InvalidMoveException(pass, "You can't pass when you have valid moves.");
            }

            TrustedPlay(Move.PassMove, Move.PassString);
        }

        public bool TryPlayMove(Move move, string moveString = "")
        {
            var validMoves = GetValidMoves();

            if (validMoves.Contains(move))
            {
                TrustedPlay(move, moveString);
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
                    SetPosition(lastMove.Move.PieceName, lastMove.Move.Source, true);
                }

                BoardHistory.UndoLast();

                CurrentTurn--;

                ResetState();
                ResetCaches();

                return true;
            }

            return false;
        }

        public string GetMoveString(Move move)
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
                for (int dir = 0; dir < (int)Direction.NumDirections; dir++)
                {
                    Position neighborPosition = move.Destination.GetNeighborAt((Direction)dir);
                    PieceName neighbor = GetPieceOnTopAt(in neighborPosition);

                    if (neighbor != PieceName.INVALID && neighbor != move.PieceName)
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
            }

            if (endPiece != "")
            {
                return $"{startPiece} {endPiece}";
            }

            throw new ArgumentOutOfRangeException(nameof(move));
        }

        public bool TryGetMoveString(Move move, out string result)
        {
            try
            {
                result = GetMoveString(move);
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

                Position source = m_piecePositions[(int)startPiece];

                Position destination = Position.OriginPosition;

                if (endPiece != PieceName.INVALID)
                {
                    Position targetPosition = m_piecePositions[(int)endPiece];

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

        public bool IsNoisyMove(Move move)
        {
            if (move == Move.PassMove)
            {
                return false;
            }

            if (null == _cachedEnemyQueenNeighbors)
            {
                _cachedEnemyQueenNeighbors = new PositionSet();

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

            Task<long?> task = CalculatePerftAsync(depth, cts.Token);
            task.Wait();

            return task.Result ?? 0;
        }

        public async Task<long?> CalculatePerftAsync(int depth, CancellationToken token)
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

                TrustedPlay(move);

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

        public async Task<long?> ParallelPerftAsync(int depth, int maxThreads, CancellationToken token)
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

            long? nodes = await Task.Run(() =>
            {
                ParallelOptions po = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(1, maxThreads)
                };

                long n = 0;
                ParallelLoopResult loopResult = Parallel.ForEach(validMoves, po, async (move, state) =>
                {
                    if (token.IsCancellationRequested)
                    {
                        state.Stop();
                        return;
                    }

                    var clone = Clone();
                    clone.TrustedPlay(move);
                    long? value = await clone.CalculatePerftAsync(depth - 1, token);

                    if (!value.HasValue)
                    {
                        state.Stop();
                        return;
                    }

                    Interlocked.Add(ref n, value.Value);
                });

                return loopResult.IsCompleted && !token.IsCancellationRequested ? (long?)n : null;
            });

            return nodes;
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
                var moveSet = new MoveSet();   
                SetCurrentPlayerMetrics(boardMetrics, moveSet);

                // Save off current valid placements since we'll be returning to it
                //PositionSet? validPlacements = m_cachedValidPlacements;
                //m_cachedValidPlacements = null;

                PositionSet? enemyQueenNeighbors = _cachedEnemyQueenNeighbors;
                _cachedEnemyQueenNeighbors = null;

                PieceName lastPieceMoved = _lastPieceMoved;
                _lastPieceMoved = PieceName.INVALID;

                // Spoof going to the next turn to get the opponent's metrics
                CurrentTurn++;
                moveSet.Clear();
                SetCurrentPlayerMetrics(boardMetrics, moveSet);
                CurrentTurn--;

                // Returned, so reload saved valid moves/placements into cache
                _lastPieceMoved = lastPieceMoved;
                _cachedEnemyQueenNeighbors = enemyQueenNeighbors;
                //m_cachedValidPlacements = validPlacements;
            }

            return boardMetrics;
        }

        private void SetCurrentPlayerMetrics(BoardMetrics boardMetrics, MoveSet moveSet)
        {
            bool pullbugEnabled = Enums.BugTypeIsEnabledForGameType(BugType.Pillbug, GameType);
            bool mosquitoEnabled = Enums.BugTypeIsEnabledForGameType(BugType.Mosquito, GameType);

            var pillbugMoves = new MoveSet();
            GetValidMoves(CurrentColor == PlayerColor.White ? PieceName.wP : PieceName.bP, pillbugMoves);

            var mosquitoMoves = new MoveSet();
            GetValidMoves(CurrentColor == PlayerColor.White ? PieceName.wM : PieceName.bM, mosquitoMoves);

            for (int pn = (int)(CurrentColor == PlayerColor.White ? PieceName.wQ : PieceName.bQ); pn < (int)(CurrentColor == PlayerColor.White ? PieceName.bQ : PieceName.NumPieceNames); pn++)
            {
                var pieceName = (PieceName)pn;

                if (Enums.PieceNameIsEnabledForGameType(pieceName, GameType))
                {
                    if (PieceInPlay(pieceName))
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

                    if (isPinned && pullbugEnabled)
                    {
                        bool pullbugCanMove = pillbugMoves.Contains(pieceName);
                        bool mosquitoCanMove = mosquitoEnabled && mosquitoMoves.Contains(pieceName);

                        if (Enums.GetBugType(pieceName) == BugType.Pillbug)
                        {
                            // Check if the current player's mosquito can move it
                            isPinned = !mosquitoCanMove;

                        }
                        else if (Enums.GetBugType(pieceName) == BugType.Mosquito)
                        {
                            // Check if the current player's pillbug can move it
                            isPinned = !pullbugCanMove;
                        }
                        else
                        {
                            // Check if the current player's pillbug or mosquito can move it
                            isPinned = !(mosquitoCanMove || pullbugCanMove);
                        }
                    }

                    boardMetrics[pieceName].IsPinned = isPinned ? 1 : 0;
                    boardMetrics[pieceName].IsCovered = PieceIsOnTop(pieceName) ? 0 : 1;

                    CountNeighbors(pieceName, out boardMetrics[pieceName].FriendlyNeighborCount, out boardMetrics[pieceName].EnemyNeighborCount);
                }
            }
        }

        private bool IsPinned(PieceName pieceName, MoveSet moveSet, out int noisyCount, out int quietCount)
        {
            noisyCount = 0;
            quietCount = 0;

            int previousMoves = moveSet.Count;
            GetValidMoves(pieceName, moveSet);
            
            if (moveSet.Count > previousMoves)
            {
                foreach (var move in moveSet)
                {
                    if (move.PieceName == pieceName)
                    {
                        if (IsNoisyMove(move))
                        {
                            noisyCount++;
                        }
                        else
                        {
                            quietCount++;
                        }
                    }
                }

                return false;

            }

            return true;
        }

        public Board Clone()
        {
            var board = new Board(GameType);
            foreach (var item in BoardHistory)
            {
                board.TrustedPlay(item.Move, item.MoveString);
            }

            return board;
        }

        private void GetValidMoves(PieceName pieceName, MoveSet moveSet)
        {
            if (Enums.PieceNameIsEnabledForGameType(pieceName, GameType) && GameInProgress && CurrentColor == Enums.GetColor(pieceName) && PlacingPieceInOrder(pieceName))
            {
                int pieceIndex = (int)pieceName;

                if (CurrentTurn == 0)
                {
                    // First turn by white
                    if (pieceName != PieceName.wQ)
                    {
                        moveSet.Add(new Move(pieceName, m_piecePositions[pieceIndex], Position.OriginPosition));
                    }
                }
                else if (CurrentTurn == 1)
                {
                    // First turn by black
                    if (pieceName != PieceName.bQ)
                    {
                        CalculateValidPlacements();
                        foreach (var placement in m_cachedValidPlacements)
                        {
                            moveSet.Add(new Move(pieceName, m_piecePositions[pieceIndex], placement));
                        }
                    }
                }
                else if (PieceInHand(pieceName))
                {
                    // Piece is in hand
                    if ((CurrentPlayerTurn != 4 ||
                         (CurrentPlayerTurn == 4 &&
                          (CurrentTurnQueenInPlay || (!CurrentTurnQueenInPlay && Enums.GetBugType(pieceName) == BugType.QueenBee)))))
                    {
                        CalculateValidPlacements();
                        foreach (var placement in m_cachedValidPlacements)
                        {
                            moveSet.Add(new Move(pieceName, m_piecePositions[pieceIndex], placement));
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
                                GetValidPillbugBasicMoves(pieceName, moveSet);
                                GetValidPillbugSpecialMoves(pieceName, moveSet);
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
            if (!m_cachedValidPlacementsReady)
            {
                m_cachedValidPlacements.Clear();

                if (CurrentTurn == 0)
                {
                    m_cachedValidPlacements.Add(Position.OriginPosition);
                }
                else if (CurrentTurn == 1)
                {
                    for (int dir = 0; dir < (int)Direction.NumDirections; dir++)
                    {
                        m_cachedValidPlacements.Add(Position.OriginPosition.GetNeighborAt((Direction)dir));
                    }
                }
                else
                {
                    //m_visitedPositions.Clear();

                    for (int pn = (int)(CurrentColor == PlayerColor.White ? PieceName.wQ : PieceName.bQ); pn < (int)(CurrentColor == PlayerColor.White ? PieceName.bQ : PieceName.NumPieceNames); pn++)
                    {
                        var pieceName = (PieceName)pn;

                        if (PieceIsOnTop(pieceName))
                        {
                            var bottomPosition = m_piecePositions[pn].GetBottom();
                            //if (m_visitedPositions.Add(bottomPosition))
                            {
                                for (int dir = 0; dir < (int)Direction.NumDirections; dir++)
                                {
                                    var neighbor = bottomPosition.GetNeighborAt((Direction)dir);

                                    if (/*!m_visitedPositions.Contains(neighbor) &&*/ !HasPieceAt(in neighbor))
                                    {
                                        //m_visitedPositions.Add(neighbor);

                                        // Neighboring position is a potential, verify its neighbors are empty or same color

                                        bool validPlacement = true;
                                        for (int dir2 = 0; dir2 < (int)Direction.NumDirections; dir2++)
                                        {
                                            var surroundingPosition = neighbor.GetNeighborAt((Direction)dir2);
                                            var surroundingPiece = GetPieceOnTopAt(in surroundingPosition);
                                            if (surroundingPiece != PieceName.INVALID && Enums.GetColor(surroundingPiece) != CurrentColor)
                                            {
                                                validPlacement = false;
                                                break;
                                            }
                                        }

                                        if (validPlacement)
                                        {
                                            m_cachedValidPlacements.Add(neighbor);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                m_cachedValidPlacementsReady = true;
            }
        }

        private void GetValidQueenBeeMoves(PieceName pieceName, MoveSet moveSet)
        {
            GetValidSlides(pieceName, moveSet, 1);
        }

        private void GetValidSpiderMoves(PieceName pieceName, MoveSet moveSet)
        {
            // Get all slides up to 3 spots away
            var upToThree = new MoveSet();
            GetValidSlides(pieceName, upToThree, 3);

            foreach (var move in upToThree)
            {
                if (CanSlideToPositionInExactRange(pieceName, move.Destination, 3))
                {
                    moveSet.Add(move);
                }
            }
        }

        private void GetValidBeetleMoves(PieceName pieceName, MoveSet moveSet)
        {
            // Look in all directions
            for (int direction = 0; direction < (int)Direction.NumDirections; direction++)
            {
                var newPosition = m_piecePositions[(int)pieceName].GetNeighborAt((Direction)direction);

                var topNeighbor = GetPieceOnTopAt(in newPosition);

                // Get positions to left and right or direction we're heading
                var leftOfTarget = Enums.LeftOf((Direction)direction);
                var rightOfTarget = Enums.RightOf((Direction)direction);
                var leftNeighborPosition = m_piecePositions[(int)pieceName].GetNeighborAt(leftOfTarget);
                var rightNeighborPosition = m_piecePositions[(int)pieceName].GetNeighborAt(rightOfTarget);

                var topLeftNeighbor = GetPieceOnTopAt(in leftNeighborPosition);
                var topRightNeighbor = GetPieceOnTopAt(in rightNeighborPosition);

                // At least one neighbor is present
                uint currentHeight = (uint)(m_piecePositions[(int)pieceName].Stack + 1);
                uint destinationHeight = (uint)(topNeighbor != PieceName.INVALID ? m_piecePositions[(int)topNeighbor].Stack + 1 : 0);

                uint topLeftNeighborHeight = (uint)(topLeftNeighbor != PieceName.INVALID ? m_piecePositions[(int)topLeftNeighbor].Stack + 1 : 0);
                uint topRightNeighborHeight = (uint)(topRightNeighbor != PieceName.INVALID ? m_piecePositions[(int)topRightNeighbor].Stack + 1 : 0);

                // "Take-off" beetle
                currentHeight--;

                if (!(currentHeight == 0 && destinationHeight == 0 && topLeftNeighborHeight == 0 && topRightNeighborHeight == 0))
                {
                    // Logic from http://boardgamegeek.com/wiki/page/Hive_FAQ#toc9
                    if (!(destinationHeight < topLeftNeighborHeight && destinationHeight < topRightNeighborHeight && currentHeight < topLeftNeighborHeight && currentHeight < topRightNeighborHeight))
                    {
                        var targetMove = new Move
                        (
                            pieceName,
                            m_piecePositions[(int)pieceName],
                            new Position
                            (
                                newPosition.Q,
                                newPosition.R,
                                (int)destinationHeight
                            )
                        );
                        moveSet.Add(targetMove);
                    }
                }
            }
        }

        private void GetValidGrasshopperMoves(PieceName pieceName, MoveSet moveSet)
        {
            var startingPosition = m_piecePositions[(int)pieceName];

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
                    moveSet.Add(new Move(pieceName, startingPosition, landingPosition));
                }
            }
        }

        private void GetValidSoldierAntMoves(PieceName pieceName, MoveSet moveSet)
        {
            GetValidSlides(pieceName, moveSet, -1);
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

                if (neighborPieceName != PieceName.INVALID && !bugTypesEvaluated[(int)(neighborBugType)])
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

                    if (newMoves.Count > 0)
                    {
                        foreach (var move in newMoves)
                        {
                            moveSet.Add(move);
                        }
                    }

                    bugTypesEvaluated[(int)(neighborBugType)] = true;
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
                    SetPosition(pieceName, firstMove.Destination, false);

                    var secondMoves = new MoveSet();
                    GetValidBeetleMoves(pieceName, secondMoves);

                    foreach (var secondMove in secondMoves)
                    {
                        if (secondMove.Destination.Stack > 0)
                        {
                            SetPosition(pieceName, secondMove.Destination, false);

                            var thirdMoves = new MoveSet();
                            GetValidBeetleMoves(pieceName, thirdMoves);

                            foreach (var thirdMove in thirdMoves)
                            {
                                if (thirdMove.Destination.Stack == 0 && thirdMove.Destination != startingPosition)
                                {
                                    var finalMove = new Move(pieceName, startingPosition, thirdMove.Destination);
                                    moveSet.Add(finalMove);
                                }
                            }

                            SetPosition(pieceName, firstMove.Destination, false);
                        }
                    }

                    SetPosition(pieceName, startingPosition, false);
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
                    if (firstMoves.Contains(firstMove))
                    {
                        // Piece can be moved on top
                        SetPosition(neighborPieceName, positionAboveTargetPiece, false);

                        var secondMoves = new MoveSet();
                        GetValidBeetleMoves(neighborPieceName, secondMoves);

                        foreach (var secondMove in secondMoves)
                        {
                            if (secondMove.Destination.Stack == 0 && secondMove.Destination != neighborPosition)
                            {
                                var finalMove = new Move(neighborPieceName, neighborPosition, secondMove.Destination);
                                moveSet.Add(finalMove);
                            }
                        }

                        SetPosition(neighborPieceName, neighborPosition, false);
                    }
                }
            }
        }

        private void GetValidSlides(PieceName pieceName, MoveSet moveSet, int maxRange)
        {
            var startingPosition = m_piecePositions[(int)pieceName];

            m_visitedPositions.Clear();
            m_visitedPositions.Add(startingPosition);

            SetPosition(pieceName, Position.NullPosition, false);
            GetValidSlides(pieceName, moveSet, startingPosition, startingPosition, 0, maxRange);
            SetPosition(pieceName, startingPosition, false);
        }

        private void GetValidSlides(PieceName pieceName, MoveSet moveSet, Position startingPosition, Position currentPosition, int currentRange, int maxRange)
        {
            if (maxRange < 0 || currentRange < maxRange)
            {
                for (int slideDirection = 0; slideDirection < (int)Direction.NumDirections; slideDirection++)
                {
                    var slidePosition = currentPosition.GetNeighborAt((Direction)slideDirection);

                    if (!m_visitedPositions.Contains(slidePosition) && !HasPieceAt(in slidePosition))
                    {
                        // Slide position is open

                        var left = Enums.LeftOf((Direction)slideDirection);
                        var right = Enums.RightOf((Direction)slideDirection);

                        if (HasPieceAt(in currentPosition, right) != HasPieceAt(in currentPosition, left))
                        {
                            // Can slide into slide position
                            var move = new Move(pieceName, startingPosition, slidePosition);

                            if (moveSet.Add(move))
                            {
                                m_visitedPositions.Add(slidePosition);
                                GetValidSlides(pieceName, moveSet, startingPosition, slidePosition, currentRange + 1, maxRange);
                            }
                        }
                    }
                }
            }
        }

        private bool CanSlideToPositionInExactRange(PieceName pieceName, Position targetPosition, int targetRange)
        {
            Position startingPosition = m_piecePositions[(int)pieceName];

            SetPosition(pieceName, Position.NullPosition, false);
            bool result = CanSlideToPositionInExactRange(pieceName, targetPosition, Position.NullPosition, startingPosition, 0, targetRange);
            SetPosition(pieceName, startingPosition, false);

            return result;
        }

        private bool CanSlideToPositionInExactRange(PieceName pieceName, Position targetPosition, Position lastPosition, Position currentPosition, int currentRange, int targetRange)
        {
            bool result = false;
            if (currentRange < targetRange)
            {
                for (int slideDirection = 0; slideDirection < (int)Direction.NumDirections; slideDirection++)
                {
                    Position slidePosition = currentPosition.GetNeighborAt((Direction)slideDirection);

                    if (slidePosition != lastPosition && !HasPieceAt(in slidePosition))
                    {
                        // Slide position is open

                        var right = Enums.RightOf((Direction)slideDirection);
                        var left = Enums.LeftOf((Direction)slideDirection);

                        if (HasPieceAt(in currentPosition, right) != HasPieceAt(in currentPosition, left))
                        {
                            // Can slide into slide position

                            if (targetPosition == slidePosition && currentRange + 1 == targetRange)
                            {
                                return true;
                            }
                            else
                            {
                                result = result || CanSlideToPositionInExactRange(pieceName, targetPosition, currentPosition, slidePosition, currentRange + 1, targetRange);
                            }
                        }
                    }
                }
            }

            return result;
        }

        internal void TrustedPlay(Move move, string moveStr = "")
        {
            BoardHistory.Add(move, moveStr);

            if (move != Move.PassMove)
            {
                SetPosition(move.PieceName, move.Destination, true);
            }

            CurrentTurn++;
            LastPieceMoved = move.PieceName;

            ResetState();
            ResetCaches();
        }

        private bool PlacingPieceInOrder(PieceName pieceName)
        {
            if (PieceInHand(pieceName))
            {
                switch (pieceName)
                {
                    case PieceName.wS2:
                        return PieceInPlay(PieceName.wS1);
                    case PieceName.wB2:
                        return PieceInPlay(PieceName.wB1);
                    case PieceName.wG2:
                        return PieceInPlay(PieceName.wG1);
                    case PieceName.wG3:
                        return PieceInPlay(PieceName.wG2);
                    case PieceName.wA2:
                        return PieceInPlay(PieceName.wA1);
                    case PieceName.wA3:
                        return PieceInPlay(PieceName.wA2);
                    case PieceName.bS2:
                        return PieceInPlay(PieceName.bS1);
                    case PieceName.bB2:
                        return PieceInPlay(PieceName.bB1);
                    case PieceName.bG2:
                        return PieceInPlay(PieceName.bG1);
                    case PieceName.bG3:
                        return PieceInPlay(PieceName.bG2);
                    case PieceName.bA2:
                        return PieceInPlay(PieceName.bA1);
                    case PieceName.bA3:
                        return PieceInPlay(PieceName.bA2);
                }
            }

            return true;
        }

        public Position GetPosition(PieceName pieceName)
        {
            return m_piecePositions[(int)pieceName];
        }

        internal void SetPosition(PieceName pieceName, Position position, bool updateZobrist)
        {
            var oldPosition = GetPosition(pieceName);

            if (oldPosition.Stack >= 0)
            {
                if (updateZobrist)
                {
                    _zobristHash.TogglePiece(pieceName, oldPosition);
                }

                m_pieceGrid[(Position.BoardSize / 2) + oldPosition.Q, (Position.BoardSize / 2) + oldPosition.R, oldPosition.Stack] = PieceName.INVALID;
            }

            m_piecePositions[(int)pieceName] = position;

            if (position.Stack >= 0)
            {
                if (updateZobrist)
                {
                    _zobristHash.TogglePiece(pieceName, position);
                }

                m_pieceGrid[(Position.BoardSize / 2) + position.Q, (Position.BoardSize / 2) + position.R, position.Stack] = pieceName;
            }
        }

        private PieceName GetPieceAt(in Position position)
        {
            return m_pieceGrid[(Position.BoardSize / 2) + position.Q, (Position.BoardSize / 2) + position.R, position.Stack];
        }

        private PieceName GetPieceAt(in Position position, Direction direction)
        {
            return m_pieceGrid[(Position.BoardSize / 2) + position.Q + Position.NeighborDeltas[(int)direction][0], (Position.BoardSize / 2) + position.R + Position.NeighborDeltas[(int)direction][1], position.Stack + Position.NeighborDeltas[(int)direction][2]];
        }

        public PieceName GetPieceOnTopAt(in Position position)
        {
            var topPieceName = PieceName.INVALID;

            for (int stack = 0; stack < Position.BoardStackSize; stack++)
            {
                var pieceName = m_pieceGrid[(Position.BoardSize / 2) + position.Q, (Position.BoardSize / 2) + position.R, stack];
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
            return (m_piecePositions[(int)pieceName].Stack < 0);
        }

        public bool PieceInPlay(PieceName pieceName)
        {
            return (m_piecePositions[(int)pieceName].Stack >= 0);
        }

        private bool PieceIsOnTop(PieceName pieceName)
        {
            return PieceInPlay(pieceName) && !HasPieceAt(in m_piecePositions[(int)pieceName], Direction.Above);
        }

        internal bool CanMoveWithoutBreakingHive(PieceName pieceName)
        {
            int pieceIndex = (int)pieceName;
            if (m_piecePositions[pieceIndex].Stack == 0)
            {
                // Try edge heurestic
                int edges = 0;
                bool? lastHasPiece = null;
                for (int dir = 0; dir < (int)Direction.NumDirections; dir++)
                {
                    bool hasPiece = HasPieceAt(in m_piecePositions[pieceIndex], (Direction)dir);
                    if (lastHasPiece.HasValue && lastHasPiece.Value != hasPiece)
                    {
                        edges++;
                        if (edges > 2)
                        {
                            break;
                        }
                    }
                    lastHasPiece = hasPiece;
                }

                if (edges <= 2)
                {
                    return true;
                }

                var startingPosition = m_piecePositions[pieceIndex];

                // Temporarily remove piece from board
                SetPosition(pieceName, Position.NullPosition, false);

                // Determine if the hive is broken
                bool isOneHive = IsOneHive();

                // Return piece to the board
                SetPosition(pieceName, startingPosition, false);

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
                    m_partOfHive[pn] = true;
                    piecesVisited++;
                }
                else
                {
                    m_partOfHive[pn] = false;
                    if (startingPiece == PieceName.INVALID && m_piecePositions[pn].Stack == 0)
                    {
                        // Save off a starting piece on the bottom
                        startingPiece = (PieceName)pn;
                        m_partOfHive[pn] = true;
                        piecesVisited++;
                    }
                }
            }

            // There is at least one piece on the board
            if (startingPiece != PieceName.INVALID && piecesVisited < (int)PieceName.NumPieceNames)
            {
                m_piecesToLookAt.Enqueue(startingPiece);

                while (m_piecesToLookAt.Count > 0)
                {
                    var currentPiece = m_piecesToLookAt.Dequeue();

                    var currentPosition = m_piecePositions[(int)currentPiece];

                    // Check all pieces at this stack level
                    for (int dir = 0; dir < (int)Direction.NumDirections; dir++)
                    {
                        var neighborPiece = GetPieceAt(in currentPosition, (Direction)dir);
                        if (neighborPiece != PieceName.INVALID && !m_partOfHive[(int)neighborPiece])
                        {
                            m_piecesToLookAt.Enqueue(neighborPiece);
                            m_partOfHive[(int)neighborPiece] = true;
                            piecesVisited++;
                        }
                    }

                    // Check for all pieces above this one
                    var pieceAbove = GetPieceAt(in currentPosition, Direction.Above);
                    while (PieceName.INVALID != pieceAbove)
                    {
                        m_partOfHive[(int)pieceAbove] = true;
                        piecesVisited++;
                        pieceAbove = GetPieceAt(in m_piecePositions[(int)pieceAbove], Direction.Above);
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
                    var neighbor = GetPieceAt(in m_piecePositions[(int)pieceName], (Direction)dir);
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
            bool whiteQueenSurrounded = (CountNeighbors(PieceName.wQ) == 6);
            bool blackQueenSurrounded = (CountNeighbors(PieceName.bQ) == 6);

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
            m_cachedValidPlacementsReady = false;
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