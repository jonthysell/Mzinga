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

            _engine.StartAsyncCommandEvent += (s, e) =>
            {
                _interceptCancel = true;
            };

            _engine.EndAsyncCommandEvent += (s, e) =>
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
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    sw.Write(DefaultConfig);
                    sw.Flush();

                    ms.Position = 0;
                    return new GameEngineConfig(ms);
                }
            }
        }

        private const string DefaultConfig = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Mzinga.Engine>
<GameAI>
<TranspositionTableSizeMB>32</TranspositionTableSizeMB>
<PonderDuringIdle>SingleThreaded</PonderDuringIdle>
<MetricWeights>
<QueenBee.InPlayWeight>-0.58470774882045851</QueenBee.InPlayWeight>
<QueenBee.ValidMoveWeight>19.650175090357259</QueenBee.ValidMoveWeight>
<QueenBee.IsPinnedWeight>-2021.5250758356444</QueenBee.IsPinnedWeight>
<QueenBee.NeighborWeight>-41515.104902342726</QueenBee.NeighborWeight>
<Spider.InPlayWeight>1.912367026977045</Spider.InPlayWeight>
<Spider.ValidMoveWeight>20.950015292748422</Spider.ValidMoveWeight>
<Spider.IsPinnedWeight>-17.845509943103163</Spider.IsPinnedWeight>
<Spider.NeighborWeight>-76.410682779118559</Spider.NeighborWeight>
<Beetle.InPlayWeight>0.33148407060878576</Beetle.InPlayWeight>
<Beetle.ValidMoveWeight>-0.280644042462848</Beetle.ValidMoveWeight>
<Beetle.IsPinnedWeight>61.803208458475787</Beetle.IsPinnedWeight>
<Beetle.NeighborWeight>2.9861683625660125</Beetle.NeighborWeight>
<Grasshopper.InPlayWeight>0.022815110938712014</Grasshopper.InPlayWeight>
<Grasshopper.ValidMoveWeight>71.602913410450711</Grasshopper.ValidMoveWeight>
<Grasshopper.IsPinnedWeight>166.88834106248234</Grasshopper.IsPinnedWeight>
<Grasshopper.NeighborWeight>-35.30118976625932</Grasshopper.NeighborWeight>
<SoldierAnt.InPlayWeight>-0.1652831333950277</SoldierAnt.InPlayWeight>
<SoldierAnt.ValidMoveWeight>0.7424545550204219</SoldierAnt.ValidMoveWeight>
<SoldierAnt.IsPinnedWeight>25.928272122911924</SoldierAnt.IsPinnedWeight>
<SoldierAnt.NeighborWeight>0.21820900761599979</SoldierAnt.NeighborWeight>
</MetricWeights>
</GameAI>
</Mzinga.Engine>
";
    }
}
