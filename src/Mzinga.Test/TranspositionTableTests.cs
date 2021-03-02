// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Mzinga.Core;
using Mzinga.Core.AI;

namespace Mzinga.Test
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
            long expectedSizeInBytes = TranspositionTable.DefaultSizeInBytes;

            TranspositionTable tt = new TranspositionTable(expectedSizeInBytes);
            Assert.IsNotNull(tt);

            long startMemoryUsage = GC.GetTotalAllocatedBytes(true);
            for (int i = 0; i < tt.Capacity; i++)
            {
                ulong key = (ulong)i;
                tt.Store(key, CreateMaxEntry());
            }
            long endMemoryUsage = GC.GetTotalAllocatedBytes(true);

            Assert.AreEqual(tt.Capacity, tt.Count);

            long actualSizeInBytes = endMemoryUsage - startMemoryUsage;

            double usageRatio = actualSizeInBytes / (double)expectedSizeInBytes;
            Trace.WriteLine(string.Format("Usage: {0}/{1} ({2:P2})", actualSizeInBytes, expectedSizeInBytes, usageRatio));

            Assert.IsTrue(usageRatio - 1.0 <= 0.01, string.Format("Table is too big {0:P2}: {1} bytes (~{2} bytes per entry)", usageRatio, actualSizeInBytes - expectedSizeInBytes, (actualSizeInBytes - expectedSizeInBytes) / tt.Count));
        }

        [TestMethod]
        [TestCategory("Performance")]
        public void TranspositionTable_FillAndReplacePerfTest()
        {
            TimeSpan sum = TimeSpan.Zero;
            int iterations = 1000;

            for (int i = 0; i < iterations; i++)
            {
                TranspositionTable tt = new TranspositionTable(TranspositionTable.DefaultSizeInBytes / 1024);
                Assert.IsNotNull(tt);

                Stopwatch sw = Stopwatch.StartNew();

                // Fill
                for (int j = 0; j < tt.Capacity; j++)
                {
                    ulong key = (ulong)j;
                    tt.Store(key, CreateMaxEntry());
                }

                // Replace
                for (int j = tt.Capacity - 1; j >= 0; j--)
                {
                    TranspositionTableEntry newEntry = CreateMaxEntry();
                    newEntry.Depth++;
                    ulong key = (ulong)j;
                    tt.Store(key, newEntry);
                }

                sw.Stop();

                sum += sw.Elapsed;
            }

            Trace.WriteLine(string.Format("Average Ticks: {0}", sum.Ticks / iterations));
        }

        private static TranspositionTableEntry CreateMaxEntry()
        {
            TranspositionTableEntry te = new TranspositionTableEntry
            {
                Depth = 0,
                Type = TranspositionTableEntryType.Exact,
                Value = 0,
                BestMove = new Move(PieceName.WhiteSoldierAnt1, new Position(0, 0, 0, 0))
            };
            return te;
        }
    }
}
