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
<QueenBee.InPlayWeight>-29.142509782600857</QueenBee.InPlayWeight>
<QueenBee.IsPinnedWeight>-367.92746559937552</QueenBee.IsPinnedWeight>
<QueenBee.IsCoveredWeight>0</QueenBee.IsCoveredWeight>
<QueenBee.NoisyMoveWeight>0</QueenBee.NoisyMoveWeight>
<Spider.InPlayWeight>-45.567707071100024</Spider.InPlayWeight>
<Spider.IsPinnedWeight>-26831.334773078139</Spider.IsPinnedWeight>
<Spider.IsCoveredWeight>0</Spider.IsCoveredWeight>
<Spider.NoisyMoveWeight>0</Spider.NoisyMoveWeight>
<Beetle.InPlayWeight>-24.466044878707834</Beetle.InPlayWeight>
<Beetle.IsPinnedWeight>-189.67722817259119</Beetle.IsPinnedWeight>
<Beetle.IsCoveredWeight>0</Beetle.IsCoveredWeight>
<Beetle.NoisyMoveWeight>0</Beetle.NoisyMoveWeight>
<Grasshopper.InPlayWeight>-23.777069473201863</Grasshopper.InPlayWeight>
<Grasshopper.IsPinnedWeight>342.53072327550677</Grasshopper.IsPinnedWeight>
<Grasshopper.IsCoveredWeight>0</Grasshopper.IsCoveredWeight>
<Grasshopper.NoisyMoveWeight>0</Grasshopper.NoisyMoveWeight>
<SoldierAnt.InPlayWeight>219.12497547801658</SoldierAnt.InPlayWeight>
<SoldierAnt.IsPinnedWeight>5.3302371043673871</SoldierAnt.IsPinnedWeight>
<SoldierAnt.IsCoveredWeight>0</SoldierAnt.IsCoveredWeight>
<SoldierAnt.NoisyMoveWeight>0</SoldierAnt.NoisyMoveWeight>
<Mosquito.InPlayWeight>87.952390733671677</Mosquito.InPlayWeight>
<Mosquito.IsPinnedWeight>0.016229015128553265</Mosquito.IsPinnedWeight>
<Mosquito.IsCoveredWeight>0</Mosquito.IsCoveredWeight>
<Mosquito.NoisyMoveWeight>0</Mosquito.NoisyMoveWeight>
<Ladybug.InPlayWeight>0</Ladybug.InPlayWeight>
<Ladybug.IsPinnedWeight>0</Ladybug.IsPinnedWeight>
<Ladybug.IsCoveredWeight>0</Ladybug.IsCoveredWeight>
<Ladybug.NoisyMoveWeight>0</Ladybug.NoisyMoveWeight>
<Pillbug.InPlayWeight>0</Pillbug.InPlayWeight>
<Pillbug.IsPinnedWeight>0</Pillbug.IsPinnedWeight>
<Pillbug.IsCoveredWeight>0</Pillbug.IsCoveredWeight>
<Pillbug.NoisyMoveWeight>0</Pillbug.NoisyMoveWeight>
</MetricWeights>
";
    }
}
