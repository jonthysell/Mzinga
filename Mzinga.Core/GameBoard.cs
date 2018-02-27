// 
// GameBoard.cs
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
using System.Threading;
using System.Threading.Tasks;

namespace Mzinga.Core
{
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
                return _boardHistory;
            }
        }
        private BoardHistory _boardHistory = new BoardHistory();

        public event EventHandler BoardChanged;

        #endregion

        public GameBoard(ExpansionPieces expansionPieces = ExpansionPieces.None) : base(expansionPieces) { }

        protected GameBoard(string boardString) : base(boardString) { }

        public GameBoard Clone()
        {
            GameBoard clone = new GameBoard();
            foreach (BoardHistoryItem item in BoardHistory)
            {
                clone.Play(item.Move);
            }
            return clone;
        }

        public void Play(Move move)
        {
            if (null == move)
            {
                throw new ArgumentNullException("move");
            }

            if (move.IsPass)
            {
                Pass();
                return;
            }

            if (GameIsOver)
            {
                throw new InvalidMoveException(move, "You can't play, the game is over.");
            }

            if (move.Color != CurrentTurnColor)
            {
                throw new InvalidMoveException(move, "It's not that player's turn.");
            }

            if (!EnumUtils.IsEnabled(move.PieceName, ExpansionPieces))
            {
                throw new InvalidMoveException(move, "That piece is not enabled in this game.");
            }

            if (null == move.Position)
            {
                throw new InvalidMoveException(move, "You can't put a piece back into your hand.");
            }

            if (CurrentPlayerTurn == 1 && move.BugType == BugType.QueenBee)
            {
                throw new InvalidMoveException(move, "You can't play your Queen Bee on your first turn.");
            }

            Piece targetPiece = GetPiece(move.PieceName);

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

            if (HasPieceAt(move.Position))
            {
                throw new InvalidMoveException(move, "You can't move there because a piece already exists at that position.");
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

            MoveSet validMoves = GetValidMoves(targetPiece.PieceName);

            if (!validMoves.Contains(move))
            {
                throw new InvalidMoveException(move);
            }

            TrustedPlay(move);
        }

        public void Pass()
        {
            Move pass = Move.Pass;

            if (GameIsOver)
            {
                throw new InvalidMoveException(pass, "You can't pass, the game is over.");
            }

            if (!GetValidMoves().Contains(pass))
            {
                throw new InvalidMoveException(pass, "You can't pass when you have valid moves.");
            }

            TrustedPlay(pass);
        }

        internal void TrustedPlay(Move move)
        {
            Position originalPosition = null;

            if (!move.IsPass)
            {
                Piece targetPiece = GetPiece(move.PieceName);

                originalPosition = targetPiece.Position;

                MovePiece(targetPiece, move.Position);
            }

            _boardHistory.Add(move, originalPosition);

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
                MovePiece(targetPiece, item.OriginalPosition);
            }
            
            CurrentTurn--;
            OnBoardChanged();
        }

        // Following the example at https://chessprogramming.wikispaces.com/Perft
        public long CalculatePerft(int depth)
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

            foreach (Move move in validMoves)
            {
                TrustedPlay(move);
                nodes += CalculatePerft(depth - 1);
                UndoLastMove();
            }

            return nodes;
        }

        public long ParallelPerft(int depth, int maxThreads)
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

            ParallelOptions po = new ParallelOptions();
            po.MaxDegreeOfParallelism = Math.Max(1, maxThreads);

            long nodes = 0;
            Parallel.ForEach(validMoves, po, (move) =>
            {
                GameBoard clone = Clone();
                clone.TrustedPlay(move);
                Interlocked.Add(ref nodes, clone.CalculatePerft(depth - 1));
            });

            return nodes;
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

            BoardChanged?.Invoke(this, null);
        }
    }
}
