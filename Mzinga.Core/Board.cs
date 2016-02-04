// 
// Board.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016 Jon Thysell <http://jonthysell.com>
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
using System.Text;

namespace Mzinga.Core
{
    public class Board
    {
        #region State Properties

        public BoardState BoardState { get; protected set; }

        public int CurrentTurn { get; protected set; }

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

        #endregion

        #region Piece Enumerable Properties

        public IEnumerable<Piece> AllPieces
        {
            get
            {
                return _pieces.AsEnumerable();
            }
        }

        public IEnumerable<Piece> CurrentTurnPieces
        {
            get
            {
                return _pieces.Where<Piece>((piece) => { return piece.Color == CurrentTurnColor; });
            }
        }

        public IEnumerable<Piece> WhitePieces
        {
            get
            {
                return _pieces.Where<Piece>((piece) => { return piece.Color == Color.White; });
            }
        }

        public IEnumerable<Piece> WhiteHand
        {
            get
            {
                return _pieces.Where<Piece>((piece) => { return (piece.Color == Color.White && piece.InHand); });
            }
        }

        public IEnumerable<Piece> BlackPieces
        {
            get
            {
                return _pieces.Where<Piece>((piece) => { return piece.Color == Color.Black; });
            }
        }

        public IEnumerable<Piece> BlackHand
        {
            get
            {
                return _pieces.Where<Piece>((piece) => { return (piece.Color == Color.Black && piece.InHand); });
            }
        }

        public IEnumerable<Piece> PiecesInPlay
        {
            get
            {
                return _pieces.Where<Piece>((piece) => { return piece.InPlay; });
            }
        }

        private Piece[] _pieces;

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

        public Board()
        {
            _pieces = new Piece[EnumUtils.NumPieceNames];

            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                _pieces[(int)pieceName] = new Piece(pieceName);
            }

            CurrentTurn = 0;
            BoardState = BoardState.NotStarted;
        }

        public Board(string boardString) : this()
        {
            if (String.IsNullOrWhiteSpace(boardString))
            {
                throw new ArgumentOutOfRangeException("boardString");
            }

            string[] split = boardString.Split(Board.BoardStringSeparator);

            string boardStateString = split[0];

            BoardState boardState;
            if (!Enum.TryParse<BoardState>(boardStateString, out boardState))
            {
                throw new ArgumentException("Couldn't parse board state.", "boardString");
            }
            BoardState = boardState;

            string[] currentTurnSplit = split[1].Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);

            string currentTurnColorString = currentTurnSplit[0];

            Color currentTurnColor;
            if (!Enum.TryParse<Color>(currentTurnColorString, out currentTurnColor))
            {
                throw new ArgumentException("Couldn't parse current turn color.", "boardString");
            }

            string currentPlayerTurnString = currentTurnSplit[1];

            int currentPlayerTurn;
            if (!Int32.TryParse(currentPlayerTurnString, out currentPlayerTurn))
            {
                throw new ArgumentException("Couldn't parse current player turn.", "boardString");
            }

            CurrentTurn = 2 * (currentPlayerTurn - 1) + (int)currentTurnColor;

            for (int i = 2; i < split.Length; i++)
            {
                Piece parsedPiece = new Piece(split[i]);
                if (parsedPiece.InPlay)
                {
                    Piece piece = GetPiece(parsedPiece.PieceName);
                    piece.Move(parsedPiece.Position);
                }
            }

            if (!IsOneHive())
            {
                throw new ArgumentException("The boardString violates the one-hive rule.", "boardString");
            }
        }

        public bool HasPieceAt(Position position)
        {
            if (null == position)
            {
                throw new ArgumentNullException("position");
            }

            return (null != GetPiece(position));
        }

        public Piece GetPiece(PieceName pieceName)
        {
            if (pieceName == PieceName.INVALID)
            {
                throw new ArgumentOutOfRangeException("pieceName");
            }

            return _pieces[(int)pieceName];
        }

        public Piece GetPiece(Position position)
        {
            if (null == position)
            {
                throw new ArgumentNullException("position");
            }

            foreach (Piece piece in PiecesInPlay)
            {
                if (piece.Position == position)
                {
                    return piece;
                }
            }

            return null;
        }

        public Piece GetPieceOnTop(Position position)
        {
            if (null == position)
            {
                throw new ArgumentNullException("position");
            }

            Piece topPiece = null;

            foreach (Piece piece in PiecesInPlay)
            {
                if (piece.Position.X == position.X &&
                    piece.Position.Y == position.Y &&
                    piece.Position.Z == position.Z)
                {
                    if (null == topPiece || piece.Position.Stack > topPiece.Position.Stack)
                    {
                        topPiece = piece;
                    }
                }
            }

            return topPiece;
        }

        public bool PieceIsOnTop(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            foreach (Piece piece in PiecesInPlay)
            {
                if (piece.Position.X == targetPiece.Position.X &&
                    piece.Position.Y == targetPiece.Position.Y &&
                    piece.Position.Z == targetPiece.Position.Z &&
                    piece.Position.Stack > targetPiece.Position.Stack)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsOneHive()
        {
            // Whether or not a piece has been found to be part of the hive
            bool[] partOfHive = new bool[EnumUtils.NumPieceNames];

            // Find a piece on the board to start checking
            Piece startingPiece = null;
            foreach (PieceName pieceName in EnumUtils.PieceNames)
            {
                Piece piece = GetPiece(pieceName);
                if (piece.InHand)
                {
                    partOfHive[(int)pieceName] = true;
                }
                else
                {
                    partOfHive[(int)pieceName] = false;
                    if (null == startingPiece && piece.Position.Stack == 0)
                    {
                        // Save off a starting piece on the bottom
                        startingPiece = piece;
                        partOfHive[(int)pieceName] = true;
                    }
                }
            }

            // There is at least one piece on the board
            if (null != startingPiece)
            {
                Queue<Piece> piecesToLookAt = new Queue<Piece>();
                piecesToLookAt.Enqueue(startingPiece);

                while (piecesToLookAt.Count > 0)
                {
                    Piece currentPiece = piecesToLookAt.Dequeue();

                    // Check all pieces at this stack level
                    foreach (Position neighbor in currentPiece.Position.Neighbors)
                    {
                        Piece neighborPiece = GetPiece(neighbor);
                        if (null != neighborPiece && !partOfHive[(int)neighborPiece.PieceName])
                        {
                            piecesToLookAt.Enqueue(neighborPiece);
                            partOfHive[(int)neighborPiece.PieceName] = true;
                        }
                    }

                    // Check for all pieces above this one
                    Piece pieceAbove = GetPiece(currentPiece.Position.GetShifted(0, 0, 0, 1));
                    while (null != pieceAbove)
                    {
                        partOfHive[(int)pieceAbove.PieceName] = true;
                        pieceAbove = GetPiece(pieceAbove.Position.GetShifted(0, 0, 0, 1));
                    }
                }

                // Return true iff every piece was part of the one-hive
                return partOfHive.All((value) => { return value; });
            }

            // If there's no startingPiece, there's nothing on the board
            return true;
        }

        #region Metrics

        public BoardMetrics GetBoardMetrics()
        {
            BoardMetrics boardMetrics = new BoardMetrics(BoardState);

            // Get the metrics for the current turn
            TurnMetrics currentTurnMetrics = GetCurrentTurnMetrics();
            boardMetrics.TurnMetrics[CurrentTurnColor].CopyFrom(currentTurnMetrics);

            // Spoof going to the next turn to get the opponent's metrics
            CurrentTurn++;
            TurnMetrics opponentTurnMetrics = GetCurrentTurnMetrics();
            boardMetrics.TurnMetrics[CurrentTurnColor].CopyFrom(opponentTurnMetrics);
            CurrentTurn--;

            return boardMetrics;
        }

        public TurnMetrics GetCurrentTurnMetrics()
        {
            TurnMetrics turnMetrics = new TurnMetrics(CurrentTurnColor);

            foreach (Piece piece in CurrentTurnPieces)
            {
                PieceMetrics pieceMetrics = turnMetrics.PieceMetrics[piece.PieceName];

                pieceMetrics.NeighborCount = CountNeighbors(piece.PieceName);

                // Move metrics
                MoveSet validMoves = GetValidMoves(piece);
                pieceMetrics.MoveCount = validMoves.Count;
            }

            return turnMetrics;
        }

        public int CountNeighbors(PieceName pieceName)
        {
            Piece piece = GetPiece(pieceName);

            if (piece.InHand)
            {
                return 0;
            }

            int count = 0;

            foreach (Position neighbor in piece.Position.Neighbors)
            {
                count += HasPieceAt(neighbor) ? 1 : 0;
            }

            return count;
        }

        #endregion

        #region GetValidMoves

        public MoveSet GetValidMoves()
        {
            MoveSet moves = new MoveSet();

            if (BoardState == BoardState.NotStarted || BoardState == BoardState.InProgress)
            {
                foreach (Piece piece in AllPieces)
                {
                    moves.Add(GetValidMoves(piece));
                }

                if (moves.Count == 0)
                {
                    moves.Add(Move.Pass);
                }
            }

            return moves;
        }

        public MoveSet GetValidMoves(PieceName pieceName)
        {
            Piece piece = GetPiece(pieceName);
            return GetValidMoves(piece);
        }

        protected MoveSet GetValidMoves(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            MoveSet validMoves = new MoveSet();

            if (BoardState == BoardState.NotStarted || BoardState == BoardState.InProgress)
            {
                if (targetPiece.Color == CurrentTurnColor && PlacingPieceInOrder(targetPiece))
                {
                    if (CurrentTurn == 0 && targetPiece.InHand && targetPiece.PieceName != PieceName.WhiteQueenBee)
                    {
                        // First move must be at the origin and not the White Queen Bee
                        validMoves.Add(new Move(targetPiece.PieceName, Position.Origin));
                    }
                    else if (CurrentTurn == 1 && targetPiece.InHand && targetPiece.PieceName != PieceName.BlackQueenBee)
                    {
                        // Second move must be around the origin and not the Black Queen Bee
                        foreach (Position neighbor in Position.Origin.Neighbors)
                        {
                            validMoves.Add(new Move(targetPiece.PieceName, neighbor));
                        }
                    }
                    else if (targetPiece.InHand && (CurrentPlayerTurn != 4 ||
                                                    (CurrentPlayerTurn == 4 &&
                                                     (CurrentTurnQueenInPlay || (!CurrentTurnQueenInPlay && targetPiece.BugType == BugType.QueenBee)))))
                    {
                        // Look for valid new placements
                        validMoves.Add(GetValidPlacements(targetPiece));
                    }
                    else if (targetPiece.InPlay && CurrentTurnQueenInPlay && PieceIsOnTop(targetPiece) && CanMoveWithoutBreakingHive(targetPiece))
                    {
                        // Look for valid moves of already played pieces
                        switch (targetPiece.BugType)
                        {
                            case BugType.QueenBee:
                                validMoves.Add(GetValidQueenBeeMovements(targetPiece));
                                break;
                            case BugType.Spider:
                                validMoves.Add(GetValidSpiderMovements(targetPiece));
                                break;
                            case BugType.Beetle:
                                validMoves.Add(GetValidBeetleMovements(targetPiece));
                                break;
                            case BugType.Grasshopper:
                                validMoves.Add(GetValidGrasshopperMovements(targetPiece));
                                break;
                            case BugType.SoldierAnt:
                                validMoves.Add(GetValidSoldierAntMovements(targetPiece));
                                break;
                        }
                    }
                }
            }

            return validMoves;
        }

        private MoveSet GetValidPlacements(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            MoveSet validMoves = new MoveSet();

            foreach (Piece piece in PiecesInPlay)
            {
                if (piece.Position.Stack == 0 && GetPieceOnTop(piece.Position).Color == targetPiece.Color)
                {
                    // Piece is in play, on the bottom, and the top is the right color, look through neighbors
                    foreach (Position neighbor in piece.Position.Neighbors)
                    {
                        if (!HasPieceAt(neighbor))
                        {
                            // Neighboring position is a potential, verify its neighbors are empty or same color
                            bool validPlacement = true;
                            foreach (Position surroundingPosition in neighbor.Neighbors)
                            {
                                Piece surroundingPiece = GetPieceOnTop(surroundingPosition);
                                if (null != surroundingPiece && surroundingPiece.Color != targetPiece.Color)
                                {
                                    validPlacement = false;
                                    break;
                                }
                            }

                            if (validPlacement)
                            {
                                Move move = new Move(targetPiece.PieceName, neighbor);
                                validMoves.Add(move);
                            }
                        }
                    }
                }
            }

            return validMoves;
        }

        private MoveSet GetValidQueenBeeMovements(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            // Get all slides one away
            return GetValidSlides(targetPiece, 1);
        }

        private MoveSet GetValidSpiderMovements(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            MoveSet validMoves = new MoveSet();

            // Get all slides up to 2 spots away
            MoveSet upToTwo = GetValidSlides(targetPiece, 2);

            if (upToTwo.Count > 0)
            {
                // Get all slides up to 3 spots away
                MoveSet upToThree = GetValidSlides(targetPiece, 3);

                // Get all slides ONLY 3 spots away
                upToThree.Remove(upToTwo);

                validMoves.Add(upToThree);
            }

            return validMoves;
        }

        private MoveSet GetValidBeetleMovements(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            MoveSet validMoves = new MoveSet();

            // Look in all directions
            foreach (Direction direction in EnumUtils.Directions)
            {
                Position newPosition = targetPiece.Position.NeighborAt(direction);
                newPosition = newPosition.GetShifted(0, 0, 0, -newPosition.Stack);

                Piece topNeighbor = GetPieceOnTop(newPosition);

                // Get positions to left and right or direction we're heading
                Direction leftOfTarget = EnumUtils.LeftOf(direction);
                Direction rightOfTarget = EnumUtils.RightOf(direction);
                Position leftNeighborPosition = targetPiece.Position.NeighborAt(leftOfTarget);
                Position rightNeighborPosition = targetPiece.Position.NeighborAt(rightOfTarget);

                Piece topLeftNeighbor = GetPieceOnTop(leftNeighborPosition);
                Piece topRightNeighbor = GetPieceOnTop(rightNeighborPosition);

                // At least one neighbor is present
                int currentHeight = targetPiece.Position.Stack + 1;
                int destinationHeight = null != topNeighbor ? topNeighbor.Position.Stack + 1 : 0;

                int topLeftNeighborHeight = null != topLeftNeighbor ? topLeftNeighbor.Position.Stack + 1 : 0;
                int topRightNeighborHeight = null != topRightNeighbor ? topRightNeighbor.Position.Stack + 1 : 0;

                Position targetPosition = newPosition.GetShifted(0, 0, 0, destinationHeight);
                Move targetMove = new Move(targetPiece.PieceName, targetPosition);

                // "Take-off" beetle
                currentHeight--;

                if (currentHeight == destinationHeight && destinationHeight == 0)
                {
                    if ((topLeftNeighborHeight == 0 && topRightNeighborHeight != 0) ||
                        (topLeftNeighborHeight != 0 && topRightNeighborHeight == 0))
                    {
                        // Slide on bottom
                        validMoves.Add(targetMove);
                    }
                }
                else
                {
                    if ((topLeftNeighborHeight <= destinationHeight && topRightNeighborHeight <= destinationHeight) ||
                            (topLeftNeighborHeight > destinationHeight && topRightNeighborHeight == destinationHeight) ||
                            (topLeftNeighborHeight == destinationHeight && topRightNeighborHeight > destinationHeight))
                    {
                        // Slide on top
                        validMoves.Add(targetMove);
                    }
                    else if ((topLeftNeighborHeight > destinationHeight && topRightNeighborHeight > destinationHeight) &&
                             (topLeftNeighborHeight <= currentHeight && topRightNeighborHeight <= currentHeight))
                    {
                        // Step up and out or down and in
                        validMoves.Add(targetMove);
                    }
                }
            }

            return validMoves;
        }

        private MoveSet GetValidGrasshopperMovements(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

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
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            // Get all slides all the way around
            return GetValidSlides(targetPiece, null);
        }

        private MoveSet GetValidSlides(Piece targetPiece, int? maxRange)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            if (maxRange.HasValue && maxRange.Value < 1)
            {
                throw new ArgumentOutOfRangeException("maxRange");
            }

            MoveSet validMoves = new MoveSet();

            GetValidSlides(targetPiece, targetPiece.Position, 0, maxRange, validMoves);

            return validMoves;
        }

        private void GetValidSlides(Piece targetPiece, Position startingPosition, int currentRange, int? maxRange, MoveSet validMoves)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            if (null == startingPosition)
            {
                throw new ArgumentNullException("startingPosition");
            }

            if (maxRange.HasValue && maxRange.Value < 1)
            {
                throw new ArgumentOutOfRangeException("maxRange");
            }

            if (null == validMoves)
            {
                throw new ArgumentNullException("validMoves");
            }

            if (!maxRange.HasValue || currentRange < maxRange.Value)
            {
                foreach (Direction slideDirection in EnumUtils.Directions)
                {
                    Position slidePosition = targetPiece.Position.NeighborAt(slideDirection);

                    if (slidePosition != startingPosition && CanSlide(targetPiece.Position, slideDirection))
                    {
                        Move move = new Move(targetPiece.PieceName, slidePosition);

                        if (validMoves.Add(move))
                        {
                            Position preSlidePosition = targetPiece.Position;
                            targetPiece.Move(move.Position);
                            GetValidSlides(targetPiece, startingPosition, currentRange + 1, maxRange, validMoves);
                            targetPiece.Move(preSlidePosition);
                        }
                    }
                }
            }
        }

        private bool CanSlide(Position position, Direction direction)
        {
            if (null == position)
            {
                throw new ArgumentNullException("position");
            }

            Direction right = EnumUtils.RightOf(direction);
            Direction left = EnumUtils.LeftOf(direction);

            return !HasPieceAt(position.NeighborAt(direction)) && HasPieceAt(position.NeighborAt(right)) != HasPieceAt(position.NeighborAt(left));
        }

        protected bool CanMoveWithoutBreakingHive(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

            if (targetPiece.InPlay && targetPiece.Position.Stack == 0)
            {
                // Temporarily remove piece from board
                Position originalPosition = targetPiece.Position;
                targetPiece.Move(null);

                // Determine if the hive is broken
                bool isOneHive = IsOneHive();

                // Return piece to the board
                targetPiece.Move(originalPosition);

                return isOneHive;
            }

            return true;
        }

        protected bool PlacingPieceInOrder(Piece targetPiece)
        {
            if (null == targetPiece)
            {
                throw new ArgumentNullException("targetPiece");
            }

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

        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}{1}", BoardState.ToString(), BoardStringSeparator);

            sb.AppendFormat("{0}[{1}]{2}", CurrentTurnColor.ToString(), CurrentPlayerTurn, BoardStringSeparator);

            foreach (Piece piece in PiecesInPlay)
            {
                sb.AppendFormat("{0}{1}", piece.ToString(), BoardStringSeparator);
            }

            return sb.ToString().TrimEnd(BoardStringSeparator);
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
