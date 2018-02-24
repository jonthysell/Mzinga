// 
// Board.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017, 2018 Jon Thysell <http://jonthysell.com>
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
using System.Text;

namespace Mzinga.Core
{
    public class Board
    {
        #region State Properties

        public BoardState BoardState { get; protected set; } = BoardState.NotStarted;

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
                    throw new ArgumentOutOfRangeException();
                }
                Color oldColor = CurrentTurnColor;

                _currentTurn = value;

                if (oldColor != CurrentTurnColor)
                {
                    // Turn has changed
                    _zobristHash.ToggleTurn();
                }

                ResetCaches();
            }
        }
        private int _currentTurn = 0;

        public Color CurrentTurnColor
        {
            get
            {
                return (Color)(CurrentTurn % 2);
            }
        }

        public int CurrentPlayerTurn
        {
            get
            {
                return 1 + (CurrentTurn / 2);
            }
        }

        public bool GameInProgress
        {
            get
            {
                return (BoardState == BoardState.NotStarted || BoardState == BoardState.InProgress);
            }
        }

        public bool GameIsOver
        {
            get
            {
                return (BoardState == BoardState.WhiteWins || BoardState == BoardState.BlackWins || BoardState == BoardState.Draw);
            }
        }

        public string BoardString
        {
            get
            {
                if (null == _boardString)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendFormat("{0}{1}", BoardState.ToString(), BoardStringSeparator);

                    sb.AppendFormat("{0}[{1}]{2}", CurrentTurnColor.ToString(), CurrentPlayerTurn, BoardStringSeparator);

                    for (int i = 0; i < EnumUtils.NumPieceNames; i++)
                    {
                        if (_pieces[i].InPlay)
                        {
                            sb.AppendFormat("{0}{1}", _pieces[i].ToString(), BoardStringSeparator);
                        }
                    }

                    _boardString = sb.ToString().TrimEnd(BoardStringSeparator);
                }

                return _boardString;
            }
        }
        private string _boardString = null;

        public long ZobristKey
        {
            get
            {
                return _zobristHash.Value;
            }
        }

        private ZobristHash _zobristHash = new ZobristHash();

        private BoardMetrics _boardMetrics = new BoardMetrics();

        #endregion

        #region Piece Enumerable Properties

        public IEnumerable<PieceName> CurrentTurnPieces
        {
            get
            {
                return CurrentTurnColor == Color.White ? EnumUtils.WhitePieceNames : EnumUtils.BlackPieceNames;
            }
        }

        public IEnumerable<PieceName> PiecesInPlay
        {
            get
            {
                for (int i = 0; i < EnumUtils.NumPieceNames; i++)
                {
                    if (_pieces[i].InPlay)
                    {
                        yield return (PieceName)i;
                    }
                }
            }
        }

        public IEnumerable<PieceName> WhiteHand
        {
            get
            {
                for (int i = 0; i < EnumUtils.NumPieceNames / 2; i++)
                {
                    if (_pieces[i].InHand)
                    {
                        yield return (PieceName)i;
                    }
                }
            }
        }

        public IEnumerable<PieceName> BlackHand
        {
            get
            {
                for (int i = EnumUtils.NumPieceNames / 2; i < EnumUtils.NumPieceNames; i++)
                {
                    if (_pieces[i].InHand)
                    {
                        yield return (PieceName)i;
                    }
                }
            }
        }

        private Piece[] _pieces = new Piece[EnumUtils.NumPieceNames];

        private Dictionary<Position, Piece> _piecesByPosition = new Dictionary<Position, Piece>();

        #endregion

        #region Piece State Properties

        public bool WhiteQueenInPlay
        {
            get
            {
                return GetPiece(PieceName.WhiteQueenBee).InPlay;
            }
        }

        public bool BlackQueenInPlay
        {
            get
            {
                return GetPiece(PieceName.BlackQueenBee).InPlay;
            }
        }

        public bool CurrentTurnQueenInPlay
        {
            get
            {
                return ((CurrentTurnColor == Color.White && WhiteQueenInPlay) || (CurrentTurnColor == Color.Black && BlackQueenInPlay));
            }
        }

        public bool OpponentQueenInPlay
        {
            get
            {
                return ((CurrentTurnColor == Color.White && BlackQueenInPlay) || (CurrentTurnColor == Color.Black && WhiteQueenInPlay));
            }
        }

        #endregion

        #region Caches

        private MoveSet[] _cachedValidMovesByPiece = null;

        private HashSet<Position> _cachedValidPlacementPositions = null;
        private HashSet<Position> _visitedPlacements = new HashSet<Position>();

        public CacheMetricsSet ValidMoveCacheMetricsSet { get; private set; } = new CacheMetricsSet();

        public int ValidMoveCacheResets { get; private set; } = 0;

        #endregion

        public Board()
        {
            for (int i = 0; i < EnumUtils.NumPieceNames; i++)
            {
                _pieces[i] = new Piece((PieceName)i);
            }
        }

        public Board(string boardString) : this()
        {
            if (string.IsNullOrWhiteSpace(boardString))
            {
                throw new ArgumentNullException("boardString");
            }

            string[] split = boardString.Split(BoardStringSeparator);

            string boardStateString = split[0];

            BoardState boardState;
            if (!Enum.TryParse(boardStateString, out boardState))
            {
                throw new ArgumentException("Couldn't parse board state.", "boardString");
            }
            BoardState = boardState;

            string[] currentTurnSplit = split[1].Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);

            string currentTurnColorString = currentTurnSplit[0];

            Color currentTurnColor;
            if (!Enum.TryParse(currentTurnColorString, out currentTurnColor))
            {
                throw new ArgumentException("Couldn't parse current turn color.", "boardString");
            }

            string currentPlayerTurnString = currentTurnSplit[1];

            int currentPlayerTurn;
            if (!int.TryParse(currentPlayerTurnString, out currentPlayerTurn))
            {
                throw new ArgumentException("Couldn't parse current player turn.", "boardString");
            }

            CurrentTurn = 2 * (currentPlayerTurn - 1) + (int)currentTurnColor;

            Queue<Piece> parsedPieces = new Queue<Piece>(EnumUtils.NumPieceNames);

            for (int i = 2; i < split.Length; i++)
            {
                parsedPieces.Enqueue(new Piece(split[i]));
            }

            while (parsedPieces.Count > 0)
            {
                Piece parsedPiece = parsedPieces.Dequeue();
                if (parsedPiece.InPlay)
                {
                    if (parsedPiece.Position.Stack > 0 && !HasPieceAt(parsedPiece.Position.GetBelow()))
                    {
                        parsedPieces.Enqueue(parsedPiece);
                    }
                    else
                    {
                        Piece piece = GetPiece(parsedPiece.PieceName);
                        MovePiece(piece, parsedPiece.Position, true);
                    }
                }
            }

            if (!IsOneHive())
            {
                throw new ArgumentException("The boardString violates the one-hive rule.", "boardString");
            }
        }

        protected bool HasPieceAt(Position position)
        {
            return (null != GetPiece(position));
        }

        public Position GetPiecePosition(PieceName pieceName)
        {
            if (pieceName == PieceName.INVALID)
            {
                throw new ArgumentOutOfRangeException("pieceName");
            }

            return GetPiece(pieceName).Position;
        }

        protected Piece GetPiece(PieceName pieceName)
        {
            return _pieces[(int)pieceName];
        }

        protected Piece GetPiece(Position position)
        {
            Piece piece;
            if (_piecesByPosition.TryGetValue(position, out piece))
            {
                return piece;
            }

            return null;
        }

        public PieceName GetPieceOnTop(Position position)
        {
            if (null == position)
            {
                throw new ArgumentNullException("position");
            }

            Piece piece = GetPieceOnTopInternal(position);
            return (null != piece) ? piece.PieceName : PieceName.INVALID;
        }

        private Piece GetPieceOnTopInternal(Position position)
        {
            while (position.Stack > 0)
            {
                position = position.GetBelow();
            }

            Piece topPiece = GetPiece(position);

            if (null != topPiece)
            {
                topPiece = GetPieceOnTop(topPiece);
            }

            return topPiece;
        }

        private Piece GetPieceOnTop(Piece piece)
        {
            while (null != piece.PieceAbove)
            {
                piece = piece.PieceAbove;
            }

            return piece;
        }

        private Piece GetPieceOnBottom(Piece piece)
        {
            while (null != piece.PieceBelow)
            {
                piece = piece.PieceBelow;
            }

            return piece;
        }

        protected void MovePiece(Piece piece, Position newPosition)
        {
            MovePiece(piece, newPosition, true);
        }

        private void MovePiece(Piece piece, Position newPosition, bool updateZobrist)
        {
            if (piece.InPlay)
            {
                _piecesByPosition[piece.Position] = null;
                if (null != piece.PieceBelow)
                {
                    piece.PieceBelow.PieceAbove = null;
                    piece.PieceBelow = null;
                }

                // Remove from old position
                if (updateZobrist)
                {
                    _zobristHash.TogglePiece(piece.PieceName, piece.Position);
                }
            }

            piece.Move(newPosition);

            if (piece.InPlay)
            {
                _piecesByPosition[piece.Position] = piece;
                if (newPosition.Stack > 0)
                {
                    Position posBelow = newPosition.GetBelow();
                    Piece pieceBelow = GetPiece(posBelow);
                    pieceBelow.PieceAbove = piece;
                    piece.PieceBelow = pieceBelow;
                }

                // Add to new position
                if (updateZobrist)
                {
                    _zobristHash.TogglePiece(piece.PieceName, piece.Position);
                }
            }
        }

        protected bool PieceIsOnTop(Piece targetPiece)
        {
            return (null == targetPiece.PieceAbove);
        }

        protected bool PieceIsOnBottom(Piece targetPiece)
        {
            return (null == targetPiece.PieceBelow);
        }

        public bool IsOneHive()
        {
            // Whether or not a piece has been found to be part of the hive
            bool[] partOfHive = new bool[EnumUtils.NumPieceNames];
            int piecesVisited = 0;

            // Find a piece on the board to start checking
            Piece startingPiece = null;
            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                Piece piece = GetPiece(pieceName);
                if (piece.InHand)
                {
                    partOfHive[(int)pieceName] = true;
                    piecesVisited++;
                }
                else
                {
                    partOfHive[(int)pieceName] = false;
                    if (null == startingPiece && piece.Position.Stack == 0)
                    {
                        // Save off a starting piece on the bottom
                        startingPiece = piece;
                        partOfHive[(int)pieceName] = true;
                        piecesVisited++;
                    }
                }
            }

            // There is at least one piece on the board
            if (null != startingPiece && piecesVisited < EnumUtils.NumPieceNames)
            {
                Queue<Piece> piecesToLookAt = new Queue<Piece>();
                piecesToLookAt.Enqueue(startingPiece);

                while (piecesToLookAt.Count > 0)
                {
                    Piece currentPiece = piecesToLookAt.Dequeue();

                    // Check all pieces at this stack level
                    for (int i = 0; i < EnumUtils.NumDirections; i++)
                    {
                        Position neighbor = currentPiece.Position.NeighborAt(i);
                        Piece neighborPiece = GetPiece(neighbor);
                        if (null != neighborPiece && !partOfHive[(int)neighborPiece.PieceName])
                        {
                            piecesToLookAt.Enqueue(neighborPiece);
                            partOfHive[(int)neighborPiece.PieceName] = true;
                            piecesVisited++;
                        }
                    }

                    // Check for all pieces above this one
                    Piece pieceAbove = currentPiece.PieceAbove;
                    while (null != pieceAbove)
                    {
                        partOfHive[(int)pieceAbove.PieceName] = true;
                        piecesVisited++;
                        pieceAbove = pieceAbove.PieceAbove;
                    }
                }
            }

            return piecesVisited == EnumUtils.NumPieceNames;
        }

        #region Metrics

        public BoardMetrics GetBoardMetrics()
        {
            _boardMetrics.BoardState = BoardState;

            // Get the metrics for the current turn
            SetCurrentPlayerMetrics();

            // Save off current valid moves/placements since we'll be returning to it
            MoveSet[] validMoves = _cachedValidMovesByPiece;
            _cachedValidMovesByPiece = null;

            HashSet<Position> validPlacements = _cachedValidPlacementPositions;
            _cachedValidPlacementPositions = null;

            // Spoof going to the next turn to get the opponent's metrics
            _currentTurn++;
            _zobristHash.ToggleTurn();
            SetCurrentPlayerMetrics();
            _currentTurn--;
            _zobristHash.ToggleTurn();

            // Returned, so reload saved valid moves/placements into cache
            _cachedValidPlacementPositions = validPlacements;
            _cachedValidMovesByPiece = validMoves;

            return _boardMetrics;
        }

        private void SetCurrentPlayerMetrics()
        {
            foreach (PieceName pieceName in CurrentTurnPieces)
            {
                Piece targetPiece = GetPiece(pieceName);

                _boardMetrics[pieceName].InPlay = targetPiece.InPlay ? 1 : 0;

                // Move metrics
                MoveSet validMoves = GetValidMoves(targetPiece.PieceName);

                _boardMetrics[pieceName].ValidMoveCount = validMoves.Count;
                _boardMetrics[pieceName].IsPinned = _boardMetrics[pieceName].ValidMoveCount == 0 ? 1 : 0;
                _boardMetrics[pieceName].NeighborCount = CountNeighbors(targetPiece);
            }
        }

        protected int CountNeighbors(PieceName pieceName)
        {
            return CountNeighbors(GetPiece(pieceName));
        }

        private int CountNeighbors(Piece piece)
        {
            if (piece.InHand)
            {
                return 0;
            }

            int count = 0;

            for (int i = 0; i < EnumUtils.NumDirections; i++)
            {
                count += HasPieceAt(piece.Position.NeighborAt(i)) ? 1 : 0;
            }

            return count;
        }

        #endregion

        #region GetValidMoves

        public MoveSet GetValidMoves()
        {
            MoveSet moves = new MoveSet();

            if (GameInProgress)
            {
                foreach (PieceName pieceName in CurrentTurnPieces)
                {
                    moves.Add(GetValidMoves(pieceName));
                }

                if (moves.Count == 0)
                {
                    moves.Add(Move.Pass);
                }
            }

            moves.Lock();

            return moves;
        }

        public MoveSet GetValidMoves(PieceName pieceName)
        {
            if (pieceName == PieceName.INVALID)
            {
                throw new ArgumentOutOfRangeException("pieceName");
            }

            if (null == _cachedValidMovesByPiece)
            {
                _cachedValidMovesByPiece = new MoveSet[EnumUtils.NumPieceNames];
            }

            int pieceNameIndex = (int)pieceName;

            if (null != _cachedValidMovesByPiece[pieceNameIndex])
            {
                // MoveSet is cached in L1 cache
                ValidMoveCacheMetricsSet["ValidMoves." + EnumUtils.GetShortName(pieceName)].Hit();
            }
            else
            {
                // MoveSet is not cached in L1 cache
                ValidMoveCacheMetricsSet["ValidMoves." + EnumUtils.GetShortName(pieceName)].Miss();

                // Calculate MoveSet
                Piece targetPiece = GetPiece(pieceName);
                MoveSet moves = GetValidMovesInternal(targetPiece);
                moves.Lock();

                // Populate cache
                _cachedValidMovesByPiece[pieceNameIndex] = moves;
            }

            return _cachedValidMovesByPiece[pieceNameIndex];
        }

        private MoveSet GetValidMovesInternal(Piece targetPiece)
        {
            if (GameInProgress)
            {
                if (targetPiece.Color == CurrentTurnColor && PlacingPieceInOrder(targetPiece))
                {
                    if (CurrentTurn == 0 && targetPiece.Color == Color.White && targetPiece.InHand && targetPiece.PieceName != PieceName.WhiteQueenBee)
                    {
                        // First move must be at the origin and not the White Queen Bee
                        MoveSet validMoves = new MoveSet();
                        validMoves.Add(new Move(targetPiece.PieceName, Position.Origin));
                        return validMoves;
                    }
                    else if (CurrentTurn == 1 && targetPiece.Color == Color.Black && targetPiece.InHand && targetPiece.PieceName != PieceName.BlackQueenBee)
                    {
                        MoveSet validMoves = new MoveSet();
                        // Second move must be around the origin and not the Black Queen Bee
                        for (int i = 0; i < EnumUtils.NumDirections; i++)
                        {
                            Position neighbor = Position.Origin.NeighborAt(i);
                            validMoves.Add(new Move(targetPiece.PieceName, neighbor));
                        }
                        return validMoves;
                    }
                    else if (targetPiece.InHand && (CurrentPlayerTurn != 4 || // Normal turn OR
                                                    (CurrentPlayerTurn == 4 && // Turn 4 and AND
                                                     (CurrentTurnQueenInPlay || (!CurrentTurnQueenInPlay && targetPiece.BugType == BugType.QueenBee))))) // Queen is in play or you're trying to play it
                    {
                        // Look for valid new placements
                        return GetValidPlacements(targetPiece);
                    }
                    else if (targetPiece.InPlay && CurrentTurnQueenInPlay && PieceIsOnTop(targetPiece) && CanMoveWithoutBreakingHive(targetPiece))
                    {
                        // Look for valid moves of already played pieces
                        switch (targetPiece.BugType)
                        {
                            case BugType.QueenBee:
                                return GetValidQueenBeeMovements(targetPiece);
                            case BugType.Spider:
                                return GetValidSpiderMovements(targetPiece);
                            case BugType.Beetle:
                                return GetValidBeetleMovements(targetPiece);
                            case BugType.Grasshopper:
                                return GetValidGrasshopperMovements(targetPiece);
                            case BugType.SoldierAnt:
                                return GetValidSoldierAntMovements(targetPiece);
                        }
                    }
                }
            }

            return new MoveSet();
        }

        private MoveSet GetValidPlacements(Piece targetPiece)
        {
            MoveSet validMoves = new MoveSet();

            Color targetColor = CurrentTurnColor;

            if (targetPiece.Color != targetColor)
            {
                return validMoves;
            }

            if (null == _cachedValidPlacementPositions)
            {
                _cachedValidPlacementPositions = new HashSet<Position>();

                _visitedPlacements.Clear();

                for (int i = 0; i < EnumUtils.NumPieceNames; i++)
                {
                    Piece piece = _pieces[i];
                    if (piece.InPlay && PieceIsOnTop(piece) && piece.Color == targetColor)
                    {
                        // Piece is in play, on the top and is the right color, look through neighbors
                        Position bottomPosition = GetPieceOnBottom(piece).Position;
                        _visitedPlacements.Add(bottomPosition);

                        for (int j = 0; j < EnumUtils.NumDirections; j++)
                        {
                            Position neighbor = bottomPosition.NeighborAt(j);

                            if (_visitedPlacements.Add(neighbor) && !HasPieceAt(neighbor))
                            {
                                // Neighboring position is a potential, verify its neighbors are empty or same color
                                bool validPlacement = true;
                                for (int k = 0; k < EnumUtils.NumDirections; k++)
                                {
                                    Position surroundingPosition = neighbor.NeighborAt(k);
                                    Piece surroundingPiece = GetPieceOnTopInternal(surroundingPosition);
                                    if (null != surroundingPiece && surroundingPiece.Color != targetColor)
                                    {
                                        validPlacement = false;
                                        break;
                                    }
                                }

                                if (validPlacement)
                                {
                                    _cachedValidPlacementPositions.Add(neighbor);
                                }
                            }
                        }
                    }
                }

                ValidMoveCacheMetricsSet["ValidPlacements"].Miss();
            }
            else
            {
                ValidMoveCacheMetricsSet["ValidPlacements"].Hit();
            }

            foreach (Position validPlacement in _cachedValidPlacementPositions)
            {
                validMoves.Add(new Move(targetPiece.PieceName, validPlacement));
            }

            return validMoves;
        }

        private MoveSet GetValidQueenBeeMovements(Piece targetPiece)
        {
            // Get all slides one away
            return GetValidSlides(targetPiece, 1);
        }

        private MoveSet GetValidSpiderMovements(Piece targetPiece)
        {
            MoveSet validMoves = new MoveSet();

            // Get all slides up to 2 spots away
            MoveSet upToTwo = GetValidSlides(targetPiece, 2);

            if (upToTwo.Count > 0)
            {
                // Get all slides up to 3 spots away
                MoveSet upToThree = GetValidSlides(targetPiece, 3);

                if (upToThree.Count > 0)
                {
                    // Get all slides ONLY 3 spots away
                    upToThree.Remove(upToTwo);

                    if (upToThree.Count > 0)
                    {
                        validMoves.Add(upToThree);
                    }
                }
            }

            return validMoves;
        }

        private MoveSet GetValidBeetleMovements(Piece targetPiece)
        {
            MoveSet validMoves = new MoveSet();

            // Look in all directions
            foreach (Direction direction in EnumUtils.Directions)
            {
                Position newPosition = targetPiece.Position.NeighborAt(direction);

                Piece topNeighbor = GetPieceOnTopInternal(newPosition);

                // Get positions to left and right or direction we're heading
                Direction leftOfTarget = EnumUtils.LeftOf(direction);
                Direction rightOfTarget = EnumUtils.RightOf(direction);
                Position leftNeighborPosition = targetPiece.Position.NeighborAt(leftOfTarget);
                Position rightNeighborPosition = targetPiece.Position.NeighborAt(rightOfTarget);

                Piece topLeftNeighbor = GetPieceOnTopInternal(leftNeighborPosition);
                Piece topRightNeighbor = GetPieceOnTopInternal(rightNeighborPosition);

                // At least one neighbor is present
                int currentHeight = targetPiece.Position.Stack + 1;
                int destinationHeight = null != topNeighbor ? topNeighbor.Position.Stack + 1 : 0;

                int topLeftNeighborHeight = null != topLeftNeighbor ? topLeftNeighbor.Position.Stack + 1 : 0;
                int topRightNeighborHeight = null != topRightNeighbor ? topRightNeighbor.Position.Stack + 1 : 0;

                // "Take-off" beetle
                currentHeight--;

                if (!(currentHeight == 0 && destinationHeight == 0 && topLeftNeighborHeight == 0 && topRightNeighborHeight == 0))
                {
                    // Logic from http://boardgamegeek.com/wiki/page/Hive_FAQ#toc9
                    if (!(destinationHeight < topLeftNeighborHeight && destinationHeight < topRightNeighborHeight && currentHeight < topLeftNeighborHeight && currentHeight < topRightNeighborHeight))
                    {
                        Position targetPosition = (newPosition.Stack == destinationHeight) ? newPosition : topNeighbor.Position.GetAbove();
                        Move targetMove = new Move(targetPiece.PieceName, targetPosition);
                        validMoves.Add(targetMove);
                    }
                }
            }

            return validMoves;
        }

        private MoveSet GetValidGrasshopperMovements(Piece targetPiece)
        {
            MoveSet validMoves = new MoveSet();

            Position startingPosition = targetPiece.Position;

            foreach (Direction direction in EnumUtils.Directions)
            {
                Position landingPosition = startingPosition.NeighborAt(direction);

                int distance = 0;
                while (HasPieceAt(landingPosition))
                {
                    // Jump one more in the same direction
                    landingPosition = landingPosition.NeighborAt(direction);
                    distance++;
                }

                if (distance > 0)
                {
                    // Can only move if there's at least one piece in the way
                    Move move = new Move(targetPiece.PieceName, landingPosition);
                    validMoves.Add(move);
                }
            }

            return validMoves;
        }

        private MoveSet GetValidSoldierAntMovements(Piece targetPiece)
        {
            // Get all slides all the way around
            return GetValidSlides(targetPiece, null);
        }

        private MoveSet GetValidSlides(Piece targetPiece, int? maxRange)
        {
            MoveSet validMoves = new MoveSet();

            HashSet<Position> visitedPositions = new HashSet<Position>();
            visitedPositions.Add(targetPiece.Position);

            GetValidSlides(targetPiece, visitedPositions, 0, maxRange, validMoves);

            return validMoves;
        }

        private void GetValidSlides(Piece targetPiece, HashSet<Position> visitedPositions, int currentRange, int? maxRange, MoveSet validMoves)
        {
            if (!maxRange.HasValue || currentRange < maxRange.Value)
            {
                foreach (Direction slideDirection in EnumUtils.Directions)
                {
                    Position slidePosition = targetPiece.Position.NeighborAt(slideDirection);

                    if (!visitedPositions.Contains(slidePosition) && !HasPieceAt(slidePosition))
                    {
                        // Slide position is open

                        Direction right = EnumUtils.RightOf(slideDirection);
                        Direction left = EnumUtils.LeftOf(slideDirection);

                        if (HasPieceAt(targetPiece.Position.NeighborAt(right)) != HasPieceAt(targetPiece.Position.NeighborAt(left)))
                        {
                            // Can slide into slide position
                            Move move = new Move(targetPiece.PieceName, slidePosition);

                            if (validMoves.Add(move))
                            {
                                // Sliding from this position has not been tested yet
                                visitedPositions.Add(move.Position);

                                Position preSlidePosition = targetPiece.Position;
                                MovePiece(targetPiece, move.Position, false);
                                GetValidSlides(targetPiece, visitedPositions, currentRange + 1, maxRange, validMoves);
                                MovePiece(targetPiece, preSlidePosition, false);
                            }
                        }
                    }
                }
            }
        }

        protected bool CanMoveWithoutBreakingHive(Piece targetPiece)
        {
            if (targetPiece.InPlay && targetPiece.Position.Stack == 0)
            {
                // Try edge heurestic
                int edges = 0;
                bool? lastHasPiece = null;
                for (int i = 0; i < EnumUtils.NumDirections; i++)
                {
                    Position neighbor = targetPiece.Position.NeighborAt(i);

                    bool hasPiece = HasPieceAt(neighbor);
                    if (lastHasPiece.HasValue)
                    {
                        if (lastHasPiece.Value != hasPiece)
                        {
                            edges++;
                            if (edges > 2)
                            {
                                break;
                            }
                        }
                    }
                    lastHasPiece = hasPiece;
                }

                if (edges <= 2)
                {
                    return true;
                }

                // Temporarily remove piece from board
                Position originalPosition = targetPiece.Position;
                MovePiece(targetPiece, null, false);

                // Determine if the hive is broken
                bool isOneHive = IsOneHive();

                // Return piece to the board
                MovePiece(targetPiece, originalPosition, false);

                return isOneHive;
            }

            return true;
        }

        protected bool PlacingPieceInOrder(Piece targetPiece)
        {
            if (targetPiece.InHand)
            {
                switch (targetPiece.PieceName)
                {
                    case PieceName.WhiteSpider2:
                        return GetPiece(PieceName.WhiteSpider1).InPlay;
                    case PieceName.WhiteBeetle2:
                        return GetPiece(PieceName.WhiteBeetle1).InPlay;
                    case PieceName.WhiteGrasshopper2:
                        return GetPiece(PieceName.WhiteGrasshopper1).InPlay;
                    case PieceName.WhiteGrassHopper3:
                        return GetPiece(PieceName.WhiteGrasshopper2).InPlay;
                    case PieceName.WhiteSoldierAnt2:
                        return GetPiece(PieceName.WhiteSoldierAnt1).InPlay;
                    case PieceName.WhiteSoldierAnt3:
                        return GetPiece(PieceName.WhiteSoldierAnt2).InPlay;
                    case PieceName.BlackSpider2:
                        return GetPiece(PieceName.BlackSpider1).InPlay;
                    case PieceName.BlackBeetle2:
                        return GetPiece(PieceName.BlackBeetle1).InPlay;
                    case PieceName.BlackGrasshopper2:
                        return GetPiece(PieceName.BlackGrasshopper1).InPlay;
                    case PieceName.BlackGrassHopper3:
                        return GetPiece(PieceName.BlackGrasshopper2).InPlay;
                    case PieceName.BlackSoldierAnt2:
                        return GetPiece(PieceName.BlackSoldierAnt1).InPlay;
                    case PieceName.BlackSoldierAnt3:
                        return GetPiece(PieceName.BlackSoldierAnt2).InPlay;
                }
            }

            return true;
        }

        protected void ResetCaches()
        {
            _cachedValidMovesByPiece = null;
            _cachedValidPlacementPositions = null;
            ValidMoveCacheResets++;

            _boardString = null;
        }

        #endregion

        public override string ToString()
        {
            return BoardString;
        }

        public const char BoardStringSeparator = ';';
    }

    public enum BoardState
    {
        NotStarted,
        InProgress,
        Draw,
        WhiteWins,
        BlackWins
    }

    [Serializable]
    public class InvalidMoveException : Exception
    {
        public Move Move { get; private set; }

        public InvalidMoveException(Move move) : this(move, "You can't move that piece there.") { }

        public InvalidMoveException(Move move, string message) : base(message)
        {
            Move = move;
        }
    }
}
