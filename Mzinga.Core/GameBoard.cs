// 
// GameBoard.cs
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
    public delegate void BoardChangedEventHandler();

    public class GameBoard : Board
    {
        #region Properties

        public IEnumerable<BoardHistoryItem> BoardHistory
        {
            get
            {
                return _boardHistory.AsEnumerable();
            }
        }
        private BoardHistory _boardHistory;

        public event BoardChangedEventHandler BoardChanged;

        #endregion

        public GameBoard() : base()
        {
            _boardHistory = new BoardHistory();
        }

        public GameBoard Clone()
        {
            GameBoard clone = new GameBoard();
            foreach (BoardHistoryItem item in BoardHistory)
            {
                clone.Play(item.Move);
            }
            return clone;
        }

        public BoardState TryMove(Move move)
        {
            Play(move);
            BoardState result = BoardState;
            UndoLastMove();
            return result;
        }

        public void Play(Move move)
        {
            if (BoardState == BoardState.Draw || BoardState == BoardState.WhiteWins || BoardState == BoardState.BlackWins)
            {
                throw new InvalidMoveException(move, "You can't play, the game is over.");
            }

            if (null == move)
            {
                throw new ArgumentNullException("move");
            }

            if (move.IsPass)
            {
                Pass();
                return;
            }

            Piece targetPiece = GetPiece(move.PieceName);

            if (targetPiece.Color != CurrentTurnColor)
            {
                throw new InvalidMoveException(move, "It's not your turn.");
            }

            if (null == move.Position)
            {
                throw new InvalidMoveException(move, "You can't put a piece back into your hand.");
            }

            if (CurrentPlayerTurn == 1 && move.BugType == BugType.QueenBee)
            {
                throw new InvalidMoveException(move, "You can't play your Queen Bee on your first turn.");
            }

            if (!CurrentTurnQueenInPlay)
            {
                if (CurrentPlayerTurn == 4 && targetPiece.BugType != BugType.QueenBee)
                {
                    throw new InvalidMoveException(move, "You must play your Queen Bee on or before your fourth turn.");
                }
                else if (targetPiece.InPlay)
                {
                    throw new InvalidMoveException(move, "You can't move a piece in play until you've played your Queen Bee.");
                }
            }

            if (!PlacingPieceInOrder(targetPiece))
            {
                throw new InvalidMoveException(move, "When there are multiple pieces of the same bug type, you must play the pieces in order.");
            }

            if (targetPiece.InPlay)
            {
                if (targetPiece.Position == move.Position)
                {
                    throw new InvalidMoveException(move, "You can't move a piece to its current position.");
                }
                else if (!PieceIsOnTop(targetPiece))
                {
                    throw new InvalidMoveException(move, "You can't move that piece because it has another piece on top of it.");
                }
                else if (!CanMoveWithoutBreakingHive(targetPiece))
                {
                    throw new InvalidMoveException(move, "You can't move that piece because it will break the hive.");
                }
            }

            if (HasPieceAt(move.Position))
            {
                throw new InvalidMoveException(move, "You can't move there because a piece already exists at that position.");
            }

            MoveSet validMoves = GetValidMoves(targetPiece);

            if (!validMoves.Contains(move))
            {
                throw new InvalidMoveException(move);
            }

            Position originalPosition = targetPiece.Position;

            targetPiece.Move(move.Position);

            _boardHistory.Add(move, originalPosition);

            CurrentTurn++;
            OnBoardChanged();
        }

        public void Pass()
        {
            Move pass = Move.Pass;

            if (BoardState == BoardState.Draw || BoardState == BoardState.WhiteWins || BoardState == BoardState.BlackWins)
            {
                throw new InvalidMoveException(pass, "You can't pass, the game is over.");
            }

            if (!GetValidMoves().Contains(pass))
            {
                throw new InvalidMoveException(pass, "You can't pass when you have valid moves.");
            }

            _boardHistory.Add(pass, null);

            CurrentTurn++;
            OnBoardChanged();
        }

        public void UndoLastMove()
        {
            if (_boardHistory.Count == 0)
            {
                throw new InvalidOperationException("You can't undo any more moves.");
            }

            BoardHistoryItem item = _boardHistory.UndoLastMove();

            if (!item.Move.IsPass)
            {
                Piece targetPiece = GetPiece(item.Move.PieceName);
                targetPiece.Move(item.OriginalPosition);
            }
            
            CurrentTurn--;
            OnBoardChanged();
        }

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

        private MoveSet GetValidMoves(Piece targetPiece)
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
                if (piece.Color == targetPiece.Color)
                {
                    // Piece is in play and the right color, look through neighbors
                    foreach (Position neighbor in piece.Position.Neighbors)
                    {
                        if (!HasPieceAt(neighbor))
                        {
                            // Neighboring position is a potential, verify its neighbors are empty or same color
                            bool validPlacement = true;
                            foreach (Position surroundingPosition in neighbor.Neighbors)
                            {
                                Piece topPiece = GetPieceOnTop(surroundingPosition);
                                if (null != topPiece && topPiece.Color != targetPiece.Color)
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
                int destinationHeight = null != topNeighbor ? topNeighbor.Position.Stack + 1: 0;

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

        private void OnBoardChanged()
        {
            bool whiteQueenSurrounded = (CountNeighbors(PieceName.WhiteQueenBee) == 6);
            bool blackQueenSurrounded = (CountNeighbors(PieceName.BlackQueenBee) == 6);

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

            if (null != BoardChanged)
            {
                BoardChanged();
            }
        }

        public bool CanMoveWithoutBreakingHive(Piece targetPiece)
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

        private bool PlacingPieceInOrder(Piece targetPiece)
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

        public GameBoard GetNormalized()
        {
            GameBoard normalizedBoard = Clone();

            Piece firstWhitePiece = null;
            Piece firstBlackPiece = null;

            foreach (Piece piece in normalizedBoard.PiecesInPlay)
            {
                if (piece.Color == Color.White && null == firstWhitePiece)
                {
                    firstWhitePiece = piece;
                }
                else if (piece.Color == Color.Black && null == firstBlackPiece)
                {
                    firstBlackPiece = piece;
                }
            }

            if (null != firstWhitePiece)
            {
                // Shift first (white) piece to origin
                int deltaX = 0 - firstWhitePiece.Position.X;
                int deltaY = 0 - firstWhitePiece.Position.Y;
                int deltaZ = 0 - firstWhitePiece.Position.Z;

                if (deltaX != 0 || deltaY != 0 || deltaZ != 0)
                {
                    foreach (Piece piece in normalizedBoard.PiecesInPlay)
                    {
                        piece.Shift(deltaX, deltaY, deltaZ);
                    }
                }

                if (null != firstBlackPiece)
                {
                    
                    int rotations = 0;
                    Position rotatedPos = firstBlackPiece.Position.Clone();

                    // Rotate so that first black piece has positive Q and zero or positive R
                    while (rotatedPos.Q <= 0 || rotatedPos.R < 0)
                    {
                        rotatedPos = rotatedPos.GetRotatedRight();
                        rotations++;
                    }

                    if (rotations > 0)
                    {
                        foreach (Piece piece in normalizedBoard.PiecesInPlay)
                        {
                            for (int i = 0; i < rotations; i++)
                            {
                                piece.RotateRight();
                            }
                        }
                    }
                }
            }

            return normalizedBoard;
        }
    }
}
