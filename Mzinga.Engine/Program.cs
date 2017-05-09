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
<Maximizing.ValidMoveWeight>-0.075498979525094087</Maximizing.ValidMoveWeight>
<Maximizing.ValidPlacementWeight>-0.14202093136339761</Maximizing.ValidPlacementWeight>
<Maximizing.ValidMovementWeight>-0.34601947869445787</Maximizing.ValidMovementWeight>
<Maximizing.InHandWeight>0.0090537986784408053</Maximizing.InHandWeight>
<Maximizing.InPlayWeight>-0.20045479052286633</Maximizing.InPlayWeight>
<Maximizing.IsPinnedWeight>-0.049414635353429777</Maximizing.IsPinnedWeight>
<Maximizing.QueenBee.ValidMoveWeight>-0.079230483143581734</Maximizing.QueenBee.ValidMoveWeight>
<Maximizing.QueenBee.ValidPlacementWeight>0.96326784018241807</Maximizing.QueenBee.ValidPlacementWeight>
<Maximizing.QueenBee.ValidMovementWeight>0.17169014322585022</Maximizing.QueenBee.ValidMovementWeight>
<Maximizing.QueenBee.NeighborWeight>-739.58631865147061</Maximizing.QueenBee.NeighborWeight>
<Maximizing.QueenBee.InHandWeight>0.66800735067949724</Maximizing.QueenBee.InHandWeight>
<Maximizing.QueenBee.InPlayWeight>-0.085063577682892688</Maximizing.QueenBee.InPlayWeight>
<Maximizing.QueenBee.IsPinnedWeight>10.351344543744999</Maximizing.QueenBee.IsPinnedWeight>
<Maximizing.Spider.ValidMoveWeight>0.063462033031659537</Maximizing.Spider.ValidMoveWeight>
<Maximizing.Spider.ValidPlacementWeight>-4.7389391078633745</Maximizing.Spider.ValidPlacementWeight>
<Maximizing.Spider.ValidMovementWeight>0.011377167303786054</Maximizing.Spider.ValidMovementWeight>
<Maximizing.Spider.NeighborWeight>-8.45661778635653</Maximizing.Spider.NeighborWeight>
<Maximizing.Spider.InHandWeight>0.952579441447011</Maximizing.Spider.InHandWeight>
<Maximizing.Spider.InPlayWeight>-2.7481064300557563</Maximizing.Spider.InPlayWeight>
<Maximizing.Spider.IsPinnedWeight>-0.69529101410510719</Maximizing.Spider.IsPinnedWeight>
<Maximizing.Beetle.ValidMoveWeight>0.003765849523447537</Maximizing.Beetle.ValidMoveWeight>
<Maximizing.Beetle.ValidPlacementWeight>-1.7489242310263748</Maximizing.Beetle.ValidPlacementWeight>
<Maximizing.Beetle.ValidMovementWeight>-0.024355097980445779</Maximizing.Beetle.ValidMovementWeight>
<Maximizing.Beetle.NeighborWeight>4.3652927041450535</Maximizing.Beetle.NeighborWeight>
<Maximizing.Beetle.InHandWeight>0.021555515298245325</Maximizing.Beetle.InHandWeight>
<Maximizing.Beetle.InPlayWeight>-0.99360217388415517</Maximizing.Beetle.InPlayWeight>
<Maximizing.Beetle.IsPinnedWeight>-0.56276142856439137</Maximizing.Beetle.IsPinnedWeight>
<Maximizing.Grasshopper.ValidMoveWeight>0.18812298228619015</Maximizing.Grasshopper.ValidMoveWeight>
<Maximizing.Grasshopper.ValidPlacementWeight>-0.0041418995104384954</Maximizing.Grasshopper.ValidPlacementWeight>
<Maximizing.Grasshopper.ValidMovementWeight>3.5072659350938067</Maximizing.Grasshopper.ValidMovementWeight>
<Maximizing.Grasshopper.NeighborWeight>0.096992394592799444</Maximizing.Grasshopper.NeighborWeight>
<Maximizing.Grasshopper.InHandWeight>0.85724523959428267</Maximizing.Grasshopper.InHandWeight>
<Maximizing.Grasshopper.InPlayWeight>0.034743331848925522</Maximizing.Grasshopper.InPlayWeight>
<Maximizing.Grasshopper.IsPinnedWeight>1.9897454741714549</Maximizing.Grasshopper.IsPinnedWeight>
<Maximizing.SoldierAnt.ValidMoveWeight>0.92583343413758135</Maximizing.SoldierAnt.ValidMoveWeight>
<Maximizing.SoldierAnt.ValidPlacementWeight>0.31100406658912494</Maximizing.SoldierAnt.ValidPlacementWeight>
<Maximizing.SoldierAnt.ValidMovementWeight>1.4088853720734798</Maximizing.SoldierAnt.ValidMovementWeight>
<Maximizing.SoldierAnt.NeighborWeight>-0.30667695965706354</Maximizing.SoldierAnt.NeighborWeight>
<Maximizing.SoldierAnt.InHandWeight>1.2758077144197426</Maximizing.SoldierAnt.InHandWeight>
<Maximizing.SoldierAnt.InPlayWeight>0.0470243463776905</Maximizing.SoldierAnt.InPlayWeight>
<Maximizing.SoldierAnt.IsPinnedWeight>4.194908848724535</Maximizing.SoldierAnt.IsPinnedWeight>
<Minimizing.ValidMoveWeight>-1.869665823654745</Minimizing.ValidMoveWeight>
<Minimizing.ValidPlacementWeight>4.0573103561793644</Minimizing.ValidPlacementWeight>
<Minimizing.ValidMovementWeight>-0.36027022962172889</Minimizing.ValidMovementWeight>
<Minimizing.InHandWeight>0.40869008700934417</Minimizing.InHandWeight>
<Minimizing.InPlayWeight>2.8627208848308241</Minimizing.InPlayWeight>
<Minimizing.IsPinnedWeight>1.132811878839773</Minimizing.IsPinnedWeight>
<Minimizing.QueenBee.ValidMoveWeight>0.12874060754497368</Minimizing.QueenBee.ValidMoveWeight>
<Minimizing.QueenBee.ValidPlacementWeight>1.1445122137434838</Minimizing.QueenBee.ValidPlacementWeight>
<Minimizing.QueenBee.ValidMovementWeight>0.08861736634528522</Minimizing.QueenBee.ValidMovementWeight>
<Minimizing.QueenBee.NeighborWeight>4109.7933103188579</Minimizing.QueenBee.NeighborWeight>
<Minimizing.QueenBee.InHandWeight>-0.35450704319100212</Minimizing.QueenBee.InHandWeight>
<Minimizing.QueenBee.InPlayWeight>0.0046116358532150258</Minimizing.QueenBee.InPlayWeight>
<Minimizing.QueenBee.IsPinnedWeight>17.11963323212596</Minimizing.QueenBee.IsPinnedWeight>
<Minimizing.Spider.ValidMoveWeight>0.0029985500151337875</Minimizing.Spider.ValidMoveWeight>
<Minimizing.Spider.ValidPlacementWeight>0.22193076292434746</Minimizing.Spider.ValidPlacementWeight>
<Minimizing.Spider.ValidMovementWeight>-1.7294176665397978</Minimizing.Spider.ValidMovementWeight>
<Minimizing.Spider.NeighborWeight>-0.79355880279707836</Minimizing.Spider.NeighborWeight>
<Minimizing.Spider.InHandWeight>-0.7637216472264996</Minimizing.Spider.InHandWeight>
<Minimizing.Spider.InPlayWeight>-0.039959961702868627</Minimizing.Spider.InPlayWeight>
<Minimizing.Spider.IsPinnedWeight>-0.33212180711454059</Minimizing.Spider.IsPinnedWeight>
<Minimizing.Beetle.ValidMoveWeight>-0.10267541594485208</Minimizing.Beetle.ValidMoveWeight>
<Minimizing.Beetle.ValidPlacementWeight>0.16163404180225582</Minimizing.Beetle.ValidPlacementWeight>
<Minimizing.Beetle.ValidMovementWeight>-4.3429937974595862</Minimizing.Beetle.ValidMovementWeight>
<Minimizing.Beetle.NeighborWeight>0.17569293016338353</Minimizing.Beetle.NeighborWeight>
<Minimizing.Beetle.InHandWeight>0.029227761302961831</Minimizing.Beetle.InHandWeight>
<Minimizing.Beetle.InPlayWeight>-0.081875817176838672</Minimizing.Beetle.InPlayWeight>
<Minimizing.Beetle.IsPinnedWeight>-0.857575516367572</Minimizing.Beetle.IsPinnedWeight>
<Minimizing.Grasshopper.ValidMoveWeight>-11.881129648623469</Minimizing.Grasshopper.ValidMoveWeight>
<Minimizing.Grasshopper.ValidPlacementWeight>0.666412762731396</Minimizing.Grasshopper.ValidPlacementWeight>
<Minimizing.Grasshopper.ValidMovementWeight>0.040666647328229021</Minimizing.Grasshopper.ValidMovementWeight>
<Minimizing.Grasshopper.NeighborWeight>-0.020970753076712884</Minimizing.Grasshopper.NeighborWeight>
<Minimizing.Grasshopper.InHandWeight>-0.014843832945330981</Minimizing.Grasshopper.InHandWeight>
<Minimizing.Grasshopper.InPlayWeight>-0.00539804572564186</Minimizing.Grasshopper.InPlayWeight>
<Minimizing.Grasshopper.IsPinnedWeight>0.099050919081589514</Minimizing.Grasshopper.IsPinnedWeight>
<Minimizing.SoldierAnt.ValidMoveWeight>-0.70124813535602</Minimizing.SoldierAnt.ValidMoveWeight>
<Minimizing.SoldierAnt.ValidPlacementWeight>-2.83654640675534</Minimizing.SoldierAnt.ValidPlacementWeight>
<Minimizing.SoldierAnt.ValidMovementWeight>0.34764491447931306</Minimizing.SoldierAnt.ValidMovementWeight>
<Minimizing.SoldierAnt.NeighborWeight>0.0044933494886377863</Minimizing.SoldierAnt.NeighborWeight>
<Minimizing.SoldierAnt.InHandWeight>-0.32509664218038609</Minimizing.SoldierAnt.InHandWeight>
<Minimizing.SoldierAnt.InPlayWeight>-0.17995971583134421</Minimizing.SoldierAnt.InPlayWeight>
<Minimizing.SoldierAnt.IsPinnedWeight>-1.5886067340435788</Minimizing.SoldierAnt.IsPinnedWeight>
</MetricWeights>
</GameAI>
</Mzinga.Engine>
";
    }
}
