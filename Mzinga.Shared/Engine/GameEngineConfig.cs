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
<QueenBee.InPlayWeight>61261.984456958373</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-8191.6303775395354</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-102393.89455279487</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-5641.553783627528</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>-127016.81434594907</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-272257.85688403429</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-200665.72957169174</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-40330.153397626855</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-25244.80896291745</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>17404.708071329955</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>76170.5839915496</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>11654.014285480384</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-29555.665918432049</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-20754.634657979852</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-83817.447562581568</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>16182.997180359724</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>5462.6616986115159</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>18723.202467507952</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-8814.5804013798279</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-100848.8499455758</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-8514.8321209129772</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>-717.58326851732784</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-16332.14545856264</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>7611.4873021155927</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>67951.382095092078</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>17233.251273353293</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>20602.807195447669</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-47642.803888157294</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>106998.378929993</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-42716.736393394349</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>1278.2797294560869</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>10720.031254798147</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>-3219.442682797132</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>95545.110815220993</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-78302.818786524789</SoldierAnt.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base"">
<QueenBee.InPlayWeight>15974.860082147688</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-98405.941147851641</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-21931.401957934311</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>6105.2468837567967</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>139756.46084902741</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-188252.51745108372</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-617844.497265261</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-36582.978895619068</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-20779.8869272696</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>-575.77973010534367</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>112152.26426264294</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>-20168.562887234239</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-174.7992339679646</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-57887.798928008968</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-34217.453517117639</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>-41160.579591988651</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>-8796.3146856403018</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>128411.29824023324</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>35920.147538874371</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-33608.628255658034</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-43840.712748901213</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>3464.5619056992218</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-51126.338628558944</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>15431.869219278315</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>137893.46519784</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>-4387.8856613267681</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>5483.3154800032144</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-64716.592343835262</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>31374.329138324367</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-20052.813175138635</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>10619.824828734336</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>54702.653490312747</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>-13.6438831558514</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-38625.118676344508</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-72539.934786845377</SoldierAnt.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+M"">
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
<EndMetricWeights GameType=""Base+M"">
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
<StartMetricWeights GameType=""Base+L"">
<QueenBee.InPlayWeight>30058.771147557862</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-22471.014213363655</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-17567.282198522254</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-2263.7192455292243</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>-50871.321233979907</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-45084.011603316896</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-561275.53271563107</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-69172.725029019464</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-11262.15031279518</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>4321.9579573605815</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>55362.015258152394</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>42492.015405629143</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-77559.88799391342</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-14181.551965310677</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-96072.9916041514</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>15277.04159570121</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>9603.4093033241243</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>1673.69363182359</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-42100.919590670463</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-122466.95448922874</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-11717.211995150679</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>-29016.140391521745</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-31648.926754806937</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>2670.0632967110241</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>64466.393375787258</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>12010.146781063744</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-31774.158227342723</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-24064.331645557369</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>146566.99051213198</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-40531.971605732833</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>215.94882327518693</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>64577.343310324242</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>-8454.9051502312832</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>73577.866644380032</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-17327.524746205727</SoldierAnt.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-29498.958065743103</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-31272.737140639972</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-732.785096178455</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>44212.954005289736</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>-3435.9869051145884</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>-24508.017865163823</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-29757.627499720777</Ladybug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+L"">
<QueenBee.InPlayWeight>6810.3030851414351</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-68470.554560046992</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-48113.749808322988</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>7778.2848270123786</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>94010.831085791826</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-111353.46168544678</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-212719.45169446047</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-37396.961010914267</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-811.9386278534073</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>-2284.199156108979</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>46389.889745430657</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>5738.1340432872375</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-4943.8167153450813</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>10584.843866904574</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-48417.283606437617</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>10040.911884454405</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>2759.775705739075</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>15985.653896292131</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>18944.657571721949</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-27465.028017171288</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>26047.491317695571</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>9119.4734420583245</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-44135.290834819592</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>4390.5139102449875</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>55444.302303069853</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>4011.4752813519985</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>44816.911787529541</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>10690.941486977165</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>85586.958883109037</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-8898.7935327294563</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>1034.8081819335596</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>-26091.221677380378</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>7925.7545003906962</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-2111.6516562658512</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-3578.9126569927707</SoldierAnt.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-4092.3010981134144</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-16154.963935695378</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>1951.9837977441134</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>23267.115796249589</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>12996.441933271799</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>10988.108517114268</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-27143.0697476885</Ladybug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+ML"">
<QueenBee.InPlayWeight>75387.19587895622</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-51805.096191315039</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>162.84462196040545</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-71872.906642074624</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>-63789.570886133079</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>60156.480368511584</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>2159.6375025311559</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-211567.24203062186</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-90658.725092065288</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>855.288668065932</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>159024.75991242504</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>109762.33656541766</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-132123.64568887965</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>9741.83960261388</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-570619.89135280007</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>41413.185171138895</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>-76.67487445569509</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>1260.6910476338844</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-123996.23531608887</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-562404.336122139</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-19487.7189005771</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>6215.4465624114828</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-53899.13802492719</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>1237.0150480408208</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>92878.315839467308</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>5298.8868766890346</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-40314.71057291879</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>739.08036349272913</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>227979.94470841374</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-86974.551341512721</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>-544.01000045163892</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>-16968.603108087147</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>-1308.0562317548449</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-34116.346069130232</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-103853.05059184412</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>199318.93043355391</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-10102.47024507421</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-333.14130497603367</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>4023.0422526085977</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>-326.74851325389147</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>-22877.917337244933</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>45665.420053426511</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>23834.997503346021</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-50184.636385975929</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>259.89709933033532</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>25746.818740300438</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>-19158.087807140037</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>757.95374047029213</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>12315.615032284775</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-79227.137538782641</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>17154.986805267985</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>-94.333588957634149</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>26689.451415634736</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-3615.3322842401144</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>-83371.361644608</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>55757.6006609717</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+ML"">
<QueenBee.InPlayWeight>22880.62473359779</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-97508.3918069251</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>7812.0671573234149</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>97664.101102262764</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>126466.31663517196</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-399200.05541037116</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-520438.95559239091</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>68468.375400024743</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-27458.064271514857</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>7235.8507297899305</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>167850.98297046204</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>-28113.773324575737</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>26690.625067014105</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>59493.0322426666</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-81841.959435051918</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>-5964.4266546515864</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>-9348.104270495578</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>125950.62754693662</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>10204.30965788948</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>72544.728917347355</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>56159.316605823624</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>95936.258733905575</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-58384.927360311682</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>22767.894444753023</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>90429.627035226193</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>-1961.146009162419</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>9793.0056659224156</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>54883.490310575791</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>18722.226204929571</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-63213.579519100771</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>-11200.22697047853</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>62669.174000734842</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>1945.6212601485966</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-5058.6194115549652</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>52016.420995133369</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>52276.042149226778</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-50765.541490797106</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-8799.2503411203561</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>18200.941929262823</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>2893.6144605877676</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>21057.504356784284</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>26057.313194343078</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>139443.33471936954</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-104288.87898873218</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>7019.0398260646889</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>4184.2331613179877</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>1062.0393232042165</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>49018.401626800922</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>54047.656385919923</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>10584.710288574206</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>-11729.783774419675</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>-3868.2224560318382</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>41095.255728959644</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-2805.9715804826646</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>71499.8962166</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>80188.945274004727</Pillbug.EnemyNeighborWeight>
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
<EndMetricWeights GameType=""Base+MP"">
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
<StartMetricWeights GameType=""Base+LP"">
<QueenBee.InPlayWeight>-6.0178794817896</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>4.4309230412034895</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-109.10459066343708</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-8.23408902316079</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>45.387671616947117</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-588.10513826600527</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-3835.5966605411822</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-26.419570243430591</Spider.InPlayWeight>
<Spider.IsPinnedWeight>1.6423540511482182</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>16.565243152624575</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>183.01203622171283</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>3.5843465056197465</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>58.820145911742081</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-11.119437580839469</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-1.0226613562519948</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>56.192331568930456</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>1.5208163316947485</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>3.8941805913784453</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-0.50286248188599125</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-10.388010228300935</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-12.990342455627557</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>12.62417274058758</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-58.762755313577479</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>104.12232336741982</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>13.108233170764628</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>154.96735683023343</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-5.186108777653943</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-4.1009274763385433</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>9.7867786103611714</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>4.2926754282380815</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>0.39519165051877109</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>2.8612704676628442</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>31.034645582211496</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-3.0410046940534396</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-162.05160234103053</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>14.893355526436752</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-28.512630338833031</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-6.5920126389488631</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-30.522394396794212</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>10.165337523380451</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>151.33787147736078</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-16.829603095855379</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-14.666721497879701</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-21.001644490147779</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-13.487218507429221</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-4.182367361515</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>24.774119294310978</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>106.87096318815415</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-42.326260428864629</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-0.80660169131866855</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>280.451146490288</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>3.34195832929898</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-15.607639598861633</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-10.156675325509477</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>6.8246036353449346</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>158.80183016264434</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+LP"">
<QueenBee.InPlayWeight>-6.0178794817896</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>4.4309230412034895</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-109.10459066343708</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-8.23408902316079</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>45.387671616947117</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-588.10513826600527</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-3835.5966605411822</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-26.419570243430591</Spider.InPlayWeight>
<Spider.IsPinnedWeight>1.6423540511482182</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>16.565243152624575</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>183.01203622171283</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>3.5843465056197465</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>58.820145911742081</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-11.119437580839469</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-1.0226613562519948</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>56.192331568930456</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>1.5208163316947485</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>3.8941805913784453</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-0.50286248188599125</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-10.388010228300935</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-12.990342455627557</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>12.62417274058758</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-58.762755313577479</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>104.12232336741982</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>13.108233170764628</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>154.96735683023343</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-5.186108777653943</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-4.1009274763385433</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>9.7867786103611714</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>4.2926754282380815</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>0.39519165051877109</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>2.8612704676628442</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>31.034645582211496</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-3.0410046940534396</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-162.05160234103053</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>14.893355526436752</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-28.512630338833031</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-6.5920126389488631</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-30.522394396794212</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>10.165337523380451</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>151.33787147736078</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-16.829603095855379</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-14.666721497879701</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-21.001644490147779</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-13.487218507429221</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-4.182367361515</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>24.774119294310978</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>106.87096318815415</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-42.326260428864629</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-0.80660169131866855</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>280.451146490288</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>3.34195832929898</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-15.607639598861633</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-10.156675325509477</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>6.8246036353449346</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>158.80183016264434</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+MLP"">
<QueenBee.InPlayWeight>75387.19587895622</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-51805.096191315039</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>162.84462196040545</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-71872.906642074624</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>-63789.570886133079</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>60156.480368511584</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>2159.6375025311559</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-211567.24203062186</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-90658.725092065288</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>855.288668065932</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>159024.75991242504</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>109762.33656541766</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-132123.64568887965</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>9741.83960261388</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-570619.89135280007</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>41413.185171138895</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>-76.67487445569509</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>1260.6910476338844</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-123996.23531608887</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-562404.336122139</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-19487.7189005771</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>6215.4465624114828</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-53899.13802492719</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>1237.0150480408208</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>92878.315839467308</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>5298.8868766890346</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-40314.71057291879</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>739.08036349272913</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>227979.94470841374</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-86974.551341512721</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>-544.01000045163892</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>-16968.603108087147</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>-1308.0562317548449</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-34116.346069130232</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-103853.05059184412</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>199318.93043355391</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-10102.47024507421</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-333.14130497603367</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>4023.0422526085977</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>-326.74851325389147</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>-22877.917337244933</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>45665.420053426511</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>23834.997503346021</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-50184.636385975929</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>259.89709933033532</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>25746.818740300438</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>-19158.087807140037</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>757.95374047029213</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>12315.615032284775</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-79227.137538782641</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>17154.986805267985</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>-94.333588957634149</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>26689.451415634736</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-3615.3322842401144</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>-83371.361644608</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>55757.6006609717</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+MLP"">
<QueenBee.InPlayWeight>22880.62473359779</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-97508.3918069251</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>7812.0671573234149</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>97664.101102262764</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>126466.31663517196</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-399200.05541037116</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-520438.95559239091</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>68468.375400024743</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-27458.064271514857</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>7235.8507297899305</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>167850.98297046204</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>-28113.773324575737</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>26690.625067014105</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>59493.0322426666</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-81841.959435051918</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>-5964.4266546515864</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>-9348.104270495578</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>125950.62754693662</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>10204.30965788948</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>72544.728917347355</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>56159.316605823624</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>95936.258733905575</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-58384.927360311682</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>22767.894444753023</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>90429.627035226193</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>-1961.146009162419</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>9793.0056659224156</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>54883.490310575791</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>18722.226204929571</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-63213.579519100771</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>-11200.22697047853</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>62669.174000734842</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>1945.6212601485966</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-5058.6194115549652</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>52016.420995133369</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>52276.042149226778</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-50765.541490797106</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-8799.2503411203561</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>18200.941929262823</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>2893.6144605877676</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>21057.504356784284</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>26057.313194343078</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>139443.33471936954</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-104288.87898873218</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>7019.0398260646889</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>4184.2331613179877</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>1062.0393232042165</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>49018.401626800922</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>54047.656385919923</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>10584.710288574206</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>-11729.783774419675</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>-3868.2224560318382</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>41095.255728959644</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-2805.9715804826646</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>71499.8962166</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>80188.945274004727</Pillbug.EnemyNeighborWeight>
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