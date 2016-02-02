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
        #region Properties

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

        public IEnumerable<Piece> AllPieces
        {
            get
            {
                return _pieces.AsEnumerable();
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

        public bool WhiteQueenInPlay
        {
            get
            {
                return GetPiece(PieceName.WhiteQueenBee).InPlay;
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

        public IEnumerable<Piece> PiecesInPlay
        {
            get
            {
                return _pieces.Where<Piece>((piece) => { return piece.InPlay; });
            }
        }

        private Piece[] _pieces;

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
