// 
// MoveSet.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015 Jon Thysell <http://jonthysell.com>
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
    public class MoveSet : IEnumerable<Move>
    {
        public int Count
        {
            get
            {
                return _moves.Count;
            }
        }

        private List<Move> _moves;

        public MoveSet()
        {
            _moves = new List<Move>();
        }

        public void Add(IEnumerable<Move> moves)
        {
            if (null == moves)
            {
                throw new ArgumentNullException("moves");
            }

            foreach (Move move in moves)
            {
                Add(move);
            }
        }

        public bool Add(Move move)
        {
            if (null == move)
            {
                throw new ArgumentNullException("move");
            }

            int index = SearchFor(move);

            if (index < 0)
            {
                index = ~index;

                if (index == _moves.Count)
                {
                    _moves.Add(move);
                }
                else
                {
                    _moves.Insert(index, move);
                }

                return true;
            }

            return false;
        }

        public void Remove(IEnumerable<Move> moves)
        {
            if (null == moves)
            {
                throw new ArgumentNullException("moves");
            }

            foreach (Move move in moves)
            {
                Remove(move);
            }
        }

        public bool Remove(Move move)
        {
            if (null == move)
            {
                throw new ArgumentNullException("move");
            }

            int index = SearchFor(move);

            if (index >= 0)
            {
                _moves.RemoveAt(index);
                return true;
            }

            return false;
        }

        public bool Contains(Move move)
        {
            if (null == move)
            {
                throw new ArgumentNullException("move");
            }

            return SearchFor(move) >= 0;
        }

        private int SearchFor(Move move)
        {
            List<Move> tempList = new List<Move>(_moves);
            return Array.BinarySearch<Move>(tempList.ToArray(), move);
        }

        public IEnumerator<Move> GetEnumerator()
        {
            foreach (Move move in _moves)
            {
                yield return move;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Move move in _moves)
            {
                sb.AppendFormat("{0}{1}", move.ToString(), MoveStringSeparator);
            }

            return sb.ToString().TrimEnd(MoveStringSeparator);
        }

        public const char MoveStringSeparator = ';';
    }
}
