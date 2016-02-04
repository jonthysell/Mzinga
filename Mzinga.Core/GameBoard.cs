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

        public int BoardHistoryCount
        {
            get
            {
                return _boardHistory.Count;
            }
        }

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

        public BoardMetrics TryMove(Move move)
        {
            Play(move);
            BoardMetrics result = GetBoardMetrics();
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
