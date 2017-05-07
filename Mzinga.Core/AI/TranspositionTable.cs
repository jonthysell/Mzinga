// 
// TranspositionTable.cs
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

namespace Mzinga.Core.AI
{
    public class TranspositionTable : FixedCache<string, TranspositionTableEntry>
    {
        public TranspositionTable(long sizeInBytes = DefaultSizeInBytes) : base(GetCapacity(sizeInBytes), TranspostionTableReplaceEntryPredicate) { }

        private static int GetCapacity(long sizeInBytes)
        {
            if (sizeInBytes < EntrySizeInBytes)
            {
                throw new ArgumentOutOfRangeException("sizeInBytes");
            }

            return 1 + (int)Math.Round(FillFactor * sizeInBytes / EntrySizeInBytes);
        }

        private static bool TranspostionTableReplaceEntryPredicate(TranspositionTableEntry existingEntry, TranspositionTableEntry entry)
        {
            return entry.Depth > existingEntry.Depth;
        }

        public new void Store(string key, TranspositionTableEntry entry)
        {
            if (null == entry)
            {
                throw new ArgumentNullException("entry");
            }

            base.Store(key, entry);
        }

        public new bool TryLookup(string key, out TranspositionTableEntry entry)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }

            if (base.TryLookup(key, out entry))
            {
                return true;
            }

            entry = null;
            return false;
        }

        private static readonly long EntrySizeInBytes = (2 * IntPtr.Size) // Key pointers x2
                                                        + ((2 * 8 + 4 * 11 + 16 * 9) * sizeof(char)) // Key length (2x Q, 4x B, 16x other)
                                                        + IntPtr.Size // Entry pointer
                                                        + TranspositionTableEntry.SizeInBytes; // Entry object

        public const long DefaultSizeInBytes = 32 * 1024 * 1024;

        private const double FillFactor = 0.92; // To leave room for unaccounted for overhead and unused dictionary capcacity
    }

    public class TranspositionTableEntry
    {
        public TranspositionTableEntryType Type;
        public double Value;
        public int Depth;
        public string BestMove;

        public static readonly long SizeInBytes = sizeof(TranspositionTableEntryType)
                                                    + sizeof(double) // Value
                                                    + sizeof(int) // Depth
                                                    + IntPtr.Size // BestMove pointer
                                                    + (14 * sizeof(char)); // BestMove length
    }

    public enum TranspositionTableEntryType : byte
    {
        Exact = 0,
        LowerBound,
        UpperBound
    }
}
