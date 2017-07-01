// 
// Program.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017 Jon Thysell <http://jonthysell.com>
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

        static void Main(string[] args)
        {
            GameEngineConfig config = null != args && args.Length > 0 ? LoadConfig(args[0]) : GetDefaultConfig();

            GameEngine engine = new GameEngine(ID, config, PrintLine);
            engine.ParseCommand("info");

            while (!engine.ExitRequested)
            {
                string command = Console.In.ReadLine();
                if (!string.IsNullOrWhiteSpace(command))
                {
                    engine.ParseCommand(command);
                }
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
<MetricWeights>
<QueenBee.InPlayWeight>0.3009958832537965</QueenBee.InPlayWeight>
<QueenBee.ValidMoveWeight>78.17629526702197</QueenBee.ValidMoveWeight>
<QueenBee.IsPinnedWeight>-2626.36156987178</QueenBee.IsPinnedWeight>
<QueenBee.NeighborWeight>-19531.395871986202</QueenBee.NeighborWeight>
<Spider.InPlayWeight>7.72796941450716</Spider.InPlayWeight>
<Spider.ValidMoveWeight>-11.51032999069065</Spider.ValidMoveWeight>
<Spider.IsPinnedWeight>-1.5342142823990914</Spider.IsPinnedWeight>
<Spider.NeighborWeight>-20.352925193177803</Spider.NeighborWeight>
<Beetle.InPlayWeight>-0.34202128120365455</Beetle.InPlayWeight>
<Beetle.ValidMoveWeight>-12.873662591404161</Beetle.ValidMoveWeight>
<Beetle.IsPinnedWeight>63.703004319560449</Beetle.IsPinnedWeight>
<Beetle.NeighborWeight>-42.463238128864852</Beetle.NeighborWeight>
<Grasshopper.InPlayWeight>-0.94545626789982951</Grasshopper.InPlayWeight>
<Grasshopper.ValidMoveWeight>241.04859800436216</Grasshopper.ValidMoveWeight>
<Grasshopper.IsPinnedWeight>239.34262348409851</Grasshopper.IsPinnedWeight>
<Grasshopper.NeighborWeight>60.458800204641051</Grasshopper.NeighborWeight>
<SoldierAnt.InPlayWeight>-0.040700159737701525</SoldierAnt.InPlayWeight>
<SoldierAnt.ValidMoveWeight>1.4329644018790138</SoldierAnt.ValidMoveWeight>
<SoldierAnt.IsPinnedWeight>-38.503148272532371</SoldierAnt.IsPinnedWeight>
<SoldierAnt.NeighborWeight>-0.85676195210937178</SoldierAnt.NeighborWeight>
</MetricWeights>
</GameAI>
</Mzinga.Engine>
";
    }
}
