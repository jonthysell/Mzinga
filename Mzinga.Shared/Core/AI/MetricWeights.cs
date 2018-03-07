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
using System.Xml;

namespace Mzinga.Core.AI
{
    public class MetricWeights
    {
        private double[] _bugTypeWeights;

        public MetricWeights()
        {
            _bugTypeWeights = new double[EnumUtils.NumBugTypes * NumBugTypeWeights];
        }

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
            if (null == source)
            {
                throw new ArgumentNullException("source");
            }

            IterateOverWeights((bugType, bugTypeWeight) =>
            {
                double value = source.Get(bugType, bugTypeWeight);
                Set(bugType, bugTypeWeight, value);
            });
        }

        public MetricWeights Clone()
        {
            MetricWeights clone = new MetricWeights();
            clone.CopyFrom(this);

            return clone;
        }

        public MetricWeights GetNormalized(double targetMaxValue = 100.0, bool round = true, int decimals = 2)
        {
            if (targetMaxValue <= 0.0)
            {
                throw new ArgumentOutOfRangeException("targetMaxValue");
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

                    BugType bugType;
                    BugTypeWeight bugTypeWeight;

                    if (TryParseKeyName(key, out bugType, out bugTypeWeight))
                    {
                        mw.Set(bugType, bugTypeWeight, value + mw.Get(bugType, bugTypeWeight));
                    }
                }
            }

            return mw;
        }

        public static bool TryParseKeyName(string key, out BugType bugType, out BugTypeWeight bugTypeWeight)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                try
                {
                    string[] split = key.Split(KeySeperator[0]);

                    if (Enum.TryParse(split[split.Length - 1], out bugTypeWeight))
                    {
                        if (Enum.TryParse(split[split.Length - 2], out bugType))
                        {
                            return true;
                        }
                    }
                }
                catch (Exception) { }
            }

            bugType = default(BugType);
            bugTypeWeight = default(BugTypeWeight);
            return false;
        }

        public static string GetKeyName(BugType bugType, BugTypeWeight bugTypeWeight)
        {
            return string.Join(KeySeperator, bugType.ToString(), bugTypeWeight.ToString());
        }

        public static void IterateOverWeights(Action<BugType, BugTypeWeight> action)
        {
            if (null == action)
            {
                throw new ArgumentNullException("action");
            }

            for (int bugTypeInt = 0; bugTypeInt < EnumUtils.NumBugTypes; bugTypeInt++)
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
