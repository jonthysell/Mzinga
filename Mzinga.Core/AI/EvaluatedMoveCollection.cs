// 
// EvaluatedMoveCollection.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016 Jon Thysell <http://jonthysell.com>
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

namespace Mzinga.Core.AI
{
    public class EvaluatedMoveCollection : IEnumerable<EvaluatedMove>
    {
        public int Count
        {
            get
            {
                return _evaluatedMoves.Count;
            }
        }

        public EvaluatedMove this[int index]
        {
            get
            {
                return _evaluatedMoves[index];
            }
        }

        public bool SortAscending { get; private set; }

        public double BestScore { get; private set; }

        private IComparer<EvaluatedMove> _comparer;

        private List<EvaluatedMove> _evaluatedMoves;

        public EvaluatedMoveCollection(bool sortAscending = false)
        {
            SortAscending = sortAscending;

            _evaluatedMoves = new List<EvaluatedMove>(DefaultCapacity);

            if (SortAscending)
            {
                _comparer = new EvaluatedMoveAscendingComparer();
            }
            else
            {
                _comparer = new EvaluatedMoveDescendingComparer();
            }
        }

        public void Add(IEnumerable<EvaluatedMove> evaluatedMoves)
        {
            foreach (EvaluatedMove evaluatedMove in evaluatedMoves)
            {
                Add(evaluatedMove);
            }
        }

        public bool Add(EvaluatedMove evaluatedMove)
        {
            if (null == evaluatedMove)
            {
                throw new ArgumentNullException("evaluatedMove");
            }

            int index = SearchFor(evaluatedMove);

            if (index < 0)
            {
                index = ~index;

                if (index == 0)
                {
                    BestScore = evaluatedMove.ScoreAfterMove;
                }

                if (index == _evaluatedMoves.Count)
                {
                    _evaluatedMoves.Add(evaluatedMove);
                }
                else
                {
                    _evaluatedMoves.Insert(index, evaluatedMove);
                }

                return true;
            }

            return false;
        }

        public IEnumerable<EvaluatedMove> GetBestMoves()
        {
            foreach (EvaluatedMove evaluatedMove in this)
            {
                if (evaluatedMove.ScoreAfterMove != BestScore)
                {
                    break;
                }

                yield return evaluatedMove;
            }
        }

        public IEnumerator<EvaluatedMove> GetEnumerator()
        {
            foreach (EvaluatedMove evaluatedMove in _evaluatedMoves)
            {
                yield return evaluatedMove;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private int SearchFor(EvaluatedMove evaluatedMove)
        {
            return _evaluatedMoves.BinarySearch(evaluatedMove, _comparer);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (EvaluatedMove evaluatedMove in this)
            {
                sb.AppendFormat("{0}{1}", evaluatedMove.Move, EvaluatedMoveStringSeparator);
            }

            return sb.ToString().TrimEnd(EvaluatedMoveStringSeparator);
        }

        private const int DefaultCapacity = 256;

        public const char EvaluatedMoveStringSeparator = ';';

        private class EvaluatedMoveAscendingComparer : IComparer<EvaluatedMove>
        {
            public int Compare(EvaluatedMove a, EvaluatedMove b)
            {
                if (null == a)
                {
                    throw new ArgumentNullException("a");
                }

                if (null == b)
                {
                    throw new ArgumentNullException("b");
                }

                int result = a.CompareTo(b);

                if (result == 0)
                {
                    result = a.Move.CompareTo(b.Move);
                }

                return result;
            }
        }

        private class EvaluatedMoveDescendingComparer : IComparer<EvaluatedMove>
        {
            public int Compare(EvaluatedMove a, EvaluatedMove b)
            {
                if (null == a)
                {
                    throw new ArgumentNullException("a");
                }

                if (null == b)
                {
                    throw new ArgumentNullException("b");
                }

                int result = b.CompareTo(a);

                if (result == 0)
                {
                    result = a.Move.CompareTo(b.Move);
                }

                return result;
            }
        }
    }
}
