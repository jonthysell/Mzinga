// 
// GameEngineConfig.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2018, 2019 Jon Thysell <http://jonthysell.com>
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

        public Dictionary<ExpansionPieces, TranspositionTable> InitialTranspositionTables { get; private set; } = new Dictionary<ExpansionPieces, TranspositionTable>();

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
                        case "InitialTranspositionTable":
                            SetInitialTranspositionTable(expansionPieces, TranspositionTable.ReadTranspositionTableXml(reader.ReadSubtree()));
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
                InitialTranspositionTable = GetInitialTranspositionTable(expansionPieces),
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

        public MetricWeights[] GetMetricWeights(ExpansionPieces expansionPieces)
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

        private void SetInitialTranspositionTable(ExpansionPieces expansionPieces, TranspositionTable transpositionTable)
        {
            InitialTranspositionTables[expansionPieces] = transpositionTable;
        }

        public TranspositionTable GetInitialTranspositionTable(ExpansionPieces expansionPieces)
        {
            TranspositionTable result;
            if (!InitialTranspositionTables.TryGetValue(expansionPieces, out result))
            {
                result = null;
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
</StartMetricWeights>
<EndMetricWeights GameType=""Base"">
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
<QueenBee.InPlayWeight>63372.267967443746</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-16067.687225081914</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-10847.567653899931</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-27548.363843068229</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>-58894.529888366516</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>5897.66015648484</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-16777.906712227239</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-80849.827917611037</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-40380.250217461442</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>3522.4688484300345</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>96533.760806393242</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>24922.399500237472</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>-41154.285475958168</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-24008.006681178387</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-285722.80079303018</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>43083.839539047542</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>4481.1788458262208</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>17665.014704305027</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-32888.2568926812</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-324385.12625412148</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-13035.155688109544</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>-63858.360206370278</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-37382.758164338331</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>7376.7071386644275</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>81774.277112401774</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>3424.9241269286663</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-48269.41901059544</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-29912.902853964733</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>150390.37102821292</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-54347.584005878518</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>2181.3845855769014</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>-12190.782082844375</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>-3529.2769345792676</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-24755.356833984759</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-125174.60484694976</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>146668.3872826054</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-3237.4085063251164</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>274.477890120718</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>7418.8160739939394</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>-1184.5701378737892</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>5500.6583121094372</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>11776.009356513741</Mosquito.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+ML"">
<QueenBee.InPlayWeight>13401.683666948999</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-129954.36732766283</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-20359.393475776742</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>72291.279470980546</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>95769.111067911785</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-317497.86761769938</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-538148.83427047916</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>25047.271247502231</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-46573.959555435358</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>7318.0624455331526</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>139756.22020687832</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>-20620.530736359175</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>20646.867409996259</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>47114.779770721972</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-46793.9659865222</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>-47948.146790531711</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>9325.20680229596</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>142117.44307757894</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>11224.484046016101</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>37577.850221157816</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>64379.510363643909</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>62393.217199612584</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-82598.030838865525</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>24024.225201639405</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>105510.87832342245</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>-12596.415297987831</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>13345.477142356704</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>24260.611219917839</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>132394.59649365061</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-51035.8531595224</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>-1515.3280863367781</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>53866.8750489757</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>4030.2113445858308</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>14317.815161198059</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>21201.504270947065</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>35642.691395213136</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-47912.663071188537</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-5471.6583403421873</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>34855.368897836735</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>3864.4798381937458</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>9134.98097241003</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>20130.795512734141</Mosquito.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+P"">
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
<Pillbug.InPlayWeight>-0.80660169131866855</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>280.451146490288</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>3.34195832929898</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-15.607639598861633</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-10.156675325509477</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>6.8246036353449346</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>158.80183016264434</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+P"">
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
<Pillbug.InPlayWeight>-0.80660169131866855</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>280.451146490288</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>3.34195832929898</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-15.607639598861633</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-10.156675325509477</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>6.8246036353449346</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>158.80183016264434</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+MP"">
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
<Pillbug.InPlayWeight>-206.54781406363119</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>321658.29441728379</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>727.98845010305945</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-30353.474905396481</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-4721.5913874428425</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>4835.1476492376987</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>44929.765023024556</Pillbug.EnemyNeighborWeight>
</StartMetricWeights>
<EndMetricWeights GameType=""Base+MP"">
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
<Pillbug.InPlayWeight>-8313.88974419674</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>38673.973849618618</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>222.23756756526225</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-13070.606991504526</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-1156.6141556723287</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>11675.0719761247</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>613.20695251393215</Pillbug.EnemyNeighborWeight>
</EndMetricWeights>
<StartMetricWeights GameType=""Base+LP"">
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
<EndMetricWeights GameType=""Base+LP"">
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