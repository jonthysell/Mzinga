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
<QueenBee.InPlayWeight>-1268.5186585802085</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>75.750588373559864</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-14269.364643124452</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-3473.6675290520207</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>9356.0713190189144</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-142897.68623268209</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-200627.88320259558</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-2587.7168712795324</Spider.InPlayWeight>
<Spider.IsPinnedWeight>805.35425279002459</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>1802.6645907877942</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>29097.759512935456</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>383.15176829141291</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>5398.4731733795</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-3227.1582515743617</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-80.087301570460028</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>4636.7913782375463</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>4329.6795092761186</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>445.09901408631958</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-187.50336922174927</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-3806.72847342561</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-438.95510644436183</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>2358.7191915820854</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-5026.4561261719509</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>5435.8646144954082</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>3555.4858473429945</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>3643.8551778730139</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-549.15443520415465</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-134.84618321989893</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>8227.2693843277921</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>159.67640424177225</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>80.730984998296847</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>84.251072945808559</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>2515.2887833820837</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-164.38600995261049</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-8208.9726448915171</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>2255.423636418896</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-3426.611950298402</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-267.00830278017924</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-7477.6339269388691</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>1154.4230370348882</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>46823.71823863496</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-751.62273806822452</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-5650.6760160045433</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-3048.1795894138431</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-3922.4436786962815</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-889.70414128665288</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>921.94989259092279</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>1526.5886176609442</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-3286.4070429436074</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-2193.2532843509553</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>2133.1824178125353</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>383.48301037753225</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-5791.8846395358723</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-498.75621477429831</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>7852.5415303013442</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>16910.774018234853</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+M"">
<QueenBee.InPlayWeight>-6479.2464301149694</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>34.656270706452361</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-53927.851508854059</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-3996.4581858652964</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>1649.5604354696654</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-218571.08103023595</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-500159.93248483154</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-181.82264702359922</Spider.InPlayWeight>
<Spider.IsPinnedWeight>3322.3589523985697</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>6420.1215594828936</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>52123.302906252975</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>731.4484423508386</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>6.8876201726078445</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-14580.83227781596</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-6663.6683775262482</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>8094.43457883784</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>8016.00302621949</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>2336.0936616300464</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-67.62926285681408</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-2381.2935526693427</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-830.38742968335612</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>129.07646052930338</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-14987.16492868643</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>1736.6011740258039</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>801.87134258306583</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>13175.426349195579</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-547.39478800531276</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-163.77652823843982</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>134818.85495977823</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>1647.9650070862401</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>135.43321656503508</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>2688.6033388279179</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>1584.3353402534242</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-204.68159990029523</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-662.30636535823919</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>2198.7565694901291</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-598.1800458598259</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-9699.4868904468585</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-3816.3551184369494</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>1560.0686823548815</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>65829.716192142791</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-2610.8285335303513</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-1862.8330256638983</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-3798.577696404393</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-1118.0481648860202</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-609.44480189626108</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>4955.9435356768627</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>2696.0840823420917</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-719.30528612687</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-4941.3051290090989</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>1456.0183060088011</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>314.78406897091128</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-15263.911565657514</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-454.20851028884573</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>732.45207399737853</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>2406.10919155985</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+L"">
<QueenBee.InPlayWeight>-2888.6213411244789</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>97.080973254497735</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-17528.875045838828</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-2171.1104200117747</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>13857.866199499315</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-39073.8007617732</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-645436.64146095351</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-3807.023170448886</Spider.InPlayWeight>
<Spider.IsPinnedWeight>1828.5834592222277</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>4568.3741231778795</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>26880.236651849034</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>497.75826895831551</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>2800.2020166900993</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-2535.8651460285355</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>2.3346615882909263</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>994.81696424491031</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>9658.01668744516</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>40.146878561445227</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-107.10962576595466</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-1519.1098651435086</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-428.8388663546034</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>6445.2820602882339</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-6834.0967548104591</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>2402.3423925570919</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>3435.870861090993</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>27036.428225525131</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-2215.8586767008264</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-276.13787673764324</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>3181.0882988720973</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>2665.5877394154541</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>68.077444470547718</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>446.51959867493053</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>4235.6996784521507</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-4066.0220217251849</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-2322.2848654071845</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>318.89157070504683</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-988.60472428296953</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-2053.925404598082</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-3232.8208952074315</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>200.99749034538453</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>1262.27002001377</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-3961.2473888947789</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-1736.6941644788674</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-4412.7325524238249</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-765.5920844704367</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-899.02796787717955</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>2214.3768337081497</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>15356.18611591298</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-7907.2659437467373</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-1361.4462450297476</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>623.13924370209759</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>946.6119967575039</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-29238.761395346974</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-288.25044756568769</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>3140.6368616248355</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>14159.077733684944</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+L"">
<QueenBee.InPlayWeight>-1256.1432692614069</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>5435.0433163398693</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-45459.535499099395</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-20311.484643287717</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>1487.0860117305888</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-10493.507759818993</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-170250.02862222027</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-13445.95991718084</Spider.InPlayWeight>
<Spider.IsPinnedWeight>14005.8735293475</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>895.30886002585532</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>24102.870592637089</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>2482.9765558540357</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>1955.2611567796275</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-13175.28434874529</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-6410.8243729735659</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>5918.4563369313491</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>1518.056764394373</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>1982.9337081983731</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-2.8927923175861863</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-166.17212198066605</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-1066.6811690385034</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>2558.7630392345977</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-3821.9098087968523</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>4483.6663824855805</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>2656.9664325832691</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>9559.993216939858</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-214.51387041103644</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-522.02997601975244</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>49564.052170882176</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>3985.0002409979984</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>452.46620745344148</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>25.624581114779982</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>1283.2716485062767</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-17561.392850802386</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-21.655975792425597</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>2291.8356552704372</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-4162.6075661021914</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-27792.02156217839</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-53.961847490301444</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>2476.8695264092</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>144571.72891980238</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-2643.9311912415515</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-8334.0756164634531</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-606.56144337237788</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-232.06002296637971</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-697.43058201263182</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>3358.4700912008361</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>550.51401276657543</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-25493.436377158996</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-7317.6875805884138</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>31473.011025991986</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>152.59008437937891</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-1044.9471335710202</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-3042.5633724341296</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>865.22087736919559</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>12108.504805735307</Pillbug.EnemyNeighborWeight>
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
<Pillbug.InPlayWeight>0</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>0</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>0</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>0</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>0</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>0</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>0</Pillbug.EnemyNeighborWeight>
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
<Pillbug.InPlayWeight>0</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>0</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>0</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>0</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>0</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>0</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>0</Pillbug.EnemyNeighborWeight>
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
<QueenBee.InPlayWeight>-1562.6683882286063</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>182.82459298085109</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-28810.469840488553</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-5014.224333568678</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>17139.017565894195</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-297918.65738732257</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-600985.56782633567</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-7413.2438164833529</Spider.InPlayWeight>
<Spider.IsPinnedWeight>4182.7424222006148</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>21436.801863251814</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>18931.471057133676</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>1728.3239296704371</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>1704.1332874872403</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-828.78741417588208</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-14582.301858439963</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>34121.889606780926</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>2974.5554691230659</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>375.83296409371781</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-726.759688031548</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-4682.8020045698386</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-4496.81515349777</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>1237.4162814337126</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-19319.027865847991</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>2715.5823554334088</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>3477.2978830993516</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>94013.870079270026</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-170.5561359096414</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-1498.7537349438126</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>31913.197032684886</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>7894.0077511131894</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>229.85475171853506</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>1315.0507936067213</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>10054.916946328072</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-1841.3436464605552</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-104394.75304653328</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>2722.0569486481259</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-863.96868911932427</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-4897.2460065320165</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-8507.5719571175723</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>122.11212223743294</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>135652.62229448979</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-779.31657571878054</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-5558.1462432352164</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-15698.019000722821</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-3092.3347173742395</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-5619.407018876188</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>7699.0299728222026</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>8632.2196886006041</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-632.88757748422483</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-7.7134033556309758</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>18191.91588760294</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>291.03644807264749</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-83562.794312796177</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-3367.8191156465505</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>5173.5107862330615</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>13755.70174256582</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+MP"">
<QueenBee.InPlayWeight>-73.538212301446933</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>700.14175833838055</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-10660.939399766003</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-1407.879010879479</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>1402.3831785946966</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-27698.313322025871</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-926637.269079423</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-3224.1461664455637</Spider.InPlayWeight>
<Spider.IsPinnedWeight>6519.9433763714205</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>3753.4624782449114</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>369.13662795872534</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>501.94364525251365</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>316.6765078463701</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-1032.6938232760592</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-1417.4940352708943</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>857.80094285966311</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>858.68625275191971</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>339.39473440350753</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-42.858030783492651</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-601.74249271343308</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-1116.5541454895024</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>3189.404328225864</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-494.6780311867044</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>8237.8867879048285</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>1057.6324369120985</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>21376.04078824661</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-535.70689620681833</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-304.74579529940644</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>3018.59695130744</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>0.26731118201808596</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>1.7449855095489624</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>152.10204075600296</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>2698.0047944862595</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-1082.2987410788448</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-1465.5255935281193</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>414.06009442745494</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-1925.401717512297</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-265.17454563065104</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-3237.5924046077644</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>335.88325241198089</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>7920.26791839217</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-664.91663712814545</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-2282.4250204948735</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-6293.1083077772528</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-2341.6412787745903</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-412.14123971561867</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>4202.8297018651338</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>56469.257763333779</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-2041.2265639256173</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>7.456233200223795</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>1890.3248394994071</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>66.785725837339768</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-6229.0986006476251</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-306.43106994084786</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>1776.4076351701845</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>3561.7956245568271</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+LP"">
<QueenBee.InPlayWeight>-2120.175465212149</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>1504.9005026930051</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-160570.7810378685</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-2951.079044932304</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>47068.00395492954</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-365737.99232998129</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-925055.36847620062</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-7711.0260655097818</Spider.InPlayWeight>
<Spider.IsPinnedWeight>41455.074060684929</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>9142.8571796049</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>31322.285965922409</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>34281.664841453938</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>24106.738771087312</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-1271.3713914591287</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-182.71748305083688</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>68492.231506326047</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>7565.3040623805609</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>389.77023648497357</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-649.34042488320119</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-5958.4804978413431</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-7492.4255123295652</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>40597.906796897</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-28849.556151532455</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>1930.7752599133955</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>5531.0871216224323</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>21489.905560300624</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-10678.676440800411</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-340.43232293872165</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>15951.56076618203</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>3268.0989186900792</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>210.72162861907017</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>2045.3491150348862</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>6688.969502495931</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-20104.035958323017</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-24185.806596195591</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>1979.1182315726119</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-4741.8534626131222</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-5667.9636198853605</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-7409.8513505921856</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>509.87049233958368</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>170768.92855386162</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-309.26123107747912</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-342.70465969580692</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-3884.2722133356338</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-8869.0212368656066</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-17308.41298773844</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>1883.2451277219834</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>97201.732844685423</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-14825.822555492465</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-12872.278408750255</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>52872.608371827613</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>2205.3549240327675</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-93915.638920218844</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-2194.8957696366306</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>32288.649322076257</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>55803.82678130678</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+LP"">
<QueenBee.InPlayWeight>-6639.7163780844985</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>1589.1224515675722</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-179741.6892222988</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-12816.83779945893</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>2895.6591544124913</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-144330.35012589608</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-942211.7715028635</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-81528.903739536225</Spider.InPlayWeight>
<Spider.IsPinnedWeight>5672.1575267832613</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>13037.146657548236</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>51508.476282613759</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>2159.5537603953671</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>9641.7809714454688</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-48.299019843644423</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-16960.667034403672</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>25007.704891149151</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>5386.6210048728017</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>10995.296774676812</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-95.834177960504746</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-3667.7741543485704</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-1673.3125625001187</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>23614.004290313118</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-4288.3444512642927</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>16020.173072670379</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>7728.4805531490711</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>12171.072298186202</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-839.07192026524683</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-351.57090343254805</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>54379.115428970996</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>7907.9820553807249</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>370.2958741667743</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>808.22077194219423</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>6029.2782787256874</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-19798.610354383767</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-23386.941887926223</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>1279.1212764370307</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-4187.8977400519261</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-7629.1144692097087</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-917.21971683645529</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>4094.2013872696421</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>108140.88181086414</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-1008.91778487467</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-20326.362467363902</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-1840.8331471177146</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-13630.91337000185</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-4277.7916242415067</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>6479.2344824517941</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>185537.38642859767</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-30041.482528945573</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-16835.249756262921</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>33919.580888256918</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>1000.3404743392473</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-7109.5099143443322</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-395.93373667163058</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>5005.8178562923149</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>2837.6543221806523</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+MLP"">
<QueenBee.InPlayWeight>-3457.4262661728194</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>1423.7171737996473</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-13285.301896980311</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-7922.6245006319486</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>8072.9126313138031</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-161936.00538285638</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-731156.67876367038</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-5243.870026734704</Spider.InPlayWeight>
<Spider.IsPinnedWeight>12219.143664951096</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>5512.34535623133</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>60650.020408463395</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>1344.7700039689289</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>10556.352239683001</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-28047.495095313472</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-1755.9941559189974</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>10429.986943767866</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>5159.3663872081634</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>676.76213207123908</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-46.98185184551722</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-12505.160885555089</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-3668.7884418380954</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>5978.2204034517754</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-14598.25772112813</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>31502.264986430728</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>2665.0325807257964</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>16359.386202932286</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-1257.285557584644</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-3804.4431940584736</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>6856.4812820805264</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>3071.3113370700162</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>172.85111728340439</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>120.69024252666171</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>1026.4282198036872</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-10629.549357012054</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-17057.228641441394</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>14154.392800814345</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-7309.9405348749224</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-30173.080266241013</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-1284.0810133033995</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>4197.0400942910828</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>62238.910132193334</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-2324.3817882165226</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-1849.9126123192873</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-4031.4987793484188</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-16136.37791958096</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-1131.5133795046775</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>4379.0474838003829</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>41997.019485766381</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-15296.827637456743</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-6814.2782802380689</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>41154.428471547442</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>398.17178721657103</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-13548.215543468659</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-41990.970289396435</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>1050.2419115905145</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>-0.97773812145820738</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+MLP"">
<QueenBee.InPlayWeight>-1863.7724612596319</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>2206.0812202065167</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-19914.203696074863</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-1426.3901205231155</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>389.34482789368826</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-32223.250821950918</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-778468.90319817187</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-9513.8709300191367</Spider.InPlayWeight>
<Spider.IsPinnedWeight>3465.5180434996905</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>4041.3895067791054</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>21104.411373505827</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>176.30984142756782</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>1191.1885172324483</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-9884.0676428007446</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-139.09612018541023</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>1252.1628283130765</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>903.44782454822121</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>799.53733873213753</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>5.7954610754995946</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-658.60457736968283</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-913.98507461938311</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>4.0127591026307829</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-81.010298122312079</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>4031.5618914023676</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>1714.1652133924724</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>5731.5129071713027</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-631.5870234006768</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-64.91920479619607</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>16810.222558557391</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>2099.7126655632924</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>-49.243910531191624</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>503.602547615046</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>626.71609702854232</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-299.81056113073868</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-3337.4868418920355</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>2461.8791231026194</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-3928.677692400398</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-9156.8224061099736</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-36.097214815410425</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>491.90117315779491</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>68392.090240330232</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-412.33301307320977</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-3033.6348995520084</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-722.13940408543658</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-2498.6016142397939</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-134.67268566512627</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>2508.7557566476826</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>16603.960617654295</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-7363.0506412663472</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-1921.909653269518</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>14749.536434406455</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>399.08308978835259</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-6417.4018954350113</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-612.92745028759248</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>961.40495383948553</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>7228.9224937329827</Pillbug.EnemyNeighborWeight>
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