// 
// MetricWeightsTests.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2017 Jon Thysell <http://jonthysell.com>
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

using System.IO;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Core.AI;

namespace Mzinga.CoreTest
{
    [TestClass]
    public class MetricWeightsTests
    {
        [TestMethod]
        public void MetricWeights_NewTest()
        {
            MetricWeights mw = new MetricWeights();
            Assert.IsNotNull(mw);
        }

        [TestMethod]
        public void MetricWeights_GetNormalizedTest()
        {
            MetricWeights mw = TestMetricWeights.Clone();
            Assert.IsNotNull(mw);

            MetricWeights normalized = mw.GetNormalized(short.MaxValue, true);
            Assert.IsNotNull(mw);
        }

        public static MetricWeights TestMetricWeights
        {
            get
            {
                if (null == _testMetricWeights)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (StreamWriter sw = new StreamWriter(ms))
                        {
                            sw.Write(TestMetricWeightsXml);
                            sw.Flush();

                            ms.Position = 0;
                            _testMetricWeights = MetricWeights.ReadMetricWeightsXml(XmlReader.Create(ms));
                        }
                    }
                }

                return _testMetricWeights;
            }
        }
        private static MetricWeights _testMetricWeights;

        public const string TestMetricWeightsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<MetricWeights>
<DrawScore>0</DrawScore>
<Maximizing.ValidMoveWeight>-15.71114904022428</Maximizing.ValidMoveWeight>
<Maximizing.ValidPlacementWeight>-99.12379911006542</Maximizing.ValidPlacementWeight>
<Maximizing.ValidMovementWeight>327.37388933407885</Maximizing.ValidMovementWeight>
<Maximizing.InHandWeight>12.195468032488332</Maximizing.InHandWeight>
<Maximizing.InPlayWeight>-12.29018611449402</Maximizing.InPlayWeight>
<Maximizing.IsPinnedWeight>-35.467207429458817</Maximizing.IsPinnedWeight>
<Maximizing.QueenBee.ValidMoveWeight>-84.039190048441952</Maximizing.QueenBee.ValidMoveWeight>
<Maximizing.QueenBee.ValidPlacementWeight>398.21115891978815</Maximizing.QueenBee.ValidPlacementWeight>
<Maximizing.QueenBee.ValidMovementWeight>9.8031026184425</Maximizing.QueenBee.ValidMovementWeight>
<Maximizing.QueenBee.InHandWeight>14.616394121836834</Maximizing.QueenBee.InHandWeight>
<Maximizing.QueenBee.InPlayWeight>-590.18618474665061</Maximizing.QueenBee.InPlayWeight>
<Maximizing.QueenBee.IsPinnedWeight>-1726.941787545758</Maximizing.QueenBee.IsPinnedWeight>
<Maximizing.QueenBee.NeighborWeight>-34308.1384716975</Maximizing.QueenBee.NeighborWeight>
<Maximizing.Spider.ValidMoveWeight>3.3783860224459743</Maximizing.Spider.ValidMoveWeight>
<Maximizing.Spider.ValidPlacementWeight>-210.88721193105897</Maximizing.Spider.ValidPlacementWeight>
<Maximizing.Spider.ValidMovementWeight>-39.249280313060204</Maximizing.Spider.ValidMovementWeight>
<Maximizing.Spider.InHandWeight>-206.70485169718867</Maximizing.Spider.InHandWeight>
<Maximizing.Spider.InPlayWeight>1.9600044248854367</Maximizing.Spider.InPlayWeight>
<Maximizing.Spider.IsPinnedWeight>-27.97638014960496</Maximizing.Spider.IsPinnedWeight>
<Maximizing.Spider.NeighborWeight>845.41642076774826</Maximizing.Spider.NeighborWeight>
<Maximizing.Beetle.ValidMoveWeight>-4.6387844623915129</Maximizing.Beetle.ValidMoveWeight>
<Maximizing.Beetle.ValidPlacementWeight>307.38523051403604</Maximizing.Beetle.ValidPlacementWeight>
<Maximizing.Beetle.ValidMovementWeight>6.1577948897839745</Maximizing.Beetle.ValidMovementWeight>
<Maximizing.Beetle.InHandWeight>948.460782165593</Maximizing.Beetle.InHandWeight>
<Maximizing.Beetle.InPlayWeight>1.0693948051635061</Maximizing.Beetle.InPlayWeight>
<Maximizing.Beetle.IsPinnedWeight>-33.252774829291873</Maximizing.Beetle.IsPinnedWeight>
<Maximizing.Beetle.NeighborWeight>2950.0774272770877</Maximizing.Beetle.NeighborWeight>
<Maximizing.Grasshopper.ValidMoveWeight>72.528827119696373</Maximizing.Grasshopper.ValidMoveWeight>
<Maximizing.Grasshopper.ValidPlacementWeight>-580.50008795342967</Maximizing.Grasshopper.ValidPlacementWeight>
<Maximizing.Grasshopper.ValidMovementWeight>43.815372308557286</Maximizing.Grasshopper.ValidMovementWeight>
<Maximizing.Grasshopper.InHandWeight>13.359500301314322</Maximizing.Grasshopper.InHandWeight>
<Maximizing.Grasshopper.InPlayWeight>10.966165715900658</Maximizing.Grasshopper.InPlayWeight>
<Maximizing.Grasshopper.IsPinnedWeight>-54.438131485773368</Maximizing.Grasshopper.IsPinnedWeight>
<Maximizing.Grasshopper.NeighborWeight>148.56008154720854</Maximizing.Grasshopper.NeighborWeight>
<Maximizing.SoldierAnt.ValidMoveWeight>-36.450601060314412</Maximizing.SoldierAnt.ValidMoveWeight>
<Maximizing.SoldierAnt.ValidPlacementWeight>-830.01260391356027</Maximizing.SoldierAnt.ValidPlacementWeight>
<Maximizing.SoldierAnt.ValidMovementWeight>-2.4485546573299746</Maximizing.SoldierAnt.ValidMovementWeight>
<Maximizing.SoldierAnt.InHandWeight>115.29515857130087</Maximizing.SoldierAnt.InHandWeight>
<Maximizing.SoldierAnt.InPlayWeight>-7.9119339264221225</Maximizing.SoldierAnt.InPlayWeight>
<Maximizing.SoldierAnt.IsPinnedWeight>729.4549104888124</Maximizing.SoldierAnt.IsPinnedWeight>
<Maximizing.SoldierAnt.NeighborWeight>-778.04091825105172</Maximizing.SoldierAnt.NeighborWeight>
<Minimizing.ValidMoveWeight>51.376876371737957</Minimizing.ValidMoveWeight>
<Minimizing.ValidPlacementWeight>-11.076861549843972</Minimizing.ValidPlacementWeight>
<Minimizing.ValidMovementWeight>-2.9776544580139843</Minimizing.ValidMovementWeight>
<Minimizing.InHandWeight>0.5156574697023335</Minimizing.InHandWeight>
<Minimizing.InPlayWeight>-247.37051023489633</Minimizing.InPlayWeight>
<Minimizing.IsPinnedWeight>-55.261847518829256</Minimizing.IsPinnedWeight>
<Minimizing.QueenBee.ValidMoveWeight>-2.7229523794399748</Minimizing.QueenBee.ValidMoveWeight>
<Minimizing.QueenBee.ValidPlacementWeight>877.14496987732946</Minimizing.QueenBee.ValidPlacementWeight>
<Minimizing.QueenBee.ValidMovementWeight>-10.336143217414294</Minimizing.QueenBee.ValidMovementWeight>
<Minimizing.QueenBee.InHandWeight>-90.284398682706637</Minimizing.QueenBee.InHandWeight>
<Minimizing.QueenBee.InPlayWeight>-0.053580454323220485</Minimizing.QueenBee.InPlayWeight>
<Minimizing.QueenBee.IsPinnedWeight>-2617.4511085408162</Minimizing.QueenBee.IsPinnedWeight>
<Minimizing.QueenBee.NeighborWeight>43473.76189589125</Minimizing.QueenBee.NeighborWeight>
<Minimizing.Spider.ValidMoveWeight>-9.0865464105325415</Minimizing.Spider.ValidMoveWeight>
<Minimizing.Spider.ValidPlacementWeight>-5.7670662564758564</Minimizing.Spider.ValidPlacementWeight>
<Minimizing.Spider.ValidMovementWeight>2.0975128344375125</Minimizing.Spider.ValidMovementWeight>
<Minimizing.Spider.InHandWeight>3.6082411831805232</Minimizing.Spider.InHandWeight>
<Minimizing.Spider.InPlayWeight>1.0963767779788891</Minimizing.Spider.InPlayWeight>
<Minimizing.Spider.IsPinnedWeight>41.025932716149271</Minimizing.Spider.IsPinnedWeight>
<Minimizing.Spider.NeighborWeight>6.0836537151198762</Minimizing.Spider.NeighborWeight>
<Minimizing.Beetle.ValidMoveWeight>13.123266730299255</Minimizing.Beetle.ValidMoveWeight>
<Minimizing.Beetle.ValidPlacementWeight>-7.4001122436177971</Minimizing.Beetle.ValidPlacementWeight>
<Minimizing.Beetle.ValidMovementWeight>55.408108220389295</Minimizing.Beetle.ValidMovementWeight>
<Minimizing.Beetle.InHandWeight>8.775710734375302</Minimizing.Beetle.InHandWeight>
<Minimizing.Beetle.InPlayWeight>0.52011271143628213</Minimizing.Beetle.InPlayWeight>
<Minimizing.Beetle.IsPinnedWeight>48.950861904063956</Minimizing.Beetle.IsPinnedWeight>
<Minimizing.Beetle.NeighborWeight>7.9184295821486144</Minimizing.Beetle.NeighborWeight>
<Minimizing.Grasshopper.ValidMoveWeight>-1490.6022919356251</Minimizing.Grasshopper.ValidMoveWeight>
<Minimizing.Grasshopper.ValidPlacementWeight>-1161.2066594817482</Minimizing.Grasshopper.ValidPlacementWeight>
<Minimizing.Grasshopper.ValidMovementWeight>4.9345061082134647</Minimizing.Grasshopper.ValidMovementWeight>
<Minimizing.Grasshopper.InHandWeight>-11.387869201241218</Minimizing.Grasshopper.InHandWeight>
<Minimizing.Grasshopper.InPlayWeight>-5.0354725106050093</Minimizing.Grasshopper.InPlayWeight>
<Minimizing.Grasshopper.IsPinnedWeight>-191.9152053003493</Minimizing.Grasshopper.IsPinnedWeight>
<Minimizing.Grasshopper.NeighborWeight>-4.2041125385114784</Minimizing.Grasshopper.NeighborWeight>
<Minimizing.SoldierAnt.ValidMoveWeight>13.80616490002032</Minimizing.SoldierAnt.ValidMoveWeight>
<Minimizing.SoldierAnt.ValidPlacementWeight>152.88109414352351</Minimizing.SoldierAnt.ValidPlacementWeight>
<Minimizing.SoldierAnt.ValidMovementWeight>326.99395986037882</Minimizing.SoldierAnt.ValidMovementWeight>
<Minimizing.SoldierAnt.InHandWeight>-375.07029981965559</Minimizing.SoldierAnt.InHandWeight>
<Minimizing.SoldierAnt.InPlayWeight>-66.6587502139953</Minimizing.SoldierAnt.InPlayWeight>
<Minimizing.SoldierAnt.IsPinnedWeight>175.34338024474178</Minimizing.SoldierAnt.IsPinnedWeight>
<Minimizing.SoldierAnt.NeighborWeight>0.88265845765676365</Minimizing.SoldierAnt.NeighborWeight>
</MetricWeights>
";
    }
}
