// 
// MetricWeights.cs
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
using System.Collections.Generic;

namespace Mzinga.Core.AI
{
    public class MetricWeights
    {
        public double DrawScore
        {
            get
            {
                return Get("DrawScore", 0.0);
            }
        }

        public double this[string key]
        {
            get
            {
                return Get(key);
            }
        }

        private Dictionary<string, double> _weights;

        private double?[] _playerWeights;
        private double?[] _bugTypeWeights;

        public MetricWeights()
        {
            _weights = new Dictionary<string, double>();

            _playerWeights = new double?[NumPlayers * NumPlayerWeights];
            _bugTypeWeights = new double?[NumPlayers * EnumUtils.NumBugTypes * NumBugTypeWeights];
        }

        public double Get(Player player, PlayerWeight playerWeight, double defaultValue = 0.0)
        {
            int key = GetKey(player, playerWeight);
            return _playerWeights[key].HasValue ? _playerWeights[key].Value : defaultValue;
        }

        public double Get(Player player, BugType bugType, BugTypeWeight bugTypeWeight, double defaultValue = 0.0)
        {
            int key = GetKey(player, bugType, bugTypeWeight);
            return _bugTypeWeights[key].HasValue ? _bugTypeWeights[key].Value : defaultValue;
        }

        public double Get(string key, double defaultValue = 0.0)
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }

            double value;
            if (TryGet(key, out value))
            {
                return value;
            }

            return defaultValue;
        }

        public bool TryGet(string key, out double result)
        {
            if (_weights.ContainsKey(key))
            {
                result = _weights[key];
                return true;
            }

            result = default(double);
            return false;
        }

        internal void Set(Player player, PlayerWeight playerWeight, double value)
        {
            int key = GetKey(player, playerWeight);
            _playerWeights[key] = value;
        }

        internal void Set(Player player, BugType bugType, BugTypeWeight bugTypeWeight, double value)
        {
            int key = GetKey(player, bugType, bugTypeWeight);
            _bugTypeWeights[key] = value;
        }

        internal void Set(string key, double value)
        {
            _weights[key] = value;
        }

        private int GetKey(Player player, PlayerWeight playerWeight)
        {
            return ((int)player * NumPlayerWeights) + (int)playerWeight;
        }

        private int GetKey(Player player, BugType bugType, BugTypeWeight bugTypeWeight)
        {
            return ((int)player * EnumUtils.NumBugTypes * NumBugTypeWeights) + ((int)bugType * NumBugTypeWeights) + (int)bugTypeWeight;
        }

        private const string KeySeperator = ".";
        private const int NumPlayers = 2;
        private const int NumPlayerWeights = 6;
        private const int NumBugTypeWeights = 7;
    }

    public enum Player
    {
        Maximizing = 0,
        Minimizing
    }

    public enum PlayerWeight
    {
        ValidMoveWeight = 0,
        ValidPlacementWeight,
        ValidMovementWeight,
        InHandWeight,
        InPlayWeight,
        IsPinnedWeight,
    }

    public enum BugTypeWeight
    {
        ValidMoveWeight = 0,
        ValidPlacementWeight,
        ValidMovementWeight,
        NeighborWeight,
        InHandWeight,
        InPlayWeight,
        IsPinnedWeight,
    }
}
