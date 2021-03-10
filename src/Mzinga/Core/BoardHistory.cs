// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mzinga.Core
{
    public class BoardHistory : IReadOnlyList<BoardHistoryItem>
    {
        public int Count => m_items.Count;

        public BoardHistoryItem this[int index] => m_items[index];

        public Move? LastMove => Count > 0 ? m_items[^1].Move : null;

        private readonly List<BoardHistoryItem> m_items = new List<BoardHistoryItem>();

        internal void Add(Move move, string moveStr)
        {
            m_items.Add(new BoardHistoryItem(move, moveStr));
        }

        internal void UndoLast()
        {
            m_items.RemoveAt(Count - 1);
        }

        public IEnumerator<BoardHistoryItem> GetEnumerator()
        {
            return m_items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_items.GetEnumerator();
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
    }
}
