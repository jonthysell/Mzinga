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
</StartMetricWeights>
<EndMetricWeights GameType=""Base"">
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
</EndMetricWeights>
<StartMetricWeights GameType=""Base+M"">
<QueenBee.InPlayWeight>64059.78244019331</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-6250.5764355827123</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-101859.10488097466</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-6943.3244572485628</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>-126605.38031020798</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-238709.70221242763</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-178911.96951035498</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-37594.741126055465</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-32325.890888833019</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>17534.352451414685</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>84022.050078055428</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>8958.0333966413455</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-23733.485704680657</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-23946.520949579444</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-94801.525138531753</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>17077.340869258518</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>5442.1046853093812</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>19020.675879184215</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-4235.8301223753742</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-114174.31772467693</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-6062.9874420148408</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>3625.422545646471</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-16950.077084741428</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>7989.8780499895111</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>68281.257109664992</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>11601.368439165002</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>26655.001622295273</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-58093.322661714927</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>112865.36438198024</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-52257.988320094119</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>1507.4935481330194</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>19026.555229606467</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>-4002.8803806847636</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>75523.5066364839</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-86690.528026801665</SoldierAnt.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+M"">
<QueenBee.InPlayWeight>17405.347119840742</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-100234.74508323276</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-14455.148613562573</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>9320.7338726410926</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>134820.97634610976</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-202919.63258140741</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-567715.26101538411</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-27491.560179029068</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-40186.495611157654</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>231.13918520295817</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>120335.59921698205</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>-13478.527460485086</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>26007.60863326244</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-59621.47960361591</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-31460.240512919605</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>-41170.583569577786</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>-6612.9293364019522</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>135641.20377961287</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>21286.057518808371</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-9712.0335331095466</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-31472.843387572466</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>3838.3134995050236</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-43701.58494355077</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>15882.464271749324</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>132247.09197938826</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>-3960.8786026431876</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-1210.6842636107342</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-69515.346692042673</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>29640.156020143175</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-24985.752733465742</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>10445.045261281604</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>45897.14962422377</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>9068.8347927924769</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-31837.986110977981</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-72758.204021972211</SoldierAnt.EnemyNeighborWeight>
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
<Ladybug.InPlayWeight>-2860.000150988225</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-17274.652661099277</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-1515.9964305012263</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-3715.9466553908587</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>1816.1967749200665</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>17502.495528113053</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-3285.1270609933558</Ladybug.EnemyNeighborWeight>
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
<Ladybug.InPlayWeight>-4332.9446957843411</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-5755.24239665745</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-14711.160168509019</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-3267.9163886370288</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>2573.6743922436153</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>102254.81910237302</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-41834.819638065004</Ladybug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+ML"">
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
</StartMetricWeights>
<EndMetricWeights GameType=""Base+ML"">
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
<Pillbug.InPlayWeight>-8313.88974419674</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>38673.973849618618</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>222.23756756526225</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-13070.606991504526</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-1156.6141556723287</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>11675.0719761247</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>613.20695251393215</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+MP"">
<QueenBee.InPlayWeight>-457.35401742487863</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>5551.00047282498</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-9254.8475079569453</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-4935.453499084485</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>65770.751931144638</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-322675.52227749175</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-971513.68912053935</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-5745.6457648638479</Spider.InPlayWeight>
<Spider.IsPinnedWeight>11719.755026133071</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>1787.5603369500618</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>651.73825681724861</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>2604.5820797239535</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>2661.6961980172669</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-2707.4575419167418</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-66.7486682717351</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>7413.9136256522943</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>5825.6984032748414</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>263.34526266271064</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-174.95988790965174</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-1906.4287508488928</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-2264.8768498231757</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>11395.622461851779</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-14689.678878882392</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>1973.3381275819979</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>6669.5687129663411</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>9024.61531605787</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-10343.810012429834</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-385.02043974820742</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>314.49033309957872</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>115.23137262623348</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>13.735672262883261</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>269.04501655792626</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>11105.210072282762</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-1196.7173379054971</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-27438.070284455996</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>1661.1548239992303</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-860.92125865564276</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-3383.4935934221776</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-11053.803633289468</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>191.5347701399985</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>111192.68957489515</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-205.30822057672228</Mosquito.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-7204.87163192838</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>3335.6582831614683</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>59.15547520774944</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-421.94507805651568</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-1276.5077182512332</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>6574.9746477433937</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>14772.41665505657</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+MP"">
<QueenBee.InPlayWeight>-14996.142131118068</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>11403.377698081458</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-434356.42809435813</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-60152.609597176481</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>672.82973163028817</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-175416.99625916034</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-314133.1683197213</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-121687.84869001528</Spider.InPlayWeight>
<Spider.IsPinnedWeight>30952.353511690049</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>13527.621397357258</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>19562.921514216087</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>3448.748669341529</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>6633.2375032369582</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-22727.685510287472</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-89793.143390732817</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>22564.917864833333</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>8142.938286862498</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>7721.1930945800514</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-22.391524616666032</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-6591.7254760930764</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-1082.5904724575778</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>7869.0203054953054</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-14853.163599422474</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>5492.6568393945017</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>33629.925768907342</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>17096.38524519292</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-745.10819238550528</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-831.60615386539553</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>26632.018369699235</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>66.171944945562586</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>806.93436453603056</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>2864.0803167662707</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>3729.0250121731456</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-21034.676063196548</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-7741.3261158872419</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>7170.9542299109753</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-5845.1274881605677</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-31523.971981752671</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-1950.2090742802807</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>5101.382535183644</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>186216.99260876741</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-2252.1755421761568</Mosquito.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-62767.120348075667</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>70863.72180703815</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>2086.2250184507029</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-20704.300380879249</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-16958.593779054703</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>7145.6012209930414</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>17477.201869702854</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+LP"">
<QueenBee.InPlayWeight>-276.319347639189</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>1169.637718238743</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-7799.9176802433376</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-6641.4694444551542</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>8425.4837113311314</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-90497.520265187763</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-226539.50261769496</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-616.06366895083715</Spider.InPlayWeight>
<Spider.IsPinnedWeight>4225.2094173520427</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>8847.5549602714073</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>98141.987658793238</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>8722.5615126930843</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>6900.978967979564</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-5185.3292175134893</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-1145.1420030681122</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>16612.056443037287</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>1774.5534305191802</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>3378.6987228853836</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-443.93121307902368</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-18167.042142838109</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-4697.111101180828</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>343.83777987605743</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-44846.86302802404</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>2060.4696563787761</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>1784.0315791031908</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>8801.1638634224837</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-14006.735164601223</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-1281.7204938977593</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>961.2063209148763</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>32243.963491097984</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>241.23335757499098</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>2126.3379985840515</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>8858.823789207976</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-22167.092315176353</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-128528.60593891531</SoldierAnt.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-2796.6864553073387</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-1477.0478853985364</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-5227.67321677771</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-648.68857979666768</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>9264.6117107500049</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>62401.126615311172</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-13394.561476933188</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-739.66145689529912</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>16619.649153883231</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>14.717732507375153</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-4797.9333067978623</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-16638.779450652033</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>12213.302286001765</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>3147.7773477027304</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+LP"">
<QueenBee.InPlayWeight>-438.62691135378577</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>2395.805436511529</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-11104.276455921783</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-834.08210299355335</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>6874.8823944428914</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-197622.0107155375</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-830395.3530973267</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-7135.5270572478885</Spider.InPlayWeight>
<Spider.IsPinnedWeight>6868.9013694044652</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>3736.12221652875</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>20230.022994144907</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>4422.8713896784147</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>20232.111846823616</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-4481.6298733597932</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>0.12567158011382357</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>14209.643755633169</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>2391.517153757015</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>274.2524146667385</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-307.32906526472112</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-3743.3595589593929</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-11277.642421676719</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>3115.2725946302789</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-7145.6136845831279</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>12780.208272990973</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>10027.440102688213</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>7908.4326990703257</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-1655.3209251449798</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-1221.7450226104304</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>1470.2672128475331</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>1165.7114858294128</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>78.086704785854025</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>0.70810068998429965</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>1599.8770715496398</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-11173.254941628133</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-2824.8387501960265</SoldierAnt.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-483.52474910147771</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>-2266.7892646899973</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-3424.0532079717746</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-1302.4128819538041</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>5070.20842630706</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>42650.800536943818</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-12761.759517284114</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-31.718154260890788</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>12312.854147671358</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>58.870679315797574</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-34975.936864765885</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-385.52790315457275</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>748.6939046871388</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>5871.9045442134047</Pillbug.EnemyNeighborWeight>
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