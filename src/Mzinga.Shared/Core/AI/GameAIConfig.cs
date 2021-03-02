// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

namespace Mzinga.Core.AI
{
    public class GameAIConfig
    {
        public MetricWeights StartMetricWeights = null;
        public MetricWeights EndMetricWeights = null;

        public int? MaxBranchingFactor
        {
            get
            {
                return _maxBranchingFactor;
            }
            set
            {
                if (value.HasValue && value.Value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _maxBranchingFactor = value;
            }
        }
        private int? _maxBranchingFactor = null;

        public int? TranspositionTableSizeMB
        {
            get
            {
                return _transpositionTableSizeMB;
            }
            set
            {
                if (value.HasValue && value.Value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _transpositionTableSizeMB = value;
            }
        }
        private int? _transpositionTableSizeMB = null;

        public TranspositionTable InitialTranspositionTable = null;
    }
}
