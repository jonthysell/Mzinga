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
        <DrawScore>0</DrawScore>
        <Maximizing.ValidMoveWeight>0.1242347188643563</Maximizing.ValidMoveWeight>
        <Maximizing.ValidPlacementWeight>-0.01155535895973704</Maximizing.ValidPlacementWeight>
        <Maximizing.ValidMovementWeight>0.00990221700188951</Maximizing.ValidMovementWeight>
        <Maximizing.InHandWeight>0.013258797569237457</Maximizing.InHandWeight>
        <Maximizing.InPlayWeight>0.001105154324503459</Maximizing.InPlayWeight>
        <Maximizing.IsPinnedWeight>-0.0059374028173930764</Maximizing.IsPinnedWeight>
        <Maximizing.QueenBee.ValidMoveWeight>-0.009571272373658804</Maximizing.QueenBee.ValidMoveWeight>
        <Maximizing.QueenBee.ValidPlacementWeight>0.014318855766040377</Maximizing.QueenBee.ValidPlacementWeight>
        <Maximizing.QueenBee.ValidMovementWeight>0.0023705289484500093</Maximizing.QueenBee.ValidMovementWeight>
        <Maximizing.QueenBee.NeighborWeight>-13.985783279095095</Maximizing.QueenBee.NeighborWeight>
        <Maximizing.QueenBee.InHandWeight>-0.012178801204762495</Maximizing.QueenBee.InHandWeight>
        <Maximizing.QueenBee.InPlayWeight>0.0051891719759048025</Maximizing.QueenBee.InPlayWeight>
        <Maximizing.QueenBee.IsPinnedWeight>-13.414078453645901</Maximizing.QueenBee.IsPinnedWeight>
        <Maximizing.Spider.ValidMoveWeight>0.0052854962145531346</Maximizing.Spider.ValidMoveWeight>
        <Maximizing.Spider.ValidPlacementWeight>0.004417309281310869</Maximizing.Spider.ValidPlacementWeight>
        <Maximizing.Spider.ValidMovementWeight>-0.0011778469029668968</Maximizing.Spider.ValidMovementWeight>
        <Maximizing.Spider.NeighborWeight>-0.018023357378927209</Maximizing.Spider.NeighborWeight>
        <Maximizing.Spider.InHandWeight>-0.021455406778364917</Maximizing.Spider.InHandWeight>
        <Maximizing.Spider.InPlayWeight>0.011981582250603669</Maximizing.Spider.InPlayWeight>
        <Maximizing.Spider.IsPinnedWeight>-0.0067160679973202169</Maximizing.Spider.IsPinnedWeight>
        <Maximizing.Beetle.ValidMoveWeight>0.0027569808570720826</Maximizing.Beetle.ValidMoveWeight>
        <Maximizing.Beetle.ValidPlacementWeight>0.0015626336142793259</Maximizing.Beetle.ValidPlacementWeight>
        <Maximizing.Beetle.ValidMovementWeight>0.0074199536344020239</Maximizing.Beetle.ValidMovementWeight>
        <Maximizing.Beetle.NeighborWeight>0.012799460102902928</Maximizing.Beetle.NeighborWeight>
        <Maximizing.Beetle.InHandWeight>-0.0059400391148987279</Maximizing.Beetle.InHandWeight>
        <Maximizing.Beetle.InPlayWeight>-0.0024578200897982139</Maximizing.Beetle.InPlayWeight>
        <Maximizing.Beetle.IsPinnedWeight>-0.0062104946993438749</Maximizing.Beetle.IsPinnedWeight>
        <Maximizing.Grasshopper.ValidMoveWeight>-0.010720891193040972</Maximizing.Grasshopper.ValidMoveWeight>
        <Maximizing.Grasshopper.ValidPlacementWeight>-0.016018821180522919</Maximizing.Grasshopper.ValidPlacementWeight>
        <Maximizing.Grasshopper.ValidMovementWeight>0.0092060316525219617</Maximizing.Grasshopper.ValidMovementWeight>
        <Maximizing.Grasshopper.NeighborWeight>-0.0047098800808348358</Maximizing.Grasshopper.NeighborWeight>
        <Maximizing.Grasshopper.InHandWeight>0.0058006661959405723</Maximizing.Grasshopper.InHandWeight>
        <Maximizing.Grasshopper.InPlayWeight>0.0012145235279384734</Maximizing.Grasshopper.InPlayWeight>
        <Maximizing.Grasshopper.IsPinnedWeight>-0.0054530581270101665</Maximizing.Grasshopper.IsPinnedWeight>
        <Maximizing.SoldierAnt.ValidMoveWeight>0.025679248030782916</Maximizing.SoldierAnt.ValidMoveWeight>
        <Maximizing.SoldierAnt.ValidPlacementWeight>0.036865434905624529</Maximizing.SoldierAnt.ValidPlacementWeight>
        <Maximizing.SoldierAnt.ValidMovementWeight>0.005339467055833178</Maximizing.SoldierAnt.ValidMovementWeight>
        <Maximizing.SoldierAnt.NeighborWeight>-0.013383508606971224</Maximizing.SoldierAnt.NeighborWeight>
        <Maximizing.SoldierAnt.InHandWeight>-0.0017501291224093717</Maximizing.SoldierAnt.InHandWeight>
        <Maximizing.SoldierAnt.InPlayWeight>0.0024547633145058975</Maximizing.SoldierAnt.InPlayWeight>
        <Maximizing.SoldierAnt.IsPinnedWeight>0.017570716800803767</Maximizing.SoldierAnt.IsPinnedWeight>
        <Minimizing.ValidMoveWeight>-0.0082693642496773842</Minimizing.ValidMoveWeight>
        <Minimizing.ValidPlacementWeight>0.010519233861257249</Minimizing.ValidPlacementWeight>
        <Minimizing.ValidMovementWeight>0.0042013956420442955</Minimizing.ValidMovementWeight>
        <Minimizing.InHandWeight>0.009067544530093891</Minimizing.InHandWeight>
        <Minimizing.InPlayWeight>0.011828588637987967</Minimizing.InPlayWeight>
        <Minimizing.IsPinnedWeight>-0.018186123940083686</Minimizing.IsPinnedWeight>
        <Minimizing.QueenBee.ValidMoveWeight>0.0098505798208627891</Minimizing.QueenBee.ValidMoveWeight>
        <Minimizing.QueenBee.ValidPlacementWeight>-0.01092337279032231</Minimizing.QueenBee.ValidPlacementWeight>
        <Minimizing.QueenBee.ValidMovementWeight>0.00023442975885728409</Minimizing.QueenBee.ValidMovementWeight>
        <Minimizing.QueenBee.NeighborWeight>23.700860936853516</Minimizing.QueenBee.NeighborWeight>
        <Minimizing.QueenBee.InHandWeight>-0.022944082841304531</Minimizing.QueenBee.InHandWeight>
        <Minimizing.QueenBee.InPlayWeight>-0.0022475983920182247</Minimizing.QueenBee.InPlayWeight>
        <Minimizing.QueenBee.IsPinnedWeight>12.0132737744951</Minimizing.QueenBee.IsPinnedWeight>
        <Minimizing.Spider.ValidMoveWeight>0.0037950214347492765</Minimizing.Spider.ValidMoveWeight>
        <Minimizing.Spider.ValidPlacementWeight>-0.0050475917416610205</Minimizing.Spider.ValidPlacementWeight>
        <Minimizing.Spider.ValidMovementWeight>-0.011259514991750156</Minimizing.Spider.ValidMovementWeight>
        <Minimizing.Spider.NeighborWeight>-0.00751235520374646</Minimizing.Spider.NeighborWeight>
        <Minimizing.Spider.InHandWeight>-0.019109244276011048</Minimizing.Spider.InHandWeight>
        <Minimizing.Spider.InPlayWeight>-0.0011410836286553528</Minimizing.Spider.InPlayWeight>
        <Minimizing.Spider.IsPinnedWeight>0.0048444778445037549</Minimizing.Spider.IsPinnedWeight>
        <Minimizing.Beetle.ValidMoveWeight>0.0075344355624903754</Minimizing.Beetle.ValidMoveWeight>
        <Minimizing.Beetle.ValidPlacementWeight>-0.001610225797932635</Minimizing.Beetle.ValidPlacementWeight>
        <Minimizing.Beetle.ValidMovementWeight>0.020061033930357835</Minimizing.Beetle.ValidMovementWeight>
        <Minimizing.Beetle.NeighborWeight>-0.010916657505675323</Minimizing.Beetle.NeighborWeight>
        <Minimizing.Beetle.InHandWeight>0.019028363500236324</Minimizing.Beetle.InHandWeight>
        <Minimizing.Beetle.InPlayWeight>-0.0022783231692421958</Minimizing.Beetle.InPlayWeight>
        <Minimizing.Beetle.IsPinnedWeight>0.010983099613386688</Minimizing.Beetle.IsPinnedWeight>
        <Minimizing.Grasshopper.ValidMoveWeight>-0.013522899773588292</Minimizing.Grasshopper.ValidMoveWeight>
        <Minimizing.Grasshopper.ValidPlacementWeight>0.012043606722355662</Minimizing.Grasshopper.ValidPlacementWeight>
        <Minimizing.Grasshopper.ValidMovementWeight>0.013678890096972266</Minimizing.Grasshopper.ValidMovementWeight>
        <Minimizing.Grasshopper.NeighborWeight>0.00034635366614876325</Minimizing.Grasshopper.NeighborWeight>
        <Minimizing.Grasshopper.InHandWeight>0.0093235630275692819</Minimizing.Grasshopper.InHandWeight>
        <Minimizing.Grasshopper.InPlayWeight>0.001622186447792822</Minimizing.Grasshopper.InPlayWeight>
        <Minimizing.Grasshopper.IsPinnedWeight>0.014304543199903784</Minimizing.Grasshopper.IsPinnedWeight>
        <Minimizing.SoldierAnt.ValidMoveWeight>-0.019706672588487427</Minimizing.SoldierAnt.ValidMoveWeight>
        <Minimizing.SoldierAnt.ValidPlacementWeight>-5.97859251308841E-05</Minimizing.SoldierAnt.ValidPlacementWeight>
        <Minimizing.SoldierAnt.ValidMovementWeight>-0.0054639996950789724</Minimizing.SoldierAnt.ValidMovementWeight>
        <Minimizing.SoldierAnt.NeighborWeight>-0.00041224159872584764</Minimizing.SoldierAnt.NeighborWeight>
        <Minimizing.SoldierAnt.InHandWeight>0.018736407181494028</Minimizing.SoldierAnt.InHandWeight>
        <Minimizing.SoldierAnt.InPlayWeight>0.0082911206577697561</Minimizing.SoldierAnt.InPlayWeight>
        <Minimizing.SoldierAnt.IsPinnedWeight>-0.014146347278111208</Minimizing.SoldierAnt.IsPinnedWeight>
      </MetricWeights>
  </GameAI>
</Mzinga.Engine>
";
    }
}
