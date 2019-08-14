// 
// MoveSet.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017, 2018, 2019 Jon Thysell <http://jonthysell.com>
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

namespace Mzinga.Core
{
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
                throw new ArgumentOutOfRangeException("pieceName");
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
