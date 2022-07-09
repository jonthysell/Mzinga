// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

namespace Mzinga.Core.AI
{
    public class GameAIConfig
    {
        public MetricWeights? StartMetricWeights = null;
        public MetricWeights? EndMetricWeights = null;

        public const int MinMaxBranchingFactor = 1;
        public const int DefaultMaxBranchingFactor = 256; // To prevent search explosion
        public const int MaxMaxBranchingFactor = 512;

        public int? MaxBranchingFactor
        {
            get
            {
                return _maxBranchingFactor;
            }
            set
            {
                if (value.HasValue && (value.Value < MinMaxBranchingFactor || value > MaxMaxBranchingFactor))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _maxBranchingFactor = value;
            }
        }
        private int? _maxBranchingFactor = null;

        public const int MinQuiescentSearchMaxDepth = 0;
        public const int DefaultQuiescentSearchMaxDepth = 6; // To prevent runaway stack overflows
        public const int MaxQuiescentSearchMaxDepth = 12;

        public int? QuiescentSearchMaxDepth
        {
            get
            {
                return _quiescentSearchMaxDepth;
            }
            set
            {
                if (value.HasValue && (value.Value < MinQuiescentSearchMaxDepth || value > MaxQuiescentSearchMaxDepth))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _quiescentSearchMaxDepth = value;
            }
        }
        private int? _quiescentSearchMaxDepth = null;

        public const int MinTranspositionTableSizeMB = 0;
        public const int DefaultTranspositionTableSizeMB = 2;
        public const int MaxTranspositionTableSizeMB = 1024;

        public int? TranspositionTableSizeMB
        {
            get
            {
                return _transpositionTableSizeMB;
            }
            set
            {
                if (value.HasValue && (value < MinTranspositionTableSizeMB || value > MaxTranspositionTableSizeMB))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                _transpositionTableSizeMB = value;
            }
        }
        private int? _transpositionTableSizeMB = null;

        public const bool DefaultUseNullAspirationWindow = false;

        public bool? UseNullAspirationWindow { get; set; } = null;
    }
}
