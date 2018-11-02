// 
// GameRecording.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2018 Jon Thysell <http://jonthysell.com>
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
using System.Collections.Generic;
using System.IO;
using System.Text;

using Mzinga.Core;

namespace Mzinga.SharedUX
{
    public class GameRecording
    {
        #region Mandatory Tags

        public string Event { get; private set; } = ""; // Event name
        public string Site { get; private set; } = ""; // City, Region COUNTRY
        public string Date { get; private set; } = ""; // Date played in YYYY.MM.DD
        public string Round { get; private set; } = ""; // Round number
        public string White { get; private set; } = ""; // White player Lastname, Firstname
        public string Black { get; private set; } = ""; // Black player Lastname, Firstname

        public ExpansionPieces GameType { get; private set; } = ExpansionPieces.None;
        public BoardState Result { get; private set; } = BoardState.NotStarted;

        #endregion

        #region Optional Tags

        public IReadOnlyDictionary<string, string> OptionalTags
        {
            get
            {
                return _optionalTags;
            }
        }

        private Dictionary<string, string> _optionalTags = new Dictionary<string, string>();

        #endregion

        public GameBoard GameBoard { get; private set; }

        private GameRecording() { }

        public GameRecording(GameBoard gameBoard)
        {
            if (null == gameBoard)
            {
                throw new ArgumentNullException("gameBoard");
            }

            GameBoard = gameBoard.Clone();

            GameType = gameBoard.ExpansionPieces;
            Result = gameBoard.BoardState;
        }

        public void SetTag(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }

            value = null != value ? value.Replace("\"", "").Trim() : "";

            switch (key)
            {
                case "Event":
                    Event = value;
                    break;
                case "Site":
                    Site = value;
                    break;
                case "Date":
                    Date = value;
                    break;
                case "Round":
                    Round = value;
                    break;
                case "White":
                    White = value;
                    break;
                case "Black":
                    Black = value;
                    break;
                case "GameType":
                    GameType = EnumUtils.ParseExpansionPieces(value);
                    break;
                case "Result":
                    Result = (BoardState)Enum.Parse(typeof(BoardState), value);
                    break;
                default:
                    _optionalTags[key] = value;
                    break;
            }
        }

        public void SavePGN(Stream outputStream)
        {
            if (null == outputStream)
            {
                throw new ArgumentNullException("outputStream");
            }

            using (StreamWriter sw = new StreamWriter(outputStream, Encoding.ASCII))
            {
                // Write Mandatory Tags
                sw.WriteLine(GetPGNTag("Event", Event));
                sw.WriteLine(GetPGNTag("Site", Site));
                sw.WriteLine(GetPGNTag("Date", Date));
                sw.WriteLine(GetPGNTag("Round", Round));
                sw.WriteLine(GetPGNTag("White", White));
                sw.WriteLine(GetPGNTag("Black", Black));

                sw.WriteLine(GetPGNTag("GameType", EnumUtils.GetExpansionPiecesString(GameType)));
                sw.WriteLine(GetPGNTag("Result", Result.ToString()));

                // Write Optional Tags
                foreach (KeyValuePair<string, string> tag in OptionalTags)
                {
                    sw.WriteLine(GetPGNTag(tag.Key, tag.Value));
                }

                if (GameBoard.BoardHistoryCount > 0)
                {
                    sw.WriteLine();

                    // Write Moves
                    int count = 1;
                    foreach (BoardHistoryItem item in GameBoard.BoardHistory)
                    {
                        sw.WriteLine("{0}. {1}", count, NotationUtils.NormalizeBoardSpaceMoveString(item.MoveString));
                        count++;
                    }
                }

                // Write Result
                if (EnumUtils.GameIsOver(Result))
                {
                    sw.WriteLine();
                    sw.WriteLine(Result.ToString());
                }
            }
        }

        public static GameRecording LoadPGN(Stream inputStream)
        {
            if (null == inputStream)
            {
                throw new ArgumentNullException("inputStream");
            }

            GameRecording gr = new GameRecording();

            List<string> moveList = new List<string>();

            using (StreamReader sr = new StreamReader(inputStream, Encoding.ASCII))
            {
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        // Line has contents
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            // Line is a tag
                            KeyValuePair<string, string> kvp = ParsePGNTag(line);
                            gr.SetTag(kvp.Key, kvp.Value);
                        }
                        else
                        {
                            // Line is a move or result
                            if (!Enum.TryParse(line, out BoardState result))
                            {
                                // Not a result, add as moveString
                                moveList.Add(line.TrimStart('.', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0'));
                            }
                        }
                    }
                }
            }

            GameBoard gb = new GameBoard(gr.GameType);

            foreach (string moveString in moveList)
            {
                Move move = null;
                try
                {
                    move = NotationUtils.ParseMoveString(gb, moveString);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Unable to parse '{0}'.", moveString), ex);
                }

                gb.TrustedPlay(move, moveString);
            }

            gr.GameBoard = gb;

            return gr;
        }

        private static KeyValuePair<string, string> ParsePGNTag(string line)
        {
            string key = "";
            string value = "";

            line = line.TrimStart('[').TrimEnd(']');

            int spaceIndex = line.IndexOf(' ');

            key = line.Substring(0, spaceIndex).Trim();
            value = line.Substring(spaceIndex).Replace("\"", "").Trim();

            return new KeyValuePair<string, string>(key, value);
        }

        private static string GetPGNTag(string key, string value)
        {
            return string.Format("[{0} \"{1}\"]", key.Trim(), null != value ? value.Trim() : "");
        }
    }
}
