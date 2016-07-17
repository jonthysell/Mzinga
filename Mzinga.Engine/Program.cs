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
    <MaxTime>00:00:01</MaxTime>
    <AlphaBetaPruning>true</AlphaBetaPruning>
    <TranspositionTable>true</TranspositionTable>
    <MetricWeights>
      <Maximizing.ValidMoveWeight>0.11627506999779658</Maximizing.ValidMoveWeight>
      <Maximizing.ValidPlacementWeight>-0.016627895341110924</Maximizing.ValidPlacementWeight>
      <Maximizing.ValidMovementWeight>0.010071608505134066</Maximizing.ValidMovementWeight>
      <Maximizing.InHandWeight>0.01217016340929911</Maximizing.InHandWeight>
      <Maximizing.InPlayWeight>-0.0025067106539816242</Maximizing.InPlayWeight>
      <Maximizing.IsPinnedWeight>-0.0071246765841697255</Maximizing.IsPinnedWeight>
      <Maximizing.QueenBee.ValidMoveWeight>-0.0085466616666113829</Maximizing.QueenBee.ValidMoveWeight>
      <Maximizing.QueenBee.ValidPlacementWeight>0.018180950674681284</Maximizing.QueenBee.ValidPlacementWeight>
      <Maximizing.QueenBee.ValidMovementWeight>-0.00081514267640334147</Maximizing.QueenBee.ValidMovementWeight>
      <Maximizing.QueenBee.NeighborWeight>-15.943628134412197</Maximizing.QueenBee.NeighborWeight>
      <Maximizing.QueenBee.InHandWeight>-0.010348147302793398</Maximizing.QueenBee.InHandWeight>
      <Maximizing.QueenBee.InPlayWeight>0.00044865059699544042</Maximizing.QueenBee.InPlayWeight>
      <Maximizing.QueenBee.IsPinnedWeight>-16.297146661689343</Maximizing.QueenBee.IsPinnedWeight>
      <Maximizing.Spider.ValidMoveWeight>0.002707694946237496</Maximizing.Spider.ValidMoveWeight>
      <Maximizing.Spider.ValidPlacementWeight>0.0016779558643242819</Maximizing.Spider.ValidPlacementWeight>
      <Maximizing.Spider.ValidMovementWeight>-0.0033777810536844348</Maximizing.Spider.ValidMovementWeight>
      <Maximizing.Spider.NeighborWeight>-0.017765148245073539</Maximizing.Spider.NeighborWeight>
      <Maximizing.Spider.InHandWeight>-0.024024541519170387</Maximizing.Spider.InHandWeight>
      <Maximizing.Spider.InPlayWeight>0.014445697349072358</Maximizing.Spider.InPlayWeight>
      <Maximizing.Spider.IsPinnedWeight>-0.0063278910620002812</Maximizing.Spider.IsPinnedWeight>
      <Maximizing.Beetle.ValidMoveWeight>0.0050324744654495412</Maximizing.Beetle.ValidMoveWeight>
      <Maximizing.Beetle.ValidPlacementWeight>0.0029848315749378979</Maximizing.Beetle.ValidPlacementWeight>
      <Maximizing.Beetle.ValidMovementWeight>0.0096045828247058439</Maximizing.Beetle.ValidMovementWeight>
      <Maximizing.Beetle.NeighborWeight>0.013795642796563619</Maximizing.Beetle.NeighborWeight>
      <Maximizing.Beetle.InHandWeight>-0.010223139617091249</Maximizing.Beetle.InHandWeight>
      <Maximizing.Beetle.InPlayWeight>-0.0028582608163600885</Maximizing.Beetle.InPlayWeight>
      <Maximizing.Beetle.IsPinnedWeight>-0.0054409122126401217</Maximizing.Beetle.IsPinnedWeight>
      <Maximizing.Grasshopper.ValidMoveWeight>-0.014576215683687098</Maximizing.Grasshopper.ValidMoveWeight>
      <Maximizing.Grasshopper.ValidPlacementWeight>-0.015791574906369003</Maximizing.Grasshopper.ValidPlacementWeight>
      <Maximizing.Grasshopper.ValidMovementWeight>0.010815237280844671</Maximizing.Grasshopper.ValidMovementWeight>
      <Maximizing.Grasshopper.NeighborWeight>-0.0017261564158623</Maximizing.Grasshopper.NeighborWeight>
      <Maximizing.Grasshopper.InHandWeight>0.0062225407664948636</Maximizing.Grasshopper.InHandWeight>
      <Maximizing.Grasshopper.InPlayWeight>0.000807293096970805</Maximizing.Grasshopper.InPlayWeight>
      <Maximizing.Grasshopper.IsPinnedWeight>-0.00740897554748242</Maximizing.Grasshopper.IsPinnedWeight>
      <Maximizing.SoldierAnt.ValidMoveWeight>0.025305272419283046</Maximizing.SoldierAnt.ValidMoveWeight>
      <Maximizing.SoldierAnt.ValidPlacementWeight>0.030929142482500723</Maximizing.SoldierAnt.ValidPlacementWeight>
      <Maximizing.SoldierAnt.ValidMovementWeight>0.00922211829420929</Maximizing.SoldierAnt.ValidMovementWeight>
      <Maximizing.SoldierAnt.NeighborWeight>-0.014808346049304387</Maximizing.SoldierAnt.NeighborWeight>
      <Maximizing.SoldierAnt.InHandWeight>0.0012916508320156793</Maximizing.SoldierAnt.InHandWeight>
      <Maximizing.SoldierAnt.InPlayWeight>0.0086672636926724347</Maximizing.SoldierAnt.InPlayWeight>
      <Maximizing.SoldierAnt.IsPinnedWeight>0.014631164232582579</Maximizing.SoldierAnt.IsPinnedWeight>
      <Minimizing.ValidMoveWeight>-0.0097496955735486574</Minimizing.ValidMoveWeight>
      <Minimizing.ValidPlacementWeight>0.01650076582240724</Minimizing.ValidPlacementWeight>
      <Minimizing.ValidMovementWeight>0.0034194721893541047</Minimizing.ValidMovementWeight>
      <Minimizing.InHandWeight>0.0092562201809722883</Minimizing.InHandWeight>
      <Minimizing.InPlayWeight>0.0061539526874224808</Minimizing.InPlayWeight>
      <Minimizing.IsPinnedWeight>-0.018783885599460041</Minimizing.IsPinnedWeight>
      <Minimizing.QueenBee.ValidMoveWeight>0.0080425794442683055</Minimizing.QueenBee.ValidMoveWeight>
      <Minimizing.QueenBee.ValidPlacementWeight>-0.01927200005045121</Minimizing.QueenBee.ValidPlacementWeight>
      <Minimizing.QueenBee.ValidMovementWeight>0.0045929510123875385</Minimizing.QueenBee.ValidMovementWeight>
      <Minimizing.QueenBee.NeighborWeight>19.701598682015</Minimizing.QueenBee.NeighborWeight>
      <Minimizing.QueenBee.InHandWeight>-0.035653251778258156</Minimizing.QueenBee.InHandWeight>
      <Minimizing.QueenBee.InPlayWeight>-0.0045806665423302013</Minimizing.QueenBee.InPlayWeight>
      <Minimizing.QueenBee.IsPinnedWeight>14.216617320492212</Minimizing.QueenBee.IsPinnedWeight>
      <Minimizing.Spider.ValidMoveWeight>0.0064143097876252428</Minimizing.Spider.ValidMoveWeight>
      <Minimizing.Spider.ValidPlacementWeight>-0.0049792065018354984</Minimizing.Spider.ValidPlacementWeight>
      <Minimizing.Spider.ValidMovementWeight>-0.011462825631937769</Minimizing.Spider.ValidMovementWeight>
      <Minimizing.Spider.NeighborWeight>-0.013800993036497613</Minimizing.Spider.NeighborWeight>
      <Minimizing.Spider.InHandWeight>-0.015644975846404414</Minimizing.Spider.InHandWeight>
      <Minimizing.Spider.InPlayWeight>-3.4159845682926752E-05</Minimizing.Spider.InPlayWeight>
      <Minimizing.Spider.IsPinnedWeight>0.0094383107664727482</Minimizing.Spider.IsPinnedWeight>
      <Minimizing.Beetle.ValidMoveWeight>0.0063796902053726134</Minimizing.Beetle.ValidMoveWeight>
      <Minimizing.Beetle.ValidPlacementWeight>-0.0025825750986148812</Minimizing.Beetle.ValidPlacementWeight>
      <Minimizing.Beetle.ValidMovementWeight>0.01303653838596304</Minimizing.Beetle.ValidMovementWeight>
      <Minimizing.Beetle.NeighborWeight>-0.013069990195608972</Minimizing.Beetle.NeighborWeight>
      <Minimizing.Beetle.InHandWeight>0.021026804418119802</Minimizing.Beetle.InHandWeight>
      <Minimizing.Beetle.InPlayWeight>-0.0009648618490232842</Minimizing.Beetle.InPlayWeight>
      <Minimizing.Beetle.IsPinnedWeight>0.0069884792351129167</Minimizing.Beetle.IsPinnedWeight>
      <Minimizing.Grasshopper.ValidMoveWeight>-0.0097494968830894868</Minimizing.Grasshopper.ValidMoveWeight>
      <Minimizing.Grasshopper.ValidPlacementWeight>0.0094679858571237539</Minimizing.Grasshopper.ValidPlacementWeight>
      <Minimizing.Grasshopper.ValidMovementWeight>0.011946528882566934</Minimizing.Grasshopper.ValidMovementWeight>
      <Minimizing.Grasshopper.NeighborWeight>-0.0028858735589876784</Minimizing.Grasshopper.NeighborWeight>
      <Minimizing.Grasshopper.InHandWeight>0.011077948209145223</Minimizing.Grasshopper.InHandWeight>
      <Minimizing.Grasshopper.InPlayWeight>0.00710516719146759</Minimizing.Grasshopper.InPlayWeight>
      <Minimizing.Grasshopper.IsPinnedWeight>0.015815203992212375</Minimizing.Grasshopper.IsPinnedWeight>
      <Minimizing.SoldierAnt.ValidMoveWeight>-0.02605416275201803</Minimizing.SoldierAnt.ValidMoveWeight>
      <Minimizing.SoldierAnt.ValidPlacementWeight>0.00014155087808603108</Minimizing.SoldierAnt.ValidPlacementWeight>
      <Minimizing.SoldierAnt.ValidMovementWeight>-0.00656443772969133</Minimizing.SoldierAnt.ValidMovementWeight>
      <Minimizing.SoldierAnt.NeighborWeight>0.0040260566514119765</Minimizing.SoldierAnt.NeighborWeight>
      <Minimizing.SoldierAnt.InHandWeight>0.019652082448312468</Minimizing.SoldierAnt.InHandWeight>
      <Minimizing.SoldierAnt.InPlayWeight>0.0039818408109889864</Minimizing.SoldierAnt.InPlayWeight>
      <Minimizing.SoldierAnt.IsPinnedWeight>-0.01393445757185236</Minimizing.SoldierAnt.IsPinnedWeight>
    </MetricWeights>
  </GameAI>
</Mzinga.Engine>";
    }
}
