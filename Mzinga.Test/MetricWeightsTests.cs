// 
// MetricWeightsTests.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2017, 2018 Jon Thysell <http://jonthysell.com>
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
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Core.AI;

namespace Mzinga.Test
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

            MetricWeights normalized = mw.GetNormalized();
            Assert.IsNotNull(mw);
        }

        public static MetricWeights TestMetricWeights
        {
            get
            {
                if (null == _testMetricWeights)
                {
                    byte[] rawData = Encoding.UTF8.GetBytes(TestMetricWeightsXml);

                    using (MemoryStream ms = new MemoryStream(rawData))
                    {
                        _testMetricWeights = MetricWeights.ReadMetricWeightsXml(XmlReader.Create(ms));
                    }
                }

                return _testMetricWeights;
            }
        }
        private static MetricWeights _testMetricWeights;

        public const string TestMetricWeightsXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<MetricWeights>
<QueenBee.InPlayWeight>-31.271265238491477</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>2.0334710106223222</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>-9.4245904810096075</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>-70.808251671610989</QueenBee.NoisyMoveWeight>
<QueenBee.QuietMoveWeight>89.310825113084078</QueenBee.QuietMoveWeight>
<QueenBee.FriendlyNeighborWeight>-1292.144333086947</QueenBee.FriendlyNeighborWeight>
<QueenBee.EnemyNeighborWeight>-2369.901737091086</QueenBee.EnemyNeighborWeight>
<Spider.InPlayWeight>-149.30840826867541</Spider.InPlayWeight>
<Spider.IsPinnedWeight>40.694851291829188</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>54.938846900842073</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>120.6824977665965</Spider.NoisyMoveWeight>
<Spider.QuietMoveWeight>4.237933253980211</Spider.QuietMoveWeight>
<Spider.FriendlyNeighborWeight>6.6842247969257773</Spider.FriendlyNeighborWeight>
<Spider.EnemyNeighborWeight>-36.287365364328664</Spider.EnemyNeighborWeight>
<Beetle.InPlayWeight>-21.298671861013247</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>44.975440006673075</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>0.22640443368181792</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>11.799687995838319</Beetle.NoisyMoveWeight>
<Beetle.QuietMoveWeight>-0.41972015855122363</Beetle.QuietMoveWeight>
<Beetle.FriendlyNeighborWeight>-47.835946773298062</Beetle.FriendlyNeighborWeight>
<Beetle.EnemyNeighborWeight>-34.152794853100922</Beetle.EnemyNeighborWeight>
<Grasshopper.InPlayWeight>27.821419259296462</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>-9.1776263769379</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>87.385857538232031</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>24.3511057438334</Grasshopper.NoisyMoveWeight>
<Grasshopper.QuietMoveWeight>10.463797931011674</Grasshopper.QuietMoveWeight>
<Grasshopper.FriendlyNeighborWeight>-8.5728600941518582</Grasshopper.FriendlyNeighborWeight>
<Grasshopper.EnemyNeighborWeight>-15.15464964418423</Grasshopper.EnemyNeighborWeight>
<SoldierAnt.InPlayWeight>14.791404237533643</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>3.5479715260690874</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>0.86876704527939075</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>12.544588833928383</SoldierAnt.NoisyMoveWeight>
<SoldierAnt.QuietMoveWeight>44.651134348684522</SoldierAnt.QuietMoveWeight>
<SoldierAnt.FriendlyNeighborWeight>-1.0205554548560434</SoldierAnt.FriendlyNeighborWeight>
<SoldierAnt.EnemyNeighborWeight>-3.7158092609214641</SoldierAnt.EnemyNeighborWeight>
<Mosquito.InPlayWeight>114.90974779522037</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>-10.359089252634018</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>-68.03524522408155</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>-7.6081300321585186</Mosquito.NoisyMoveWeight>
<Mosquito.QuietMoveWeight>5.1726287565252882</Mosquito.QuietMoveWeight>
<Mosquito.FriendlyNeighborWeight>623.91444520598009</Mosquito.FriendlyNeighborWeight>
<Mosquito.EnemyNeighborWeight>-17.764958039189207</Mosquito.EnemyNeighborWeight>
<Ladybug.InPlayWeight>-63.491944838844688</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>1.3151225751801965</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>-30.39286922922026</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>-1.2132434501234646</Ladybug.NoisyMoveWeight>
<Ladybug.QuietMoveWeight>11.565416396917039</Ladybug.QuietMoveWeight>
<Ladybug.FriendlyNeighborWeight>233.56274141844025</Ladybug.FriendlyNeighborWeight>
<Ladybug.EnemyNeighborWeight>-195.58021587206994</Ladybug.EnemyNeighborWeight>
<Pillbug.InPlayWeight>-17.177470113910957</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>337.50185100135252</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>4.1548128944983764</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>-70.394767625781128</Pillbug.NoisyMoveWeight>
<Pillbug.QuietMoveWeight>-6.5562542009708737</Pillbug.QuietMoveWeight>
<Pillbug.FriendlyNeighborWeight>33.684978236251034</Pillbug.FriendlyNeighborWeight>
<Pillbug.EnemyNeighborWeight>131.05545890920584</Pillbug.EnemyNeighborWeight>
</MetricWeights>
";
    }
}
