// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Xml;

namespace Mzinga.Core.AI
{
    public class MetricWeights
    {
        private readonly double[] _bugTypeWeights = new double[(int)BugType.NumBugTypes * NumBugTypeWeights];

        public MetricWeights() { }

        public double Get(BugType bugType, BugTypeWeight bugTypeWeight)
        {
            int key = GetKey(bugType, bugTypeWeight);
            return _bugTypeWeights[key];
        }

        public void Set(BugType bugType, BugTypeWeight bugTypeWeight, double value)
        {
            int key = GetKey(bugType, bugTypeWeight);
            _bugTypeWeights[key] = value;
        }

        public void CopyFrom(MetricWeights source)
        {
            Array.Copy(source._bugTypeWeights, _bugTypeWeights, source._bugTypeWeights.Length);
        }

        public MetricWeights Clone()
        {
            MetricWeights clone = new MetricWeights();
            clone.CopyFrom(this);

            return clone;
        }

        public MetricWeights GetNormalized(double targetMaxValue = 100.0, bool round = true, int decimals = 6)
        {
            if (targetMaxValue <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(targetMaxValue));
            }

            MetricWeights clone = Clone();

            // Copy bug weights into local array
            double[] dblWeights = new double[clone._bugTypeWeights.Length];
            Array.Copy(clone._bugTypeWeights, dblWeights, clone._bugTypeWeights.Length);

            double max = double.MinValue;
            foreach (double weight in dblWeights)
            {
                max = Math.Max(max, Math.Abs(weight));
            }

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
                clone._bugTypeWeights[i] = round ? Math.Round(dblWeights[i], decimals) : dblWeights[i];
            }

            return clone;
        }

        public void Add(MetricWeights a)
        {
            for (int i = 0; i < _bugTypeWeights.Length; i++)
            {
                _bugTypeWeights[i] += a._bugTypeWeights[i];
            }
        }

        public void Scale(double factor)
        {
            for (int i = 0; i < _bugTypeWeights.Length; i++)
            {
                _bugTypeWeights[i] *= factor;
            }
        }

        public static MetricWeights ReadMetricWeightsXml(XmlReader xmlReader)
        {
            MetricWeights mw = new MetricWeights();

            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement() && !xmlReader.Name.EndsWith("MetricWeights"))
                {
                    string key = xmlReader.Name;
                    double value = xmlReader.ReadElementContentAsDouble();

                    if (TryParseKeyName(key, out BugType bugType, out BugTypeWeight bugTypeWeight))
                    {
                        mw.Set(bugType, bugTypeWeight, value + mw.Get(bugType, bugTypeWeight));
                    }
                }
            }

            return mw;
        }

        public void WriteMetricWeightsXml(XmlWriter xmlWriter, string name = "MetricWeights", GameType? gameType = null)
        {
            xmlWriter.WriteStartElement(name);

            if (gameType.HasValue)
            {
                xmlWriter.WriteAttributeString("GameType", Enums.GetGameTypeString(gameType.Value));
            }

            IterateOverWeights((bugType, bugTypeWeight) =>
            {
                if (!gameType.HasValue || Enums.BugTypeIsEnabledForGameType(bugType, gameType.Value))
                {
                    string key = GetKeyName(bugType, bugTypeWeight);
                    double value = Get(bugType, bugTypeWeight);

                    if (value != 0.0)
                    {
                        xmlWriter.WriteStartElement(key);
                        xmlWriter.WriteValue(value);
                        xmlWriter.WriteEndElement();
                    }
                }
            });

            xmlWriter.WriteEndElement();
        }

        public static bool TryParseKeyName(string key, out BugType bugType, out BugTypeWeight bugTypeWeight)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                try
                {
                    string[] split = key.Split(KeySeperator[0]);

                    if (Enum.TryParse(split[^1], out bugTypeWeight))
                    {
                        if (Enum.TryParse(split[^2], out bugType))
                        {
                            return true;
                        }
                    }
                }
                catch (Exception) { }
            }

            bugType = default;
            bugTypeWeight = default;
            return false;
        }

        public static string GetKeyName(BugType bugType, BugTypeWeight bugTypeWeight)
        {
            return string.Join(KeySeperator, bugType.ToString(), bugTypeWeight.ToString());
        }

        public static void IterateOverWeights(Action<BugType, BugTypeWeight> action)
        {
            for (int bugTypeInt = 0; bugTypeInt < (int)BugType.NumBugTypes; bugTypeInt++)
            {
                BugType bugType = (BugType)bugTypeInt;
                for (int bugTypeWeightInt = 0; bugTypeWeightInt < NumBugTypeWeights; bugTypeWeightInt++)
                {
                    BugTypeWeight bugTypeWeight = (BugTypeWeight)bugTypeWeightInt;

                    action(bugType, bugTypeWeight);
                }
            }
        }

        private static int GetKey(BugType bugType, BugTypeWeight bugTypeWeight)
        {
            return ((int)bugType * NumBugTypeWeights) + (int)bugTypeWeight;
        }

        private const string KeySeperator = ".";
        public const int NumBugTypeWeights = 7;
    }

    public enum BugTypeWeight
    {
        InPlayWeight = 0,
        IsPinnedWeight,
        IsCoveredWeight,
        NoisyMoveWeight,
        QuietMoveWeight,
        FriendlyNeighborWeight,
        EnemyNeighborWeight,
    }
}
