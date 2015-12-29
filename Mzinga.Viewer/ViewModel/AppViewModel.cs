// 
// AppViewModel.cs
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
using System.Reflection;
using System.Text;

using GalaSoft.MvvmLight;

namespace Mzinga.Viewer.ViewModel
{
    public delegate void DoOnUIThread(Action action);

    public class AppViewModel : ViewModelBase
    {
        public static AppViewModel Instance { get; private set; }

        public string ProgramTitle
        {
            get
            {
                AssemblyName name = Assembly.GetEntryAssembly().GetName();
                return String.Format("{0} v{1}", name.Name, name.Version.ToString());
            }
        }

        public string BoardString
        {
            get
            {
                return _boardString;
            }
            private set
            {
                _boardString = value;
                RaisePropertyChanged("BoardString");
            }
        }
        private string _boardString = "";

        public string EngineText
        {
            get
            {
                return _engineText.ToString();
            }
        }

        public DoOnUIThread DoOnUIThread { get; private set; }

        private StringBuilder _engineText;

        private Process _engine;
        private StreamReader _engineReader;
        private StreamWriter _engineWriter;

        public static void Init(DoOnUIThread doOnUIThread)
        {
            if (null != Instance)
            {
                throw new NotSupportedException();
            }

            Instance = new AppViewModel(doOnUIThread);
        }

        private AppViewModel(DoOnUIThread doOnUIThread)
        {
            if (null == doOnUIThread)
            {
                throw new ArgumentNullException("doOnUIThread");
            }

            DoOnUIThread = doOnUIThread;

            _engineText = new StringBuilder();
            StartEngine("Mzinga.Engine.exe");
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

            _engine = Process.Start(startInfo);
            _engineReader = _engine.StandardOutput;
            _engineWriter = _engine.StandardInput;

            ReadEngineOutput(EngineCommand.Info);
        }

        public void Close()
        {
            _engineWriter.WriteLine("exit");
            _engine.WaitForExit();
            _engine.Close();
            _engine = null;
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
            _engineWriter.WriteLine(command);
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
            while (null != (line = _engineReader.ReadLine()))
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
            RaisePropertyChanged("EngineText");
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
    }
}
