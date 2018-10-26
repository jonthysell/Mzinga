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
<QueenBee.InPlayWeight>-3123.9174454836161</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>3011.69079754912</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-102935.50729160674</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-1151.5007055429467</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>2307.3519587969308</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-437290.18561431806</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-321990.47535627766</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-1915.0464604182325</Spider.InPlayWeight>
<Spider.IsPinnedWeight>6964.1210656549647</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>16487.031731952266</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>28660.151862136005</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>982.60693682647855</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>1145.9362758848927</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-1209.9217149227345</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-430.31780636515185</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>14817.397639664505</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>5957.8343978729936</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>322.61894861618225</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-8.1045783183701339</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-3085.320386300561</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-470.97835409347539</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>10727.061206567621</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-1399.9253714195547</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>5762.129875646976</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>302.21527378219514</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>107977.34397182446</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-2690.5114616101009</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-426.03669711805748</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>2895.8854262871887</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>3106.9915388164186</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>71.328384397156142</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>1515.8522544761731</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>7035.8617912899663</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-1682.0820867405771</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-46830.269247187221</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>2785.0501978439006</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-1886.6469792491662</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-3702.9218510258956</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-4837.2833354761633</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>4243.5610618735827</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>25185.592552936338</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-299.36844616153809</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-338.66788828341032</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-13776.411694561508</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-6083.9832499455315</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-918.87363838331123</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>6904.0367082098528</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>139.26854200873115</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-6648.9923661266357</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-105.69579149822064</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>1233.4530614538137</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>611.71945746234735</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-39627.143991391851</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-2233.4311641126228</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>479.83661823009362</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>51683.449133872557</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base"">
<QueenBee.InPlayWeight>-2313.8185212115595</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>5590.0955356650311</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-16916.612650510317</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-11042.3350072588</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>94.922573604362242</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-21291.232953796913</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-967463.40556333924</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-36721.02749992122</Spider.InPlayWeight>
<Spider.IsPinnedWeight>10053.857668700732</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>3065.076884399497</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>23706.733281981215</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>7111.8732520227386</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>68.068893725407051</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-36782.74343207373</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-18620.980535565108</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>2301.6710845431539</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>3207.1600294749092</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>1749.2897782092769</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-34.751785473358943</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-1062.8974661606922</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-1609.9969549345951</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>5827.7661058540907</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-10184.850262265914</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>7449.7534253983067</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>3278.3445222497289</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>34898.869278022816</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-223.09029086584619</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-96.475149834343455</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>11861.647319637215</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>8331.02858671345</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>1314.9084614597214</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>124.43771314526572</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>880.91267343776815</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-39420.672919628654</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-527.19869379172712</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>2063.6271105425812</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-7739.5098939468971</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-5865.38187475025</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-2913.1885733386534</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>4346.697712737965</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>33626.482130000273</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-2219.9989262177182</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-9611.7994570057181</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-53.394229189087589</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-1103.5171715257361</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-1199.2792644543272</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>10244.144462435461</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>2832.9577913909275</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-9905.6742632902588</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-16859.921676163474</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>77123.73606247628</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>323.00037201893667</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-716.27448445542552</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-3475.3327868487922</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>1509.1808015663589</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>34958.78249216624</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+M"">
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
<Mosquito.InPlayWeight>22019.76398755252</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-10877.036693455011</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-10413.384771369838</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-1457.1927531883353</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>1214.2213259555042</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>217760.93840840182</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-2317.3542777957573</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-4821.6743613037761</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-5706.2682422690914</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-2462.1375679236921</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-489.4270319904731</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>5301.393952377739</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>21942.742737919332</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-7214.9343771637114</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-1273.6440414919327</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>110577.5009963717</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>82.7839351626988</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-2631.0435295082566</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-11758.34770426457</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>6595.0350240316357</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>25753.816634633309</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+M"">
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
<Mosquito.InPlayWeight>22966.134384894805</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-21806.522272829872</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-14947.609977951159</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-2722.5021963266472</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>947.3212808063015</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>43035.317026578305</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-2989.0113611776605</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-2111.0249621747084</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-46018.5445537882</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-6627.1183928636983</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-2032.9579541412541</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>26908.565762872338</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>100475.85162719995</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-78289.0044431386</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-10.983594497394225</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>30804.539468331077</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>120.5943463271983</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-24735.047944628914</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-7379.5637785084309</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>5094.84444937076</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>15357.513055243669</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+L"">
<QueenBee.InPlayWeight>-383.49275930752657</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>769.51475536003034</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-3613.5764668660818</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-1445.4524217869105</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>4707.43980757862</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-178056.28770449833</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-195254.69873931943</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-9475.4354414725985</Spider.InPlayWeight>
<Spider.IsPinnedWeight>4542.44117844181</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>677.880917897691</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>8181.2646863264408</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>1085.2303913889869</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>2369.5849045544728</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-733.61752570374972</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-8049.1745923234848</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>5229.44840816217</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>2311.0391738339526</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>2061.9013763678827</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-151.08620335343434</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-4778.96384635272</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-267.56485866181157</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>189.87145926722349</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-360.26608207514681</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>94.070969251847416</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>162.10868179863272</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>1746.3154059773729</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-543.09448725914388</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-993.38894386553886</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>6610.5683895341272</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>885.690222632025</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>195.09307748514564</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>1062.5492588374086</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>12637.899729637435</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-273.55540114127274</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-15259.200615007558</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>1971.9556895758133</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-689.6603210736057</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-8481.2462474474141</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-755.03497121143107</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>1302.8187494229692</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>57014.77828409517</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-562.29012641220777</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-2860.000150988225</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-17274.652661099277</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-1515.9964305012263</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-3715.9466553908587</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>1816.1967749200665</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>17502.495528113053</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-3285.1270609933558</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-425.87263898431144</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>43550.295625822248</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>35.050051991475485</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-7514.5554538987017</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-4771.1907290491117</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>1607.3376992054004</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>73842.445702449113</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+L"">
<QueenBee.InPlayWeight>-3016.5881751057409</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>1348.8307696323836</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-14573.200752611427</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-16032.0795459876</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>17600.512519157324</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-361321.92310808052</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-463161.73167966667</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-2600.8148705158787</Spider.InPlayWeight>
<Spider.IsPinnedWeight>29070.127281993355</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>12234.553580755519</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>20524.250184519278</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>1044.0545489353408</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>2360.8351160762359</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-5492.0008138873118</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-7478.0209165980168</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>5059.45039620507</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>1467.6179472426679</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>1586.9145155103925</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-122.86600511814545</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-7310.5339012847244</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-2793.693015434982</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>601.748113164656</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-7891.4446950157489</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>4086.2743136825356</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>5877.0987728017426</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>10705.763732195099</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-11866.013154826605</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-938.40689904372618</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>6899.83522580254</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>15697.5123033378</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>104.98251500521168</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>852.72780486643819</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>2593.4946459181197</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-5491.2192433564778</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-56644.521554438194</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>4761.14419990924</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-2071.7803385319603</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-480.04727812048282</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-1267.9454538336627</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>162.6951339393697</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>23459.706986165293</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-1016.8713648469284</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-4332.9446957843411</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-5755.24239665745</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-14711.160168509019</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-3267.9163886370288</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>2573.6743922436153</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>102254.81910237302</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-41834.819638065004</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-16907.603281843312</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>44096.0248607874</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>994.81731784050271</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-65960.469827347275</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-2372.5520815997411</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>4264.3041771764665</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>2983.9094514979488</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+ML"">
<QueenBee.InPlayWeight>50993.54950541148</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-35049.706254843331</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-261.32307263133947</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-23025.216616464255</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>-84483.875595065329</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-2818.9502753065522</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-8448.2614503993282</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-185072.383650251</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-52297.525267192104</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>214.21234665678995</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>95580.85748581661</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>95179.5739379346</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-170697.9264011714</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-22100.423960011234</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-324301.94395302079</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>54214.745033198742</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>-335.36873411037919</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>-401.73378048361781</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-107767.12059885105</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-361100.09664345841</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-8631.1430245752781</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>-27614.956369999552</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-33572.678710603475</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>565.65622003611873</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>78199.309707292967</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>16582.909772193569</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-67570.093509493367</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>14425.64012835603</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>201721.5439136549</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-62032.944417289909</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>-501.23863110771276</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>9765.63007288695</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>-685.42046343437573</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-12978.820754287935</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-56088.065646947747</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>141717.26577920321</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>22389.170501224802</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-233.10247141340341</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>33996.116466757427</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>-1767.0264582532086</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>12930.945586931137</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>74987.140014586141</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>1616.172861790938</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-34672.583215063685</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>189.03840087883475</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>17409.553780395498</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>-7005.3249242179245</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>-23694.150138705412</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>3066.3970440977914</Ladybug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+ML"">
<QueenBee.InPlayWeight>17899.772953318541</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-151885.33777420706</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-12616.927321484051</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>80443.568613593088</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>132095.22429287859</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-382476.23642586637</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-526085.676551712</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-12537.800772323522</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-61788.815618282126</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>-936.16885136677377</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>121422.6108328097</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>-29490.394839614248</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>45505.372235443945</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>112059.60267161719</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-103702.5014421411</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>7441.9530455924369</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>-13142.415023508684</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>76729.973279127022</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>6175.5248926404793</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>9840.5369067353877</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>54199.845673405332</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>26512.791463332684</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-82882.629740123564</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>6165.8486252172634</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>70498.6326865587</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>-1597.9406379448769</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>28926.460619012505</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>81037.8799774361</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>30410.349968743263</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-50662.322208955928</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>-19561.786288813342</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>24912.185327571471</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>-2867.9287876640178</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>6798.2598542916694</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>90256.736469698022</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>42042.869623593331</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-36063.989960738007</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-9339.5144823942337</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-2731.7903108690825</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>4929.0388813884538</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>-2813.0217764659778</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>69687.222976965335</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>60544.696375408683</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-65527.366843581447</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>2174.1081191596336</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>22316.076516653538</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>4680.455279954178</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>34366.864034470178</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>58754.453143467625</Ladybug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+P"">
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
<EndMetricWeights GameType=""Base+P"">
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
<StartMetricWeights GameType=""Base+MP"">
<QueenBee.InPlayWeight>-2002.364970987783</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>104.34705561516481</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-21427.404106372334</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-2298.9294042301417</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>1650.0357624387289</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-99356.030728891419</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-605648.11744333629</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-5675.9020905778762</Spider.InPlayWeight>
<Spider.IsPinnedWeight>1673.175346090484</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>6311.206452716342</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>25656.894633653166</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>392.31736492366406</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>2072.6806011809217</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-3521.5762283514832</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-278.35433441964187</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>374.82065496810537</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>1579.1197628853404</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>473.98779192828073</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-35.456246928050319</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-200.68957267309787</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-233.15389073650903</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>3525.8239236280583</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-4313.3370929848888</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>9315.81740668635</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>3870.9248970737926</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>31827.128089538743</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-1582.6451675625822</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-60.709585826374237</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>1320.1585502447185</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>2393.13228292777</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>130.03256099116484</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>3.250532830296947</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>261.46674559190836</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-1523.665111568213</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-12856.908978006068</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>1980.2990341833522</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-1069.3794020436553</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-615.58781007587243</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-1199.2234543020538</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>634.9916347729685</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>48318.907588826238</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-823.985545392082</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-2853.196514715672</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-2807.7807879651446</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-431.11941662899989</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-524.46738545689561</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>1498.1464544740197</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>2106.1054454456603</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-1232.9927192552216</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-650.61034006200623</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>1048.423569653251</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>641.2354303780985</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-16441.895543389914</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-386.98825328363353</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>2563.8662136303369</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>2573.8395742835487</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+MP"">
<QueenBee.InPlayWeight>-4298.0753527240895</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>15586.770812299028</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-142470.24096041895</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-13301.554851378791</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>1514.2956453537222</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-56568.582030910489</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-619029.6793571345</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-35212.962175460394</Spider.InPlayWeight>
<Spider.IsPinnedWeight>32251.434269959762</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>5409.0708786582818</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>52399.01544370818</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>3232.3874142700638</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>5203.7936341212107</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-35899.206578140147</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-7713.952325631526</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>19701.238018390897</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>3927.1452789787777</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>3851.5148440393996</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-16.189970603866165</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-189.861794223421</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-1648.4309031171158</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>7096.3223806659607</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-14968.575428914113</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>7197.8840093921081</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>4352.1584701931788</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>24834.731052706131</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-1033.7939534653453</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-1385.6996782097917</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>78537.298111892669</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>4339.003640671167</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>599.172581603279</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>199.696861191003</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>64.420044274597728</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-28413.244304402524</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-1314.6705894632419</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>6035.9234412806964</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-4561.514646991478</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-77919.656519859345</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-2089.0523392357481</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>3803.3745763991792</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>282013.49522606138</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-1091.581377096802</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-21297.27731031943</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-1379.4911366949036</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-58.674729087914393</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-2160.8312797329841</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>3137.49411357349</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>1527.1940846515447</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-15348.135375712316</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-12704.411976160554</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>88259.828896474879</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>782.617294827001</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-2194.46568917606</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-1607.8304721702848</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>3921.9932956868961</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>8751.8558336791684</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+LP"">
<QueenBee.InPlayWeight>-4369.4090191249816</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>1719.089013574938</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-42716.9526336412</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-1383.995374112232</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>25587.929247771812</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-353951.89270660456</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-955686.54810352542</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-169401.64462653338</Spider.InPlayWeight>
<Spider.IsPinnedWeight>8847.51775856128</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>599.89152384454019</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>50801.534541523557</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>1803.6577219073095</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>12750.29560887033</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-7305.569449755284</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-1825.4612178935065</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>37408.840187463196</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>6569.3142121796109</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>860.66953020980361</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-18.94800568847446</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-15307.986912579645</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-1338.6973330162903</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>2771.79976606054</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-331.00798881307213</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>713.21292106448982</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>3282.969664983078</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>2741.4363242426803</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-2584.0788474360515</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-1407.9735055911419</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>9799.5065427021582</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>413.64065956188648</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>77.0142598712409</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>38.286629742957615</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>1626.7546127248904</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-3880.9449770874571</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-22133.541401562496</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>13552.273128762978</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-681.88984639697</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-23734.891603529468</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-15044.32890665219</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>2205.6224163188976</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>99512.865615687391</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-417.91592947178344</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-2514.3572551265925</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-8018.9589061427005</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-6880.4854695633394</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-6580.66203769444</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>6812.0294123823078</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>3658.561252313973</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-1417.5855032923791</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-2786.5469135270523</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>17300.551767931065</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>587.09235896917187</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-1600.4142493146496</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-8777.2988003985356</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>16445.725178164339</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>5087.7459740871345</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+LP"">
<QueenBee.InPlayWeight>-2124.4461045680487</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>88.651646396091166</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-49615.717507800633</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-5215.0030383900767</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>6764.8156335692156</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-99268.735443508427</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-474600.9368294389</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-349.27398436670779</Spider.InPlayWeight>
<Spider.IsPinnedWeight>4859.9073176857964</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>18587.736861736619</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>27580.705813108678</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>330.56554011468546</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>5711.505406448312</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-14418.514711987065</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-2803.4738944267888</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>7593.9637876122</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>625.30197648672686</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>344.54456942424156</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-8.8565195043674034</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-1846.765593394699</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-36.638332577502311</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>4251.0900213727164</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-11861.339435080992</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>2541.5269039465252</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>2052.4840468845723</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>41526.337817086271</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-1988.3472756487715</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-845.11396880767575</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>16356.522318115225</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>1539.8919861464096</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>161.21992330001322</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>363.20205575458175</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>2504.1898174801418</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-172.65512388722203</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-7074.6519208790078</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>5334.5980793787348</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-367.50621234352542</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-5247.600553920226</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-1720.8224236530791</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>552.40640176530064</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>37962.469863169004</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-64.815291910129744</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-15441.655187354962</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-9273.837191512006</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-1303.0107346211134</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-13.339116830800052</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>2659.51613132047</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>4311.6812248526721</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-3483.7453180039511</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-403.24913471736</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>860.39802726654625</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>208.82784580827072</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-9876.26726420107</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-2119.2175276986627</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>2334.2167486575395</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>9929.11873093958</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+MLP"">
<QueenBee.InPlayWeight>-58.839928842491894</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>1.55736891441856</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-809.17292178501737</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-48.45062717400986</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>374.56017142287931</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-3370.2132787522173</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-9310.3925971828357</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-107.66473131087362</Spider.InPlayWeight>
<Spider.IsPinnedWeight>68.948962006424068</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>73.242806189950926</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>496.74349111535753</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>11.931358833858447</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>19.560348888654424</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-84.9509644449111</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>1.2118163418941277</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>14.667523120882695</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>176.40852410366455</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>39.179302115940629</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-3.8506510618867584</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-154.48006005330947</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-16.487719302618746</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>239.10936478661347</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-753.04635453157414</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>19.555528324435244</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>68.056652389828429</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>479.528455740981</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-112.8632084901157</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-7.2581338460827869</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>414.2665874011891</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>50.847581675403092</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>2.3251436688216138</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>13.362756557642834</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>197.55293054199686</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-162.1924245797691</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-178.67916668472304</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>15.19488875798643</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-21.382333094963027</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-23.065890953122121</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-81.162285399663844</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>2.6453295959463943</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>1976.2729570608087</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-54.927965295351093</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-193.04247386899428</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-81.052078126022622</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-68.816270382842632</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-45.13406965812392</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>64.660032564112925</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>278.79329770325842</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-214.05775264903332</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-163.21665913128604</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>27.132561702231211</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>25.553814124569211</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-646.444685520248</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-8.2170301317828844</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>115.51546644598265</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>238.49943671612041</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+MLP"">
<QueenBee.InPlayWeight>-62.099936259398206</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>39.84533088959536</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-508.63404537571552</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-199.88733099801806</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>9.5196911521347634</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-1264.4463526514294</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-3733.2126629693489</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-180.05018847913024</Spider.InPlayWeight>
<Spider.IsPinnedWeight>218.25155575032881</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>33.212394272793269</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>455.30103535701568</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>15.346815102150112</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>8.4863248015224588</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-156.83567479835389</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-114.15732203617567</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>58.120211728361994</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>37.535643590106922</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>31.618996602375525</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-0.24052221498243614</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-3.1721452301983466</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-3.0256070060588449</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>38.759820818649523</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-96.278357175680966</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>73.98131080161842</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>66.81815286596219</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>112.83739249093337</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-0.10225676785793927</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-3.88304612063479</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>482.60146011495095</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>40.427820445668338</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>3.136300187844923</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>13.244722361559852</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>7.6447987791988075</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-175.12876750239579</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-9.586278323857707</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>34.467095090927138</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-42.953525052300435</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-148.10304731916546</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-2.346848665350977</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>19.222054012737729</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>1230.0544954901536</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-27.665417709646476</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-238.30541384557051</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-113.43193196534735</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-53.782724416131494</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-5.9861054847278181</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>44.69042850334683</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>307.69733489835511</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-249.78655376147782</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-104.76314235140949</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>365.4846250955037</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>4.0242885377417732</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-3.8592903734228066</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-72.678674240353828</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>48.301425001428662</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>82.898081873677711</Pillbug.EnemyNeighborWeight>
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