// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mzinga.Core
{
    public class MoveSet : IReadOnlyCollection<Move>
    {
        private readonly List<Move> _moves = new List<Move>(32);

        public int Count => _moves.Count;

        public bool Contains(Move move)
        {
            return _moves.Contains(move);
        }

        public bool Contains(PieceName pieceName)
        {
            foreach (var item in this)
            {
                if (item.PieceName == pieceName)
                {
                    return true;
                }
            }

            return false;
        }
        
        internal bool Add(Move move)
        {
            if (_moves.Contains(move))
            {
                return false;
            }

            _moves.Add(move);
            return true;
        }

        internal void FastAdd(Move move)
        {
            _moves.Add(move);
        }

        internal void Clear()
        {
            _moves.Clear();
        }

        internal void ValidateSet()
        {
            var set = _moves.ToHashSet();
            if (set.Count != Count)
            {
                throw new Exception("MoveSet contains duplicates.");
            }
        }

        public static MoveSet ParseMoveList(Board board, string moveList, string separator = ";")
        {
            var moves = new MoveSet();
            foreach (var inputMoveStr in moveList.Split(separator))
            {
                if (!board.TryParseMove(inputMoveStr, out Move move, out string _))
                {
                    throw new Exception($"Unable to parse '{inputMoveStr}'.");
                }
                moves.Add(move);
            }
            return moves;
        }

        public IEnumerator<Move> GetEnumerator()
        {
            return _moves.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _moves.GetEnumerator();
        }
    }
}