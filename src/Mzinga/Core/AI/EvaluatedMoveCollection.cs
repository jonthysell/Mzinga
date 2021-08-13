// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mzinga.Core.AI
{
    class EvaluatedMoveCollection : IReadOnlyList<EvaluatedMove>
    {
        public int Count => _evaluatedMoves.Count;

        public EvaluatedMove this[int index] => _evaluatedMoves[index];

        public EvaluatedMove? BestMove
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

        private readonly List<EvaluatedMove> _evaluatedMoves = new List<EvaluatedMove>();

        public EvaluatedMoveCollection() { }

        public EvaluatedMoveCollection(IEnumerable<EvaluatedMove> evaluatedMoves, bool resort) : this()
        {
            Add(evaluatedMoves, resort);
        }

        public void Add(IEnumerable<EvaluatedMove> evaluatedMoves, bool resort)
        {
            foreach (EvaluatedMove evaluatedMove in evaluatedMoves)
            {
                if (resort)
                {
                    Add(evaluatedMove);
                }
                else
                {
                    _evaluatedMoves.Add(evaluatedMove);
                }
            }
        }

        public void Add(EvaluatedMove evaluatedMove)
        {
            int index = SearchFor(evaluatedMove);

            if (index < 0)
            {
                index = ~index;
            }

            if (index == _evaluatedMoves.Count)
            {
                _evaluatedMoves.Add(evaluatedMove);
            }
            else
            {
                _evaluatedMoves.Insert(index, evaluatedMove);
            }
        }

        public void PruneGameLosingMoves()
        {
            int firstGameLosingMoveIndex = -1;

            for (int i = 0; i < _evaluatedMoves.Count; i++)
            {
                if (double.IsNegativeInfinity(_evaluatedMoves[i].ScoreAfterMove))
                {
                    firstGameLosingMoveIndex = i;
                    break;
                }
            }

            if (firstGameLosingMoveIndex > 0)
            {
                _evaluatedMoves.RemoveRange(firstGameLosingMoveIndex, _evaluatedMoves.Count - firstGameLosingMoveIndex);
            }
        }

        public IEnumerator<EvaluatedMove> GetEnumerator()
        {
            return _evaluatedMoves.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private int SearchFor(EvaluatedMove evaluatedMove)
        {
            return _evaluatedMoves.BinarySearch(evaluatedMove);
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
    }
}
