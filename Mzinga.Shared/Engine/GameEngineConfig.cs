// 
// GameEngineConfig.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2018 Jon Thysell <http://jonthysell.com>
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
            });
        }

        public static GameEngineConfig GetDefaultConfig()
        {
            byte[] rawData = Encoding.UTF8.GetBytes(DefaultConfig);

            using (MemoryStream ms = new MemoryStream(rawData))
            {
                return new GameEngineConfig(ms);
            }
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

        private MetricWeights[] GetMetricWeights(ExpansionPieces expansionPieces)
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

        private const int MinTranspositionTableSizeMB = 1;
        private const int MaxTranspositionTableSizeMB = 1024;

        private const int MinMaxHelperThreads = 0;
        private static int MaxMaxHelperThreads { get { return (Environment.ProcessorCount / 2) - 1; } }

        private const int MinMaxBranchingFactor = 1;

        private const string DefaultConfig = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Mzinga.Engine>
<GameAI>
<TranspositionTableSizeMB>32</TranspositionTableSizeMB>
<MaxHelperThreads>Auto</MaxHelperThreads>
<PonderDuringIdle>SingleThreaded</PonderDuringIdle>
<ReportIntermediateBestMoves>False</ReportIntermediateBestMoves>
<StartMetricWeights GameType=""Base"">
<QueenBee.InPlayWeight>-1040.9353892252457</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>4021.6417508785448</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-53352.839403082871</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-1006.7272047212866</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>11800.108434613907</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-41700.7303325567</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-141835.30216265252</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-10118.679051063802</Spider.InPlayWeight>
<Spider.IsPinnedWeight>2339.1224920095765</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>5653.6558053645331</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>100732.11356795469</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>14309.943401192075</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>7403.0720448182492</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-10382.133869046265</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-251.43486944957567</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>7564.6858756226684</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>1390.8874453747021</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>1219.8842331844455</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-289.94350671627774</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-11088.888806558145</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-1554.4332482167379</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>3201.9045297964976</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-43904.333395824695</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>8781.7770735543345</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>7500.1157113377167</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>5872.8036810668391</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-9714.1093356762231</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-131.41816267617506</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>730.9985239809713</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>32840.014075596962</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>44.799199349557462</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>796.19420619766652</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>5163.3332841362162</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-21776.343656798828</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-79791.842095420245</SoldierAnt.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base"">
<QueenBee.InPlayWeight>-529.76741343319054</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>2965.0213529770313</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-23459.408791836053</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-2029.1133047180397</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>49018.30246334141</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-259745.67753006346</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-816684.78861036</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-29164.713136887432</Spider.InPlayWeight>
<Spider.IsPinnedWeight>9538.5676212137187</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>5177.9803415505321</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>18481.973546921763</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>6442.8022627688151</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>51176.535906938559</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-5523.4586424794788</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-164.95898633738918</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>30102.75857001957</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>6137.3748554773429</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>487.92268350916146</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-394.75433306237272</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-1925.3255690345316</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-18144.080098628459</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>2478.277740895137</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-3623.52205230407</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>24246.481852295346</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>15707.651877241542</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>7490.30825537788</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-12.6850525721307</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-1640.8162883705904</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>3424.246757915962</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>106.58572000345195</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>89.27723748064318</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>698.88099892238517</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>2837.2571083243297</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-29068.593302453926</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-90580.816268408191</SoldierAnt.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+M"">
<QueenBee.InPlayWeight>73744.134488834243</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-29196.233686249612</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-29620.073215634777</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-8534.6027120316357</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>-102354.66397299136</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-192564.40770450581</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-430579.19546651532</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-58800.098598990648</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-42025.47920783623</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>21363.592841427104</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>87465.672316329728</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>31816.994246760481</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-48419.667005231</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-48595.523917015977</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-117364.4401271847</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>43797.6464186607</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>3014.4072331737511</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>6365.8157118371246</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-59163.698853955233</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-128923.81005336253</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-15260.937264095142</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>-48777.448951033759</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-36872.121296408513</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>3009.4217415519561</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>81224.0034595018</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>28972.279764247611</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-49649.84978282162</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-62047.510471730078</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>115811.5605736838</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-36235.667362656321</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>149.35004099327256</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>9940.42396915917</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>882.65314962830519</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>42531.509909523149</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-151024.074809899</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>51521.140554352773</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>11476.262365602346</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-4947.44358761967</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>44271.4473294817</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>-6455.161891675697</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>131736.01773864884</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>29274.2186252158</Mosquito.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+M"">
<QueenBee.InPlayWeight>18115.539052860153</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-66199.6173320722</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-23111.444041530871</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>13210.685432126858</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>131203.16439605763</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-159170.81740827145</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-597126.25346344663</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-11525.684294496963</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-30223.589052574305</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>2513.1708431003594</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>90299.248428526247</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>-9864.8756064758836</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-26212.354385318748</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-54031.723786808543</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-42166.391617371592</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>-4926.1246400062328</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>-2254.4435454592972</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>77951.0933679477</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>16447.42172618274</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-23929.212233832408</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-11691.680793587006</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>-14948.228445143954</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-21409.252219509297</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>10495.028816032793</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>127506.68764739338</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>-787.89166745152693</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>9761.841076214625</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-90987.0643034688</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>18236.113561614082</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-27073.293403117685</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>-3012.9607149798244</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>60318.847365918853</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>304.91655599098794</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-32869.168870878559</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-68114.3090708631</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>9930.1418677086112</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-1272.9827485769902</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>1476.9935305325521</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>48569.394605340276</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>-1498.9432106356437</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>-44134.835925970328</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-22297.601874350898</Mosquito.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+L"">
<QueenBee.InPlayWeight>28831.283294251785</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-10796.96376591604</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-14331.824319162344</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-2841.0925360443639</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>-60133.31395022534</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-94292.105447510563</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-171446.42391926696</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-39539.037923607553</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-3229.4395882199024</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>1882.8289272169702</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>38778.397227677553</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>20987.964402738322</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-33142.072235331187</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-14853.125592075259</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-38272.292514325294</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>6367.2678575193222</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>4573.0839623275861</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>8614.5404165553155</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-147.45474309184152</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-52640.101939558357</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-6027.2276208349067</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>-25532.739426780561</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-13742.607106443636</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>5629.1269665747495</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>25924.300063437957</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>10970.829294160207</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-28220.524568934012</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-15267.614284340179</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>57315.732919648537</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-13664.699177927309</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>646.20972734276506</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>17384.2470954367</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>-1628.0221273098127</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>59766.392885092275</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-14636.65905439151</SoldierAnt.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+L"">
<QueenBee.InPlayWeight>1445.0257857309507</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-53263.33930084257</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-60735.647076897105</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>7885.2540406581993</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>71563.912510971786</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-227310.22191807572</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-465735.60631239414</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-9685.0909544948263</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-10742.845140847148</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>5939.3187382335209</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>69888.810849744841</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>-8428.65268983996</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-4410.6047642901758</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-5870.8399859216879</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-12896.676342878278</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>-5353.9977682321905</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>8904.3846072903234</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>58489.06312059475</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>552.56561288967066</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-3886.1182366502144</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-2733.4953752495126</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>829.632207809672</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-46921.268519715122</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>5485.8482960027632</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>60680.634784597714</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>-10262.612707318895</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>3741.1547252354003</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-15936.499787614937</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>145190.48834066998</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-7139.6255299620925</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>-471.36957860956954</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>26631.365825547906</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>2067.9049025633167</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>5721.78784201303</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>104.52469312394469</SoldierAnt.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+ML"">
<QueenBee.InPlayWeight>50837.031620256952</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-35419.478140500571</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-243.45622986720107</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-22355.989482597503</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>-84105.741673420329</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-4947.40542859837</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-9121.7862907225845</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-183922.24346427282</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-51088.715966831391</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>206.990683117213</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>94212.195816425025</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>94605.450724741677</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-169652.09402653066</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-21579.439803066332</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-319624.73751234385</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>53938.865528570306</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>-337.4041303961796</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>-485.60517579567255</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-106382.99553773669</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-356638.51686288341</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-8573.7450425364077</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>-27178.525364857123</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-33404.490951421416</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>548.44065050905192</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>77276.245224015787</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>15766.311363153041</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-67886.490066017082</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>14355.229523645041</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>200139.2683608809</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-62143.443626915083</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>-506.30530226706622</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>9421.88332525417</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>-2784.606961465232</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-13518.397319103129</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-56076.88001448063</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>140612.44480080935</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>22789.37675340896</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-245.83031252758963</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>34465.408908340622</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>723.10221734775473</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>13179.741673821443</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>75343.559007605232</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-123.08865780878313</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-34703.001558689095</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>182.04585589515855</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>16856.510990022292</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>-11683.000580421282</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>-25937.8236052361</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>2758.7798774658741</Ladybug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+ML"">
<QueenBee.InPlayWeight>17832.752038692164</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-153259.6446560958</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-12062.809088303911</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>80822.665556267631</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>134978.9720693233</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-381617.8635138495</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-521129.20243836124</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-12791.45541050752</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-61584.831349148</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>-775.35572307100165</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>120090.56161788374</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>-25620.550067509335</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>50071.490767260431</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>115729.74517664181</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-104764.43582698153</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>8148.1334677123405</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>-13504.915458214411</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>75441.89545110683</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>8154.507392742652</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>2083.30649676445</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>53817.23998276201</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>26486.8616248504</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-81940.610176146263</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>5987.60021560749</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>71575.748863625078</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>-7989.0958909230549</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>26619.553949671186</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>80307.851786135026</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>29983.953942488319</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-50928.471194140635</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>-19457.846451490077</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>25338.286810615977</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>3628.0368716020935</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>7118.0742514099165</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>88105.512723272492</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>41845.753703256662</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-36109.167261800008</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-9416.3896152440375</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-2170.3769898818791</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>2900.7926429185395</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>-6796.5473067725316</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>71063.218088971043</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>59515.93691288244</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-65568.30497491463</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>2051.6736028353162</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>24421.858543236438</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>5705.3410600898223</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>36667.608632668169</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>55179.844574520866</Ladybug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+P"">
<QueenBee.InPlayWeight>-40.153216303837148</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>610.97895075626616</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-5674.59263891028</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-2808.2626792403066</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>1929.9229459301355</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-12430.738003855466</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-551100.58177923807</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-3887.7405420869973</Spider.InPlayWeight>
<Spider.IsPinnedWeight>359.14243440936156</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>9474.6786133811875</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>33481.440576184796</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>409.64540080853749</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>8691.8457485279432</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-4123.4819244684459</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-389.28263096412559</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>2330.2292617753806</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>291.81429233582105</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>373.50369124554521</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-140.81443239182374</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-1534.0126553536838</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-563.87864266483155</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>1837.7984128716746</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-1511.2642017184719</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>7985.6770680441459</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>2634.5126999716863</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>6470.0637036309745</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-469.68839421426</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-87.224607244345577</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>964.03000110112794</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>175.6665871537962</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>56.040178223990623</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>657.91603676351326</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>3377.9485379561411</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-143.15204315400681</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-15089.737560052967</SoldierAnt.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-132.83136994606377</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>2332.632711454025</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>262.02233309887845</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-14579.795464712264</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-438.80886290190915</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>5272.8956927438276</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>24231.790822530354</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+P"">
<QueenBee.InPlayWeight>-634.65278474028742</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>481.41268922135947</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-8415.082960626818</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-3212.88665098636</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>3127.2380057913738</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-43910.264143116037</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-836696.35101766139</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-517.37655833931728</Spider.InPlayWeight>
<Spider.IsPinnedWeight>3772.7986007031973</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>6536.1486117759168</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>1208.9182506671361</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>467.41658672731666</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>424.07816269671395</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-4447.4178933048233</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-375.4156787267363</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>3093.3743629769797</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>1034.9061639467459</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>1043.5422287508188</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-4.224191747554161</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-2481.1065452633457</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-3182.3616057616873</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>1339.6666594818976</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-7529.900830571094</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>6760.5345593442253</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>1908.9337443072639</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>3239.4680707144103</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-233.89712278753271</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-239.98918017892527</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>671.03440067783231</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>282.21889966186285</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>14.877242101803317</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>11.66327868332238</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>1349.1294820601884</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-335.16805516879913</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-15922.43975018982</SoldierAnt.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-37.189784513070272</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>8668.689955627633</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>346.37077208393526</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-6798.9484228706051</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-289.18602165130346</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>2205.7209194401785</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>1024.5092727747576</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+MP"">
<QueenBee.InPlayWeight>-3297.6854103771279</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>154.69556363860323</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-1105.4290600961085</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-6425.8687528479077</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>8190.15422280396</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-183786.96023078603</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-842784.13670560555</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-5808.4047782077769</Spider.InPlayWeight>
<Spider.IsPinnedWeight>16573.316637234206</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>2791.495025909865</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>8496.4630947130045</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>1620.16107482649</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>6396.192572459111</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-4691.2949638435248</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-2346.5223481475491</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>4001.1926065003618</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>1110.4623994966855</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>782.0201923060614</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-72.961921113729019</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-10000.370651108489</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-124.51247737787428</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>114.4331414763552</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-19617.425890168495</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>16776.682313994461</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>1126.2544341692051</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>5867.7641247638539</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-439.82304406099291</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-1810.1038902815149</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>8057.6933087808911</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>631.24190271135171</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>235.6900966186077</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>174.534647707968</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>15296.016603805656</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-4544.9608153168147</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-8420.3855703813424</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>44496.974177782766</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-5847.2610022692252</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-22034.277281763374</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-606.063616651303</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>506.40423419663136</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>53392.327370439678</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-218.3402483504567</Mosquito.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-1853.3502941969382</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>64884.335215454455</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>466.99595383472189</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-2429.6612309796783</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-7731.4067546853421</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>5665.1939358343443</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>7974.2773794459317</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+MP"">
<QueenBee.InPlayWeight>-2897.3174583535133</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>4281.1768807729641</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-10532.504151526047</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-4760.1347218681822</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>1660.5413914664596</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-263952.72966926417</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-507049.6907477965</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-3283.6912818323563</Spider.InPlayWeight>
<Spider.IsPinnedWeight>29431.780841578675</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>18867.598597509437</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>168302.77094311683</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>3485.7837079214605</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>321.86563982125045</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-3800.4293324326018</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-3786.8971246714596</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>1669.6927489342802</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>4181.26235841499</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>4589.4738727955528</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-321.627981639517</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-848.49456913307722</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-1389.5111237907727</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>1084.2706694992371</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-1237.3946395389648</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>797.44525233623062</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>22232.738126157426</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>14160.8636164854</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-551.40313036382065</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-822.41817088274126</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>11793.847154577328</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>9639.79049720487</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>39.506614322039212</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>299.53728399137333</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>2175.7425217656732</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-307.08104662801696</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-13391.193515644816</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>528.02056756383547</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-6938.43146282435</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-34948.253303363272</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-3284.6766017014338</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>1143.6987911993231</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>116601.3299454988</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-4180.916215139875</Mosquito.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-10319.988990535712</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>123575.58631937616</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>2603.994189972278</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-32687.92375457142</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-4457.8560280914089</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>2069.1195966478067</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>748.295344292237</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+LP"">
<QueenBee.InPlayWeight>-8748.0100406638</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>742.90592017629922</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-18323.517380239115</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-595.05482224976242</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>3735.500825134869</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-69474.111182303124</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-781093.80078189727</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-185501.55544959707</Spider.InPlayWeight>
<Spider.IsPinnedWeight>6306.6974694193568</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>2128.6171897470258</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>10134.129497899397</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>396.48438641838391</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>20336.508083120345</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-479.3603327770598</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-503.09769142398426</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>13723.442209393314</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>2829.6206817716125</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>698.71889988901364</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-134.10565456647012</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-12558.284275898128</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-1999.7214957816175</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>2041.3924094534482</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-4353.043701786597</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>12513.168464904775</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>457.40091078653842</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>10430.430160261429</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-1955.2300783082103</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-534.067179710123</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>11180.730575384123</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>2382.6214227603596</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>1353.7005587260128</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>485.23260225334764</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>7821.64197889109</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-4793.0432785783669</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-51187.343495210691</SoldierAnt.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-19733.23823770184</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-49655.433286039013</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-13713.304536410047</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-8415.8803509657009</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>8057.3685029729177</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>13013.275350191978</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-4714.1472524141354</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-206.54781406363119</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>321658.29441728379</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>727.98845010305945</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-30353.474905396481</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-4721.5913874428425</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>4835.1476492376987</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>44929.765023024556</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+LP"">
<QueenBee.InPlayWeight>-10532.635391220048</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>2264.0484122093294</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-47921.924101667872</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-4915.133572569669</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>7131.7482120337227</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-143170.437837971</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-405461.23541163339</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-8441.6976144937835</Spider.InPlayWeight>
<Spider.IsPinnedWeight>5428.9722821265823</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>2006.8142444480063</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>88967.412776183672</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>1833.0521510172302</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>677.69848857012721</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-21085.406896373464</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-7641.0203924036941</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>6457.6001567164858</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>5977.7251892275726</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>1675.302550729494</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-160.31913768229748</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-4069.5830886442668</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-559.04704806745644</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>2897.6530408344888</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-14014.838606764246</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>2015.2440771724369</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>6524.0200834794714</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>34191.1495416305</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-1063.8132477141896</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-397.18844493109219</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>117461.15826911696</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>7044.9416338737938</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>212.76592499362951</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>684.63895359861658</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>4844.257939648317</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-175.84815812254453</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-14767.8474612564</SoldierAnt.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-3309.9806151122129</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-18977.325084997956</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-2911.1142749726469</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-284.97165736957129</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>3438.9430609955293</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>24568.297879773534</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-5072.71941988165</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-8313.88974419674</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>38673.973849618618</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>222.23756756526225</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-13070.606991504526</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-1156.6141556723287</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>11675.0719761247</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>613.20695251393215</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+MLP"">
<QueenBee.InPlayWeight>-1957.0779689889287</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>69.228975315017522</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-16259.942772938033</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-7454.7583829345285</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>14805.229859001825</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-137901.75644983604</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-730570.64460705523</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-2799.0821095483825</Spider.InPlayWeight>
<Spider.IsPinnedWeight>24297.534898598358</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>566.90967229915509</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>16049.441761487506</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>851.80817986866339</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>2674.0582229395845</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-20074.966347621554</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-30302.227176511802</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>5797.3629346060752</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>3781.1956806803828</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>1063.4314782213705</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-151.17246469596452</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-11836.537480926796</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-964.37411977306556</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>6507.047788088561</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-17994.04939715721</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>386.39546569882714</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>5055.9087062161207</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>19080.18837401221</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-3049.4590027147597</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-3931.0217337642325</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>7526.0088210655558</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>1946.8440284593519</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>428.56897216352843</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>690.165496471477</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>7313.2267047009027</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-2184.227491876808</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-16909.028101648953</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>29259.482654262767</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-5324.7027744220713</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-11848.830608380362</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-1530.4884508088771</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>57.674397733975773</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>25601.822287396284</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-1834.435569993638</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-838.73587088033935</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-1330.6094355089667</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-9340.2349407880247</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-2671.3170688501668</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>1915.8799171312494</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>2341.0325383199984</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-4269.7111610529273</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-1186.9587897927497</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>38824.472083326938</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>490.52035078796268</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-16306.40251347242</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-18701.65856068522</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>3412.2667821029218</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>9604.8487792409924</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+MLP"">
<QueenBee.InPlayWeight>-139.53218742267774</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>9518.211602095269</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-36560.21552357325</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-4514.4977676650078</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>9810.5054354361509</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-125626.90473744011</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-850186.69197101286</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-115428.41401386964</Spider.InPlayWeight>
<Spider.IsPinnedWeight>563.11231187407338</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>4690.77648482076</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>117116.40637499519</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>2809.9493178411117</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>8508.8901996853547</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-7273.5774677040536</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-143.78749416688626</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>3329.9887102555526</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>1909.6848970316989</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>9721.5264263134868</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-192.60267096049776</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-6960.0073578419888</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-2016.5052744488617</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>19151.189798365689</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-7413.8563497458317</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>38.558973162637145</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>12764.039896774133</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>11027.12228025036</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-2691.2042960460567</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-6316.9165835380763</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>2595.0063403935114</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>326.60179504324043</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>1035.7327414539629</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>1207.95316974924</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>7790.5039428926748</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-2411.7012913165036</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-29478.51329902848</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>20945.101381651381</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-4059.9331796565848</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-11388.844473730344</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-541.54268967588757</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>408.1450274773693</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>85946.316049489178</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-4156.879288039233</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-35186.126092416649</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-73091.350225276008</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-494.39821575007733</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-2013.1531287140856</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>3784.4769802330857</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>137733.80428389224</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-43943.169653606252</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-232.27427974496985</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>20613.21820706331</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>141.46158573782711</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-67297.286508068108</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-6240.4015015736031</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>3257.6537423248556</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>21863.383447357566</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
</GameAI>
</Mzinga.Engine>
";
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