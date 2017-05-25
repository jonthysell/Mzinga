// 
// MetricWeights.cs
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
using System.Linq;
using System.Xml;

namespace Mzinga.Core.AI
{
    public class MetricWeights
    {
        public double DrawScore { get; set; }

        private double[] _playerWeights;
        private double[] _bugTypeWeights;

        public MetricWeights()
        {
            _playerWeights = new double[NumPlayers * NumPlayerWeights];
            _bugTypeWeights = new double[NumPlayers * EnumUtils.NumBugTypes * NumBugTypeWeights];
        }

        public double Get(Player player, PlayerWeight playerWeight)
        {
            int key = GetKey(player, playerWeight);
            return _playerWeights[key];
        }

        public double Get(Player player, BugType bugType, BugTypeWeight bugTypeWeight)
        {
            int key = GetKey(player, bugType, bugTypeWeight);
            return _bugTypeWeights[key];
        }

        public void Set(Player player, PlayerWeight playerWeight, double value)
        {
            int key = GetKey(player, playerWeight);
            _playerWeights[key] = value;
        }

        public void Set(Player player, BugType bugType, BugTypeWeight bugTypeWeight, double value)
        {
            int key = GetKey(player, bugType, bugTypeWeight);
            _bugTypeWeights[key] = value;
        }

        public void CopyFrom(MetricWeights source)
        {
            if (null == source)
            {
                throw new ArgumentNullException("source");
            }

            DrawScore = source.DrawScore;

            IterateOverWeights((player, playerWeight) =>
            {
                double value = source.Get(player, playerWeight);
                Set(player, playerWeight, value);
            },
            (player, bugType, bugTypeWeight) =>
            {
                double value = source.Get(player, bugType, bugTypeWeight);
                Set(player, bugType, bugTypeWeight, value);
            });
        }

        public MetricWeights Clone()
        {
            MetricWeights clone = new MetricWeights();
            clone.CopyFrom(this);

            return clone;
        }

        public MetricWeights GetNormalized(double targetMaxValue = short.MaxValue, bool round = true)
        {
            if (targetMaxValue <= 0.0)
            {
                throw new ArgumentOutOfRangeException("targetMaxValue");
            }

            MetricWeights clone = Clone();

            // Apply player weights to appropriate bug weights
            for (int playerInt = 0; playerInt < NumPlayers; playerInt++)
            {
                Player player = (Player)playerInt;

                for (int weightInt = 0; weightInt < NumPlayerWeights; weightInt++)
                {
                    PlayerWeight playerWeight = (PlayerWeight)weightInt;

                    double playerWeightValue = clone.Get(player, playerWeight);

                    for (int bugTypeInt = 0; bugTypeInt < EnumUtils.NumBugTypes; bugTypeInt++)
                    {
                        BugType bugType = (BugType)bugTypeInt;

                        BugTypeWeight bugTypeWeight = (BugTypeWeight)weightInt;

                        double bugWeightValue = clone.Get(player, bugType, bugTypeWeight);

                        clone.Set(player, bugType, bugTypeWeight, playerWeightValue + bugWeightValue);
                    }

                    clone.Set(player, playerWeight, 0.0);
                }
            }

            // Copy bug weights into local array
            double[] dblWeights = new double[clone._bugTypeWeights.Length];
            Array.Copy(clone._bugTypeWeights, dblWeights, clone._bugTypeWeights.Length);

            double max = dblWeights.Max(d => Math.Abs(d));

            // Normalize to new range
            for (int i= 0; i < dblWeights.Length; i++)
            {
                double value = dblWeights[i];
                int sign = Math.Sign(value);
                double absValue = Math.Abs(value);

                dblWeights[i] = sign * (absValue / max) * targetMaxValue;
            }

            // Populate clone with normalized weights
            for (int i = 0; i < clone._bugTypeWeights.Length; i++)
            {
                clone._bugTypeWeights[i] = round ? Math.Round(dblWeights[i]) : dblWeights[i];
            }

            return clone;
        }

        public static MetricWeights ReadMetricWeightsXml(XmlReader xmlReader)
        {
            if (null == xmlReader)
            {
                throw new ArgumentNullException("xmlReader");
            }

            MetricWeights mw = new MetricWeights();

            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement() && xmlReader.Name != "MetricWeights")
                {
                    string key = xmlReader.Name;
                    double value = xmlReader.ReadElementContentAsDouble();

                    if (key == "DrawScore")
                    {
                        mw.DrawScore = value;
                    }
                    else
                    {
                        Player player;
                        PlayerWeight playerWeight;
                        BugType bugType;
                        BugTypeWeight bugTypeWeight;

                        if (TryParseKeyName(key, out player, out playerWeight))
                        {
                            mw.Set(player, playerWeight, value);
                        }
                        else if (TryParseKeyName(key, out player, out bugType, out bugTypeWeight))
                        {
                            mw.Set(player, bugType, bugTypeWeight, value);
                        }
                    }
                }
            }

            return mw;
        }

        public static bool TryParseKeyName(string key, out Player player, out PlayerWeight playerWeight)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                try
                {
                    string[] split = key.Split(KeySeperator[0]);

                    if (Enum.TryParse(split[0], out player))
                    {
                        if (Enum.TryParse(split[1], out playerWeight))
                        {
                            return true;
                        }
                    }
                }
                catch (Exception) { }
            }

            player = default(Player);
            playerWeight = default(PlayerWeight);
            return false;
        }

        public static bool TryParseKeyName(string key, out Player player, out BugType bugType, out BugTypeWeight bugTypeWeight)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                try
                {
                    string[] split = key.Split(KeySeperator[0]);

                    if (Enum.TryParse(split[0], out player))
                    {
                        if (Enum.TryParse(split[1], out bugType))
                        {
                            if (Enum.TryParse(split[2], out bugTypeWeight))
                            {
                                return true;
                            }
                        }
                    }
                }
                catch (Exception) { }
            }

            player = default(Player);
            bugType = default(BugType);
            bugTypeWeight = default(BugTypeWeight);
            return false;
        }

        public static string GetKeyName(Player player, PlayerWeight playerWeight)
        {
            return string.Join(KeySeperator, player.ToString(), playerWeight.ToString());
        }

        public static string GetKeyName(Player player, BugType bugType, BugTypeWeight bugTypeWeight)
        {
            return string.Join(KeySeperator, player.ToString(), bugType.ToString(), bugTypeWeight.ToString());
        }

        public static void IterateOverWeights(Action<Player, PlayerWeight> playerWeightAction, Action<Player, BugType, BugTypeWeight> bugTypeWeightAction)
        {
            for (int playerInt = 0; playerInt < NumPlayers; playerInt++)
            {
                Player player = (Player)playerInt;

                if (null != playerWeightAction)
                {
                    for (int playerWeightInt = 0; playerWeightInt < NumPlayerWeights; playerWeightInt++)
                    {
                        PlayerWeight playerWeight = (PlayerWeight)playerWeightInt;

                        playerWeightAction(player, playerWeight);
                    }
                }

                if (null != bugTypeWeightAction)
                {
                    for (int bugTypeInt = 0; bugTypeInt < EnumUtils.NumBugTypes; bugTypeInt++)
                    {
                        BugType bugType = (BugType)bugTypeInt;
                        for (int bugTypeWeightInt = 0; bugTypeWeightInt < NumBugTypeWeights; bugTypeWeightInt++)
                        {
                            BugTypeWeight bugTypeWeight = (BugTypeWeight)bugTypeWeightInt;

                            bugTypeWeightAction(player, bugType, bugTypeWeight);
                        }
                    }
                }
            }
        }

        private static int GetKey(Player player, PlayerWeight playerWeight)
        {
            return ((int)player * NumPlayerWeights) + (int)playerWeight;
        }

        private static int GetKey(Player player, BugType bugType, BugTypeWeight bugTypeWeight)
        {
            return ((int)player * EnumUtils.NumBugTypes * NumBugTypeWeights) + ((int)bugType * NumBugTypeWeights) + (int)bugTypeWeight;
        }

        private const string KeySeperator = ".";
        public const int NumPlayers = 2;
        public const int NumPlayerWeights = 6;
        public const int NumBugTypeWeights = 7;
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
        InHandWeight,
        InPlayWeight,
        IsPinnedWeight,
        NeighborWeight,
    }
}
