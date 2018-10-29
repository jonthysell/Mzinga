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
<Mosquito.InPlayWeight>27845.474069835822</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>2505.7914304812489</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-334.75458655812514</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>20319.982496006742</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>-583.53128834112169</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>62622.768164149995</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>11424.059385783276</Mosquito.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+M"">
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
<Mosquito.InPlayWeight>8952.585135456211</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-6389.1504996360363</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-14440.893776309902</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>28648.146671316023</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>3799.6093259948184</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>36791.400703680869</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>3739.3850594645955</Mosquito.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+L"">
<QueenBee.InPlayWeight>-29.345688872649653</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>9308.7201970222977</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-9690.7024962726064</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-5866.8271450848006</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>73926.38796703596</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-52631.169340364861</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-129029.26387723035</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-2560.6927449615405</Spider.InPlayWeight>
<Spider.IsPinnedWeight>7039.4793774670434</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>6377.9210792659969</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>1165.7182785002881</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>1139.8288831859472</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>1599.2801810776812</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-14568.017122963614</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>82.12475436096301</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>1094.6697812057753</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>10193.768801330509</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>205.64226711807879</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-27.055615898112588</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-1531.0650553056903</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-1248.1734088691526</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>5031.396248849911</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-5984.4732044835864</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>2650.4708791975286</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>2465.2631540991729</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>25.540303366992692</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-6588.4815421865815</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-2728.4264497195445</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>19688.162732310404</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>1472.5856974849412</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>21.675204570305166</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>268.68710278206839</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>7697.8925561767783</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-1055.8432754878784</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-19290.198828455563</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>677.32002908669176</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-2078.3950201801</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-533.64352729119287</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-2981.2989836306165</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>177.691798150652</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>49562.494502018963</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-11.147127061566394</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-3841.5997796117817</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-194.16448640340218</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-8096.7878923218468</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-6309.2671598635116</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>648.31608979973987</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>11041.888920211353</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-9487.3678427225386</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-8061.8340946800063</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>8960.6338589190236</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>0.41178290211406354</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-2133.4480706984587</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-9516.5032264021138</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>5881.9630173899577</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>12265.382872621871</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+L"">
<QueenBee.InPlayWeight>-501.72988101883323</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>1104.6954159033558</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-96829.7948953588</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-70052.646891794575</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>5003.055192697353</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-42036.138976224087</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-176667.71989086072</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-73559.659365800631</Spider.InPlayWeight>
<Spider.IsPinnedWeight>342.42408574628837</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>9270.0101542345856</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>47801.112806336052</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>4245.712157378247</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>2536.8319048969311</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-18447.073110377569</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-26110.012292246014</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>5176.6060576177788</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>5687.3270546805352</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>371.57117028005945</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-39.984340457496863</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-2054.2665777314996</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-444.15606380770726</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>2093.9625084654417</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-5722.6933911158812</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>7578.4463645652095</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>7731.9691452911811</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>11195.732577382392</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-1501.1476541269326</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-10.727619644136922</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>125389.35161265085</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>12863.867325076944</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>6.3325045569578675</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>2999.5983311101068</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>2982.0677907332165</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-4654.2307354041523</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-13002.940181192895</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>12862.379442917776</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-7681.912855924812</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-41919.080948950213</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-2050.8099668058849</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>4720.1975231769338</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>57528.462251117126</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-509.81201546852105</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-5507.2145075528388</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-1665.5220844832491</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-8385.1407746688474</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-2649.9954009083262</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>4563.7961613236166</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>26778.304654776188</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-102074.60791338283</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-8640.06043317302</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>22526.098594516356</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>884.18772175768993</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-6211.3429604947542</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-2220.262479103716</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>1632.5868239090114</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>7814.7287730945218</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+ML"">
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
<Mosquito.InPlayWeight>27845.474069835822</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>2505.7914304812489</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-334.75458655812514</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>20319.982496006742</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>-583.53128834112169</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>62622.768164149995</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>11424.059385783276</Mosquito.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+ML"">
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
<Mosquito.InPlayWeight>8952.585135456211</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-6389.1504996360363</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-14440.893776309902</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>28648.146671316023</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>3799.6093259948184</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>36791.400703680869</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>3739.3850594645955</Mosquito.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+P"">
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
<EndMetricWeights GameType=""Base+P"">
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
<StartMetricWeights GameType=""Base+MP"">
<QueenBee.InPlayWeight>38068.098587578461</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-8742.9537907032263</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-28974.8883848267</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-4489.5371097897569</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>-82990.292024427632</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-240363.5975930962</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-548379.74539775448</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-40161.836601183168</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-6621.5469264523681</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>21285.337788905526</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>35368.103579170034</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>9401.8772550616486</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-35484.80686940506</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-18005.473095360223</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-57332.95853608477</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>40186.589790795777</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>2979.8686213298643</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>2301.9409390879387</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-42778.190465709449</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-60347.999487854941</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-10412.907742817615</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>-14316.807048535911</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-32946.00071706163</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>2811.4662321043606</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>43691.16382339687</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>37391.397300487479</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-21528.17369469894</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-29186.02410663191</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>82696.810734737723</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-7980.82786795498</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>256.23538006136465</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>31884.607113503134</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>-16151.936651356269</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>76522.009950815453</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-130833.84483184198</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>25422.0667562016</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>11568.199072158534</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-4904.6595940085481</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>32037.150522334345</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>-5871.424139291139</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>161109.90907959969</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>34202.42169656694</Mosquito.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-31629.488831872713</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>881.91670619681952</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>154.89443007965363</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-70331.275872456725</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>29873.253940280796</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>-37068.111142846647</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>5539.4447961092092</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+MP"">
<QueenBee.InPlayWeight>8862.06246309242</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-31871.398255539923</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-12628.530754608002</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>5197.4743877560632</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>41881.582712408068</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-74235.835842974586</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-798295.28576368489</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-19804.0197602826</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-419.432365956674</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>1870.4815496029414</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>28457.767696908988</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>21210.041156110128</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-30446.049206096541</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-21652.950080982322</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-30515.598624715039</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>6119.3159508961708</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>115.39381821364628</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>21935.851460414557</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>12155.979310409195</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-48141.680131773021</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-11989.40364523473</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>-1800.274234173296</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-15945.569611566387</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>8779.9978024636112</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>63166.7874078684</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>-15335.161776297811</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-23127.659965863444</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-55648.38830686346</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>14247.620942787431</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-8580.9382952130527</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>-396.92939179959</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>59823.026369923253</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>10451.114321778734</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-18537.414484182111</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-36222.563106545233</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>4937.4476840884208</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-1033.9198062259375</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>1373.7742224193326</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>47606.0466547042</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>1760.7060788361596</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>-14468.144094533211</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>19141.498750303876</Mosquito.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-9020.4506461195269</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>-9218.3142197012949</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>-1545.7037728184237</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>22267.384132943338</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-24705.257299683235</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>-12775.645977780729</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>-4880.9115431483106</Pillbug.EnemyNeighborWeight>
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
<Ladybug.InPlayWeight>-35097.410267844018</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-38761.749983930764</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-1756.237604628298</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>6207.6445894949584</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>4066.7055101837996</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>-11979.593433642536</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>2070.5520767861481</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-7549.4775370743619</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>2627.2712825371068</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>-4.6158031086926163</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-17112.853662862108</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-1762.0343061818824</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>1779.1103645430176</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>15711.121587210604</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+MLP"">
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
<Ladybug.InPlayWeight>22583.682123986931</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-56189.7180922671</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>3397.5610061212192</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>24083.558902846144</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>523.55897952097064</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>57879.073981381931</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>12450.107223164992</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>8380.9021593403068</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>-3190.3598833314636</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>-556.93035777941839</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>551.62836556447519</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-1191.30038667527</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>722.197604811424</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>51605.21317328867</Pillbug.EnemyNeighborWeight>
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