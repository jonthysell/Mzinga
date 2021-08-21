// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;

namespace Mzinga.Core
{
    public class BoardHistory : IReadOnlyList<BoardHistoryItem>
    {
        public int Count => _items.Count;

        public BoardHistoryItem this[int index] => _items[index];

        public Move? LastMove => Count > 0 ? _items[^1].Move : null;

        private readonly List<BoardHistoryItem> _items = new List<BoardHistoryItem>();

        internal void Add(Move move, string moveStr)
        {
            _items.Add(new BoardHistoryItem(move, moveStr));
        }

        internal void UndoLast()
        {
            _items.RemoveAt(Count - 1);
        }

        public IEnumerator<BoardHistoryItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }

    public class BoardHistoryItem
    {
        public readonly Move Move;
        public readonly string MoveString;

        public BoardHistoryItem(Move move, string moveStr)
        {
            Move = move;
            MoveString = moveStr;
        }

        public override string ToString()
        {
            return MoveString;
        }
    }
}
