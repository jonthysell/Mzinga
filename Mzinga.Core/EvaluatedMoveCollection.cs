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

namespace Mzinga.Core
{
    public class EvaluatedMoveCollection
    {
        public int Count
        {
            get
            {
                int count = 0;

                foreach (double score in _evaluatedMoves.Keys)
                {
                    if (null != _evaluatedMoves[score])
                    {
                        count += _evaluatedMoves[score].Count;
                    }
                }

                return count;
            }
        }

        private Dictionary<double, List<EvaluatedMove>> _evaluatedMoves;

        public EvaluatedMoveCollection()
        {
            _evaluatedMoves = new Dictionary<double, List<EvaluatedMove>>();
        }

        public void Add(IEnumerable<EvaluatedMove> evaluatedMoves)
        {
            if (null == evaluatedMoves)
            {
                throw new ArgumentNullException("evaluatedMoves");
            }

            foreach (EvaluatedMove evaluatedMove in evaluatedMoves)
            {
                Add(evaluatedMove);
            }
        }

        public void Add(EvaluatedMove evaluatedMove)
        {
            if (null == evaluatedMove)
            {
                throw new ArgumentNullException("evaluatedMove");
            }

            double score = evaluatedMove.ScoreDelta;

            if (!_evaluatedMoves.ContainsKey(score))
            {
                _evaluatedMoves[score] = new List<EvaluatedMove>();
            }

            _evaluatedMoves[score].Add(evaluatedMove);
        }

        public IEnumerable<EvaluatedMove> GetBestMoves()
        {
            if (_evaluatedMoves.Count == 0)
            {
                return null;
            }

            double maxScore = _evaluatedMoves.Keys.Max();
            return _evaluatedMoves[maxScore].AsEnumerable<EvaluatedMove>();
        }
    }
}
