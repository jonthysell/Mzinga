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
        <Maximizing.ValidMoveWeight>0.43764936938852611</Maximizing.ValidMoveWeight>
        <Maximizing.ValidPlacementWeight>0.16784060484277671</Maximizing.ValidPlacementWeight>
        <Maximizing.ValidMovementWeight>0.81705648146065135</Maximizing.ValidMovementWeight>
        <Maximizing.InHandWeight>0.022301172283649476</Maximizing.InHandWeight>
        <Maximizing.InPlayWeight>-0.96863840543003921</Maximizing.InPlayWeight>
        <Maximizing.IsPinnedWeight>-0.0232074175892136</Maximizing.IsPinnedWeight>
        <Maximizing.QueenBee.ValidMoveWeight>0.044300177583464294</Maximizing.QueenBee.ValidMoveWeight>
        <Maximizing.QueenBee.ValidPlacementWeight>-0.23553296871255611</Maximizing.QueenBee.ValidPlacementWeight>
        <Maximizing.QueenBee.ValidMovementWeight>2.0352254734465234</Maximizing.QueenBee.ValidMovementWeight>
        <Maximizing.QueenBee.NeighborWeight>-847.89336927683564</Maximizing.QueenBee.NeighborWeight>
        <Maximizing.QueenBee.InHandWeight>-2.3615665350040418</Maximizing.QueenBee.InHandWeight>
        <Maximizing.QueenBee.InPlayWeight>-2.9179650871979432</Maximizing.QueenBee.InPlayWeight>
        <Maximizing.QueenBee.IsPinnedWeight>-55.066040421055796</Maximizing.QueenBee.IsPinnedWeight>
        <Maximizing.Spider.ValidMoveWeight>-0.98481746293516637</Maximizing.Spider.ValidMoveWeight>
        <Maximizing.Spider.ValidPlacementWeight>-2.8123849396842351</Maximizing.Spider.ValidPlacementWeight>
        <Maximizing.Spider.ValidMovementWeight>-0.40628397017625273</Maximizing.Spider.ValidMovementWeight>
        <Maximizing.Spider.NeighborWeight>11.415131122563778</Maximizing.Spider.NeighborWeight>
        <Maximizing.Spider.InHandWeight>5.3207279429686807</Maximizing.Spider.InHandWeight>
        <Maximizing.Spider.InPlayWeight>-3.7472679449114517</Maximizing.Spider.InPlayWeight>
        <Maximizing.Spider.IsPinnedWeight>-2.4927691327175814</Maximizing.Spider.IsPinnedWeight>
        <Maximizing.Beetle.ValidMoveWeight>0.0004539371334367133</Maximizing.Beetle.ValidMoveWeight>
        <Maximizing.Beetle.ValidPlacementWeight>-6.3706149656750766</Maximizing.Beetle.ValidPlacementWeight>
        <Maximizing.Beetle.ValidMovementWeight>-0.063182152412572218</Maximizing.Beetle.ValidMovementWeight>
        <Maximizing.Beetle.NeighborWeight>-11.969836080880603</Maximizing.Beetle.NeighborWeight>
        <Maximizing.Beetle.InHandWeight>-0.12427517570595031</Maximizing.Beetle.InHandWeight>
        <Maximizing.Beetle.InPlayWeight>2.2352870473133311</Maximizing.Beetle.InPlayWeight>
        <Maximizing.Beetle.IsPinnedWeight>1.2608479925700646</Maximizing.Beetle.IsPinnedWeight>
        <Maximizing.Grasshopper.ValidMoveWeight>-0.051334169652472846</Maximizing.Grasshopper.ValidMoveWeight>
        <Maximizing.Grasshopper.ValidPlacementWeight>-0.048860512888906631</Maximizing.Grasshopper.ValidPlacementWeight>
        <Maximizing.Grasshopper.ValidMovementWeight>12.538468786059434</Maximizing.Grasshopper.ValidMovementWeight>
        <Maximizing.Grasshopper.NeighborWeight>-0.32136387036452452</Maximizing.Grasshopper.NeighborWeight>
        <Maximizing.Grasshopper.InHandWeight>-0.61113622945250579</Maximizing.Grasshopper.InHandWeight>
        <Maximizing.Grasshopper.InPlayWeight>0.21190555310054204</Maximizing.Grasshopper.InPlayWeight>
        <Maximizing.Grasshopper.IsPinnedWeight>-6.3858385882469495</Maximizing.Grasshopper.IsPinnedWeight>
        <Maximizing.SoldierAnt.ValidMoveWeight>-0.78708807893594912</Maximizing.SoldierAnt.ValidMoveWeight>
        <Maximizing.SoldierAnt.ValidPlacementWeight>-2.4085915694590656</Maximizing.SoldierAnt.ValidPlacementWeight>
        <Maximizing.SoldierAnt.ValidMovementWeight>7.5623720732581834</Maximizing.SoldierAnt.ValidMovementWeight>
        <Maximizing.SoldierAnt.NeighborWeight>-0.39384002848197269</Maximizing.SoldierAnt.NeighborWeight>
        <Maximizing.SoldierAnt.InHandWeight>2.3586440056471671</Maximizing.SoldierAnt.InHandWeight>
        <Maximizing.SoldierAnt.InPlayWeight>0.099233314682441745</Maximizing.SoldierAnt.InPlayWeight>
        <Maximizing.SoldierAnt.IsPinnedWeight>-5.1142910146665139</Maximizing.SoldierAnt.IsPinnedWeight>
        <Minimizing.ValidMoveWeight>0.20200083326230853</Minimizing.ValidMoveWeight>
        <Minimizing.ValidPlacementWeight>-2.2989387054170125</Minimizing.ValidPlacementWeight>
        <Minimizing.ValidMovementWeight>0.011668085634540549</Minimizing.ValidMovementWeight>
        <Minimizing.InHandWeight>0.57723249520993292</Minimizing.InHandWeight>
        <Minimizing.InPlayWeight>-2.5163978024737648</Minimizing.InPlayWeight>
        <Minimizing.IsPinnedWeight>2.5634517794046792</Minimizing.IsPinnedWeight>
        <Minimizing.QueenBee.ValidMoveWeight>0.25911088607881283</Minimizing.QueenBee.ValidMoveWeight>
        <Minimizing.QueenBee.ValidPlacementWeight>-6.0141627733395966</Minimizing.QueenBee.ValidPlacementWeight>
        <Minimizing.QueenBee.ValidMovementWeight>-2.1493492294879841</Minimizing.QueenBee.ValidMovementWeight>
        <Minimizing.QueenBee.NeighborWeight>14527.459493176659</Minimizing.QueenBee.NeighborWeight>
        <Minimizing.QueenBee.InHandWeight>2.6675022295794095</Minimizing.QueenBee.InHandWeight>
        <Minimizing.QueenBee.InPlayWeight>-0.29565146008546223</Minimizing.QueenBee.InPlayWeight>
        <Minimizing.QueenBee.IsPinnedWeight>42.358727116852933</Minimizing.QueenBee.IsPinnedWeight>
        <Minimizing.Spider.ValidMoveWeight>0.10087639387768521</Minimizing.Spider.ValidMoveWeight>
        <Minimizing.Spider.ValidPlacementWeight>-0.81603060617400014</Minimizing.Spider.ValidPlacementWeight>
        <Minimizing.Spider.ValidMovementWeight>-1.6307244087443804</Minimizing.Spider.ValidMovementWeight>
        <Minimizing.Spider.NeighborWeight>-0.0482212171827915</Minimizing.Spider.NeighborWeight>
        <Minimizing.Spider.InHandWeight>7.30487352719699</Minimizing.Spider.InHandWeight>
        <Minimizing.Spider.InPlayWeight>0.11949823251404725</Minimizing.Spider.InPlayWeight>
        <Minimizing.Spider.IsPinnedWeight>2.6305334092777573</Minimizing.Spider.IsPinnedWeight>
        <Minimizing.Beetle.ValidMoveWeight>-0.045170296788393967</Minimizing.Beetle.ValidMoveWeight>
        <Minimizing.Beetle.ValidPlacementWeight>-1.815053032759971</Minimizing.Beetle.ValidPlacementWeight>
        <Minimizing.Beetle.ValidMovementWeight>-15.358197718651319</Minimizing.Beetle.ValidMovementWeight>
        <Minimizing.Beetle.NeighborWeight>-0.15624070681639785</Minimizing.Beetle.NeighborWeight>
        <Minimizing.Beetle.InHandWeight>-0.43526721723540429</Minimizing.Beetle.InHandWeight>
        <Minimizing.Beetle.InPlayWeight>-0.31019888431150444</Minimizing.Beetle.InPlayWeight>
        <Minimizing.Beetle.IsPinnedWeight>5.1625019035922763</Minimizing.Beetle.IsPinnedWeight>
        <Minimizing.Grasshopper.ValidMoveWeight>-33.378446144313436</Minimizing.Grasshopper.ValidMoveWeight>
        <Minimizing.Grasshopper.ValidPlacementWeight>-7.2659822484399372</Minimizing.Grasshopper.ValidPlacementWeight>
        <Minimizing.Grasshopper.ValidMovementWeight>-0.081468096400129933</Minimizing.Grasshopper.ValidMovementWeight>
        <Minimizing.Grasshopper.NeighborWeight>-0.005752176313978773</Minimizing.Grasshopper.NeighborWeight>
        <Minimizing.Grasshopper.InHandWeight>-0.15676142393474382</Minimizing.Grasshopper.InHandWeight>
        <Minimizing.Grasshopper.InPlayWeight>-0.009569514997070875</Minimizing.Grasshopper.InPlayWeight>
        <Minimizing.Grasshopper.IsPinnedWeight>0.025125619443158134</Minimizing.Grasshopper.IsPinnedWeight>
        <Minimizing.SoldierAnt.ValidMoveWeight>-9.3966468329988118</Minimizing.SoldierAnt.ValidMoveWeight>
        <Minimizing.SoldierAnt.ValidPlacementWeight>4.0927573883504005</Minimizing.SoldierAnt.ValidPlacementWeight>
        <Minimizing.SoldierAnt.ValidMovementWeight>4.1486654200112243</Minimizing.SoldierAnt.ValidMovementWeight>
        <Minimizing.SoldierAnt.NeighborWeight>-0.11831285834152409</Minimizing.SoldierAnt.NeighborWeight>
        <Minimizing.SoldierAnt.InHandWeight>3.7488272275247794</Minimizing.SoldierAnt.InHandWeight>
        <Minimizing.SoldierAnt.InPlayWeight>0.073887132065423453</Minimizing.SoldierAnt.InPlayWeight>
        <Minimizing.SoldierAnt.IsPinnedWeight>-0.13812796937026436</Minimizing.SoldierAnt.IsPinnedWeight>
    </MetricWeights>
  </GameAI>
</Mzinga.Engine>
";
    }
}
