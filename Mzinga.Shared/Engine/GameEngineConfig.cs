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
<StartMetricWeights>
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
<EndMetricWeights>
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