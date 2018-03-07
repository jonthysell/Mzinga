// 
// Program.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017, 2018 Jon Thysell <http://jonthysell.com>
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
using System.Reflection;
using System.Text;

namespace Mzinga.Engine
{
    public class Program
    {
        static string ID
        {
            get
            {
                return string.Format("Mzinga.Engine {0}", Assembly.GetEntryAssembly().GetName().Version.ToString());
            }
        }

        private static GameEngine _engine;

        private static volatile bool _interceptCancel = false;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            GameEngineConfig config = null != args && args.Length > 0 ? LoadConfig(args[0]) : GetDefaultConfig();

            _engine = new GameEngine(ID, config, PrintLine);
            _engine.ParseCommand("info");

            Console.CancelKeyPress += Console_CancelKeyPress;

            _engine.StartAsyncCommand += (s, e) =>
            {
                _interceptCancel = true;
            };

            _engine.EndAsyncCommand += (s, e) =>
            {
                _interceptCancel = false;
            };

            while (!_engine.ExitRequested)
            {
                string command = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(command))
                {
                    _engine.ParseCommand(command);
                }
            }
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (_interceptCancel)
            {
                _engine.TryCancelAsyncCommand();
                e.Cancel = true;
            }
        }

        static void PrintLine(string format, params object[] arg)
        {
            Console.Out.WriteLine(format, arg);
        }

        static GameEngineConfig LoadConfig(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("path");
            }

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                return new GameEngineConfig(fs);
            }
        }

        static GameEngineConfig GetDefaultConfig()
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
}
