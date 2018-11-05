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
using System.Text.RegularExpressions;

using Mzinga.Core;

namespace Mzinga.SharedUX
{
    public class GameRecording
    {
        public GameBoard GameBoard { get; private set; }

        public GameMetadata Metadata {get; private set;}

        public GameRecording(GameBoard gameBoard, GameMetadata metadata = null)
        {
            GameBoard = gameBoard?.Clone() ?? throw new ArgumentNullException("gameBoard");

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
                throw new ArgumentNullException("outputStream");
            }

            using (StreamWriter sw = new StreamWriter(outputStream, Encoding.ASCII))
            {
                // Write Mandatory Tags
                sw.WriteLine(GetPGNTag("Event", Metadata.Event));
                sw.WriteLine(GetPGNTag("Site", Metadata.Site));
                sw.WriteLine(GetPGNTag("Date", Metadata.Date));
                sw.WriteLine(GetPGNTag("Round", Metadata.Round));
                sw.WriteLine(GetPGNTag("White", Metadata.White));
                sw.WriteLine(GetPGNTag("Black", Metadata.Black));

                sw.WriteLine(GetPGNTag("GameType", EnumUtils.GetExpansionPiecesString(Metadata.GameType)));
                sw.WriteLine(GetPGNTag("Result", Metadata.Result.ToString()));

                // Write Optional Tags
                foreach (KeyValuePair<string, string> tag in Metadata.OptionalTags)
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
                if (EnumUtils.GameIsOver(Metadata.Result))
                {
                    sw.WriteLine();
                    sw.WriteLine(Metadata.Result.ToString());
                }
            }
        }

        private static string GetPGNTag(string key, string value)
        {
            return string.Format("[{0} \"{1}\"]", key.Trim(), null != value ? value.Trim() : "");
        }

        public static GameRecording LoadPGN(Stream inputStream)
        {
            if (null == inputStream)
            {
                throw new ArgumentNullException("inputStream");
            }

            GameMetadata metadata = new GameMetadata();

            List<string> moveList = new List<string>();

            string rawResult = "";

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
                            if (kvp.Key == "Result")
                            {
                                rawResult = kvp.Value;
                            }
                            else
                            {
                                metadata.SetTag(kvp.Key, kvp.Value);
                            }
                        }
                        else
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

            return new GameRecording(gameBoard, metadata);
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

        public static GameRecording LoadSGF(Stream inputStream)
        {
            if (null == inputStream)
            {
                throw new ArgumentNullException("inputStream");
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
                        else if ((m = Regex.Match(line, @"P0\[id ""(.*)""\]")).Success)
                        {
                            metadata.SetTag("White", m.Groups[1].Value);
                        }
                        else if ((m = Regex.Match(line, @"P1\[id ""(.*)""\]")).Success)
                        {
                            metadata.SetTag("Black", m.Groups[1].Value);
                        }
                        else if ((m = Regex.Match(line, @"RE\[(.*)\]")).Success)
                        {
                            rawResult = m.Groups[1].Value;
                        }
                        else if ((m = Regex.Match(line, @"DT\[(.*)\]")).Success)
                        {
                            // TODO transform properly
                            metadata.SetTag("Date", m.Groups[1].Value);
                        }
                        else if ((m = Regex.Match(line, @"((move (w|b))|(dropb)|(pdropb)) ([a-z0-9]+) ([a-z] [0-9]+) ([a-z0-9\\\-\/\.]*)", RegexOptions.IgnoreCase)).Success)
                        {
                            // Initial parse
                            string movingPiece = m.Groups[m.Groups.Count - 3].Value.ToLower();
                            string destination = m.Groups[m.Groups.Count - 1].Value.ToLower().Replace("\\\\", "\\");

                            string backupPos = m.Groups[m.Groups.Count - 2].Value;

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

                            // Remoe move that wasn't commited
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

            return new GameRecording(gameBoard, metadata);
        }
    }
}
