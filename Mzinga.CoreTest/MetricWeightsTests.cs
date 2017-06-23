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
<QueenBee.InPlayWeight>6</QueenBee.InPlayWeight>
<QueenBee.ValidMoveWeight>0</QueenBee.ValidMoveWeight>
<QueenBee.IsPinnedWeight>38</QueenBee.IsPinnedWeight>
<QueenBee.NeighborWeight>-570</QueenBee.NeighborWeight>
<Spider.InPlayWeight>4</Spider.InPlayWeight>
<Spider.ValidMoveWeight>0</Spider.ValidMoveWeight>
<Spider.IsPinnedWeight>0</Spider.IsPinnedWeight>
<Spider.NeighborWeight>0</Spider.NeighborWeight>
<Beetle.InPlayWeight>-6</Beetle.InPlayWeight>
<Beetle.ValidMoveWeight>3</Beetle.ValidMoveWeight>
<Beetle.IsPinnedWeight>0</Beetle.IsPinnedWeight>
<Beetle.NeighborWeight>26</Beetle.NeighborWeight>
<Grasshopper.InPlayWeight>13</Grasshopper.InPlayWeight>
<Grasshopper.ValidMoveWeight>-50</Grasshopper.ValidMoveWeight>
<Grasshopper.IsPinnedWeight>-4</Grasshopper.IsPinnedWeight>
<Grasshopper.NeighborWeight>0</Grasshopper.NeighborWeight>
<SoldierAnt.InPlayWeight>5</SoldierAnt.InPlayWeight>
<SoldierAnt.ValidMoveWeight>4</SoldierAnt.ValidMoveWeight>
<SoldierAnt.IsPinnedWeight>0</SoldierAnt.IsPinnedWeight>
<SoldierAnt.NeighborWeight>0</SoldierAnt.NeighborWeight>
</MetricWeights>
";
    }
}
