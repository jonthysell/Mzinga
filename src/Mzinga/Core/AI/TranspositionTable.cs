// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

namespace Mzinga.Core.AI
{
    public class TranspositionTable : FixedCache<ulong, TranspositionTableEntry>
    {
        public TranspositionTable(long sizeInBytes = DefaultSizeInBytes) : base(GetCapacity(sizeInBytes), TranspostionTableReplaceEntryPredicate) { }

        private static int GetCapacity(long sizeInBytes)
        {
            if (sizeInBytes < EntrySizeInBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(sizeInBytes));
            }

            return 1 + (int)Math.Round(FillFactor * sizeInBytes / EntrySizeInBytes);
        }

        private static bool TranspostionTableReplaceEntryPredicate(TranspositionTableEntry existingEntry, TranspositionTableEntry entry)
        {
            return entry.Depth > existingEntry.Depth;
        }

        private static readonly long EntrySizeInBytes = (4 * sizeof(ulong)) // Key size x4
                                                        + IntPtr.Size // Wrapped entry pointer
                                                        + IntPtr.Size // Wrapped entry, LinkedList node pointer
                                                        + IntPtr.Size // Wrapped entry, entry pointer
                                                        + (4 * IntPtr.Size) // LinkedList node,list,next,previous pointers
                                                        + TranspositionTableEntry.SizeInBytes; // Entry object

        public const long DefaultSizeInBytes = 32 * 1024 * 1024;

        private const double FillFactor = 0.95; // To leave room for unaccounted overhead and unused dictionary capcacity
    }

    public class TranspositionTableEntry
    {
        public TranspositionTableEntryType Type;
        public double Value;
        public int Depth;
        public Move? BestMove;

        public static readonly long SizeInBytes = sizeof(TranspositionTableEntryType) // Type
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
