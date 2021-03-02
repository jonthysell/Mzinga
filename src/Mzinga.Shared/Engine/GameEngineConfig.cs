// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

using Mzinga.Core;
using Mzinga.Core.AI;

namespace Mzinga.Engine
{
    public class GameEngineConfig
    {
        #region GameAI

        public int? TranspositionTableSizeMB { get; private set; } = null;

        public int MaxHelperThreads
        {
            get
            {
                // Hard min is 0, hard max is (Environment.ProcessorCount / 2) - 1
                return Math.Max(MinMaxHelperThreads, _maxHelperThreads.HasValue ? Math.Min(_maxHelperThreads.Value, MaxMaxHelperThreads) : MaxMaxHelperThreads);
            }
        }
        private int? _maxHelperThreads = null;

        public PonderDuringIdleType PonderDuringIdle { get; private set; } = PonderDuringIdleType.Disabled;

        public int? MaxBranchingFactor { get; private set; } = null;

        public bool ReportIntermediateBestMoves { get; private set; } = false;

        public Dictionary<ExpansionPieces, MetricWeights[]> MetricWeightSet { get; private set; } = new Dictionary<ExpansionPieces, MetricWeights[]>();

        public Dictionary<ExpansionPieces, TranspositionTable> InitialTranspositionTables { get; private set; } = new Dictionary<ExpansionPieces, TranspositionTable>();

        #endregion

        public GameEngineConfig()
        {

        }

        public GameEngineConfig(Stream inputStream) : this()
        {
            LoadConfig(inputStream);
        }

        #region Load

        public void LoadConfig(Stream inputStream)
        {
            if (null == inputStream)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

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
            if (null == reader)
            {
                throw new ArgumentNullException(nameof(reader));
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
                    }
                }
            }
        }

        #endregion

        #region Save

        public void SaveConfig(Stream outputStream, string rootName, ConfigSaveType configSaveType)
        {
            if (null == outputStream)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }

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

            writer.WriteAttributeString("version", GetVersion());
            writer.WriteAttributeString("date", DateTime.UtcNow.ToString());

            SaveGameAIConfig(writer, "GameAI", configSaveType);

            writer.WriteEndElement();
        }

        public void SaveGameAIConfig(XmlWriter writer, string rootName, ConfigSaveType configSaveType)
        {
            if (null == writer)
            {
                throw new ArgumentNullException(nameof(writer));
            }

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

                writer.WriteElementString("ReportIntermediateBestMoves", ReportIntermediateBestMoves.ToString());
            }

            if (configSaveType.HasFlag(ConfigSaveType.MetricWeights))
            {
                foreach (KeyValuePair<ExpansionPieces, MetricWeights[]> kvp in MetricWeightSet)
                {
                    ExpansionPieces gameType = kvp.Key;
                    MetricWeights[] mw = kvp.Value;

                    if (null != mw[0] && null != mw[1])
                    {
                        mw[0].WriteMetricWeightsXml(writer, "StartMetricWeights", gameType);
                        mw[1].WriteMetricWeightsXml(writer, "EndMetricWeights", gameType);
                    }
                    else if (null != mw[0] && null == mw[1])
                    {
                        mw[0].WriteMetricWeightsXml(writer, gameType: gameType);
                    }
                }
            }

            if (configSaveType.HasFlag(ConfigSaveType.InitialTranspositionTable))
            {
                foreach (KeyValuePair<ExpansionPieces, TranspositionTable> kvp in InitialTranspositionTables)
                {
                    ExpansionPieces gameType = kvp.Key;
                    TranspositionTable tt = kvp.Value;

                    tt?.WriteTranspositionTableXml(writer, "InitialTranspositionTable", gameType);
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
                TranspositionTableSizeMB = Math.Max(MinTranspositionTableSizeMB, Math.Min(intValue, MaxTranspositionTableSizeMB));
            }
        }

        public void GetTranspositionTableSizeMBValue(out string type, out string value, out string values)
        {
            type = "int";
            value = (TranspositionTableSizeMB ?? TranspositionTable.DefaultSizeInBytes / (1024 * 1024)).ToString();
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
            value = (MaxBranchingFactor ?? GameAI.MaxMaxBranchingFactor).ToString();
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

            // Start with the weights for the base game type
            if (!MetricWeightSet.TryGetValue(ExpansionPieces.None, out MetricWeights[] result))
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
            if (!InitialTranspositionTables.TryGetValue(expansionPieces, out TranspositionTable result))
            {
                result = null;
            }

            return result;
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
                    using Stream inputStream = Assembly.GetAssembly(typeof(GameEngineConfig)).GetManifestResourceStream(resourceName);
                    return new GameEngineConfig(inputStream);
                }
            }

            return null;
        }

        public GameEngineConfig GetOptionsClone()
        {
            GameEngineConfig clone = new GameEngineConfig()
            {
                TranspositionTableSizeMB = TranspositionTableSizeMB,
                _maxHelperThreads = _maxHelperThreads,
                PonderDuringIdle = PonderDuringIdle,
                MaxBranchingFactor = MaxBranchingFactor,
                ReportIntermediateBestMoves = ReportIntermediateBestMoves
            };

            return clone;
        }

        public void CopyOptionsFrom(GameEngineConfig other)
        {
            TranspositionTableSizeMB = other.TranspositionTableSizeMB;
            _maxHelperThreads = other._maxHelperThreads;
            PonderDuringIdle = other.PonderDuringIdle;
            MaxBranchingFactor = other.MaxBranchingFactor;
            ReportIntermediateBestMoves = other.ReportIntermediateBestMoves;
        }

        private static string GetVersion()
        {
            return Assembly.GetAssembly(typeof(GameEngineConfig)).GetName().Version.ToString();
        }

        #endregion

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

    [Flags]
    public enum ConfigSaveType
    {
        None                      = 0x0,
        BasicOptions              = 0x1,
        MetricWeights             = 0x2,
        InitialTranspositionTable = 0x4,
    }
}
