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
<MaxHelperThreads>Auto</MaxHelperThreads>
<PonderDuringIdle>SingleThreaded</PonderDuringIdle>
<MetricWeights>
<QueenBee.InPlayWeight>-29.142509782600857</QueenBee.InPlayWeight>
<QueenBee.ValidMoveWeight>-0.86678691010540931</QueenBee.ValidMoveWeight>
<QueenBee.IsPinnedWeight>-367.92746559937552</QueenBee.IsPinnedWeight>
<QueenBee.NeighborWeight>-26782.761909961031</QueenBee.NeighborWeight>
<Spider.InPlayWeight>-44.700920160994613</Spider.InPlayWeight>
<Spider.ValidMoveWeight>-0.053911238861221421</Spider.ValidMoveWeight>
<Spider.IsPinnedWeight>-48.57286311710866</Spider.IsPinnedWeight>
<Spider.NeighborWeight>-77.03129368255513</Spider.NeighborWeight>
<Beetle.InPlayWeight>-24.412133639846612</Beetle.InPlayWeight>
<Beetle.ValidMoveWeight>-14.794019457278704</Beetle.ValidMoveWeight>
<Beetle.IsPinnedWeight>-112.64593449003604</Beetle.IsPinnedWeight>
<Beetle.NeighborWeight>50.868238831945789</Beetle.NeighborWeight>
<Grasshopper.InPlayWeight>-8.9830500159231583</Grasshopper.InPlayWeight>
<Grasshopper.ValidMoveWeight>212.15754895450141</Grasshopper.ValidMoveWeight>
<Grasshopper.IsPinnedWeight>291.66248444356097</Grasshopper.IsPinnedWeight>
<Grasshopper.NeighborWeight>0.050349185469477906</Grasshopper.NeighborWeight>
<SoldierAnt.InPlayWeight>6.96742652351516</SoldierAnt.InPlayWeight>
<SoldierAnt.ValidMoveWeight>87.952390733671677</SoldierAnt.ValidMoveWeight>
<SoldierAnt.IsPinnedWeight>5.2798879188979093</SoldierAnt.IsPinnedWeight>
<SoldierAnt.NeighborWeight>0.016229015128553265</SoldierAnt.NeighborWeight>
</MetricWeights>
</GameAI>
</Mzinga.Engine>
";
    }
}
