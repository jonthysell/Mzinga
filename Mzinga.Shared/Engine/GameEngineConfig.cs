// 
// GameEngineConfig.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2018, 2019 Jon Thysell <http://jonthysell.com>
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
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

using Mzinga.Core;
using Mzinga.Core.AI;

namespace Mzinga.Engine
{
    public class GameEngineConfig
    {
        #region GameAI

        public int? TranspositionTableSizeMB { get; private set; } = null;

        public Dictionary<ExpansionPieces, MetricWeights[]> MetricWeightSet { get; private set; } = new Dictionary<ExpansionPieces, MetricWeights[]>();

        public Dictionary<ExpansionPieces, TranspositionTable> InitialTranspositionTables { get; private set; } = new Dictionary<ExpansionPieces, TranspositionTable>();

        public PonderDuringIdleType PonderDuringIdle { get; private set; } = PonderDuringIdleType.Disabled;

        public int MaxHelperThreads
        {
            get
            {
                // Hard min is 0, hard max is (Environment.ProcessorCount / 2) - 1
#if DEBUG
                return MinMaxHelperThreads;
#else
                return Math.Max(MinMaxHelperThreads, _maxHelperThreads.HasValue ? Math.Min(_maxHelperThreads.Value, MaxMaxHelperThreads) : MaxMaxHelperThreads);
#endif
            }
        }
        private int? _maxHelperThreads = null;

        public int? MaxBranchingFactor { get; private set; } = null;

        public bool ReportIntermediateBestMoves { get; private set; } = false;

        #endregion

        public GameEngineConfig(Stream inputStream)
        {
            LoadConfig(inputStream);
        }

        private void LoadConfig(Stream inputStream)
        {
            if (null == inputStream)
            {
                throw new ArgumentNullException("inputStream");
            }

            using (XmlReader reader = XmlReader.Create(inputStream))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "GameAI":
                                LoadGameAIConfig(reader.ReadSubtree());
                                break;
                        }
                    }
                }
            }
        }

        private void LoadGameAIConfig(XmlReader reader)
        {
            if (null == reader)
            {
                throw new ArgumentNullException("reader");
            }

            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    ExpansionPieces expansionPieces = EnumUtils.ParseExpansionPieces(reader["GameType"]);
                    
                    switch (reader.Name)
                    {
                        case "TranspositionTableSizeMB":
                            ParseTranspositionTableSizeMBValue(reader.ReadElementContentAsString());
                            break;
                        case "MetricWeights":
                            SetStartMetricWeights(expansionPieces, MetricWeights.ReadMetricWeightsXml(reader.ReadSubtree()));
                            SetEndMetricWeights(expansionPieces, MetricWeights.ReadMetricWeightsXml(reader.ReadSubtree()));
                            break;
                        case "StartMetricWeights":
                            SetStartMetricWeights(expansionPieces, MetricWeights.ReadMetricWeightsXml(reader.ReadSubtree()));
                            break;
                        case "EndMetricWeights":
                            SetEndMetricWeights(expansionPieces, MetricWeights.ReadMetricWeightsXml(reader.ReadSubtree()));
                            break;
                        case "InitialTranspositionTable":
                            SetInitialTranspositionTable(expansionPieces, TranspositionTable.ReadTranspositionTableXml(reader.ReadSubtree()));
                            break;
                        case "MaxHelperThreads":
                            ParseMaxHelperThreadsValue(reader.ReadElementContentAsString());
                            break;
                        case "PonderDuringIdle":
                            ParsePonderDuringIdleValue(reader.ReadElementContentAsString());
                            break;
                        case "MaxBranchingFactor":
                            ParseMaxBranchingFactorValue(reader.ReadElementContentAsString());
                            break;
                        case "ReportIntermediateBestMoves":
                            ParseReportIntermediateBestMovesValue(reader.ReadElementContentAsString());
                            break;
                    }
                }
            }
        }

        public void ParseTranspositionTableSizeMBValue(string rawValue)
        {
            if (int.TryParse(rawValue, out int intValue))
            {
                TranspositionTableSizeMB = Math.Max(MinTranspositionTableSizeMB, Math.Min(intValue, MaxTranspositionTableSizeMB));
            }
        }

        public void GetTranspositionTableSizeMBValue(out string type, out string value, out string values)
        {
            type = "int";
            value = (TranspositionTableSizeMB.HasValue ? TranspositionTableSizeMB.Value : TranspositionTable.DefaultSizeInBytes / (1024 * 1024)).ToString();
            values = string.Format("{0};{1}", MinTranspositionTableSizeMB, MaxTranspositionTableSizeMB);
        }

        public void ParseMaxHelperThreadsValue(string rawValue)
        {

            if (int.TryParse(rawValue, out int intValue))
            {
                _maxHelperThreads = Math.Max(MinMaxHelperThreads, Math.Min(intValue, MaxMaxHelperThreads));
            }
            else if (Enum.TryParse(rawValue, out MaxHelperThreadsType enumValue))
            {
                switch (enumValue)
                {
                    case MaxHelperThreadsType.None:
                        _maxHelperThreads = 0;
                        break;
                    case MaxHelperThreadsType.Auto:
                        _maxHelperThreads = null;
                        break;
                }
            }
        }

        public void GetMaxHelperThreadsValue(out string type, out string value, out string values)
        {
            type = "enum";
            
            if (!_maxHelperThreads.HasValue)
            {
                value = "Auto";
            }
            else if (_maxHelperThreads.Value == 0)
            {
                value = "None";
            }
            else
            {
                value = _maxHelperThreads.Value.ToString();
            }

            values = "Auto;None";

            for (int i = 1; i <= MaxMaxHelperThreads; i++)
            {
                values += ";" + i.ToString();
            }
        }

        public void ParsePonderDuringIdleValue(string rawValue)
        {
            if (Enum.TryParse(rawValue, out PonderDuringIdleType enumValue))
            {
                PonderDuringIdle = enumValue;
            }
        }

        public void GetPonderDuringIdleValue(out string type, out string value, out string values)
        {
            type = "enum";
            value = PonderDuringIdle.ToString();
            values = "Disabled;SingleThreaded;MultiThreaded";
        }

        public void ParseMaxBranchingFactorValue(string rawValue)
        {
            if (int.TryParse(rawValue, out int intValue))
            {
                MaxBranchingFactor = Math.Max(MinMaxBranchingFactor, Math.Min(intValue, GameAI.MaxMaxBranchingFactor));
            }
        }

        public void GetMaxBranchingFactorValue(out string type, out string value, out string values)
        {
            type = "int";
            value = (MaxBranchingFactor.HasValue ? MaxBranchingFactor.Value : GameAI.MaxMaxBranchingFactor).ToString();
            values = string.Format("{0};{1}", MinMaxBranchingFactor, GameAI.MaxMaxBranchingFactor);
        }

        public void ParseReportIntermediateBestMovesValue(string rawValue)
        {
            if (bool.TryParse(rawValue, out bool boolValue))
            {
                ReportIntermediateBestMoves = boolValue;
            }
        }

        public void GetReportIntermediateBestMovesValue(out string type, out string value, out string values)
        {
            type = "bool";
            value = ReportIntermediateBestMoves.ToString();
            values = "";
        }

        public GameAI GetGameAI(ExpansionPieces expansionPieces)
        {
            MetricWeights[] mw = GetMetricWeights(expansionPieces);

            return new GameAI(new GameAIConfig()
            {
                StartMetricWeights = mw[0],
                EndMetricWeights = mw[0] ?? mw[1],
                MaxBranchingFactor = MaxBranchingFactor,
                TranspositionTableSizeMB = TranspositionTableSizeMB,
                InitialTranspositionTable = GetInitialTranspositionTable(expansionPieces),
            });
        }

        public static GameEngineConfig GetDefaultEngineConfig()
        {
            foreach (string resourceName in Assembly.GetAssembly(typeof(GameEngineConfig)).GetManifestResourceNames())
            {
                if (resourceName.EndsWith("DefaultEngineConfig.xml"))
                {
                    using (Stream inputStream = Assembly.GetAssembly(typeof(GameEngineConfig)).GetManifestResourceStream(resourceName))
                    {
                        return new GameEngineConfig(inputStream);
                    }
                }
            }

            return null;
        }

        private void SetStartMetricWeights(ExpansionPieces expansionPieces, MetricWeights metricWeights)
        {
            if (!MetricWeightSet.ContainsKey(expansionPieces))
            {
                MetricWeightSet.Add(expansionPieces, new MetricWeights[2]);
            }

            MetricWeightSet[expansionPieces][0] = metricWeights;
        }

        private void SetEndMetricWeights(ExpansionPieces expansionPieces, MetricWeights metricWeights)
        {
            if (!MetricWeightSet.ContainsKey(expansionPieces))
            {
                MetricWeightSet.Add(expansionPieces, new MetricWeights[2]);
            }

            MetricWeightSet[expansionPieces][1] = metricWeights;
        }

        public MetricWeights[] GetMetricWeights(ExpansionPieces expansionPieces)
        {
            MetricWeights[] result;

            // Start with the weights for the base game type
            if (!MetricWeightSet.TryGetValue(ExpansionPieces.None, out result))
            {
                // No base game type, start with nulls
                result = new MetricWeights[2];
            }

            if (expansionPieces != ExpansionPieces.None)
            {
                // Try to get weights specific to this game type
                if (MetricWeightSet.TryGetValue(expansionPieces, out MetricWeights[] mw))
                {
                    result[0] = mw[0] ?? result[0];
                    result[1] = mw[1] ?? result[1];
                }
            }

            return result;
        }

        private void SetInitialTranspositionTable(ExpansionPieces expansionPieces, TranspositionTable transpositionTable)
        {
            InitialTranspositionTables[expansionPieces] = transpositionTable;
        }

        public TranspositionTable GetInitialTranspositionTable(ExpansionPieces expansionPieces)
        {
            TranspositionTable result;
            if (!InitialTranspositionTables.TryGetValue(expansionPieces, out result))
            {
                result = null;
            }

            return result;
        }

        private const int MinTranspositionTableSizeMB = 1;
        private const int MaxTranspositionTableSizeMB = 1024;

        private const int MinMaxHelperThreads = 0;
        private static int MaxMaxHelperThreads { get { return (Environment.ProcessorCount / 2) - 1; } }

        private const int MinMaxBranchingFactor = 1;
    }

    public enum MaxHelperThreadsType
    {
        None,
        Auto,
    }

    public enum PonderDuringIdleType
    {
        Disabled,
        SingleThreaded,
        MultiThreaded
    }
}