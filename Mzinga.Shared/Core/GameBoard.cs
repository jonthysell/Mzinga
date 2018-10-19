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
using System.Text;
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

        public BoardHistoryItem LastMove
        {
            get
            {
                return _boardHistory.LastMove;
            }
        }

        public event EventHandler BoardChanged;

        #endregion

        public GameBoard(ExpansionPieces expansionPieces = ExpansionPieces.None) : base(expansionPieces) { }

        public GameBoard(string boardString) : base(boardString) { }

        public GameBoard Clone()
        {
            GameBoard clone = new GameBoard(ExpansionPieces);
            foreach (BoardHistoryItem item in BoardHistory)
            {
                clone.TrustedPlay(item.Move, item.MoveString);
            }
            return clone;
        }

        public void Play(Move move, string moveString = null)
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

            if (!GetValidMoves().Contains(move))
            {
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

                throw new InvalidMoveException(move);
            }

            TrustedPlay(move, null != moveString ? NotationUtils.NormalizeBoardSpaceMoveString(moveString) : NotationUtils.ToBoardSpaceMoveString(this, move));
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

            TrustedPlay(pass, NotationUtils.BoardSpacePass);
        }

        internal void TrustedPlay(Move move, string moveString = null)
        {
            Position originalPosition = null;

            if (!move.IsPass)
            {
                Piece targetPiece = GetPiece(move.PieceName);

                originalPosition = targetPiece.Position;

                MovePiece(targetPiece, move.Position);
            }

            _boardHistory.Add(move, originalPosition, moveString);

            CurrentTurn++;
            LastPieceMoved = move.PieceName;

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

            Move previousMove = _boardHistory.LastMove?.Move;

            LastPieceMoved = null != previousMove ? previousMove.PieceName : PieceName.INVALID;
            CurrentTurn--;

            OnBoardChanged();
        }

        // Following the example at https://chessprogramming.wikispaces.com/Perft
        public long CalculatePerft(int depth)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            Task<long?> task = CalculatePerftAsync(depth, cts.Token);
            task.Wait();

            return task.Result.Value;
        }

        public async Task<long?> CalculatePerftAsync(int depth, CancellationToken token)
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

            long? nodes = null;

            foreach (Move move in validMoves)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                TrustedPlay(move);
                long? value = await CalculatePerftAsync(depth - 1, token);
                UndoLastMove();

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

                    GameBoard clone = Clone();
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

        public string ToGameString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0}{1}", EnumUtils.GetExpansionPiecesString(ExpansionPieces), BoardStringSeparator);

            sb.AppendFormat("{0}{1}", BoardState.ToString(), BoardStringSeparator);

            sb.AppendFormat("{0}[{1}]{2}", CurrentTurnColor.ToString(), CurrentPlayerTurn, BoardStringSeparator);

            foreach (BoardHistoryItem item in BoardHistory)
            {
                sb.AppendFormat("{0}{1}", item.MoveString ?? NotationUtils.ToBoardSpaceMoveString(this, item.Move), BoardStringSeparator);
            }

            return sb.ToString().TrimEnd(BoardStringSeparator);
        }

        public static GameBoard ParseGameString(string gameString, bool trusted = false)
        {
            if (string.IsNullOrWhiteSpace(gameString))
            {
                throw new ArgumentNullException("gameString");
            }

            string[] split = gameString.Split(BoardStringSeparator);

            if (!EnumUtils.TryParseExpansionPieces(split[0], out ExpansionPieces expansionPieces))
            {
                throw new ArgumentException("Couldn't parse expansion pieces.", "gameString");
            }

            GameBoard gb = new GameBoard(expansionPieces);

            for (int i = 3; i < split.Length; i++)
            {
                string moveString = split[i];
                Move move = NotationUtils.ParseMoveString(gb, moveString);
                gb.Play(move, moveString);
            }

            return gb;
        }
    }
}
