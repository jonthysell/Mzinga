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

            MetricWeights normalized = mw.GetNormalized();
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
<Maximizing.ValidMoveWeight>16716.685862568702</Maximizing.ValidMoveWeight>
<Maximizing.ValidPlacementWeight>2159.6937309027044</Maximizing.ValidPlacementWeight>
<Maximizing.ValidMovementWeight>27663.92208146357</Maximizing.ValidMovementWeight>
<Maximizing.InHandWeight>-8485.5590748666946</Maximizing.InHandWeight>
<Maximizing.InPlayWeight>-12.163616169198722</Maximizing.InPlayWeight>
<Maximizing.IsPinnedWeight>-78.100381828654463</Maximizing.IsPinnedWeight>
<Maximizing.QueenBee.ValidMoveWeight>-218822.59944999038</Maximizing.QueenBee.ValidMoveWeight>
<Maximizing.QueenBee.ValidPlacementWeight>254194.0389322686</Maximizing.QueenBee.ValidPlacementWeight>
<Maximizing.QueenBee.ValidMovementWeight>2158.6195299100818</Maximizing.QueenBee.ValidMovementWeight>
<Maximizing.QueenBee.InHandWeight>30360.920185144882</Maximizing.QueenBee.InHandWeight>
<Maximizing.QueenBee.InPlayWeight>-79148.06702499841</Maximizing.QueenBee.InPlayWeight>
<Maximizing.QueenBee.IsPinnedWeight>11113060.006579753</Maximizing.QueenBee.IsPinnedWeight>
<Maximizing.QueenBee.NeighborWeight>-4639310.4338139836</Maximizing.QueenBee.NeighborWeight>
<Maximizing.Spider.ValidMoveWeight>8376.8807652667165</Maximizing.Spider.ValidMoveWeight>
<Maximizing.Spider.ValidPlacementWeight>9469.6365148190889</Maximizing.Spider.ValidPlacementWeight>
<Maximizing.Spider.ValidMovementWeight>303.81190153442566</Maximizing.Spider.ValidMovementWeight>
<Maximizing.Spider.InHandWeight>4557.9035311537127</Maximizing.Spider.InHandWeight>
<Maximizing.Spider.InPlayWeight>-420.78369613532942</Maximizing.Spider.InPlayWeight>
<Maximizing.Spider.IsPinnedWeight>147505.025666903</Maximizing.Spider.IsPinnedWeight>
<Maximizing.Spider.NeighborWeight>-25980.301253002839</Maximizing.Spider.NeighborWeight>
<Maximizing.Beetle.ValidMoveWeight>-6663.7276489816923</Maximizing.Beetle.ValidMoveWeight>
<Maximizing.Beetle.ValidPlacementWeight>-3695.9978551493241</Maximizing.Beetle.ValidPlacementWeight>
<Maximizing.Beetle.ValidMovementWeight>-19618.5221357477</Maximizing.Beetle.ValidMovementWeight>
<Maximizing.Beetle.InHandWeight>-3439.2482656169191</Maximizing.Beetle.InHandWeight>
<Maximizing.Beetle.InPlayWeight>-24105.954842671887</Maximizing.Beetle.InPlayWeight>
<Maximizing.Beetle.IsPinnedWeight>6255.9093818274687</Maximizing.Beetle.IsPinnedWeight>
<Maximizing.Beetle.NeighborWeight>16476.073257584161</Maximizing.Beetle.NeighborWeight>
<Maximizing.Grasshopper.ValidMoveWeight>9774.650577363187</Maximizing.Grasshopper.ValidMoveWeight>
<Maximizing.Grasshopper.ValidPlacementWeight>-5546.7114299925061</Maximizing.Grasshopper.ValidPlacementWeight>
<Maximizing.Grasshopper.ValidMovementWeight>229462.09637624474</Maximizing.Grasshopper.ValidMovementWeight>
<Maximizing.Grasshopper.InHandWeight>-71.097582953694968</Maximizing.Grasshopper.InHandWeight>
<Maximizing.Grasshopper.InPlayWeight>-40758.562364084682</Maximizing.Grasshopper.InPlayWeight>
<Maximizing.Grasshopper.IsPinnedWeight>-104.31059213997527</Maximizing.Grasshopper.IsPinnedWeight>
<Maximizing.Grasshopper.NeighborWeight>432.16517605565826</Maximizing.Grasshopper.NeighborWeight>
<Maximizing.SoldierAnt.ValidMoveWeight>6006.31252768361</Maximizing.SoldierAnt.ValidMoveWeight>
<Maximizing.SoldierAnt.ValidPlacementWeight>10767.6889151473</Maximizing.SoldierAnt.ValidPlacementWeight>
<Maximizing.SoldierAnt.ValidMovementWeight>-13667.848563287103</Maximizing.SoldierAnt.ValidMovementWeight>
<Maximizing.SoldierAnt.InHandWeight>-5141.7524987667</Maximizing.SoldierAnt.InHandWeight>
<Maximizing.SoldierAnt.InPlayWeight>-8950.9026666171285</Maximizing.SoldierAnt.InPlayWeight>
<Maximizing.SoldierAnt.IsPinnedWeight>5868.9905159144046</Maximizing.SoldierAnt.IsPinnedWeight>
<Maximizing.SoldierAnt.NeighborWeight>-7054.4780204198769</Maximizing.SoldierAnt.NeighborWeight>
<Minimizing.ValidMoveWeight>-23137.312217812465</Minimizing.ValidMoveWeight>
<Minimizing.ValidPlacementWeight>-139061.53186506996</Minimizing.ValidPlacementWeight>
<Minimizing.ValidMovementWeight>1995.8909498474334</Minimizing.ValidMovementWeight>
<Minimizing.InHandWeight>6377.1851309785961</Minimizing.InHandWeight>
<Minimizing.InPlayWeight>4777.1596077298991</Minimizing.InPlayWeight>
<Minimizing.IsPinnedWeight>12315.208522865605</Minimizing.IsPinnedWeight>
<Minimizing.QueenBee.ValidMoveWeight>-575.18371683765326</Minimizing.QueenBee.ValidMoveWeight>
<Minimizing.QueenBee.ValidPlacementWeight>156851.5966338554</Minimizing.QueenBee.ValidPlacementWeight>
<Minimizing.QueenBee.ValidMovementWeight>172.63696977733994</Minimizing.QueenBee.ValidMovementWeight>
<Minimizing.QueenBee.InHandWeight>2729.7029079828326</Minimizing.QueenBee.InHandWeight>
<Minimizing.QueenBee.InPlayWeight>-340.9615932073533</Minimizing.QueenBee.InPlayWeight>
<Minimizing.QueenBee.IsPinnedWeight>131792917.70976582</Minimizing.QueenBee.IsPinnedWeight>
<Minimizing.QueenBee.NeighborWeight>397865819.76967984</Minimizing.QueenBee.NeighborWeight>
<Minimizing.Spider.ValidMoveWeight>732.98494459415667</Minimizing.Spider.ValidMoveWeight>
<Minimizing.Spider.ValidPlacementWeight>1930.1319353026788</Minimizing.Spider.ValidPlacementWeight>
<Minimizing.Spider.ValidMovementWeight>-713.92979066417638</Minimizing.Spider.ValidMovementWeight>
<Minimizing.Spider.InHandWeight>143879.96686591464</Minimizing.Spider.InHandWeight>
<Minimizing.Spider.InPlayWeight>3880.4555388418339</Minimizing.Spider.InPlayWeight>
<Minimizing.Spider.IsPinnedWeight>-2798.4162758793054</Minimizing.Spider.IsPinnedWeight>
<Minimizing.Spider.NeighborWeight>90.685823116796541</Minimizing.Spider.NeighborWeight>
<Minimizing.Beetle.ValidMoveWeight>51629.9354046564</Minimizing.Beetle.ValidMoveWeight>
<Minimizing.Beetle.ValidPlacementWeight>6063.8821145776719</Minimizing.Beetle.ValidPlacementWeight>
<Minimizing.Beetle.ValidMovementWeight>-1466.1472974724479</Minimizing.Beetle.ValidMovementWeight>
<Minimizing.Beetle.InHandWeight>5265.9939612298713</Minimizing.Beetle.InHandWeight>
<Minimizing.Beetle.InPlayWeight>-10422.754338137469</Minimizing.Beetle.InPlayWeight>
<Minimizing.Beetle.IsPinnedWeight>40661.930069444541</Minimizing.Beetle.IsPinnedWeight>
<Minimizing.Beetle.NeighborWeight>16993.874135157617</Minimizing.Beetle.NeighborWeight>
<Minimizing.Grasshopper.ValidMoveWeight>39120.615900160366</Minimizing.Grasshopper.ValidMoveWeight>
<Minimizing.Grasshopper.ValidPlacementWeight>6270.49130812811</Minimizing.Grasshopper.ValidPlacementWeight>
<Minimizing.Grasshopper.ValidMovementWeight>621.69764263205013</Minimizing.Grasshopper.ValidMovementWeight>
<Minimizing.Grasshopper.InHandWeight>-9345.0171218231462</Minimizing.Grasshopper.InHandWeight>
<Minimizing.Grasshopper.InPlayWeight>-1284.2072997011057</Minimizing.Grasshopper.InPlayWeight>
<Minimizing.Grasshopper.IsPinnedWeight>-495550.5076811164</Minimizing.Grasshopper.IsPinnedWeight>
<Minimizing.Grasshopper.NeighborWeight>261.974865207963</Minimizing.Grasshopper.NeighborWeight>
<Minimizing.SoldierAnt.ValidMoveWeight>-20256.179068440353</Minimizing.SoldierAnt.ValidMoveWeight>
<Minimizing.SoldierAnt.ValidPlacementWeight>-2005.0132431260818</Minimizing.SoldierAnt.ValidPlacementWeight>
<Minimizing.SoldierAnt.ValidMovementWeight>2190.3202967517386</Minimizing.SoldierAnt.ValidMovementWeight>
<Minimizing.SoldierAnt.InHandWeight>-159854.57349428989</Minimizing.SoldierAnt.InHandWeight>
<Minimizing.SoldierAnt.InPlayWeight>41778.405478375891</Minimizing.SoldierAnt.InPlayWeight>
<Minimizing.SoldierAnt.IsPinnedWeight>3967.43528310128</Minimizing.SoldierAnt.IsPinnedWeight>
<Minimizing.SoldierAnt.NeighborWeight>-199.86464832587754</Minimizing.SoldierAnt.NeighborWeight>
</MetricWeights>
";
    }
}
