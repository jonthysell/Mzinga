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
<DrawScore>0</DrawScore>
<Maximizing.ValidMoveWeight>27.018259324766998</Maximizing.ValidMoveWeight>
<Maximizing.ValidPlacementWeight>-11.043353202640006</Maximizing.ValidPlacementWeight>
<Maximizing.ValidMovementWeight>760.58987749743551</Maximizing.ValidMovementWeight>
<Maximizing.InHandWeight>17.0189584572698</Maximizing.InHandWeight>
<Maximizing.InPlayWeight>-6.18087236267368</Maximizing.InPlayWeight>
<Maximizing.IsPinnedWeight>-2.6257060895223661</Maximizing.IsPinnedWeight>
<Maximizing.QueenBee.ValidMoveWeight>-6.8601742718251248</Maximizing.QueenBee.ValidMoveWeight>
<Maximizing.QueenBee.ValidPlacementWeight>-450.60509364258633</Maximizing.QueenBee.ValidPlacementWeight>
<Maximizing.QueenBee.ValidMovementWeight>-47.937233421346917</Maximizing.QueenBee.ValidMovementWeight>
<Maximizing.QueenBee.NeighborWeight>-128428.20275061096</Maximizing.QueenBee.NeighborWeight>
<Maximizing.QueenBee.InHandWeight>0.18927507248436654</Maximizing.QueenBee.InHandWeight>
<Maximizing.QueenBee.InPlayWeight>24.301291922544284</Maximizing.QueenBee.InPlayWeight>
<Maximizing.QueenBee.IsPinnedWeight>668621.68255169352</Maximizing.QueenBee.IsPinnedWeight>
<Maximizing.Spider.ValidMoveWeight>-0.080975329511559641</Maximizing.Spider.ValidMoveWeight>
<Maximizing.Spider.ValidPlacementWeight>103.55929864050766</Maximizing.Spider.ValidPlacementWeight>
<Maximizing.Spider.ValidMovementWeight>-29.064772177509496</Maximizing.Spider.ValidMovementWeight>
<Maximizing.Spider.NeighborWeight>-256.85125595788827</Maximizing.Spider.NeighborWeight>
<Maximizing.Spider.InHandWeight>313.0998524247309</Maximizing.Spider.InHandWeight>
<Maximizing.Spider.InPlayWeight>14.722000691077595</Maximizing.Spider.InPlayWeight>
<Maximizing.Spider.IsPinnedWeight>-66.874358401643562</Maximizing.Spider.IsPinnedWeight>
<Maximizing.Beetle.ValidMoveWeight>6.7416656736225375</Maximizing.Beetle.ValidMoveWeight>
<Maximizing.Beetle.ValidPlacementWeight>99.397554005586315</Maximizing.Beetle.ValidPlacementWeight>
<Maximizing.Beetle.ValidMovementWeight>0.27541639214097163</Maximizing.Beetle.ValidMovementWeight>
<Maximizing.Beetle.NeighborWeight>1517.909050500883</Maximizing.Beetle.NeighborWeight>
<Maximizing.Beetle.InHandWeight>769.71483085658758</Maximizing.Beetle.InHandWeight>
<Maximizing.Beetle.InPlayWeight>-174.4059259701402</Maximizing.Beetle.InPlayWeight>
<Maximizing.Beetle.IsPinnedWeight>236.28911020453538</Maximizing.Beetle.IsPinnedWeight>
<Maximizing.Grasshopper.ValidMoveWeight>721.6438333467438</Maximizing.Grasshopper.ValidMoveWeight>
<Maximizing.Grasshopper.ValidPlacementWeight>-189.57365771285</Maximizing.Grasshopper.ValidPlacementWeight>
<Maximizing.Grasshopper.ValidMovementWeight>125.51365703672799</Maximizing.Grasshopper.ValidMovementWeight>
<Maximizing.Grasshopper.NeighborWeight>74.104622804593831</Maximizing.Grasshopper.NeighborWeight>
<Maximizing.Grasshopper.InHandWeight>-0.8256603193015748</Maximizing.Grasshopper.InHandWeight>
<Maximizing.Grasshopper.InPlayWeight>-3.5302684574054153</Maximizing.Grasshopper.InPlayWeight>
<Maximizing.Grasshopper.IsPinnedWeight>-9.8582246109277367</Maximizing.Grasshopper.IsPinnedWeight>
<Maximizing.SoldierAnt.ValidMoveWeight>-1167.4692720596233</Maximizing.SoldierAnt.ValidMoveWeight>
<Maximizing.SoldierAnt.ValidPlacementWeight>165.14579276335181</Maximizing.SoldierAnt.ValidPlacementWeight>
<Maximizing.SoldierAnt.ValidMovementWeight>128.45939222391536</Maximizing.SoldierAnt.ValidMovementWeight>
<Maximizing.SoldierAnt.NeighborWeight>188.40863766680516</Maximizing.SoldierAnt.NeighborWeight>
<Maximizing.SoldierAnt.InHandWeight>-338.65009206248112</Maximizing.SoldierAnt.InHandWeight>
<Maximizing.SoldierAnt.InPlayWeight>-133.97657742376163</Maximizing.SoldierAnt.InPlayWeight>
<Maximizing.SoldierAnt.IsPinnedWeight>-1791.6614051244212</Maximizing.SoldierAnt.IsPinnedWeight>
<Minimizing.ValidMoveWeight>-33.616812257131308</Minimizing.ValidMoveWeight>
<Minimizing.ValidPlacementWeight>-2602.2348339537089</Minimizing.ValidPlacementWeight>
<Minimizing.ValidMovementWeight>-20.166464946887366</Minimizing.ValidMovementWeight>
<Minimizing.InHandWeight>-558.5190135678514</Minimizing.InHandWeight>
<Minimizing.InPlayWeight>237.49474274452365</Minimizing.InPlayWeight>
<Minimizing.IsPinnedWeight>-123.61907253629022</Minimizing.IsPinnedWeight>
<Minimizing.QueenBee.ValidMoveWeight>3.5774152502816916</Minimizing.QueenBee.ValidMoveWeight>
<Minimizing.QueenBee.ValidPlacementWeight>-1965.7697299768142</Minimizing.QueenBee.ValidPlacementWeight>
<Minimizing.QueenBee.ValidMovementWeight>-43.910936513635981</Minimizing.QueenBee.ValidMovementWeight>
<Minimizing.QueenBee.NeighborWeight>1403620.2246576713</Minimizing.QueenBee.NeighborWeight>
<Minimizing.QueenBee.InHandWeight>-0.1366346066664903</Minimizing.QueenBee.InHandWeight>
<Minimizing.QueenBee.InPlayWeight>93.068237266367</Minimizing.QueenBee.InPlayWeight>
<Minimizing.QueenBee.IsPinnedWeight>-16795.144389149915</Minimizing.QueenBee.IsPinnedWeight>
<Minimizing.Spider.ValidMoveWeight>-103.096168497753</Minimizing.Spider.ValidMoveWeight>
<Minimizing.Spider.ValidPlacementWeight>-19.099108024866936</Minimizing.Spider.ValidPlacementWeight>
<Minimizing.Spider.ValidMovementWeight>-58.288662016901206</Minimizing.Spider.ValidMovementWeight>
<Minimizing.Spider.NeighborWeight>-27.621425996219369</Minimizing.Spider.NeighborWeight>
<Minimizing.Spider.InHandWeight>-868.44454011547646</Minimizing.Spider.InHandWeight>
<Minimizing.Spider.InPlayWeight>3.7575389195089062</Minimizing.Spider.InPlayWeight>
<Minimizing.Spider.IsPinnedWeight>-58.905371339810749</Minimizing.Spider.IsPinnedWeight>
<Minimizing.Beetle.ValidMoveWeight>-372.29773338073204</Minimizing.Beetle.ValidMoveWeight>
<Minimizing.Beetle.ValidPlacementWeight>2.5592771668995407</Minimizing.Beetle.ValidPlacementWeight>
<Minimizing.Beetle.ValidMovementWeight>750.01150489093925</Minimizing.Beetle.ValidMovementWeight>
<Minimizing.Beetle.NeighborWeight>-30.883492130882818</Minimizing.Beetle.NeighborWeight>
<Minimizing.Beetle.InHandWeight>-1793.2169519023887</Minimizing.Beetle.InHandWeight>
<Minimizing.Beetle.InPlayWeight>72.034093074544259</Minimizing.Beetle.InPlayWeight>
<Minimizing.Beetle.IsPinnedWeight>-63.075080222168559</Minimizing.Beetle.IsPinnedWeight>
<Minimizing.Grasshopper.ValidMoveWeight>-4741.0773827144367</Minimizing.Grasshopper.ValidMoveWeight>
<Minimizing.Grasshopper.ValidPlacementWeight>-632.38614158259475</Minimizing.Grasshopper.ValidPlacementWeight>
<Minimizing.Grasshopper.ValidMovementWeight>571.38659374783265</Minimizing.Grasshopper.ValidMovementWeight>
<Minimizing.Grasshopper.NeighborWeight>0.51109319662020181</Minimizing.Grasshopper.NeighborWeight>
<Minimizing.Grasshopper.InHandWeight>43.054247296011248</Minimizing.Grasshopper.InHandWeight>
<Minimizing.Grasshopper.InPlayWeight>46.886594526513917</Minimizing.Grasshopper.InPlayWeight>
<Minimizing.Grasshopper.IsPinnedWeight>-1013.2750725707266</Minimizing.Grasshopper.IsPinnedWeight>
<Minimizing.SoldierAnt.ValidMoveWeight>137.4495118493125</Minimizing.SoldierAnt.ValidMoveWeight>
<Minimizing.SoldierAnt.ValidPlacementWeight>83.042819530247428</Minimizing.SoldierAnt.ValidPlacementWeight>
<Minimizing.SoldierAnt.ValidMovementWeight>-87.736913219082922</Minimizing.SoldierAnt.ValidMovementWeight>
<Minimizing.SoldierAnt.NeighborWeight>-7.5780165125198078</Minimizing.SoldierAnt.NeighborWeight>
<Minimizing.SoldierAnt.InHandWeight>0.44134011143533142</Minimizing.SoldierAnt.InHandWeight>
<Minimizing.SoldierAnt.InPlayWeight>-5.950042285223839</Minimizing.SoldierAnt.InPlayWeight>
<Minimizing.SoldierAnt.IsPinnedWeight>-444.11053384284452</Minimizing.SoldierAnt.IsPinnedWeight>
</MetricWeights>
</GameAI>
</Mzinga.Engine>
";
    }
}
