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
<QueenBee.InPlayWeight>-9.9937657822918915</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>1.2911118637729029</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>0.26616060381110779</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-32.717003741290888</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>27.308454880019863</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-891.57588418814635</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-4904.2568363269129</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-22.003068274163208</Spider.InPlayWeight>
<Spider.IsPinnedWeight>7.7084710892422459</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>14.640539564788595</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>149.47389965032875</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>2.2532554332088939</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>5.7402133451216919</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-30.574873715725197</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-2.0524318288161569</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>19.653543986339844</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>2.0508976151775093</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>5.5202967438964627</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-0.25070810240260705</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-5.3825052289373732</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-21.785729757310232</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>26.43127504017729</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-31.026979668679171</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>72.580814197473615</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>4.9739249005978579</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>0.10146843308651281</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-3.9916643105943987</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-1.1080550982724202</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>18.204194128319713</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>1.467455425424248</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>0.9787036586919351</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>0.64941162420875009</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>25.108906018877821</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-2.1837339004332819</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-54.580500268326375</SoldierAnt.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base"">
<QueenBee.InPlayWeight>-6.3347369405882148</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>6.4372691180730577</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-61.585958248482065</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-13.17424854492501</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>14.988237910395599</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-713.9692225667319</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-2348.966528125557</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-44.863590195620716</Spider.InPlayWeight>
<Spider.IsPinnedWeight>4.4163426735770628</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>69.178407757663365</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>121.41049695243618</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>2.0992942683604983</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>42.6610700403252</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-16.891946907898852</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-1.1254791736442036</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>40.098723949249248</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>0.76613102736004224</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>10.500510652608011</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-0.89146008009624667</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-2.9234851571537952</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-11.491038426887728</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>4.2126106818731</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-70.02921196992007</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>45.318066436004855</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>16.671954854480898</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>72.263471446146</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-5.82792522279449</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-3.7539740931377628</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>23.550983311376061</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>2.5858888818164769</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>0.71326926539338631</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>2.6705776495163227</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>7.2967450717542066</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-0.27570951117701337</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-92.8920801060307</SoldierAnt.EnemyNeighborWeight>
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
<QueenBee.InPlayWeight>-52.246717883621677</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>0.82788070970581873</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-96.622528746851032</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-17.480324449744227</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>449.30707545330142</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-69.384953316661978</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-5942.3524627193583</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-39.450833790901029</Spider.InPlayWeight>
<Spider.IsPinnedWeight>228.22536948048295</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>8.028526096967294</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>448.98868943936606</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>10.152983486649106</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>45.317469862865977</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-6.0102455232572938</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-5.5393426249475883</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>141.52386490388952</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>3.3070581417330818</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>12.665167045390776</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-0.11922250105963204</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-120.82492882526709</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-1.7393567422192344</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>131.8834058873488</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-19.165366148429626</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>290.44227866443447</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>54.010678762691413</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>133.16319502459987</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-64.784583256013974</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-1.0606157267515621</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>95.458320938971966</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>25.293232414316492</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>1.8796008129742</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>19.462916356494144</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>91.8490609615478</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-22.112107972084591</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-334.40261304183747</SoldierAnt.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-9.1136872713075441</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>1486.6262842915492</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>0.12466972517998409</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-176.46003344968662</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-132.41559439730622</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>24.673845413636343</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>851.90752928420329</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+P"">
<QueenBee.InPlayWeight>-0.19598997834883156</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>1.6056260993055189</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-74.589771486180709</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-3.8182033200833034</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>88.196171025841579</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-515.22085596067177</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-3296.9843258135879</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-176.34332286381084</Spider.InPlayWeight>
<Spider.IsPinnedWeight>18.957938095276681</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>0.32588394318981273</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>201.41832159681636</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>0.35487148638575877</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>86.931496397164423</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-27.922372326169807</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-17.173556972850836</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>8.5997920999046382</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>14.471738958333962</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>35.911390666750442</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-0.98689876624704287</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-32.523116287658041</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-36.996907247533983</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>42.520008851089514</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-6.29501465068898</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>72.548901808240416</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>4.069443603288124</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>7.9027579592181176</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-1.5718461521886504</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-0.94945048902623841</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>8.6787604382255861</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>1.8100034993188474</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>0.36405580096135653</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>5.26453112512805</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>46.089457285371353</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-32.150008910486953</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-168.2608634902075</SoldierAnt.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-2.92230300211222</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>36.14230688121976</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>1.0348840069979819</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-327.45214315771926</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-4.7686213002021525</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>0.7586273420223163</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>24.950130621306656</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+MP"">
<QueenBee.InPlayWeight>47034.705284283686</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-1299.3991652883026</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-2669.1928126372277</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-55662.723081990407</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>-38648.096573109171</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-186332.6373806225</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-419146.16159089725</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-63340.412906000536</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-30708.0803632806</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>13166.498340290942</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>125194.23178628468</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>63424.790048124487</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-23148.979240986824</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>5337.0924052034325</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-137876.69831729116</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>70863.8732311342</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>470.83836705110321</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>-1528.6696233206797</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-110453.96326732528</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-121349.2102510345</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-23360.226802730678</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>-1012.8728431917037</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-31711.618347397449</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>2841.5272795078918</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>2951.4032669604371</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>69394.223753929662</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-56161.893067634941</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>694.89369515226622</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>143902.40598592532</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-27881.940777256925</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>1188.749217040337</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>-1865.117965059424</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>1523.3514169599157</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-16453.765580204774</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-91035.172436898458</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>166111.97759300718</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-6794.2190306216244</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-882.39868472178875</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>7099.2229299465826</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>-4421.1089714192076</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>65586.416348561426</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>43327.709509940294</Mosquito.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-7549.4775370743619</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>2627.2712825371068</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>-4.6158031086926163</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-17112.853662862108</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-1762.0343061818824</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>1779.1103645430176</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>15711.121587210604</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+MP"">
<QueenBee.InPlayWeight>4003.2001645801492</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-23685.234297865376</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-13644.576217839827</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>6371.6736714426224</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>124095.2186083909</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-256748.34705393005</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-755821.99877711106</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>222.73011425246654</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-5879.7177326795818</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>6.5890667252560453</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>34119.046686867536</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>-7951.6869620747366</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>440.47424711442437</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>37567.076132761918</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-67999.961978536463</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>2505.4939401609386</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>-14023.174904063506</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>6220.0822675296095</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-6176.9147529820248</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>12650.428267323941</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>21721.494445336572</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>26090.536378383051</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-8419.3922624171955</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>10985.012209746206</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>18020.607534051855</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>-1721.1297764929752</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>5799.1455565507922</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>47696.767416287985</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>38051.1178188649</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-21081.311866821958</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>1304.2694335796514</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>21322.409004601042</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>-961.43276945465675</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-796.05021579914467</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>47992.771372726515</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>47857.458968434643</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-12025.600717508354</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>2616.224069811346</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>13021.897878567928</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>668.30569063588916</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>306.76112793337114</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>23717.2617927785</Mosquito.EnemyNeighborWeight>
<Pillbug.InPlayWeight>8380.9021593403068</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>-3190.3598833314636</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>-556.93035777941839</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>551.62836556447519</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-1191.30038667527</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>722.197604811424</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>51605.21317328867</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+LP"">
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
<Ladybug.InPlayWeight>-16224.896147230009</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-149.15986133715336</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-8219.28538004182</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-304.05338011241264</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>1468.9849927383139</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>2722.9678485029731</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-1149.5882352176116</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-1853.3502941969382</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>64884.335215454455</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>466.99595383472189</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-2429.6612309796783</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-7731.4067546853421</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>5665.1939358343443</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>7974.2773794459317</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+LP"">
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
<Ladybug.InPlayWeight>-2841.3476058918459</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-1924.2235446411253</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-2289.7578287142896</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-167.35798165278862</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>4957.0582483714616</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>11171.976641274338</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-14502.790049245195</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-10319.988990535712</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>123575.58631937616</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>2603.994189972278</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-32687.92375457142</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-4457.8560280914089</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>2069.1195966478067</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>748.295344292237</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+MLP"">
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
<Mosquito.InPlayWeight>1124.4802210425389</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-798.34170538410217</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-19403.436707605473</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-5980.8371941100577</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>1737.8508539572931</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>59676.527339733286</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-673.855366382021</Mosquito.EnemyNeighborWeight>
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
<EndMetricWeights GameType=""Base+MLP"">
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
<Mosquito.InPlayWeight>1638.2351971125784</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-6410.1310951929672</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-3827.7301442975145</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-5650.3254069481281</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>711.17784785801723</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>40300.791954303939</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-392.00758954708914</Mosquito.EnemyNeighborWeight>
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