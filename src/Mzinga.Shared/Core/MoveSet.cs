// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Mzinga.Core
{
    [Serializable]
    public class MoveSet : HashSet<Move>
    {
        public static readonly MoveSet EmptySet = new MoveSet();

        public MoveSet() : base()
        {
        }

        public MoveSet(string moveSetString) : this()
        {
            if (string.IsNullOrWhiteSpace(moveSetString))
            {
                throw new ArgumentNullException(nameof(moveSetString));
            }

            string[] split = moveSetString.Split(MoveStringSeparator);

            for (int i = 0; i < split.Length; i++)
            {
                Move parseMove = new Move(split[i]);
                Add(parseMove);
            }
        }

        public void Add(IEnumerable<Move> moves) => UnionWith(moves);

        public void Remove(IEnumerable<Move> moves) => ExceptWith(moves);

        public bool Contains(PieceName pieceName)
        {
            if (pieceName == PieceName.INVALID)
            {
                throw new ArgumentOutOfRangeException(nameof(pieceName));
            }

            foreach (Move move in this)
            {
                if (move.PieceName == pieceName)
                {
                    return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Move move in this)
            {
                sb.AppendFormat("{0}{1}", move.ToString(), MoveStringSeparator);
            }

            return sb.ToString().TrimEnd(MoveStringSeparator);
        }

        public const char MoveStringSeparator = ';';
    }
}
