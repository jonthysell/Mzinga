// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Mzinga.Core
{
    public class MoveSet : HashSet<Move>
    {
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
    }
}