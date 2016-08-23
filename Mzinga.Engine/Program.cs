// 
// Program.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016 Jon Thysell <http://jonthysell.com>
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

using Mzinga.Core;

namespace Mzinga.Engine
{
    public class Program
    {
        static string ID
        {
            get
            {
                return String.Format("Mzinga.Engine {0}", Assembly.GetEntryAssembly().GetName().Version.ToString());
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
                if (!String.IsNullOrWhiteSpace(command))
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
            if (String.IsNullOrWhiteSpace(path))
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
    <MaxDepth>-1</MaxDepth>
    <MaxTime>00:00:05</MaxTime>
    <AlphaBetaPruning>true</AlphaBetaPruning>
    <TranspositionTable>true</TranspositionTable>
    <MetricWeights>
        <DrawScore>0</DrawScore>
        <Maximizing.ValidMoveWeight>0.1503242620626056</Maximizing.ValidMoveWeight>
        <Maximizing.ValidPlacementWeight>-0.067183795165162472</Maximizing.ValidPlacementWeight>
        <Maximizing.ValidMovementWeight>0.32265821940223544</Maximizing.ValidMovementWeight>
        <Maximizing.InHandWeight>-1.739871603114628</Maximizing.InHandWeight>
        <Maximizing.InPlayWeight>0.1265953494021044</Maximizing.InPlayWeight>
        <Maximizing.IsPinnedWeight>-0.043093338771046444</Maximizing.IsPinnedWeight>
        <Maximizing.QueenBee.ValidMoveWeight>0.22879885164650038</Maximizing.QueenBee.ValidMoveWeight>
        <Maximizing.QueenBee.ValidPlacementWeight>0.016090054784733219</Maximizing.QueenBee.ValidPlacementWeight>
        <Maximizing.QueenBee.ValidMovementWeight>0.39982670675893034</Maximizing.QueenBee.ValidMovementWeight>
        <Maximizing.QueenBee.NeighborWeight>-2985.283237081484</Maximizing.QueenBee.NeighborWeight>
        <Maximizing.QueenBee.InHandWeight>-0.35744676644468215</Maximizing.QueenBee.InHandWeight>
        <Maximizing.QueenBee.InPlayWeight>0.73701032014254431</Maximizing.QueenBee.InPlayWeight>
        <Maximizing.QueenBee.IsPinnedWeight>535.44655049695734</Maximizing.QueenBee.IsPinnedWeight>
        <Maximizing.Spider.ValidMoveWeight>0.029987092838375425</Maximizing.Spider.ValidMoveWeight>
        <Maximizing.Spider.ValidPlacementWeight>-11.714929065237419</Maximizing.Spider.ValidPlacementWeight>
        <Maximizing.Spider.ValidMovementWeight>0.071336411864958232</Maximizing.Spider.ValidMovementWeight>
        <Maximizing.Spider.NeighborWeight>-9.8491135695080168</Maximizing.Spider.NeighborWeight>
        <Maximizing.Spider.InHandWeight>-0.21644811814015352</Maximizing.Spider.InHandWeight>
        <Maximizing.Spider.InPlayWeight>-1.4130437018977264</Maximizing.Spider.InPlayWeight>
        <Maximizing.Spider.IsPinnedWeight>-0.066516892414478224</Maximizing.Spider.IsPinnedWeight>
        <Maximizing.Beetle.ValidMoveWeight>-0.0055928406027296405</Maximizing.Beetle.ValidMoveWeight>
        <Maximizing.Beetle.ValidPlacementWeight>0.69360436320846786</Maximizing.Beetle.ValidPlacementWeight>
        <Maximizing.Beetle.ValidMovementWeight>-0.0458872609823317</Maximizing.Beetle.ValidMovementWeight>
        <Maximizing.Beetle.NeighborWeight>-6.8747215902011973</Maximizing.Beetle.NeighborWeight>
        <Maximizing.Beetle.InHandWeight>0.035513948978131774</Maximizing.Beetle.InHandWeight>
        <Maximizing.Beetle.InPlayWeight>-2.9596956331902606</Maximizing.Beetle.InPlayWeight>
        <Maximizing.Beetle.IsPinnedWeight>-2.0097081739761866</Maximizing.Beetle.IsPinnedWeight>
        <Maximizing.Grasshopper.ValidMoveWeight>0.091379597436751989</Maximizing.Grasshopper.ValidMoveWeight>
        <Maximizing.Grasshopper.ValidPlacementWeight>0.86855032921283448</Maximizing.Grasshopper.ValidPlacementWeight>
        <Maximizing.Grasshopper.ValidMovementWeight>3.2767295155177569</Maximizing.Grasshopper.ValidMovementWeight>
        <Maximizing.Grasshopper.NeighborWeight>0.32011521400624748</Maximizing.Grasshopper.NeighborWeight>
        <Maximizing.Grasshopper.InHandWeight>-2.4211798205370014</Maximizing.Grasshopper.InHandWeight>
        <Maximizing.Grasshopper.InPlayWeight>0.50122357416047347</Maximizing.Grasshopper.InPlayWeight>
        <Maximizing.Grasshopper.IsPinnedWeight>-0.47922210737960241</Maximizing.Grasshopper.IsPinnedWeight>
        <Maximizing.SoldierAnt.ValidMoveWeight>5.03600557692492</Maximizing.SoldierAnt.ValidMoveWeight>
        <Maximizing.SoldierAnt.ValidPlacementWeight>0.49185092532873825</Maximizing.SoldierAnt.ValidPlacementWeight>
        <Maximizing.SoldierAnt.ValidMovementWeight>-0.30425119307086446</Maximizing.SoldierAnt.ValidMovementWeight>
        <Maximizing.SoldierAnt.NeighborWeight>0.076281108844130932</Maximizing.SoldierAnt.NeighborWeight>
        <Maximizing.SoldierAnt.InHandWeight>0.42851455567900154</Maximizing.SoldierAnt.InHandWeight>
        <Maximizing.SoldierAnt.InPlayWeight>0.082276746447543653</Maximizing.SoldierAnt.InPlayWeight>
        <Maximizing.SoldierAnt.IsPinnedWeight>-16.670764941031912</Maximizing.SoldierAnt.IsPinnedWeight>
        <Minimizing.ValidMoveWeight>0.33571042634966047</Minimizing.ValidMoveWeight>
        <Minimizing.ValidPlacementWeight>0.84239922008828516</Minimizing.ValidPlacementWeight>
        <Minimizing.ValidMovementWeight>0.35797088342048994</Minimizing.ValidMovementWeight>
        <Minimizing.InHandWeight>-0.24466892326647008</Minimizing.InHandWeight>
        <Minimizing.InPlayWeight>15.58498614179072</Minimizing.InPlayWeight>
        <Minimizing.IsPinnedWeight>3.989738399500304</Minimizing.IsPinnedWeight>
        <Minimizing.QueenBee.ValidMoveWeight>0.31750880348399274</Minimizing.QueenBee.ValidMoveWeight>
        <Minimizing.QueenBee.ValidPlacementWeight>0.16032986881995218</Minimizing.QueenBee.ValidPlacementWeight>
        <Minimizing.QueenBee.ValidMovementWeight>0.56339804241877189</Minimizing.QueenBee.ValidMovementWeight>
        <Minimizing.QueenBee.NeighborWeight>15663.605380829067</Minimizing.QueenBee.NeighborWeight>
        <Minimizing.QueenBee.InHandWeight>-0.10605567810666633</Minimizing.QueenBee.InHandWeight>
        <Minimizing.QueenBee.InPlayWeight>-0.10397512253855654</Minimizing.QueenBee.InPlayWeight>
        <Minimizing.QueenBee.IsPinnedWeight>-2249.186390075065</Minimizing.QueenBee.IsPinnedWeight>
        <Minimizing.Spider.ValidMoveWeight>-0.016557187543561971</Minimizing.Spider.ValidMoveWeight>
        <Minimizing.Spider.ValidPlacementWeight>0.18988360809524582</Minimizing.Spider.ValidPlacementWeight>
        <Minimizing.Spider.ValidMovementWeight>-0.14602727814543068</Minimizing.Spider.ValidMovementWeight>
        <Minimizing.Spider.NeighborWeight>-0.45377417625910277</Minimizing.Spider.NeighborWeight>
        <Minimizing.Spider.InHandWeight>0.76590657671316154</Minimizing.Spider.InHandWeight>
        <Minimizing.Spider.InPlayWeight>-0.0078471447462972223</Minimizing.Spider.InPlayWeight>
        <Minimizing.Spider.IsPinnedWeight>-0.43479494606592806</Minimizing.Spider.IsPinnedWeight>
        <Minimizing.Beetle.ValidMoveWeight>-0.37993205324672719</Minimizing.Beetle.ValidMoveWeight>
        <Minimizing.Beetle.ValidPlacementWeight>-0.68248136202524246</Minimizing.Beetle.ValidPlacementWeight>
        <Minimizing.Beetle.ValidMovementWeight>-7.6822203851305524</Minimizing.Beetle.ValidMovementWeight>
        <Minimizing.Beetle.NeighborWeight>0.097831133441920085</Minimizing.Beetle.NeighborWeight>
        <Minimizing.Beetle.InHandWeight>-0.63948807746856029</Minimizing.Beetle.InHandWeight>
        <Minimizing.Beetle.InPlayWeight>0.0018806708704386659</Minimizing.Beetle.InPlayWeight>
        <Minimizing.Beetle.IsPinnedWeight>-0.84055088666350708</Minimizing.Beetle.IsPinnedWeight>
        <Minimizing.Grasshopper.ValidMoveWeight>-23.637266786911489</Minimizing.Grasshopper.ValidMoveWeight>
        <Minimizing.Grasshopper.ValidPlacementWeight>1.2799883529383842</Minimizing.Grasshopper.ValidPlacementWeight>
        <Minimizing.Grasshopper.ValidMovementWeight>0.49137954971129111</Minimizing.Grasshopper.ValidMovementWeight>
        <Minimizing.Grasshopper.NeighborWeight>-0.0091976836910130869</Minimizing.Grasshopper.NeighborWeight>
        <Minimizing.Grasshopper.InHandWeight>-0.567630730108924</Minimizing.Grasshopper.InHandWeight>
        <Minimizing.Grasshopper.InPlayWeight>-0.032621091698935428</Minimizing.Grasshopper.InPlayWeight>
        <Minimizing.Grasshopper.IsPinnedWeight>-0.0043691295795513447</Minimizing.Grasshopper.IsPinnedWeight>
        <Minimizing.SoldierAnt.ValidMoveWeight>0.89061120816486772</Minimizing.SoldierAnt.ValidMoveWeight>
        <Minimizing.SoldierAnt.ValidPlacementWeight>5.4554341034574607</Minimizing.SoldierAnt.ValidPlacementWeight>
        <Minimizing.SoldierAnt.ValidMovementWeight>-0.1980075754902933</Minimizing.SoldierAnt.ValidMovementWeight>
        <Minimizing.SoldierAnt.NeighborWeight>-0.049752695958347512</Minimizing.SoldierAnt.NeighborWeight>
        <Minimizing.SoldierAnt.InHandWeight>1.2499428982294456</Minimizing.SoldierAnt.InHandWeight>
        <Minimizing.SoldierAnt.InPlayWeight>1.1113999806501409</Minimizing.SoldierAnt.InPlayWeight>
        <Minimizing.SoldierAnt.IsPinnedWeight>4.1798286416313841</Minimizing.SoldierAnt.IsPinnedWeight>
    </MetricWeights>
  </GameAI>
</Mzinga.Engine>
";
    }
}
