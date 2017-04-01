// 
// TrainerSettings.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017 Jon Thysell <http://jonthysell.com>
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

using Mzinga.Core.AI;

namespace Mzinga.Trainer
{
    public class TrainerSettings
    {
        public string ProfilesPath
        {
            get
            {
                return _profilesPath;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException();
                }
                _profilesPath = value;
            }
        }
        private string _profilesPath = null;

        public string WhiteProfilePath
        {
            get
            {
                return _whiteProfilePath;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException();
                }
                _whiteProfilePath = value;
            }
        }
        private string _whiteProfilePath = null;

        public string BlackProfilePath
        {
            get
            {
                return _blackProfilePath;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException();
                }
                _blackProfilePath = value;
            }
        }
        private string _blackProfilePath = null;

        public int CullKeepCount
        {
            get
            {
                return _cullKeepCount;
            }
            set
            {
                if (value < CullMinKeepCount && value != CullKeepMax)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _cullKeepCount = value;
            }
        }
        private int _cullKeepCount = CullKeepMax;

        public const int CullMinKeepCount = 2;
        public const int CullKeepMax = -1;

        public int GenerateCount
        {
            get
            {
                return _GenerateCount;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _GenerateCount = value;
            }
        }
        private int _GenerateCount = 1;

        public double GenerateMinWeight { get; set; } = -100.0;

        public double GenerateMaxWeight { get; set; } = 100.0;
       
        public int LifecycleGenerations
        {
            get
            {
                return _lifecycleGenerations;
            }
            set
            {
                if (value < 0)
                {
                    value = InfiniteLifeCycleGenerations;
                }
                _lifecycleGenerations = value;
            }
        }
        private int _lifecycleGenerations = 1;

        public const int InfiniteLifeCycleGenerations = -1;

        public int LifecycleBattles { get; set; } = 1;

        public int MaxDraws
        {
            get
            {
                return _maxDraws;
            }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _maxDraws = value;
            }
        }
        private int _maxDraws = 1;

        public int MaxBattles
        {
            get
            {
                return _maxBattles;
            }
            set
            {
                if (value < 1 && value != MaxMaxBattles)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _maxBattles = value;
            }
        }
        private int _maxBattles = MaxMaxBattles;

        public const int MaxMaxBattles = -1;

        public bool BattleShuffleProfiles { get; set; } = false;

        public TimeSpan BulkBattleTimeLimit
        {
            get
            {
                if (!_bulkBattleTimeLimit.HasValue)
                {
                    _bulkBattleTimeLimit = TimeSpan.MaxValue;
                }
                return _bulkBattleTimeLimit.Value;
            }
            set
            {
                _bulkBattleTimeLimit = value;
            }
        }
        private TimeSpan? _bulkBattleTimeLimit = null;

        public bool ProvisionalRules { get; set; } = true;

        public int ProvisionalGameCount { get; set; } = 30;

        public double MateMinMix { get; set; } = 0.95;

        public double MateMaxMix { get; set; } = 1.05;

        public int MateParentCount
        {
            get
            {
                return _mateParentCount;
            }
            set
            {
                if (value < MateMinParentCount && value != MateParentMax)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _mateParentCount = value;
            }
        }
        private int _mateParentCount = MateParentMax;

        public const int MateMinParentCount = 2;
        public const int MateParentMax = -1;

        public bool MateShuffleParents { get; set; } = false;

        public int MaxDepth
        {
            get
            {
                return _maxDepth;
            }
            set
            {
                if (value < 0)
                {
                    value = GameAI.IterativeDepth;
                }
                _maxDepth = value;
            }
        }
        public int _maxDepth = GameAI.IterativeDepth;

        public bool UseAlphaBetaPruning { get; set; } = true;

        public bool UseTranspositionTable { get; set; } = true;

        public TimeSpan TurnMaxTime
        {
            get
            {
                if (!_turnMaxTime.HasValue)
                {
                    _turnMaxTime = TimeSpan.FromSeconds(5.0);
                }
                return _turnMaxTime.Value;
            }
            set
            {
                _turnMaxTime = value;
            }
        }
        private TimeSpan? _turnMaxTime = null;

        public TimeSpan BattleTimeLimit
        {
            get
            {
                if (!_battleTimeLimit.HasValue)
                {
                    _battleTimeLimit = TimeSpan.FromMinutes(5.0);
                }
                return _battleTimeLimit.Value;
            }
            set
            {
                _battleTimeLimit = value;
            }
        }
        private TimeSpan? _battleTimeLimit = null;
    }
}
