// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

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
            MetricWeights mw = new MetricWeights();
            Assert.IsNotNull(mw);
            var normalized = mw.GetNormalized();
            Assert.IsNotNull(normalized);
        }
    }
}
