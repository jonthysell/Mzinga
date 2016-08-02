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
        <Maximizing.ValidMoveWeight>0.02809750656965658</Maximizing.ValidMoveWeight>
        <Maximizing.ValidPlacementWeight>-0.0061365433960202248</Maximizing.ValidPlacementWeight>
        <Maximizing.ValidMovementWeight>0.010339747455031408</Maximizing.ValidMovementWeight>
        <Maximizing.InHandWeight>0.0027393638152633632</Maximizing.InHandWeight>
        <Maximizing.InPlayWeight>0.0027812928841757407</Maximizing.InPlayWeight>
        <Maximizing.IsPinnedWeight>-0.00073174715100424192</Maximizing.IsPinnedWeight>
        <Maximizing.QueenBee.ValidMoveWeight>-0.0076925729740117271</Maximizing.QueenBee.ValidMoveWeight>
        <Maximizing.QueenBee.ValidPlacementWeight>0.034200044899190915</Maximizing.QueenBee.ValidPlacementWeight>
        <Maximizing.QueenBee.ValidMovementWeight>0.0065938071593963339</Maximizing.QueenBee.ValidMovementWeight>
        <Maximizing.QueenBee.NeighborWeight>-2.3120484482935493</Maximizing.QueenBee.NeighborWeight>
        <Maximizing.QueenBee.InHandWeight>-0.0086340900937613915</Maximizing.QueenBee.InHandWeight>
        <Maximizing.QueenBee.InPlayWeight>0.016887913824186251</Maximizing.QueenBee.InPlayWeight>
        <Maximizing.QueenBee.IsPinnedWeight>-7.8398240066657667</Maximizing.QueenBee.IsPinnedWeight>
        <Maximizing.Spider.ValidMoveWeight>0.0040534230454665771</Maximizing.Spider.ValidMoveWeight>
        <Maximizing.Spider.ValidPlacementWeight>0.0074408381785475907</Maximizing.Spider.ValidPlacementWeight>
        <Maximizing.Spider.ValidMovementWeight>-0.0027814807783737888</Maximizing.Spider.ValidMovementWeight>
        <Maximizing.Spider.NeighborWeight>-0.01202255400263998</Maximizing.Spider.NeighborWeight>
        <Maximizing.Spider.InHandWeight>-0.038491497378629107</Maximizing.Spider.InHandWeight>
        <Maximizing.Spider.InPlayWeight>0.009208797630838041</Maximizing.Spider.InPlayWeight>
        <Maximizing.Spider.IsPinnedWeight>-0.011257148687863957</Maximizing.Spider.IsPinnedWeight>
        <Maximizing.Beetle.ValidMoveWeight>-0.0010636373457851825</Maximizing.Beetle.ValidMoveWeight>
        <Maximizing.Beetle.ValidPlacementWeight>0.00891445245951275</Maximizing.Beetle.ValidPlacementWeight>
        <Maximizing.Beetle.ValidMovementWeight>0.00029855607461279354</Maximizing.Beetle.ValidMovementWeight>
        <Maximizing.Beetle.NeighborWeight>0.019411940334110138</Maximizing.Beetle.NeighborWeight>
        <Maximizing.Beetle.InHandWeight>-0.0017302976945767845</Maximizing.Beetle.InHandWeight>
        <Maximizing.Beetle.InPlayWeight>-0.0040802299890450771</Maximizing.Beetle.InPlayWeight>
        <Maximizing.Beetle.IsPinnedWeight>-0.0072107363922860679</Maximizing.Beetle.IsPinnedWeight>
        <Maximizing.Grasshopper.ValidMoveWeight>-0.0010277287236470146</Maximizing.Grasshopper.ValidMoveWeight>
        <Maximizing.Grasshopper.ValidPlacementWeight>-0.0133395239751668</Maximizing.Grasshopper.ValidPlacementWeight>
        <Maximizing.Grasshopper.ValidMovementWeight>0.017366397481401657</Maximizing.Grasshopper.ValidMovementWeight>
        <Maximizing.Grasshopper.NeighborWeight>-0.0072239838657965965</Maximizing.Grasshopper.NeighborWeight>
        <Maximizing.Grasshopper.InHandWeight>0.0053723550508983052</Maximizing.Grasshopper.InHandWeight>
        <Maximizing.Grasshopper.InPlayWeight>0.0020436143103255344</Maximizing.Grasshopper.InPlayWeight>
        <Maximizing.Grasshopper.IsPinnedWeight>-0.005898384872651887</Maximizing.Grasshopper.IsPinnedWeight>
        <Maximizing.SoldierAnt.ValidMoveWeight>0.0045718796534641006</Maximizing.SoldierAnt.ValidMoveWeight>
        <Maximizing.SoldierAnt.ValidPlacementWeight>0.013523925310881053</Maximizing.SoldierAnt.ValidPlacementWeight>
        <Maximizing.SoldierAnt.ValidMovementWeight>0.011785075722654699</Maximizing.SoldierAnt.ValidMovementWeight>
        <Maximizing.SoldierAnt.NeighborWeight>-0.0099768151537607177</Maximizing.SoldierAnt.NeighborWeight>
        <Maximizing.SoldierAnt.InHandWeight>-0.0052617912001936013</Maximizing.SoldierAnt.InHandWeight>
        <Maximizing.SoldierAnt.InPlayWeight>-0.00037571252692481828</Maximizing.SoldierAnt.InPlayWeight>
        <Maximizing.SoldierAnt.IsPinnedWeight>0.014321897230270064</Maximizing.SoldierAnt.IsPinnedWeight>
        <Minimizing.ValidMoveWeight>-0.0082567451590409985</Minimizing.ValidMoveWeight>
        <Minimizing.ValidPlacementWeight>0.011051570582664289</Minimizing.ValidPlacementWeight>
        <Minimizing.ValidMovementWeight>0.0062394200164679024</Minimizing.ValidMovementWeight>
        <Minimizing.InHandWeight>0.012805794522151411</Minimizing.InHandWeight>
        <Minimizing.InPlayWeight>0.00796481180761398</Minimizing.InPlayWeight>
        <Minimizing.IsPinnedWeight>-0.00937129377764508</Minimizing.IsPinnedWeight>
        <Minimizing.QueenBee.ValidMoveWeight>0.0034471520625101249</Minimizing.QueenBee.ValidMoveWeight>
        <Minimizing.QueenBee.ValidPlacementWeight>-0.014625352223345191</Minimizing.QueenBee.ValidPlacementWeight>
        <Minimizing.QueenBee.ValidMovementWeight>-0.0024275801902529891</Minimizing.QueenBee.ValidMovementWeight>
        <Minimizing.QueenBee.NeighborWeight>11.892427683103371</Minimizing.QueenBee.NeighborWeight>
        <Minimizing.QueenBee.InHandWeight>-0.026207930274245107</Minimizing.QueenBee.InHandWeight>
        <Minimizing.QueenBee.InPlayWeight>0.0033556735740320172</Minimizing.QueenBee.InPlayWeight>
        <Minimizing.QueenBee.IsPinnedWeight>8.9719184950258217</Minimizing.QueenBee.IsPinnedWeight>
        <Minimizing.Spider.ValidMoveWeight>0.0017786718032545921</Minimizing.Spider.ValidMoveWeight>
        <Minimizing.Spider.ValidPlacementWeight>-0.0011055690540401537</Minimizing.Spider.ValidPlacementWeight>
        <Minimizing.Spider.ValidMovementWeight>-0.011618508506448704</Minimizing.Spider.ValidMovementWeight>
        <Minimizing.Spider.NeighborWeight>-0.0071821347353003</Minimizing.Spider.NeighborWeight>
        <Minimizing.Spider.InHandWeight>-0.0083220754245768025</Minimizing.Spider.InHandWeight>
        <Minimizing.Spider.InPlayWeight>0.00028266566075747784</Minimizing.Spider.InPlayWeight>
        <Minimizing.Spider.IsPinnedWeight>0.0018106536009562743</Minimizing.Spider.IsPinnedWeight>
        <Minimizing.Beetle.ValidMoveWeight>0.0023837752753567403</Minimizing.Beetle.ValidMoveWeight>
        <Minimizing.Beetle.ValidPlacementWeight>0.00094178609582484839</Minimizing.Beetle.ValidPlacementWeight>
        <Minimizing.Beetle.ValidMovementWeight>0.044626224872722373</Minimizing.Beetle.ValidMovementWeight>
        <Minimizing.Beetle.NeighborWeight>-0.0036250004550346074</Minimizing.Beetle.NeighborWeight>
        <Minimizing.Beetle.InHandWeight>0.020513976145292951</Minimizing.Beetle.InHandWeight>
        <Minimizing.Beetle.InPlayWeight>-0.0074635961603849764</Minimizing.Beetle.InPlayWeight>
        <Minimizing.Beetle.IsPinnedWeight>0.0033458919808286277</Minimizing.Beetle.IsPinnedWeight>
        <Minimizing.Grasshopper.ValidMoveWeight>-0.019634307847590373</Minimizing.Grasshopper.ValidMoveWeight>
        <Minimizing.Grasshopper.ValidPlacementWeight>0.0095395250447393633</Minimizing.Grasshopper.ValidPlacementWeight>
        <Minimizing.Grasshopper.ValidMovementWeight>0.012191704923887659</Minimizing.Grasshopper.ValidMovementWeight>
        <Minimizing.Grasshopper.NeighborWeight>-0.00038636098311640353</Minimizing.Grasshopper.NeighborWeight>
        <Minimizing.Grasshopper.InHandWeight>0.0059587211577319292</Minimizing.Grasshopper.InHandWeight>
        <Minimizing.Grasshopper.InPlayWeight>5.0156852319539591E-05</Minimizing.Grasshopper.InPlayWeight>
        <Minimizing.Grasshopper.IsPinnedWeight>0.0024634536138759345</Minimizing.Grasshopper.IsPinnedWeight>
        <Minimizing.SoldierAnt.ValidMoveWeight>-0.00632700202766207</Minimizing.SoldierAnt.ValidMoveWeight>
        <Minimizing.SoldierAnt.ValidPlacementWeight>-0.013573865621083444</Minimizing.SoldierAnt.ValidPlacementWeight>
        <Minimizing.SoldierAnt.ValidMovementWeight>-0.00749180190611555</Minimizing.SoldierAnt.ValidMovementWeight>
        <Minimizing.SoldierAnt.NeighborWeight>-0.0015811856631576947</Minimizing.SoldierAnt.NeighborWeight>
        <Minimizing.SoldierAnt.InHandWeight>0.042668983774822408</Minimizing.SoldierAnt.InHandWeight>
        <Minimizing.SoldierAnt.InPlayWeight>0.0043379113101147749</Minimizing.SoldierAnt.InPlayWeight>
        <Minimizing.SoldierAnt.IsPinnedWeight>-0.013266942503578898</Minimizing.SoldierAnt.IsPinnedWeight>
    </MetricWeights>
  </GameAI>
</Mzinga.Engine>
";
    }
}
