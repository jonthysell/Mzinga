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
<QueenBee.IsPinnedWeight>-367.92746559937552</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>0</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>0</QueenBee.NoisyMoveWeight>
<Spider.InPlayWeight>-45.567707071100024</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-26831.334773078139</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>0</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>0</Spider.NoisyMoveWeight>
<Beetle.InPlayWeight>-24.466044878707834</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>-189.67722817259119</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>0</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>0</Beetle.NoisyMoveWeight>
<Grasshopper.InPlayWeight>-23.777069473201863</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>342.53072327550677</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>0</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>0</Grasshopper.NoisyMoveWeight>
<SoldierAnt.InPlayWeight>219.12497547801658</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>5.3302371043673871</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>0</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>0</SoldierAnt.NoisyMoveWeight>
<Mosquito.InPlayWeight>87.952390733671677</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>0.016229015128553265</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>0</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>0</Mosquito.NoisyMoveWeight>
<Ladybug.InPlayWeight>0</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>0</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>0</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>0</Ladybug.NoisyMoveWeight>
<Pillbug.InPlayWeight>0</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>0</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>0</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>0</Pillbug.NoisyMoveWeight>
</MetricWeights>
</GameAI>
</Mzinga.Engine>
";
    }
}
