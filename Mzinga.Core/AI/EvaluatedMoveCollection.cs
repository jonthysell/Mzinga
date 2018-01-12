// 
// EvaluatedMoveCollection.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017, 2018 Jon Thysell <http://jonthysell.com>
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

using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mzinga.Core.AI
{
    internal class EvaluatedMoveCollection : IEnumerable<EvaluatedMove>
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

        public double BestScore { get; private set; }

        public EvaluatedMove BestMove
        {
            get
            {
                if (_evaluatedMoves.Count > 0)
                {
                    return _evaluatedMoves[0];
                }

                return null;
            }
        }

        private IComparer<EvaluatedMove> _comparer = new EvaluatedMoveDescendingComparer();

        private List<EvaluatedMove> _evaluatedMoves;

        public EvaluatedMoveCollection()
        {
            _evaluatedMoves = new List<EvaluatedMove>();
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

        public void Update(EvaluatedMove evaluatedMove)
        {
            int foundIndex = -1;

            for (int i = 0; i < _evaluatedMoves.Count; i++)
            {
                if (evaluatedMove.Move == _evaluatedMoves[i].Move)
                {
                    if (evaluatedMove.Depth > _evaluatedMoves[i].Depth)
                    {
                        foundIndex = i;
                        break;
                    }
                }
            }

            if (foundIndex >= 0)
            {
                _evaluatedMoves.RemoveAt(foundIndex);
                Add(evaluatedMove);
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
            return GetEnumerator();
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

        public const char EvaluatedMoveStringSeparator = ';';

        private class EvaluatedMoveDescendingComparer : IComparer<EvaluatedMove>
        {
            public int Compare(EvaluatedMove a, EvaluatedMove b)
            {
                int result = b.CompareTo(a);

                if (result == 0)
                {
                    result = b.Move.CompareTo(a.Move);
                }

                return result;
            }
        }
    }
}
