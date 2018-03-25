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
using System.IO;
using System.Text;
using System.Xml;

using Mzinga.Core.AI;

namespace Mzinga.Engine
{
    public class GameEngineConfig
    {
        #region GameAI

        public int? TranspositionTableSizeMB { get; private set; } = null;

        public MetricWeights StartMetricWeights { get; private set; } = null;

        public MetricWeights EndMetricWeights { get; private set; } = null;

        public PonderDuringIdleType PonderDuringIdle { get; private set; } = PonderDuringIdleType.Disabled;

        public int MaxHelperThreads
        {
            get
            {
                // Hard min is 0, hard max is (Environment.ProcessorCount / 2) - 1
#if DEBUG
                return 0;
#else
                return Math.Max(0, _maxHelperThreads.HasValue ? Math.Min(_maxHelperThreads.Value, (Environment.ProcessorCount / 2) - 1) : (Environment.ProcessorCount / 2) - 1);
#endif
            }
        }
        private int? _maxHelperThreads = null;

        public int? MaxBranchingFactor { get; private set; } = null;

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
                    switch (reader.Name)
                    {
                        case "TranspositionTableSizeMB":
                            ParseTranspositionTableSizeMBValue(reader.ReadElementContentAsString());
                            break;
                        case "MetricWeights":
                        case "StartMetricWeights":
                            StartMetricWeights = MetricWeights.ReadMetricWeightsXml(reader.ReadSubtree());
                            break;
                        case "EndMetricWeights":
                            EndMetricWeights = MetricWeights.ReadMetricWeightsXml(reader.ReadSubtree());
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
                    }
                }
            }
        }

        private void ParseTranspositionTableSizeMBValue(string rawValue)
        {
            int intValue;
            if (int.TryParse(rawValue, out intValue))
            {
                TranspositionTableSizeMB = intValue;
            }
        }

        private void ParseMaxHelperThreadsValue(string rawValue)
        {
            int intValue;
            MaxHelperThreadsType enumValue;

            if (int.TryParse(rawValue, out intValue))
            {
                _maxHelperThreads = intValue;
            }
            else if (Enum.TryParse(rawValue, out enumValue))
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

        private void ParsePonderDuringIdleValue(string rawValue)
        {
            PonderDuringIdleType enumValue;
            if (Enum.TryParse(rawValue, out enumValue))
            {
                PonderDuringIdle = enumValue;
            }
        }

        private void ParseMaxBranchingFactorValue(string rawValue)
        {
            int intValue;
            if (int.TryParse(rawValue, out intValue))
            {
                MaxBranchingFactor = intValue;
            }
        }

        public GameAI GetGameAI()
        {
            return new GameAI(new GameAIConfig()
            {
                StartMetricWeights = StartMetricWeights,
                EndMetricWeights = EndMetricWeights ?? StartMetricWeights,
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

        private const string DefaultConfig = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Mzinga.Engine>
<GameAI>
<TranspositionTableSizeMB>32</TranspositionTableSizeMB>
<MaxHelperThreads>Auto</MaxHelperThreads>
<PonderDuringIdle>SingleThreaded</PonderDuringIdle>
<StartMetricWeights>
<QueenBee.InPlayWeight>-31.271265238491477</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>2.0334710106223222</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-9.4245904810096075</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-70.808251671610989</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>89.310825113084078</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-1292.144333086947</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-2369.901737091086</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-149.30840826867541</Spider.InPlayWeight>
<Spider.IsPinnedWeight>40.694851291829188</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>54.938846900842073</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>120.6824977665965</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>4.237933253980211</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>6.6842247969257773</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-36.287365364328664</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-21.298671861013247</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>44.975440006673075</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>0.22640443368181792</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>11.799687995838319</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-0.41972015855122363</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-47.835946773298062</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-34.152794853100922</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>27.821419259296462</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-9.1776263769379</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>87.385857538232031</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>24.3511057438334</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>10.463797931011674</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-8.5728600941518582</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-15.15464964418423</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>14.791404237533643</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>3.5479715260690874</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>0.86876704527939075</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>12.544588833928383</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>44.651134348684522</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-1.0205554548560434</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-3.7158092609214641</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>114.90974779522037</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-10.359089252634018</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-68.03524522408155</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-7.6081300321585186</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>5.1726287565252882</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>623.91444520598009</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-17.764958039189207</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-63.491944838844688</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>1.3151225751801965</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-30.39286922922026</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-1.2132434501234646</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>11.565416396917039</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>233.56274141844025</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-195.58021587206994</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-17.177470113910957</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>337.50185100135252</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>4.1548128944983764</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-70.394767625781128</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-6.5562542009708737</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>33.684978236251034</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>131.05545890920584</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights>
<QueenBee.InPlayWeight>-31.271265238491477</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>2.0334710106223222</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-9.4245904810096075</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-70.808251671610989</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>89.310825113084078</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-1292.144333086947</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-2369.901737091086</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-149.30840826867541</Spider.InPlayWeight>
<Spider.IsPinnedWeight>40.694851291829188</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>54.938846900842073</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>120.6824977665965</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>4.237933253980211</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>6.6842247969257773</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-36.287365364328664</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-21.298671861013247</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>44.975440006673075</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>0.22640443368181792</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>11.799687995838319</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-0.41972015855122363</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-47.835946773298062</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-34.152794853100922</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>27.821419259296462</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-9.1776263769379</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>87.385857538232031</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>24.3511057438334</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>10.463797931011674</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-8.5728600941518582</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-15.15464964418423</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>14.791404237533643</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>3.5479715260690874</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>0.86876704527939075</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>12.544588833928383</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>44.651134348684522</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-1.0205554548560434</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-3.7158092609214641</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>114.90974779522037</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-10.359089252634018</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-68.03524522408155</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-7.6081300321585186</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>5.1726287565252882</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>623.91444520598009</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-17.764958039189207</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-63.491944838844688</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>1.3151225751801965</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-30.39286922922026</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-1.2132434501234646</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>11.565416396917039</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>233.56274141844025</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-195.58021587206994</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-17.177470113910957</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>337.50185100135252</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>4.1548128944983764</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-70.394767625781128</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-6.5562542009708737</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>33.684978236251034</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>131.05545890920584</Pillbug.EnemyNeighborWeight>
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