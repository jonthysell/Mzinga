// 
// TranspositionTable.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2017, 2018, 2019, 2021 Jon Thysell <http://jonthysell.com>
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
using System.Xml;

namespace Mzinga.Core.AI
{
    public class TranspositionTable : FixedCache<ulong, TranspositionTableEntry>
    {
        public TranspositionTable(long sizeInBytes = DefaultSizeInBytes) : base(GetCapacity(sizeInBytes), TranspostionTableReplaceEntryPredicate) { }

        public static TranspositionTable ReadTranspositionTableXml(XmlReader xmlReader, long sizeInBytes = DefaultSizeInBytes)
        {
            if (null == xmlReader)
            {
                throw new ArgumentNullException(nameof(xmlReader));
            }

            TranspositionTable tt = new TranspositionTable(sizeInBytes);

            while (xmlReader.Read())
            {
                if (xmlReader.IsStartElement() && xmlReader.Name == "Entry")
                {
                    ulong key = ulong.Parse(xmlReader.GetAttribute("Key"));

                    string moveString = xmlReader.GetAttribute("BestMove");

                    TranspositionTableEntry entry = new TranspositionTableEntry()
                    {
                        Type = (TranspositionTableEntryType)Enum.Parse(typeof(TranspositionTableEntryType), xmlReader.GetAttribute("Type")),
                        Value = double.Parse(xmlReader.GetAttribute("Value")),
                        Depth = int.Parse(xmlReader.GetAttribute("Depth")),
                        BestMove = !string.IsNullOrWhiteSpace(moveString) ? new Move(moveString) : null,
                    };
                    tt.Store(key, entry);
                }
            }

            return tt;
        }

        public void WriteTranspositionTableXml(XmlWriter xmlWriter, string name = "TranspositionTable", ExpansionPieces? gameType = null, Predicate<TranspositionTableEntry> entryPredicate = null)
        {
            if (null == xmlWriter)
            {
                throw new ArgumentNullException(nameof(xmlWriter));
            }

            xmlWriter.WriteStartElement(name);

            if (gameType.HasValue)
            {
                xmlWriter.WriteAttributeString("GameType", EnumUtils.GetExpansionPiecesString(gameType.Value));
            }

            foreach (ulong key in Keys)
            {
                if (TryLookup(key, out TranspositionTableEntry entry))
                {
                    if (null == entryPredicate || entryPredicate(entry))
                    {
                        xmlWriter.WriteStartElement("Entry");
                        xmlWriter.WriteAttributeString("Key", key.ToString());
                        xmlWriter.WriteAttributeString("Type", entry.Type.ToString());
                        xmlWriter.WriteAttributeString("Value", entry.Value.ToString());
                        xmlWriter.WriteAttributeString("Depth", entry.Depth.ToString());
                        xmlWriter.WriteAttributeString("BestMove", entry.BestMove?.ToString() ?? "");
                        xmlWriter.WriteEndElement();
                    }
                }
            }

            xmlWriter.WriteEndElement();
        }

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

        private const double FillFactor = 0.99; // To leave room for unaccounted overhead and unused dictionary capcacity
    }

    public class TranspositionTableEntry
    {
        public TranspositionTableEntryType Type;
        public double Value;
        public int Depth;
        public Move BestMove;

        public static readonly long SizeInBytes = 2 * ( // For some reason the actual size is closer to double the amount calculated below
                                                  sizeof(TranspositionTableEntryType) // Type
                                                  + sizeof(double) // Value
                                                  + sizeof(int) // Depth
                                                  + IntPtr.Size // BestMove pointer
                                                  + sizeof(PieceName) // BestMove PieceName
                                                  + sizeof(PlayerColor) // BestMove PieceName Color
                                                  + sizeof(BugType) // BestMove PieceName BugType
                                                  + IntPtr.Size // BestMove Position pointer
                                                  + IntPtr.Size // BestMove Position internal pointer
                                                  + (3 * sizeof(int)) // BestMove Position values
                                                  );
    }

    public enum TranspositionTableEntryType : byte
    {
        Exact = 0,
        LowerBound,
        UpperBound
    }
}
