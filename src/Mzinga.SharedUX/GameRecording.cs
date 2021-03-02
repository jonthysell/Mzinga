// 
// GameRecording.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2018, 2019, 2021 Jon Thysell <http://jonthysell.com>
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
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Mzinga.Core;

namespace Mzinga.SharedUX
{
    public class GameRecording
    {
        public GameBoard GameBoard { get; private set; }

        public GameMetadata Metadata { get; private set; }

        public GameRecordingSource GameRecordingSource { get; private set; }

        public string FileName { get; set; } = null;

        public GameRecording(GameBoard gameBoard, GameRecordingSource gameRecordingSource, GameMetadata metadata = null)
        {
            GameBoard = gameBoard?.Clone() ?? throw new ArgumentNullException(nameof(gameBoard));

            GameRecordingSource = gameRecordingSource;

            if (null != metadata)
            {
                Metadata = metadata.Clone();
            }
            else
            {
                Metadata = new GameMetadata();
                Metadata.SetTag("GameType", EnumUtils.GetExpansionPiecesString(gameBoard.ExpansionPieces));
                Metadata.SetTag("Result", gameBoard.BoardState.ToString());
            }
        }

        public void SavePGN(Stream outputStream)
        {
            if (null == outputStream)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }

            using StreamWriter sw = new StreamWriter(outputStream, Encoding.ASCII);
            // Write Mandatory Tags
            sw.WriteLine(GetPGNTag("GameType", EnumUtils.GetExpansionPiecesString(Metadata.GameType)));

            sw.WriteLine(GetPGNTag("Date", Metadata.Date));
            sw.WriteLine(GetPGNTag("Event", Metadata.Event));
            sw.WriteLine(GetPGNTag("Site", Metadata.Site));
            sw.WriteLine(GetPGNTag("Round", Metadata.Round));
            sw.WriteLine(GetPGNTag("White", Metadata.White));
            sw.WriteLine(GetPGNTag("Black", Metadata.Black));

            sw.WriteLine(GetPGNTag("Result", Metadata.Result.ToString()));

            // Write Optional Tags
            foreach (KeyValuePair<string, string> tag in Metadata.OptionalTags)
            {
                sw.WriteLine(GetPGNTag(tag.Key, tag.Value));
            }

            if (GameBoard.BoardHistory.Count > 0)
            {
                sw.WriteLine();

                WritePGNMoveCommentary(sw, 0);

                // Write Moves
                int count = 1;
                foreach (BoardHistoryItem item in GameBoard.BoardHistory)
                {
                    sw.WriteLine("{0}. {1}", count, NotationUtils.NormalizeBoardSpaceMoveString(item.MoveString));
                    WritePGNMoveCommentary(sw, count);

                    count++;
                }
            }

            // Write Result
            if (EnumUtils.GameIsOver(Metadata.Result))
            {
                sw.WriteLine();
                sw.WriteLine(Metadata.Result.ToString());
            }
        }

        private static string GetPGNTag(string key, string value)
        {
            return string.Format("[{0} \"{1}\"]", key.Trim(), null != value ? value.Trim() : "");
        }

        private void WritePGNMoveCommentary(StreamWriter streamWriter,  int moveNum)
        {
            string commentary = Metadata.GetMoveCommentary(moveNum)?.Trim(new char[] { ' ', '\t', '\r', '\n' });
            if (!string.IsNullOrEmpty(commentary))
            {
                streamWriter.WriteLine("{" + commentary + "}");
            }
        }

        public static GameRecording LoadPGN(Stream inputStream, string fileName = null)
        {
            if (null == inputStream)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            GameMetadata metadata = new GameMetadata();

            List<string> moveList = new List<string>();

            string rawResult = "";

            string multiLineCommentary = null;

            using (StreamReader sr = new StreamReader(inputStream, Encoding.ASCII))
            {
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();

                    if (null != multiLineCommentary)
                    {
                        // Line is part of multiline commentary
                        multiLineCommentary += Environment.NewLine + line;

                        if (multiLineCommentary.EndsWith("}"))
                        {
                            // End of multiline commentary
                            metadata.SetMoveCommentary(moveList.Count, multiLineCommentary);
                            multiLineCommentary = null;
                        }
                    }
                    else if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        // Line is a tag
                        KeyValuePair<string, string> kvp = ParsePGNTag(line);
                        if (kvp.Key == "Result")
                        {
                            rawResult = kvp.Value;
                        }
                        else
                        {
                            metadata.SetTag(kvp.Key, kvp.Value);
                        }
                    }
                    else if (line.StartsWith("{") && line.EndsWith("}"))
                    {
                        // Line is a single line of commentary
                        metadata.SetMoveCommentary(moveList.Count, line);
                    }
                    else if (line.StartsWith("{") && null == multiLineCommentary)
                    {
                        multiLineCommentary = line;
                    }
                    else if (!string.IsNullOrWhiteSpace(line))
                    {
                        // Line is a move or result
                        if (Enum.TryParse(line, out BoardState lineResult))
                        {
                            rawResult = lineResult.ToString();
                        }
                        else
                        {
                            // Not a result, add as moveString
                            moveList.Add(line.TrimStart('.', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0'));
                        }
                    }
                }
            }

            GameBoard gameBoard = new GameBoard(metadata.GameType);

            foreach (string moveString in moveList)
            {
                Move move = null;
                try
                {
                    move = NotationUtils.ParseMoveString(gameBoard, moveString);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Unable to parse '{0}'.", moveString), ex);
                }

                gameBoard.TrustedPlay(move, moveString);
            }

            // Set result
            if (Enum.TryParse(rawResult, out BoardState result))
            {
                metadata.SetTag("Result", result.ToString());
            }
            else
            {
                metadata.SetTag("Result", gameBoard.BoardState.ToString());
            }

            return new GameRecording(gameBoard, GameRecordingSource.PGN, metadata)
            {
                FileName = fileName?.Trim()
            };
        }

        private static KeyValuePair<string, string> ParsePGNTag(string line)
        {
            line = line.TrimStart('[').TrimEnd(']');

            int spaceIndex = line.IndexOf(' ');

            string key = line.Substring(0, spaceIndex).Trim();
            string value = line.Substring(spaceIndex).Replace("\"", "").Trim();
            return new KeyValuePair<string, string>(key, value);
        }

        public static GameRecording LoadSGF(Stream inputStream, string fileName = null)
        {
            if (null == inputStream)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            GameMetadata metadata = new GameMetadata();

            List<string> moveList = new List<string>();

            Dictionary<string, Stack<string>> backupPositions = new Dictionary<string, Stack<string>>();

            string rawResult = "";
            bool lastMoveCompleted = true;
            bool whiteTurn = true;
            using (StreamReader sr = new StreamReader(inputStream))
            {
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        // Line has contents
                        Match m = null;
                        if ((m = Regex.Match(line, @"SU\[(.*)\]")).Success)
                        {
                            metadata.SetTag("GameType", m.Groups[1].Value.ToUpper().Replace("HIVE", EnumUtils.NoExpansionsString).Replace("-", "+"));
                        }
                        else if ((m = Regex.Match(line, @"EV\[(.*)\]")).Success)
                        {
                            metadata.SetTag("Event", m.Groups[1].Value);
                        }
                        else if ((m = Regex.Match(line, @"PC\[(.*)\]")).Success)
                        {
                            metadata.SetTag("Site", m.Groups[1].Value);
                        }
                        else if ((m = Regex.Match(line, @"RO\[(.*)\]")).Success)
                        {
                            metadata.SetTag("Round", m.Groups[1].Value);
                        }
                        else if ((m = Regex.Match(line, @"DT\[(.+)\]")).Success)
                        {
                            string rawDate = m.Groups[1].Value;
                            metadata.SetTag("SgfDate", rawDate);

                            rawDate = Regex.Replace(rawDate, @"(\D{3}) (\D{3}) (\d{2}) (\d{2}):(\d{2}):(\d{2}) (.{3,}) (\d{4})", @"$1 $2 $3 $4:$5:$6 $8");

                            if (DateTime.TryParseExact(rawDate, SgfDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed))
                            {
                                metadata.SetTag("Date", parsed.ToString("yyyy.MM.dd"));
                            }
                            else
                            {
                                foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
                                {
                                    if (DateTime.TryParseExact(rawDate, SgfDateFormats, ci, DateTimeStyles.None, out parsed))
                                    {
                                        metadata.SetTag("Date", parsed.ToString("yyyy.MM.dd"));
                                        break;
                                    }
                                }
                            }
                        }
                        else if ((m = Regex.Match(line, @"P0\[id ""(.*)""\]")).Success)
                        {
                            metadata.SetTag("White", m.Groups[1].Value);
                        }
                        else if ((m = Regex.Match(line, @"P1\[id ""(.*)""\]")).Success)
                        {
                            metadata.SetTag("Black", m.Groups[1].Value);
                        }
                        else if ((m = Regex.Match(line, @"RE\[(.+)\]")).Success)
                        {
                            rawResult = m.Groups[1].Value;
                            metadata.SetTag("SgfResult", m.Groups[1].Value);
                        }
                        else if ((m = Regex.Match(line, @"GN\[(.+)\]")).Success)
                        {
                            metadata.SetTag("SgfGameName", m.Groups[1].Value);
                        }
                        else if ((m = Regex.Match(line, @"((move (w|b))|(dropb)|(pdropb)) ([a-z0-9]+) ([a-z] [0-9]+) ([a-z0-9\\\-\/\.]*)", RegexOptions.IgnoreCase)).Success)
                        {
                            // Initial parse
                            string movingPiece = m.Groups[^3].Value.ToLower();
                            string destination = m.Groups[^1].Value.ToLower().Replace("\\\\", "\\");

                            string backupPos = m.Groups[^2].Value;

                            // Remove unnecessary numbers
                            movingPiece = movingPiece.Replace("m1", "m").Replace("l1", "l").Replace("p1", "p");
                            destination = destination.Replace("m1", "m").Replace("l1", "l").Replace("p1", "p");

                            // Add missing color indicator
                            if (movingPiece == "b1" || movingPiece == "b2" || !(movingPiece.StartsWith("b") || movingPiece.StartsWith("w")))
                            {
                                movingPiece = (whiteTurn ? "w" : "b") + movingPiece;
                            }

                            // Fix missing destination
                            if (destination == ".")
                            {
                                if (moveList.Count == 0)
                                {
                                    destination = "";
                                }
                                else
                                {
                                    destination = backupPositions[backupPos].Peek();
                                }
                            }

                            // Remove move that wasn't commited
                            if (!lastMoveCompleted)
                            {
                                moveList.RemoveAt(moveList.Count - 1);
                            }

                            moveList.Add(string.Format("{0} {1}", movingPiece, destination));

                            foreach (Stack<string> stack in backupPositions.Values)
                            {
                                if (stack.Count > 0 && stack.Peek() == movingPiece)
                                {
                                    stack.Pop();
                                    break;
                                }
                            }

                            if (!backupPositions.ContainsKey(backupPos))
                            {
                                backupPositions.Add(backupPos, new Stack<string>());
                            }

                            backupPositions[backupPos].Push(movingPiece);

                            lastMoveCompleted = false;
                        }
                        else if ((m = Regex.Match(line, @"P(0|1)\[[0-9]+ pass\]")).Success)
                        {
                            moveList.Add(NotationUtils.BoardSpacePass);

                            lastMoveCompleted = false;
                        }
                        else if ((m = Regex.Match(line, @"P(0|1)\[[0-9]+ done\]")).Success)
                        {
                            lastMoveCompleted = true;
                            whiteTurn = !whiteTurn;
                        }
                        else if ((m = Regex.Match(line, @"P(0|1)\[[0-9]+ resign\]")).Success)
                        {
                            rawResult = m.Groups[1].Value == "0" ? BoardState.BlackWins.ToString() : BoardState.WhiteWins.ToString();
                        }
                    }
                }
            }

            GameBoard gameBoard = new GameBoard(metadata.GameType);

            foreach (string moveString in moveList)
            {
                Move move = null;
                try
                {
                    move = NotationUtils.ParseMoveString(gameBoard, moveString);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Unable to parse '{0}'.", moveString), ex);
                }

                gameBoard.TrustedPlay(move, NotationUtils.NormalizeBoardSpaceMoveString(moveString));
            }

            // Set result
            if (rawResult.Contains(metadata.White))
            {
                metadata.SetTag("Result", BoardState.WhiteWins.ToString());
            }
            else if (rawResult.Contains(metadata.Black))
            {
                metadata.SetTag("Result", BoardState.BlackWins.ToString());
            }
            else if (rawResult == "The game is a draw")
            {
                metadata.SetTag("Result", BoardState.Draw.ToString());
            }
            else if (Enum.TryParse(rawResult, out BoardState parsed))
            {
                metadata.SetTag("Result", parsed.ToString());
            }
            else
            {
                metadata.SetTag("Result", gameBoard.BoardState.ToString());
            }

            return new GameRecording(gameBoard, GameRecordingSource.SGF, metadata)
            {
                FileName = fileName?.Trim()
            };
        }

        private static readonly string[] SgfDateFormats = new string[]
        {
            "d MMMM yyyy",
            "d. MMMM yyyy",
            "MMMM d, yyyy",
            "ddd MMM dd HH:mm:ss yyyy",
        };
    }

    public enum GameRecordingSource
    {
        Game,
        PGN,
        SGF
    }
}
