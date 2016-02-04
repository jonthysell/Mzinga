// 
// BoardHistory.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mzinga.Core
{
    public class BoardHistory : IEnumerable<BoardHistoryItem>
    {
        public int Count
        {
            get
            {
                return _items.Count;
            }
        }

        private List<BoardHistoryItem> _items;

        public BoardHistory()
        {
            _items = new List<BoardHistoryItem>();
        }

        public BoardHistory(IEnumerable<BoardHistoryItem> items) : this()
        {
            _items.AddRange(items);
        }

        public BoardHistory(string boardHistoryString) : this()
        {
            if (String.IsNullOrWhiteSpace(boardHistoryString))
            {
                throw new ArgumentOutOfRangeException("boardHistoryString");
            }

            string[] split = boardHistoryString.Split(BoardHistory.BoardHistoryItemStringSeparator);

            for (int i = 0; i < split.Length; i++)
            {
                BoardHistoryItem parseItem = new BoardHistoryItem(split[i]);
                _items.Add(parseItem);
            }
        }

        public void Add(Move move, Position originalPosition)
        {
            BoardHistoryItem item = new BoardHistoryItem(move, originalPosition);
            _items.Add(item);
        }

        public BoardHistoryItem UndoLastMove()
        {
            if (Count > 0)
            {
                BoardHistoryItem item = _items.Last();
                _items.Remove(item);
                return item;
            }

            return null;
        }

        public IEnumerator<BoardHistoryItem> GetEnumerator()
        {
            foreach (BoardHistoryItem item in _items)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (BoardHistoryItem item in _items)
            {
                sb.AppendFormat("{0}{1}", item.ToString(), BoardHistoryItemStringSeparator);
            }

            return sb.ToString().TrimEnd(BoardHistoryItemStringSeparator);
        }

        public const char BoardHistoryItemStringSeparator = ';';
    }

    public class BoardHistoryItem
    {
        public Move Move { get; private set; }
        public Position OriginalPosition { get; private set; }

        public BoardHistoryItem(Move move, Position originalPosition)
        {
            if (null == move)
            {
                throw new ArgumentNullException("move");
            }

            Move = move;
            OriginalPosition = originalPosition;
        }

        public BoardHistoryItem(string boardHistoryItemString)
        {
            if (String.IsNullOrWhiteSpace(boardHistoryItemString))
            {
                throw new ArgumentNullException("boardHistoryItemString");
            }

            if (boardHistoryItemString.Equals(Move.PassString, StringComparison.CurrentCultureIgnoreCase))
            {
                Move = Move.Pass;
                OriginalPosition = null;
            }
            else
            {
                string[] split = boardHistoryItemString.Split(new char[] { ' ', '>' }, StringSplitOptions.RemoveEmptyEntries);

                Piece startingPiece = new Piece(split[0]);
                Move move = new Move(split[1]);

                Move = move;
                OriginalPosition = startingPiece.Position;
            }
        }

        public override string ToString()
        {
            return ToString(BoardHistoryItemStringFormat.FullAlgebraic);
        }

        public string ToString(BoardHistoryItemStringFormat format)
        {
            if (Move.IsPass)
            {
                return Move.ToString();
            }

            switch (format)
            {
                case BoardHistoryItemStringFormat.ShortAlgebraic:
                    return Move.ToString();
                default:
                    Piece startingPiece = new Piece(Move.PieceName, OriginalPosition);
                    return String.Format("{0} > {1}", startingPiece, Move);
            }
        }
    }

    public enum BoardHistoryItemStringFormat
    {
        FullAlgebraic,
        ShortAlgebraic
    }
}
