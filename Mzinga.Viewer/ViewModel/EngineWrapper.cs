// 
// EngineWrapper.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015 Jon Thysell <http://jonthysell.com>
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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Mzinga.Core;

namespace Mzinga.Viewer.ViewModel
{
    public delegate void BoardUpdatedEventHandler(string boardString);

    public delegate void EngineTextUpdatedEventHandler(string boardString);

    public class EngineWrapper
    {
        public string BoardString
        {
            get
            {
                return _boardString;
            }
            private set
            {
                _boardString = value;
                OnBoardUpdate(BoardString);
            }
        }
        private string _boardString = "";

        public BoardState? BoardState { get; private set; }

        public int? CurrentTurn { get; private set; }

        public Color? CurrentTurnColor { get; private set; }

        public string EngineText
        {
            get
            {
                return _engineText.ToString();
            }
        }
        private StringBuilder _engineText;

        public event BoardUpdatedEventHandler BoardUpdated;
        public event EngineTextUpdatedEventHandler EngineTextUpdated;

        private Process _process;
        private StreamReader _reader;
        private StreamWriter _writer;

        public EngineWrapper(string engineCommand)
        {
            _engineText = new StringBuilder();
            StartEngine(engineCommand);
        }

        private void StartEngine(string engineName)
        {
            if (String.IsNullOrWhiteSpace(engineName))
            {
                throw new ArgumentNullException("engineName");
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = engineName;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;

            _engineText.Clear();

            _process = Process.Start(startInfo);
            _reader = _process.StandardOutput;
            _writer = _process.StandardInput;

            ReadEngineOutput(EngineCommand.Info);
        }

        public void Close()
        {
            _writer.WriteLine("exit");
            _process.WaitForExit();
            _process.Close();
            _process = null;
        }

        public void SendCommand(string command)
        {
            if (String.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentNullException("command");
            }

            EngineCommand cmd = IdentifyCommand(command);

            if (cmd == EngineCommand.Exit)
            {
                throw new Exception("Can't send exit command.");
            }

            EngineTextAppendLine(command);
            _writer.WriteLine(command);
            ReadEngineOutput(cmd);
        }

        private EngineCommand IdentifyCommand(string command)
        {
            if (String.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentNullException("command");
            }

            string[] split = command.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            string cmd = split[0].ToLower();

            switch (cmd)
            {
                case "info":
                    return EngineCommand.Info;
                case "?":
                case "help":
                    return EngineCommand.Help;
                case "board":
                    return EngineCommand.Board;
                case "newgame":
                    return EngineCommand.NewGame;
                case "play":
                    return EngineCommand.Play;
                case "pass":
                    return EngineCommand.Pass;
                case "validmoves":
                    return EngineCommand.ValidMoves;
                case "bestmove":
                    return EngineCommand.BestMove;
                case "undo":
                    return EngineCommand.Undo;
                case "history":
                    return EngineCommand.History;
                case "exit":
                    return EngineCommand.Exit;
                default:
                    return EngineCommand.Unknown;
            }
        }

        private void ReadEngineOutput(EngineCommand command)
        {
            StringBuilder sb = new StringBuilder();

            string errorMessage = "";
            string invalidMoveMessage = "";

            string line;
            while (null != (line = _reader.ReadLine()))
            {
                EngineTextAppendLine(line);
                if (line == "ok")
                {
                    break;
                }
                else
                {
                    if (line.StartsWith("err"))
                    {
                        errorMessage = line.Substring(line.IndexOf(' ') + 1);
                    }
                    else if (line.StartsWith("invalidmove"))
                    {
                        invalidMoveMessage = line.Substring(line.IndexOf(' ') + 1);
                    }
                    sb.AppendLine(line);
                }
            }

            if (!String.IsNullOrWhiteSpace(errorMessage))
            {
                throw new EngineException(errorMessage);
            }

            if (!String.IsNullOrWhiteSpace(invalidMoveMessage))
            {
                throw new InvalidMoveException(invalidMoveMessage);
            }

            string[] output = sb.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            // Update other properties
            switch (command)
            {
                case EngineCommand.Board:
                case EngineCommand.NewGame:
                case EngineCommand.Play:
                case EngineCommand.Pass:
                case EngineCommand.Undo:
                    BoardString = output[0];
                    break;
                case EngineCommand.ValidMoves:
                case EngineCommand.BestMove:
                case EngineCommand.History:
                case EngineCommand.Info:
                case EngineCommand.Help:
                default:
                    break;
            }
        }

        private void EngineTextAppendLine(string line)
        {
            _engineText.AppendLine(line);
            OnEngineTextUpdate(EngineText);
        }

        private void OnBoardUpdate(string boardString)
        {
            // Parse out other states
            if (!String.IsNullOrWhiteSpace(boardString))
            {
                Match match = Regex.Match(boardString, BoardStringRegex, RegexOptions.IgnoreCase);

                BoardState boardState;
                if (Enum.TryParse<BoardState>(match.Groups[1].Value, out boardState))
                {
                    BoardState = boardState;
                }
                else
                {
                    BoardState = null;
                }

                Color currentTurnColor;
                if (Enum.TryParse<Color>(match.Groups[2].Value, out currentTurnColor))
                {
                    CurrentTurnColor = currentTurnColor;
                }
                else
                {
                    CurrentTurnColor = null;
                }

                int currentTurn;
                if (Int32.TryParse(match.Groups[3].Value, out currentTurn))
                {
                    CurrentTurn = currentTurn;
                }
                else
                {
                    CurrentTurn = null;
                }
            }

            if (null != BoardUpdated)
            {
                BoardUpdated(boardString);
            }
        }

        private void OnEngineTextUpdate(string engineText)
        {
            if (null != EngineTextUpdated)
            {
                EngineTextUpdated(engineText);
            }
        }

        private enum EngineCommand
        {
            Unknown = -1,
            Info = 0,
            Help,
            Board,
            NewGame,
            Play,
            Pass,
            ValidMoves,
            BestMove,
            Undo,
            History,
            Exit
        }

        protected const string BoardStringRegex = @"^([a-z]*);(White|Black)\[([0-9]*)\]";
    }
}
