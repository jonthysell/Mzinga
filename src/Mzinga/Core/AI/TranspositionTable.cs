// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

namespace Mzinga.Core.AI
{
    public class TranspositionTable : FixedCache<ulong, TranspositionTableEntry>
    {
        public TranspositionTable(int sizeInMegaBytes) : base(GetCapacity(1024L * 1024L * sizeInMegaBytes), TranspostionTableReplaceEntryPredicate) { }

        private static int GetCapacity(long sizeInBytes)
        {
            if (sizeInBytes < EntrySizeInBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(sizeInBytes));
            }

            return 1 + (int)Math.Round(DefaultFillFactor * sizeInBytes / EntrySizeInBytes);
        }

        private static bool TranspostionTableReplaceEntryPredicate(TranspositionTableEntry existingEntry, TranspositionTableEntry entry)
        {
            return entry.Depth > existingEntry.Depth;
        }

        private static readonly long EntrySizeInBytes = EstimateSizeInBytes(sizeof(ulong), TranspositionTableEntry.SizeInBytes);
    }

    public class TranspositionTableEntry
    {
        public TranspositionTableEntryType Type;
        public double Value;
        public int Depth;
        public Move? BestMove;

        public static readonly int SizeInBytes = sizeof(TranspositionTableEntryType) // Type
                                                  + sizeof(double) // Value
                                                  + sizeof(int) // Depth
                                                  + IntPtr.Size // BestMove pointer
                                                  + sizeof(PieceName) // BestMove PieceName
                                                  + (6 * sizeof(int)) // BestMove Position values
                                                  ;
    }

    public enum TranspositionTableEntryType : byte
    {
        Exact = 0,
        LowerBound,
        UpperBound
    }
}
