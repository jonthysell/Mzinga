// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Mzinga.Core;

namespace Mzinga.Viewer
{
    public class GameRecording
    {
        public Board Board { get; private set; }

        public GameMetadata Metadata { get; private set; }

        public GameRecordingSource GameRecordingSource { get; private set; }

        public Uri FileUri { get; set; } = null;

        public GameRecording(Board board, GameRecordingSource gameRecordingSource, GameMetadata metadata = null)
        {
            Board = board?.Clone() ?? throw new ArgumentNullException(nameof(board));

            GameRecordingSource = gameRecordingSource;

            if (metadata is not null)
            {
                Metadata = metadata.Clone();
            }
            else
            {
                Metadata = new GameMetadata();
                Metadata.SetTag("GameType", Enums.GetGameTypeString(board.GameType));
                Metadata.SetTag("Result", board.BoardState.ToString());
            }
        }

        public void SavePGN(Stream outputStream)
        {
            if (outputStream is null)
            {
                throw new ArgumentNullException(nameof(outputStream));
            }

            using StreamWriter sw = new StreamWriter(outputStream, Encoding.ASCII);
            // Write Mandatory Tags
            sw.WriteLine(GetPGNTag("GameType", Enums.GetGameTypeString(Metadata.GameType)));

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

            if (Board.BoardHistory.Count > 0)
            {
                sw.WriteLine();

                WritePGNMoveCommentary(sw, 0);

                // Write Moves
                int count = 1;
                foreach (BoardHistoryItem item in Board.BoardHistory)
                {
                    sw.WriteLine("{0}. {1}", count, item.MoveString);
                    WritePGNMoveCommentary(sw, count);

                    count++;
                }
            }

            // Write Result
            if (Enums.GameIsOver(Metadata.Result))
            {
                sw.WriteLine();
                sw.WriteLine(Metadata.Result.ToString());
            }
        }

        private static string GetPGNTag(string key, string value)
        {
            return string.Format("[{0} \"{1}\"]", key.Trim(), value is not null ? value.Trim() : "");
        }

        private void WritePGNMoveCommentary(StreamWriter streamWriter,  int moveNum)
        {
            string commentary = Metadata.GetMoveCommentary(moveNum)?.Trim(new char[] { ' ', '\t', '\r', '\n' });
            if (!string.IsNullOrEmpty(commentary))
            {
                streamWriter.WriteLine("{" + commentary + "}");
            }
        }

        public static GameRecording LoadPGN(Stream inputStream, Uri fileUri = null)
        {
            if (inputStream is null)
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
                while ((line = sr.ReadLine()) is not null)
                {
                    line = line.Trim();

                    if (multiLineCommentary is not null)
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
                    else if (line.StartsWith("{") && multiLineCommentary is null)
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

            Board board = new Board(metadata.GameType);

            foreach (string inputMoveStr in moveList)
            {
                if (!board.TryParseMove(inputMoveStr, out Move move, out string moveStr))
                {
                    throw new Exception($"Unable to parse '{inputMoveStr}'.");
                }

                if (!board.TryPlayMove(move, moveStr))
                {
                    throw new Exception($"Unable to play '{inputMoveStr}'.");
                }
            }

            // Set result
            if (Enum.TryParse(rawResult, out BoardState result))
            {
                metadata.SetTag("Result", result.ToString());
            }
            else
            {
                metadata.SetTag("Result", board.BoardState.ToString());
            }

            return new GameRecording(board, GameRecordingSource.PGN, metadata)
            {
                FileUri = fileUri
            };
        }

        private static KeyValuePair<string, string> ParsePGNTag(string line)
        {
            line = line.TrimStart('[').TrimEnd(']');

            int spaceIndex = line.IndexOf(' ');

            string key = line.Substring(0, spaceIndex).Trim();
            string value = line[spaceIndex..].Replace("\"", "").Trim();
            return new KeyValuePair<string, string>(key, value);
        }

        public static GameRecording LoadSGF(Stream inputStream, Uri fileUri = null)
        {
            if (inputStream is null)
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
                while ((line = sr.ReadLine()) is not null)
                {
                    line = line.Trim();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        // Line has contents
                        Match m = null;
                        if ((m = Regex.Match(line, @"SU\[(.*)\]")).Success)
                        {
                            var gameType = GameType.Base;
                            var split = m.Groups[1].Value.ToUpper().Split("-");
                            gameType = Enums.EnableBugType(BugType.Mosquito, gameType, split.Length > 1 && split[1].Contains('M'));
                            gameType = Enums.EnableBugType(BugType.Ladybug, gameType, split.Length > 1 && split[1].Contains('L'));
                            gameType = Enums.EnableBugType(BugType.Pillbug, gameType, split.Length > 1 && split[1].Contains('P'));
                            metadata.SetTag("GameType", Enums.GetGameTypeString(gameType));
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
                            movingPiece = movingPiece.Replace("m1", "M", StringComparison.InvariantCultureIgnoreCase).Replace("l1", "L", StringComparison.InvariantCultureIgnoreCase).Replace("p1", "P", StringComparison.InvariantCultureIgnoreCase);
                            destination = destination.Replace("m1", "M", StringComparison.InvariantCultureIgnoreCase).Replace("l1", "L", StringComparison.InvariantCultureIgnoreCase).Replace("p1", "P", StringComparison.InvariantCultureIgnoreCase);

                            // Add missing color indicator
                            if (movingPiece.Equals("b1", StringComparison.InvariantCultureIgnoreCase) || movingPiece.Equals("b2", StringComparison.InvariantCultureIgnoreCase) || !(movingPiece.StartsWith("b") || movingPiece.StartsWith("w")))
                            {
                                movingPiece = (whiteTurn ? "w" : "b") + movingPiece.ToUpperInvariant();
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
                        else if ((m = Regex.Match(line, @"P(0|1)\[[0-9]+ pass\s*\]", RegexOptions.IgnoreCase)).Success)
                        {
                            moveList.Add(Move.PassString);

                            lastMoveCompleted = false;
                        }
                        else if ((m = Regex.Match(line, @"P(0|1)\[[0-9]+ done\s*\]", RegexOptions.IgnoreCase)).Success)
                        {
                            lastMoveCompleted = true;
                            whiteTurn = !whiteTurn;
                        }
                        else if ((m = Regex.Match(line, @"P(0|1)\[[0-9]+ resign\s*\]", RegexOptions.IgnoreCase)).Success)
                        {
                            rawResult = m.Groups[1].Value == "0" ? BoardState.BlackWins.ToString() : BoardState.WhiteWins.ToString();
                        }
                    }
                }
            }

            Board board = new Board(metadata.GameType);

            foreach (string inputMoveStr in moveList)
            {
                if (!board.TryParseMove(inputMoveStr, out Move move, out string moveStr))
                {
                    throw new Exception($"Unable to parse '{inputMoveStr}'.");
                }

                if (!board.TryPlayMove(move, moveStr))
                {
                    throw new Exception($"Unable to play '{inputMoveStr}'.");
                }
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
                metadata.SetTag("Result", board.BoardState.ToString());
            }

            return new GameRecording(board, GameRecordingSource.SGF, metadata)
            {
                FileUri = fileUri
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
