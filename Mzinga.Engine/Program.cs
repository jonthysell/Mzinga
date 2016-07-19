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
  <MetricWeights>
    <DrawScore>0</DrawScore>
    <Maximizing.ValidMoveWeight>0.0730622305030215</Maximizing.ValidMoveWeight>
    <Maximizing.ValidPlacementWeight>-0.0057005864068919035</Maximizing.ValidPlacementWeight>
    <Maximizing.ValidMovementWeight>0.0055181860789034389</Maximizing.ValidMovementWeight>
    <Maximizing.InHandWeight>0.00683137667773351</Maximizing.InHandWeight>
    <Maximizing.InPlayWeight>0.0031722541165019563</Maximizing.InPlayWeight>
    <Maximizing.IsPinnedWeight>-0.00070901881734529524</Maximizing.IsPinnedWeight>
    <Maximizing.QueenBee.ValidMoveWeight>-0.0060081956311482526</Maximizing.QueenBee.ValidMoveWeight>
    <Maximizing.QueenBee.ValidPlacementWeight>0.020502095436187362</Maximizing.QueenBee.ValidPlacementWeight>
    <Maximizing.QueenBee.ValidMovementWeight>0.005531167302500797</Maximizing.QueenBee.ValidMovementWeight>
    <Maximizing.QueenBee.NeighborWeight>-13.396903232463007</Maximizing.QueenBee.NeighborWeight>
    <Maximizing.QueenBee.InHandWeight>-0.026584441765813663</Maximizing.QueenBee.InHandWeight>
    <Maximizing.QueenBee.InPlayWeight>0.0077841942220647159</Maximizing.QueenBee.InPlayWeight>
    <Maximizing.QueenBee.IsPinnedWeight>-7.6949682744937942</Maximizing.QueenBee.IsPinnedWeight>
    <Maximizing.Spider.ValidMoveWeight>0.0051778475844158336</Maximizing.Spider.ValidMoveWeight>
    <Maximizing.Spider.ValidPlacementWeight>0.0075798763621079237</Maximizing.Spider.ValidPlacementWeight>
    <Maximizing.Spider.ValidMovementWeight>-0.0035451106261418473</Maximizing.Spider.ValidMovementWeight>
    <Maximizing.Spider.NeighborWeight>-0.01030759409311738</Maximizing.Spider.NeighborWeight>
    <Maximizing.Spider.InHandWeight>-0.024332677813443553</Maximizing.Spider.InHandWeight>
    <Maximizing.Spider.InPlayWeight>0.018031718407303245</Maximizing.Spider.InPlayWeight>
    <Maximizing.Spider.IsPinnedWeight>-0.0088821146978510079</Maximizing.Spider.IsPinnedWeight>
    <Maximizing.Beetle.ValidMoveWeight>-0.0005034971487845011</Maximizing.Beetle.ValidMoveWeight>
    <Maximizing.Beetle.ValidPlacementWeight>0.00697310474888418</Maximizing.Beetle.ValidPlacementWeight>
    <Maximizing.Beetle.ValidMovementWeight>0.0011866727411911982</Maximizing.Beetle.ValidMovementWeight>
    <Maximizing.Beetle.NeighborWeight>0.019761299316873961</Maximizing.Beetle.NeighborWeight>
    <Maximizing.Beetle.InHandWeight>-0.002494042318119674</Maximizing.Beetle.InHandWeight>
    <Maximizing.Beetle.InPlayWeight>-0.0035227476169376092</Maximizing.Beetle.InPlayWeight>
    <Maximizing.Beetle.IsPinnedWeight>-0.0063600913865113334</Maximizing.Beetle.IsPinnedWeight>
    <Maximizing.Grasshopper.ValidMoveWeight>-0.0022766423300403927</Maximizing.Grasshopper.ValidMoveWeight>
    <Maximizing.Grasshopper.ValidPlacementWeight>-0.02410896100046309</Maximizing.Grasshopper.ValidPlacementWeight>
    <Maximizing.Grasshopper.ValidMovementWeight>0.014274075188712994</Maximizing.Grasshopper.ValidMovementWeight>
    <Maximizing.Grasshopper.NeighborWeight>-0.0049212761284813788</Maximizing.Grasshopper.NeighborWeight>
    <Maximizing.Grasshopper.InHandWeight>0.0084691609248581327</Maximizing.Grasshopper.InHandWeight>
    <Maximizing.Grasshopper.InPlayWeight>0.003694890413577046</Maximizing.Grasshopper.InPlayWeight>
    <Maximizing.Grasshopper.IsPinnedWeight>-0.0058793501848544783</Maximizing.Grasshopper.IsPinnedWeight>
    <Maximizing.SoldierAnt.ValidMoveWeight>0.022171528142277182</Maximizing.SoldierAnt.ValidMoveWeight>
    <Maximizing.SoldierAnt.ValidPlacementWeight>0.020085026589078722</Maximizing.SoldierAnt.ValidPlacementWeight>
    <Maximizing.SoldierAnt.ValidMovementWeight>0.012276527161455756</Maximizing.SoldierAnt.ValidMovementWeight>
    <Maximizing.SoldierAnt.NeighborWeight>-0.015565993056156363</Maximizing.SoldierAnt.NeighborWeight>
    <Maximizing.SoldierAnt.InHandWeight>-0.0053758654839923588</Maximizing.SoldierAnt.InHandWeight>
    <Maximizing.SoldierAnt.InPlayWeight>-0.00048367954976442565</Maximizing.SoldierAnt.InPlayWeight>
    <Maximizing.SoldierAnt.IsPinnedWeight>0.0155311521723164</Maximizing.SoldierAnt.IsPinnedWeight>
    <Minimizing.ValidMoveWeight>-0.012695317461700292</Minimizing.ValidMoveWeight>
    <Minimizing.ValidPlacementWeight>0.013918898431327409</Minimizing.ValidPlacementWeight>
    <Minimizing.ValidMovementWeight>0.013109795046698212</Minimizing.ValidMovementWeight>
    <Minimizing.InHandWeight>0.012047270287897141</Minimizing.InHandWeight>
    <Minimizing.InPlayWeight>0.018353179674033292</Minimizing.InPlayWeight>
    <Minimizing.IsPinnedWeight>-0.011335357728418037</Minimizing.IsPinnedWeight>
    <Minimizing.QueenBee.ValidMoveWeight>0.011432675768441683</Minimizing.QueenBee.ValidMoveWeight>
    <Minimizing.QueenBee.ValidPlacementWeight>-0.020155318088164956</Minimizing.QueenBee.ValidPlacementWeight>
    <Minimizing.QueenBee.ValidMovementWeight>-0.0044974163590144993</Minimizing.QueenBee.ValidMovementWeight>
    <Minimizing.QueenBee.NeighborWeight>24.178137533248666</Minimizing.QueenBee.NeighborWeight>
    <Minimizing.QueenBee.InHandWeight>-0.02214904137363774</Minimizing.QueenBee.InHandWeight>
    <Minimizing.QueenBee.InPlayWeight>0.0038895205817387544</Minimizing.QueenBee.InPlayWeight>
    <Minimizing.QueenBee.IsPinnedWeight>7.1313615557262491</Minimizing.QueenBee.IsPinnedWeight>
    <Minimizing.Spider.ValidMoveWeight>0.0014466280436876415</Minimizing.Spider.ValidMoveWeight>
    <Minimizing.Spider.ValidPlacementWeight>-0.0057635404748092919</Minimizing.Spider.ValidPlacementWeight>
    <Minimizing.Spider.ValidMovementWeight>-0.0079699416100955274</Minimizing.Spider.ValidMovementWeight>
    <Minimizing.Spider.NeighborWeight>-0.014492945857294702</Minimizing.Spider.NeighborWeight>
    <Minimizing.Spider.InHandWeight>-0.021161611505130809</Minimizing.Spider.InHandWeight>
    <Minimizing.Spider.InPlayWeight>0.00019996477709453676</Minimizing.Spider.InPlayWeight>
    <Minimizing.Spider.IsPinnedWeight>0.0011672589774376436</Minimizing.Spider.IsPinnedWeight>
    <Minimizing.Beetle.ValidMoveWeight>0.0038232428515238572</Minimizing.Beetle.ValidMoveWeight>
    <Minimizing.Beetle.ValidPlacementWeight>0.0016968773507038688</Minimizing.Beetle.ValidPlacementWeight>
    <Minimizing.Beetle.ValidMovementWeight>0.0239785675739262</Minimizing.Beetle.ValidMovementWeight>
    <Minimizing.Beetle.NeighborWeight>-0.0073096423533904695</Minimizing.Beetle.NeighborWeight>
    <Minimizing.Beetle.InHandWeight>0.018212964256228009</Minimizing.Beetle.InHandWeight>
    <Minimizing.Beetle.InPlayWeight>-0.0037386746927711195</Minimizing.Beetle.InPlayWeight>
    <Minimizing.Beetle.IsPinnedWeight>0.0064767297799402926</Minimizing.Beetle.IsPinnedWeight>
    <Minimizing.Grasshopper.ValidMoveWeight>-0.029923711665736357</Minimizing.Grasshopper.ValidMoveWeight>
    <Minimizing.Grasshopper.ValidPlacementWeight>0.012357526854318172</Minimizing.Grasshopper.ValidPlacementWeight>
    <Minimizing.Grasshopper.ValidMovementWeight>0.011717888715649456</Minimizing.Grasshopper.ValidMovementWeight>
    <Minimizing.Grasshopper.NeighborWeight>-0.00029815296106515767</Minimizing.Grasshopper.NeighborWeight>
    <Minimizing.Grasshopper.InHandWeight>0.0070556817355236848</Minimizing.Grasshopper.InHandWeight>
    <Minimizing.Grasshopper.InPlayWeight>4.3623607575322443E-05</Minimizing.Grasshopper.InPlayWeight>
    <Minimizing.Grasshopper.IsPinnedWeight>0.0093875961626135682</Minimizing.Grasshopper.IsPinnedWeight>
    <Minimizing.SoldierAnt.ValidMoveWeight>-0.011186030991580104</Minimizing.SoldierAnt.ValidMoveWeight>
    <Minimizing.SoldierAnt.ValidPlacementWeight>-0.0082737920254695972</Minimizing.SoldierAnt.ValidPlacementWeight>
    <Minimizing.SoldierAnt.ValidMovementWeight>-0.0070052952360069249</Minimizing.SoldierAnt.ValidMovementWeight>
    <Minimizing.SoldierAnt.NeighborWeight>-0.0016992099854961332</Minimizing.SoldierAnt.NeighborWeight>
    <Minimizing.SoldierAnt.InHandWeight>0.030444388518331745</Minimizing.SoldierAnt.InHandWeight>
    <Minimizing.SoldierAnt.InPlayWeight>0.0078700895370125337</Minimizing.SoldierAnt.InPlayWeight>
    <Minimizing.SoldierAnt.IsPinnedWeight>-0.014015277679417435</Minimizing.SoldierAnt.IsPinnedWeight>
  </MetricWeights>";
    }
}
