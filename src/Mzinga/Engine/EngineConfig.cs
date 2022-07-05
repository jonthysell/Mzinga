// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Mzinga.Core;
using Mzinga.Core.AI;

namespace Mzinga.Engine
{
    public class EngineConfig
    {
        #region Options

        public const int MinMaxHelperThreads = 0;
        public static int DefaultMaxHelperThreads => (Environment.ProcessorCount / 2) - 1;
        public static int MaxMaxHelperThreads => Environment.ProcessorCount - 1;

        public int MaxHelperThreads
        {
            get
            {
                // Hard min is 0, hard max is (Environment.ProcessorCount / 2) - 1
                return Math.Max(MinMaxHelperThreads, _maxHelperThreads.HasValue ? Math.Min(_maxHelperThreads.Value, MaxMaxHelperThreads) : DefaultMaxHelperThreads);
            }
        }
        private int? _maxHelperThreads = null;

        public const PonderDuringIdleType DefaultPonderDuringIdle = PonderDuringIdleType.Disabled;

        public PonderDuringIdleType PonderDuringIdle { get; private set; } = DefaultPonderDuringIdle;

        public const bool DefaultReportIntermediateBestMoves = false;

        public bool ReportIntermediateBestMoves { get; private set; } = DefaultReportIntermediateBestMoves;

        #endregion

        #region GameAIConfig Options

        public int? MaxBranchingFactor { get; private set; } = null;

        public int? QuiescentSearchMaxDepth { get; private set; } = null;

        public int? PrincipalVariationMaxDepth { get; private set; } = null;

        public int? TranspositionTableSizeMB { get; private set; } = null;

        public Dictionary<GameType, MetricWeights[]> MetricWeightSet { get; private set; } = new Dictionary<GameType, MetricWeights[]>();

        #endregion

        public EngineConfig() { }

        public EngineConfig(Stream inputStream) : this()
        {
            LoadConfig(inputStream);
        }

        #region Load

        public void LoadConfig(Stream inputStream)
        {
            using XmlReader reader = XmlReader.Create(inputStream);
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

        public void LoadGameAIConfig(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    _ = Enums.TryParse(reader["GameType"] ?? "", out GameType gameType);
                    
                    switch (reader.Name)
                    {
                        case "TranspositionTableSizeMB":
                            ParseTranspositionTableSizeMBValue(reader.ReadElementContentAsString());
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
                        case "QuiescentSearchMaxDepth":
                            ParseQuiescentSearchMaxDepthValue(reader.ReadElementContentAsString());
                            break;
                        case "PrincipalVariationMaxDepth":
                            ParsePrincipalVariationMaxDepthValue(reader.ReadElementContentAsString());
                            break;
                        case "ReportIntermediateBestMoves":
                            ParseReportIntermediateBestMovesValue(reader.ReadElementContentAsString());
                            break;
                        case "MetricWeights":
                            SetStartMetricWeights(gameType, MetricWeights.ReadMetricWeightsXml(reader.ReadSubtree()));
                            SetEndMetricWeights(gameType, MetricWeights.ReadMetricWeightsXml(reader.ReadSubtree()));
                            break;
                        case "StartMetricWeights":
                            SetStartMetricWeights(gameType, MetricWeights.ReadMetricWeightsXml(reader.ReadSubtree()));
                            break;
                        case "EndMetricWeights":
                            SetEndMetricWeights(gameType, MetricWeights.ReadMetricWeightsXml(reader.ReadSubtree()));
                            break;
                    }
                }
            }
        }

        #endregion

        #region Save

        public void SaveConfig(Stream outputStream, string rootName, ConfigSaveType configSaveType)
        {
            if (string.IsNullOrWhiteSpace(rootName))
            {
                throw new ArgumentNullException(nameof(rootName));
            }

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true
            };

            using XmlWriter writer = XmlWriter.Create(outputStream, settings);
            writer.WriteStartElement(rootName);

            writer.WriteAttributeString("version", AppInfo.Version);
            writer.WriteAttributeString("date", DateTime.UtcNow.ToString());

            SaveGameAIConfig(writer, "GameAI", configSaveType);

            writer.WriteEndElement();
        }

        public void SaveGameAIConfig(XmlWriter writer, string rootName, ConfigSaveType configSaveType)
        {
            if (string.IsNullOrWhiteSpace(rootName))
            {
                throw new ArgumentNullException(nameof(rootName));
            }

            writer.WriteStartElement(rootName);

            if (configSaveType.HasFlag(ConfigSaveType.BasicOptions))
            {
                if (TranspositionTableSizeMB.HasValue)
                {
                    writer.WriteElementString("TranspositionTableSizeMB", TranspositionTableSizeMB.Value.ToString());
                }

                if (!_maxHelperThreads.HasValue)
                {
                    writer.WriteElementString("MaxHelperThreads", "Auto");
                }
                else if (_maxHelperThreads.Value == 0)
                {
                    writer.WriteElementString("MaxHelperThreads", "None");
                }
                else
                {
                    writer.WriteElementString("MaxHelperThreads", _maxHelperThreads.Value.ToString());
                }

                writer.WriteElementString("PonderDuringIdle", PonderDuringIdle.ToString());

                if (MaxBranchingFactor.HasValue)
                {
                    writer.WriteElementString("MaxBranchingFactor", MaxBranchingFactor.Value.ToString());
                }

                if (QuiescentSearchMaxDepth.HasValue)
                {
                    writer.WriteElementString("QuiescentSearchMaxDepth", QuiescentSearchMaxDepth.Value.ToString());
                }

                if (PrincipalVariationMaxDepth.HasValue)
                {
                    writer.WriteElementString("PrincipalVariationMaxDepth", PrincipalVariationMaxDepth.Value.ToString());
                }

                writer.WriteElementString("ReportIntermediateBestMoves", ReportIntermediateBestMoves.ToString());
            }

            if (configSaveType.HasFlag(ConfigSaveType.MetricWeights))
            {
                foreach (KeyValuePair<GameType, MetricWeights[]> kvp in MetricWeightSet)
                {
                    GameType gameType = kvp.Key;
                    MetricWeights[] mw = kvp.Value;

                    if (mw[0] is not null && mw[1] is not null)
                    {
                        mw[0].WriteMetricWeightsXml(writer, "StartMetricWeights", gameType);
                        mw[1].WriteMetricWeightsXml(writer, "EndMetricWeights", gameType);
                    }
                    else if (mw[0] is not null && mw[1] is null)
                    {
                        mw[0].WriteMetricWeightsXml(writer, gameType: gameType);
                    }
                }
            }

            writer.WriteEndElement();
        }

        #endregion

        #region Engine Helpers

        public void ParseTranspositionTableSizeMBValue(string rawValue)
        {
            if (int.TryParse(rawValue, out int intValue))
            {
                TranspositionTableSizeMB = Math.Max(GameAIConfig.MinTranspositionTableSizeMB, Math.Min(intValue, GameAIConfig.MaxTranspositionTableSizeMB));
            }
        }

        public void GetTranspositionTableSizeMBValue(out string type, out string value, out string values)
        {
            type = "int";
            value = (TranspositionTableSizeMB ?? GameAIConfig.DefaultTranspositionTableSizeMB).ToString();
            values = string.Format("{0};{1}", GameAIConfig.MinTranspositionTableSizeMB, GameAIConfig.MaxTranspositionTableSizeMB);
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
                MaxBranchingFactor = Math.Max(GameAIConfig.MinMaxBranchingFactor, Math.Min(intValue, GameAIConfig.MaxMaxBranchingFactor));
            }
        }

        public void GetMaxBranchingFactorValue(out string type, out string value, out string values)
        {
            type = "int";
            value = (MaxBranchingFactor ?? GameAIConfig.DefaultMaxBranchingFactor).ToString();
            values = string.Format("{0};{1}", GameAIConfig.MinMaxBranchingFactor, GameAIConfig.MaxMaxBranchingFactor);
        }

        public void ParseQuiescentSearchMaxDepthValue(string rawValue)
        {
            if (int.TryParse(rawValue, out int intValue))
            {
                QuiescentSearchMaxDepth = Math.Max(GameAIConfig.MinQuiescentSearchMaxDepth, Math.Min(intValue, GameAIConfig.MaxQuiescentSearchMaxDepth));
            }
        }

        public void GetQuiescentSearchMaxDepthValue(out string type, out string value, out string values)
        {
            type = "int";
            value = (QuiescentSearchMaxDepth ?? GameAIConfig.DefaultQuiescentSearchMaxDepth).ToString();
            values = string.Format("{0};{1}", GameAIConfig.MinQuiescentSearchMaxDepth, GameAIConfig.MaxQuiescentSearchMaxDepth);
        }

        public void ParsePrincipalVariationMaxDepthValue(string rawValue)
        {
            if (int.TryParse(rawValue, out int intValue))
            {
                PrincipalVariationMaxDepth = Math.Max(GameAIConfig.MinPrincipalVariationMaxDepth, Math.Min(intValue, GameAIConfig.MaxPrincipalVariationMaxDepth));
            }
        }

        public void GetPrincipalVariationMaxDepthValue(out string type, out string value, out string values)
        {
            type = "int";
            value = (PrincipalVariationMaxDepth ?? GameAIConfig.DefaultQuiescentSearchMaxDepth).ToString();
            values = string.Format("{0};{1}", GameAIConfig.MinPrincipalVariationMaxDepth, GameAIConfig.MaxPrincipalVariationMaxDepth);
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

        private void SetStartMetricWeights(GameType gameType, MetricWeights metricWeights)
        {
            if (!MetricWeightSet.ContainsKey(gameType))
            {
                MetricWeightSet.Add(gameType, new MetricWeights[2]);
            }

            MetricWeightSet[gameType][0] = metricWeights;
        }

        private void SetEndMetricWeights(GameType gameType, MetricWeights metricWeights)
        {
            if (!MetricWeightSet.ContainsKey(gameType))
            {
                MetricWeightSet.Add(gameType, new MetricWeights[2]);
            }

            MetricWeightSet[gameType][1] = metricWeights;
        }

        public MetricWeights[] GetMetricWeights(GameType expansionPieces)
        {

            // Start with the weights for the base game type
            if (!MetricWeightSet.TryGetValue(GameType.Base, out MetricWeights[]? result))
            {
                // No base game type, start with nulls
                result = new MetricWeights[2];
            }

            if (expansionPieces != GameType.Base)
            {
                // Try to get weights specific to this game type
                if (MetricWeightSet.TryGetValue(expansionPieces, out MetricWeights[]? mw))
                {
                    result[0] = mw[0] ?? result[0];
                    result[1] = mw[1] ?? result[1];
                }
            }

            return result;
        }

        public GameAI GetGameAI(GameType gameType)
        {
            MetricWeights[] mw = GetMetricWeights(gameType);

            return new GameAI(new GameAIConfig()
            {
                StartMetricWeights = mw[0],
                EndMetricWeights = mw[0] ?? mw[1],
                MaxBranchingFactor = MaxBranchingFactor,
                QuiescentSearchMaxDepth = QuiescentSearchMaxDepth,
                PrincipalVariationMaxDepth = PrincipalVariationMaxDepth,
                TranspositionTableSizeMB = TranspositionTableSizeMB
            });
        }

        public static EngineConfig GetDefaultEngineConfig()
        {
            using Stream inputStream = AssemblyUtils.GetEmbeddedResource<EngineConfig>("DefaultEngineConfig.xml");
            return new EngineConfig(inputStream);
        }

        public EngineConfig GetOptionsClone()
        {
            EngineConfig clone = new EngineConfig()
            {
                TranspositionTableSizeMB = TranspositionTableSizeMB,
                _maxHelperThreads = _maxHelperThreads,
                PonderDuringIdle = PonderDuringIdle,
                MaxBranchingFactor = MaxBranchingFactor,
                QuiescentSearchMaxDepth = QuiescentSearchMaxDepth,
                PrincipalVariationMaxDepth = PrincipalVariationMaxDepth,
                ReportIntermediateBestMoves = ReportIntermediateBestMoves
            };

            return clone;
        }

        public void CopyOptionsFrom(EngineConfig other)
        {
            TranspositionTableSizeMB = other.TranspositionTableSizeMB;
            _maxHelperThreads = other._maxHelperThreads;
            PonderDuringIdle = other.PonderDuringIdle;
            MaxBranchingFactor = other.MaxBranchingFactor;
            QuiescentSearchMaxDepth = other.QuiescentSearchMaxDepth;
            PrincipalVariationMaxDepth = other.PrincipalVariationMaxDepth;
            ReportIntermediateBestMoves = other.ReportIntermediateBestMoves;
        }

        #endregion
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

    [Flags]
    public enum ConfigSaveType
    {
        None                      = 0x0,
        BasicOptions              = 0x1,
        MetricWeights             = 0x2,
    }
}
