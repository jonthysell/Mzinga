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
    <Maximizing.ValidMoveWeight>-24.963902370786098</Maximizing.ValidMoveWeight>
    <Maximizing.ValidPlacementWeight>-3.68491462985642</Maximizing.ValidPlacementWeight>
    <Maximizing.ValidMovementWeight>-1.0443535218089031</Maximizing.ValidMovementWeight>
    <Maximizing.InHandWeight>-15.86432137353755</Maximizing.InHandWeight>
    <Maximizing.InPlayWeight>-1.6054702660773665</Maximizing.InPlayWeight>
    <Maximizing.IsPinnedWeight>-2.8419680922960686</Maximizing.IsPinnedWeight>
    <Maximizing.QueenBee.ValidMoveWeight>12.438057993771921</Maximizing.QueenBee.ValidMoveWeight>
    <Maximizing.QueenBee.ValidPlacementWeight>-1.7564328090739774</Maximizing.QueenBee.ValidPlacementWeight>
    <Maximizing.QueenBee.ValidMovementWeight>-1.2450408224746852</Maximizing.QueenBee.ValidMovementWeight>
    <Maximizing.QueenBee.NeighborWeight>-11974.498408790743</Maximizing.QueenBee.NeighborWeight>
    <Maximizing.QueenBee.InHandWeight>2.2409368120346302</Maximizing.QueenBee.InHandWeight>
    <Maximizing.QueenBee.InPlayWeight>-1.7597902765021634</Maximizing.QueenBee.InPlayWeight>
    <Maximizing.QueenBee.IsPinnedWeight>-335.53047979206167</Maximizing.QueenBee.IsPinnedWeight>
    <Maximizing.Spider.ValidMoveWeight>-1.2615068980950961</Maximizing.Spider.ValidMoveWeight>
    <Maximizing.Spider.ValidPlacementWeight>-15.947262438967481</Maximizing.Spider.ValidPlacementWeight>
    <Maximizing.Spider.ValidMovementWeight>1.1266153695980654</Maximizing.Spider.ValidMovementWeight>
    <Maximizing.Spider.NeighborWeight>-11.244960644482266</Maximizing.Spider.NeighborWeight>
    <Maximizing.Spider.InHandWeight>25.145777198116072</Maximizing.Spider.InHandWeight>
    <Maximizing.Spider.InPlayWeight>4.145713781921974</Maximizing.Spider.InPlayWeight>
    <Maximizing.Spider.IsPinnedWeight>-2.6587353776714049</Maximizing.Spider.IsPinnedWeight>
    <Maximizing.Beetle.ValidMoveWeight>0.18442978184678915</Maximizing.Beetle.ValidMoveWeight>
    <Maximizing.Beetle.ValidPlacementWeight>0.66437863898848948</Maximizing.Beetle.ValidPlacementWeight>
    <Maximizing.Beetle.ValidMovementWeight>0.020844182963897367</Maximizing.Beetle.ValidMovementWeight>
    <Maximizing.Beetle.NeighborWeight>49.661900597451591</Maximizing.Beetle.NeighborWeight>
    <Maximizing.Beetle.InHandWeight>3.1884094992993135</Maximizing.Beetle.InHandWeight>
    <Maximizing.Beetle.InPlayWeight>0.70021222845718456</Maximizing.Beetle.InPlayWeight>
    <Maximizing.Beetle.IsPinnedWeight>10.095809977912747</Maximizing.Beetle.IsPinnedWeight>
    <Maximizing.Grasshopper.ValidMoveWeight>-0.60554000572039834</Maximizing.Grasshopper.ValidMoveWeight>
    <Maximizing.Grasshopper.ValidPlacementWeight>-2.6775318723955261</Maximizing.Grasshopper.ValidPlacementWeight>
    <Maximizing.Grasshopper.ValidMovementWeight>17.445683765389</Maximizing.Grasshopper.ValidMovementWeight>
    <Maximizing.Grasshopper.NeighborWeight>-0.0074706171576293329</Maximizing.Grasshopper.NeighborWeight>
    <Maximizing.Grasshopper.InHandWeight>6.4139338483559563</Maximizing.Grasshopper.InHandWeight>
    <Maximizing.Grasshopper.InPlayWeight>0.9070042908878938</Maximizing.Grasshopper.InPlayWeight>
    <Maximizing.Grasshopper.IsPinnedWeight>5.392077764115478</Maximizing.Grasshopper.IsPinnedWeight>
    <Maximizing.SoldierAnt.ValidMoveWeight>-9.0755187989448967</Maximizing.SoldierAnt.ValidMoveWeight>
    <Maximizing.SoldierAnt.ValidPlacementWeight>3.4172713493479492</Maximizing.SoldierAnt.ValidPlacementWeight>
    <Maximizing.SoldierAnt.ValidMovementWeight>4.59141683777273</Maximizing.SoldierAnt.ValidMovementWeight>
    <Maximizing.SoldierAnt.NeighborWeight>-0.38315570599346488</Maximizing.SoldierAnt.NeighborWeight>
    <Maximizing.SoldierAnt.InHandWeight>-5.982214204449158</Maximizing.SoldierAnt.InHandWeight>
    <Maximizing.SoldierAnt.InPlayWeight>-1.4805172684790506</Maximizing.SoldierAnt.InPlayWeight>
    <Maximizing.SoldierAnt.IsPinnedWeight>40.322633049938432</Maximizing.SoldierAnt.IsPinnedWeight>
    <Minimizing.ValidMoveWeight>-20.316474256780133</Minimizing.ValidMoveWeight>
    <Minimizing.ValidPlacementWeight>0.48244389243090424</Minimizing.ValidPlacementWeight>
    <Minimizing.ValidMovementWeight>1.9798472561931735</Minimizing.ValidMovementWeight>
    <Minimizing.InHandWeight>-6.4360290939573046</Minimizing.InHandWeight>
    <Minimizing.InPlayWeight>-18.809814711493338</Minimizing.InPlayWeight>
    <Minimizing.IsPinnedWeight>-2.4118451306781004</Minimizing.IsPinnedWeight>
    <Minimizing.QueenBee.ValidMoveWeight>8.62395873066201</Minimizing.QueenBee.ValidMoveWeight>
    <Minimizing.QueenBee.ValidPlacementWeight>-0.24118350664005309</Minimizing.QueenBee.ValidPlacementWeight>
    <Minimizing.QueenBee.ValidMovementWeight>-3.1193140918692035</Minimizing.QueenBee.ValidMovementWeight>
    <Minimizing.QueenBee.NeighborWeight>46596.085182486386</Minimizing.QueenBee.NeighborWeight>
    <Minimizing.QueenBee.InHandWeight>0.98617018107205534</Minimizing.QueenBee.InHandWeight>
    <Minimizing.QueenBee.InPlayWeight>-0.39106964408865624</Minimizing.QueenBee.InPlayWeight>
    <Minimizing.QueenBee.IsPinnedWeight>2804.8219253589609</Minimizing.QueenBee.IsPinnedWeight>
    <Minimizing.Spider.ValidMoveWeight>-0.82678695029088356</Minimizing.Spider.ValidMoveWeight>
    <Minimizing.Spider.ValidPlacementWeight>0.76716980413332914</Minimizing.Spider.ValidPlacementWeight>
    <Minimizing.Spider.ValidMovementWeight>4.854619373626992</Minimizing.Spider.ValidMovementWeight>
    <Minimizing.Spider.NeighborWeight>2.9686609346071235</Minimizing.Spider.NeighborWeight>
    <Minimizing.Spider.InHandWeight>-4.8851503470946742</Minimizing.Spider.InHandWeight>
    <Minimizing.Spider.InPlayWeight>0.12976935757518993</Minimizing.Spider.InPlayWeight>
    <Minimizing.Spider.IsPinnedWeight>-5.1180701052894406</Minimizing.Spider.IsPinnedWeight>
    <Minimizing.Beetle.ValidMoveWeight>0.98072463615445515</Minimizing.Beetle.ValidMoveWeight>
    <Minimizing.Beetle.ValidPlacementWeight>-0.32037703118242439</Minimizing.Beetle.ValidPlacementWeight>
    <Minimizing.Beetle.ValidMovementWeight>-22.939479047011552</Minimizing.Beetle.ValidMovementWeight>
    <Minimizing.Beetle.NeighborWeight>2.0544173542986583</Minimizing.Beetle.NeighborWeight>
    <Minimizing.Beetle.InHandWeight>-2.38106810133378</Minimizing.Beetle.InHandWeight>
    <Minimizing.Beetle.InPlayWeight>-0.024410716896235994</Minimizing.Beetle.InPlayWeight>
    <Minimizing.Beetle.IsPinnedWeight>-0.48893432597450481</Minimizing.Beetle.IsPinnedWeight>
    <Minimizing.Grasshopper.ValidMoveWeight>-169.63614203692919</Minimizing.Grasshopper.ValidMoveWeight>
    <Minimizing.Grasshopper.ValidPlacementWeight>-11.874927870857398</Minimizing.Grasshopper.ValidPlacementWeight>
    <Minimizing.Grasshopper.ValidMovementWeight>1.0670213802571134</Minimizing.Grasshopper.ValidMovementWeight>
    <Minimizing.Grasshopper.NeighborWeight>-0.213473932918422</Minimizing.Grasshopper.NeighborWeight>
    <Minimizing.Grasshopper.InHandWeight>2.5966636476783354</Minimizing.Grasshopper.InHandWeight>
    <Minimizing.Grasshopper.InPlayWeight>0.0021157184586576014</Minimizing.Grasshopper.InPlayWeight>
    <Minimizing.Grasshopper.IsPinnedWeight>-3.8971797496714178</Minimizing.Grasshopper.IsPinnedWeight>
    <Minimizing.SoldierAnt.ValidMoveWeight>-2.62611364094756</Minimizing.SoldierAnt.ValidMoveWeight>
    <Minimizing.SoldierAnt.ValidPlacementWeight>-2.57061541082512</Minimizing.SoldierAnt.ValidPlacementWeight>
    <Minimizing.SoldierAnt.ValidMovementWeight>-0.54820621158255145</Minimizing.SoldierAnt.ValidMovementWeight>
    <Minimizing.SoldierAnt.NeighborWeight>0.48547067214741296</Minimizing.SoldierAnt.NeighborWeight>
    <Minimizing.SoldierAnt.InHandWeight>28.923468454473756</Minimizing.SoldierAnt.InHandWeight>
    <Minimizing.SoldierAnt.InPlayWeight>-2.6262564240785524</Minimizing.SoldierAnt.InPlayWeight>
    <Minimizing.SoldierAnt.IsPinnedWeight>-4.7739532372065066</Minimizing.SoldierAnt.IsPinnedWeight>
</MetricWeights>
";
    }
}
