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
<QueenBee.InPlayWeight>-1028.9522889755306</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>2772.1771360833741</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-98455.599048615943</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-6400.4189685549527</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>65678.1870378142</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-53183.3386888371</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-575351.53819758038</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-5318.9508599289511</Spider.InPlayWeight>
<Spider.IsPinnedWeight>9098.29634080316</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>5893.7571183441323</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>6445.9731077916986</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>14010.612385365703</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>1138.2225437498223</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-2254.6077138311016</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-16475.78900603966</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>16763.419460502395</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>1555.0178825004819</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>1483.9835542316143</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-149.89812768370629</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-3261.8641946841421</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-592.50599755079213</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>1675.0455121079142</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-28959.165306619911</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>10129.673405263135</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>3498.2363588193639</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>7768.9955115546582</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-3291.3337477378705</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-472.19131518871274</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>10584.562678812163</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>6307.108559461396</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>272.32064776337444</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>731.06064094082774</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>4069.91387884014</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-14423.624220846568</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-56393.528209260505</SoldierAnt.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base"">
<QueenBee.InPlayWeight>-6383.7105286995438</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>2127.4543420088407</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-14678.039677936433</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-619.00578882861453</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>22592.136210719334</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-31342.622833237441</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-375729.53743838216</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-30342.124311657604</Spider.InPlayWeight>
<Spider.IsPinnedWeight>41249.880773759869</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>6537.6570111341371</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>43815.062290129361</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>7812.4556293172</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>32614.015275932477</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-14803.25691687146</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-624.48638050911268</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>11258.933761216038</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>4949.5633653537361</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>2757.7640096416</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-92.349039901617274</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-709.86026328198614</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-4921.1156779530711</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>2100.7683182963674</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-8764.8749631517676</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>5952.7888850149484</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>40695.078362266722</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>3276.9395297997121</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-3550.1191380176078</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-446.8941184916419</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>13415.757734493855</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>286.28170662852614</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>199.60436575871267</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>406.6566652376697</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>5539.0305846723113</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-42439.329312883325</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-32681.285640582235</SoldierAnt.EnemyNeighborWeight>
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
<QueenBee.InPlayWeight>54046.864080121806</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-37332.785466553534</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>1019.3959930344854</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-86649.280986725527</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>-55127.105683646507</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>58898.580482532132</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-16192.660977691314</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-290382.06319810508</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-123410.42088480575</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>2176.8883703719571</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>154994.0091435248</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>116223.40333504909</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-123407.22919112143</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>18776.1460868664</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-671713.31282657769</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>34979.687968426835</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>-16.113394695267008</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>10062.453218718034</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-119195.07335083014</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-607117.23853398242</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-23642.804838927641</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>7474.1391707694047</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-47190.906916301617</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>2783.1267248621307</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>68277.7254594643</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>1700.8854262808509</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-41084.02074540998</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-4208.5767797230428</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>191859.19891478022</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-75195.223980880226</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>10.164603217840101</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>-18780.034269998076</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>-404.84481356538743</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-26567.288102871822</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-86881.2618657483</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>207757.53585097977</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-9182.07022645712</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-297.13208307863022</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>10304.876033854438</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>4445.0517571796436</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>-12492.958005987392</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>2626.046474816093</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>134954.33502609431</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-25030.879204510155</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>450.21201963634411</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>18889.46931251117</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>-6715.65853164985</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>9423.5795736836062</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>52163.792231725594</Ladybug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+ML"">
<QueenBee.InPlayWeight>17974.230594391371</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-82774.851738301033</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>41679.123425725367</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>138219.49352136225</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>125167.4404228778</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-381600.12615119433</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-508134.79384860891</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>120710.68388856297</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-7703.6375543688755</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>20538.935964253629</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>149003.98014308626</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>-23212.783426855829</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>32305.029465246025</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>51958.810388550817</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-2779.8960908108197</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>-30308.854138266419</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>15479.010059825576</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>131507.39671816962</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-2427.0202994891665</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>34845.429064717042</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>59145.266201744191</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>134014.00586540034</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-57876.021551186226</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>29768.704531650135</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>95122.5012339548</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>1027.4296591887417</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>22657.94897013544</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>34079.250995545568</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>1317.1778466573821</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-58632.3697641871</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>-18788.093620904765</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>54502.7599256545</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>-78.291941588286434</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>10620.838130643404</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>34779.414239284073</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>55510.719524343265</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-37527.167935092781</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-4358.6977013710293</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>15709.300474810518</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>-594.54328687460077</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>20468.174427395836</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>61177.795086074635</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>162192.07467522862</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-118387.77628906404</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>14068.976194064464</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>6464.1047122027094</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>2500.6095566441395</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>-19288.126461537846</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>20894.606631192975</Ladybug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+P"">
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
<Pillbug.InPlayWeight>-1853.3502941969382</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>64884.335215454455</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>466.99595383472189</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-2429.6612309796783</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-7731.4067546853421</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>5665.1939358343443</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>7974.2773794459317</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+P"">
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
<Pillbug.InPlayWeight>-10319.988990535712</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>123575.58631937616</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>2603.994189972278</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-32687.92375457142</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-4457.8560280914089</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>2069.1195966478067</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>748.295344292237</Pillbug.EnemyNeighborWeight>
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
<Ladybug.InPlayWeight>-4494.0371804430461</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-5543.12988753058</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-567.96641260987792</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-95.676747616653486</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>1006.6760051760265</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>1601.0593338368017</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-4767.6021347950746</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-132.83136994606377</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>2332.632711454025</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>262.02233309887845</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-14579.795464712264</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-438.80886290190915</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>5272.8956927438276</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>24231.790822530354</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+LP"">
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
<Ladybug.InPlayWeight>-491.140000996503</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-1819.5021969538686</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-1728.171823086986</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-425.10017332778483</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>2532.5514332997968</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>9471.299551139904</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-813.33862146594061</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-37.189784513070272</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>8668.689955627633</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>346.37077208393526</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-6798.9484228706051</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-289.18602165130346</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>2205.7209194401785</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>1024.5092727747576</Pillbug.EnemyNeighborWeight>
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