// 
// TranspositionTableTests.cs
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

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Core.AI;

namespace Mzinga.CoreTest
{
    [TestClass]
    public class TranspositionTableTests
    {
        [TestMethod]
        public void TranspositionTable_NewTest()
        {
            TranspositionTable tt = new TranspositionTable();
            Assert.IsNotNull(tt);
        }

        [TestMethod]
        public void TranspositionTable_MaxMemoryTest()
        {
            long expectedSizeInBytes =TranspositionTable.DefaultSizeInBytes;

            TranspositionTable tt = new TranspositionTable(expectedSizeInBytes);
            Assert.IsNotNull(tt);

            int count = tt.Capacity;

            long startMemoryUsage = GC.GetTotalMemory(true);
            for (int i = 0; i < tt.Capacity; i++)
            {
                string key = i.ToString().PadLeft(204);
                tt.Store(key, CreateMaxEntry(i));
            }
            long endMemoryUsage = GC.GetTotalMemory(true);

            Assert.AreEqual(tt.Capacity, tt.Count);

            long actualSizeInBytes = endMemoryUsage - startMemoryUsage;

            double usageRatio = actualSizeInBytes / (double)expectedSizeInBytes;
            Trace.WriteLine(string.Format("Usage: {0} ({1:P2})", actualSizeInBytes, usageRatio));

            Assert.IsTrue(usageRatio - 1.0 <= 0.01, string.Format("Table is too big {0:P2}: {1} bytes (~{2} bytes per entry)", usageRatio, actualSizeInBytes - expectedSizeInBytes, (actualSizeInBytes - expectedSizeInBytes) / tt.Count));
        }


        private TranspositionTableEntry CreateMaxEntry(int id)
        {
            TranspositionTableEntry te = new TranspositionTableEntry();
            te.Depth = 0;
            te.Type = TranspositionTableEntryType.Exact;
            te.Value = 0;
            te.BestMove = id.ToString().PadLeft(14);
            return te;
        }
    }
}
