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

        public MetricWeights MetricWeights { get; private set; } = null;

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
                            MetricWeights = MetricWeights.ReadMetricWeightsXml(reader.ReadSubtree());
                            break;
                        case "MaxHelperThreads":
                            ParseMaxHelperThreadsValue(reader.ReadElementContentAsString());
                            break;
                        case "PonderDuringIdle":
                            ParsePonderDuringIdleValue(reader.ReadElementContentAsString());
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

        public GameAI GetGameAI()
        {
            if (TranspositionTableSizeMB.HasValue)
            {
                return null != MetricWeights? new GameAI(MetricWeights, TranspositionTableSizeMB.Value) : new GameAI(TranspositionTableSizeMB.Value);
            }
            else if (null != MetricWeights)
            {
                return new GameAI(MetricWeights);
            }

            return new GameAI();
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
<MetricWeights>
<QueenBee.InPlayWeight>0.17400519464817243</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>4.6668299914648887</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-0.20637051677628016</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-3.6505129996922392</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>7.4086318804922655</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-100</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-1000</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-8.7187173770362119</Spider.InPlayWeight>
<Spider.IsPinnedWeight>1.9441455192603847</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>6.6751638225629755</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>5.5293976867242716</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>3.5185058384754253</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>3.9700486855441941</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-8.8877635350859556</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-8.6778174334568057</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>4.5077450734133588</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>3.82122070240845</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>3.6133896436604633</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-3.5625411446963167</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>0.71831753045242053</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-7.174050657625334</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>-3.6057653993395</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-4.7747886622207192</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>4.9027981678502623</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>5.7118395230322339</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>-9.9965263623728067</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-2.07552360001743</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-0.34304187183409951</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>-3.0236163144109849</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>-1.2506377656248588</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>-0.60639504837170044</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>2.2806644590015814</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>3.1950565954647292</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-7.8530518793748</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-5.6304881515123366</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>9.9791409540824318</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>1.3210967701492358</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-8.1232226817511126</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>1.8979682549359129</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>8.3677771773039247</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>7.7415573307040866</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-6.2545292527668774</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-5.4438330025616253</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>0.10055064694050309</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-4.2118336419630023</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>8.0698034717095091</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>-0.74804385693187037</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>9.462294941517662</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-2.9117009942008654</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-7.5376667536504876</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>9.090092200362168</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>5.7973141296754189</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-4.5512554117251449</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-1.7742334919861662</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>8.5415989898804554</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>-6.2202294060123293</Pillbug.EnemyNeighborWeight>
</MetricWeights>
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