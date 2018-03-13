// 
// EngineWrapper.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017, 2018 Jon Thysell <http://jonthysell.com>
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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Mzinga.Core;

namespace Mzinga.Viewer.ViewModel
{
    public abstract class EngineWrapper
    {
        public ViewerBoard Board
        {
            get
            {
                return _board;
            }
            private set
            {
                _board = value;
                OnBoardUpdate();
            }
        }
        private ViewerBoard _board = null;

        public MoveSet ValidMoves
        {
            get
            {
                return _validMoves;
            }
            private set
            {
                _validMoves = value;
                OnValidMovesUpdate();

            }
        }
        private MoveSet _validMoves;

        public BoardHistory BoardHistory
        {
            get
            {
                return _boardHistory;
            }
            private set
            {
                _boardHistory = value;
                OnBoardHistoryUpdate();
            }
        }
        private BoardHistory _boardHistory;

        public bool IsIdle
        {
            get
            {
                return _isIdle;
            }
            protected set
            {
                _isIdle = value;
                OnIsIdleUpdate();
            }
        }
        private volatile bool _isIdle = true;

        public bool GameInProgress
        {
            get
            {
                return (null != Board && Board.GameInProgress);
            }
        }

        public bool GameIsOver
        {
            get
            {
                return (null != Board && Board.GameIsOver);
            }
        }

        public bool CurrentTurnIsHuman
        {
            get
            {
                return (null != Board &&
                        ((Board.CurrentTurnColor == Color.White && CurrentGameSettings.WhitePlayerType == PlayerType.Human) ||
                         (Board.CurrentTurnColor == Color.Black && CurrentGameSettings.BlackPlayerType == PlayerType.Human)));
            }
        }

        public bool CurrentTurnIsEngineAI
        {
            get
            {
                return (GameInProgress &&
                        ((Board.CurrentTurnColor == Color.White && CurrentGameSettings.WhitePlayerType == PlayerType.EngineAI) ||
                         (Board.CurrentTurnColor == Color.Black && CurrentGameSettings.BlackPlayerType == PlayerType.EngineAI)));
            }
        }

        public PieceName TargetPiece
        {
            get
            {
                return _targetPiece;
            }
            set
            {
                PieceName oldValue = _targetPiece;

                _targetPiece = value;

                if (oldValue != value)
                {
                    OnTargetPieceUpdate();
                }
            }
        }
        private PieceName _targetPiece = PieceName.INVALID;

        public Position TargetPosition
        {
            get
            {
                return _targetPosition;
            }
            set
            {
                Position oldValue = _targetPosition;

                _targetPosition = value;

                if (oldValue != value)
                {
                    OnTargetPositionUpdate();
                }
            }
        }
        private Position _targetPosition = null;

        public Move TargetMove { get; private set; }

        public bool CanPlayTargetMove
        {
            get
            {
                return CanPlayMove(TargetMove) && !TargetMove.IsPass;
            }
        }

        public bool CanPass
        {
            get
            {
                return (GameInProgress && CurrentTurnIsHuman && null != ValidMoves && ValidMoves.Contains(Move.Pass));
            }
        }

        public bool CanUndoLastMove
        {
            get
            {
                return (null != Board && CanUndoMoveCount > 0);
            }
        }

        public int CanUndoMoveCount
        {
            get
            {
                int moves = 0;

                int historyCount = null != BoardHistory ? BoardHistory.Count : 0;

                if (null != Board && historyCount > 0)
                {
                    if (CurrentGameSettings.WhitePlayerType == PlayerType.Human && CurrentGameSettings.BlackPlayerType == PlayerType.Human)
                    {
                        moves = 1; // Can only undo one Human move
                    }
                    else if (CurrentGameSettings.WhitePlayerType != CurrentGameSettings.BlackPlayerType)
                    {
                        if (CurrentTurnIsHuman)
                        {
                            moves = 2; // Undo the previous move (AI) and the move before that (Human)
                        }
                        else if (GameIsOver)
                        {
                            moves = 1; // Undo the previous, game-ending move (Human)
                        }
                    }
                    else if (CurrentGameSettings.WhitePlayerType == PlayerType.EngineAI && CurrentGameSettings.BlackPlayerType == PlayerType.EngineAI)
                    {
                        moves = 0; // Can't undo an AI vs AI's moves
                    }
                }

                // Only undo moves if there are enough moves to undo
                if (moves <= historyCount)
                {
                    return moves;
                }

                return 0;
            }
        }

        public bool CanFindBestMove
        {
            get
            {
                return CurrentTurnIsHuman && GameInProgress && null != ValidMoves && ValidMoves.Count > 0;
            }
        }

        public string EngineText { get; private set; } = "";

        public GameSettings CurrentGameSettings
        {
            get
            {
                if (null == _currentGameSettings)
                {
                    _currentGameSettings = new GameSettings();
                }

                return _currentGameSettings.Clone();
            }
            private set
            {
                _currentGameSettings = (null != value) ? value.Clone() : null;
            }
        }
        private GameSettings _currentGameSettings;

        public event EventHandler IsIdleUpdated;

        public event EventHandler BoardUpdated;
        public event EventHandler ValidMovesUpdated;
        public event EventHandler BoardHistoryUpdated;
        public event EventHandler EngineTextUpdated;

        public event EventHandler TargetPieceUpdated;
        public event EventHandler TargetPositionUpdated;

        public event EventHandler<TimedCommandProgressEventArgs> TimedCommandProgressUpdated;

        private List<string> _outputLines;

        private Queue<string> _inputToProcess;
        private EngineCommand? _currentlyRunningCommand = null;

        private CancellationTokenSource _timedCommandCTS = null;
        private Task _timedCommandTask = null;

        public EngineWrapper()
        {
            _outputLines = new List<string>();
            _inputToProcess = new Queue<string>();

            _inputToProcess.Enqueue("info");
            _currentlyRunningCommand = EngineCommand.Info;
        }

        public abstract void StartEngine();

        public abstract void StopEngine();

        protected abstract void OnEngineInput(string command);

        protected void OnEngineOutput(string line)
        {
            EngineTextAppendLine(line);
            _outputLines.Add(line);

            if (line == "ok")
            {
                try
                {
                    string[] outputLines = _outputLines.ToArray();
                    ProcessEngineOutput(IdentifyCommand(_inputToProcess.Peek()), outputLines);
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
                finally
                {
                    _inputToProcess.Dequeue();
                    _outputLines.Clear();
                    RunNextCommand();
                }
            }
            else if (_currentlyRunningCommand == EngineCommand.BestMove)
            {
                // Got a preliminary bestmove result, update the TargetMove but don't autoplay it
                ProcessBestMove(line, false);
            }
        }

        public void NewGame(GameSettings settings)
        {
            if (null == settings)
            {
                throw new ArgumentNullException("settings");
            }

            CurrentGameSettings = settings;

            SendCommand("newgame {0}", EnumUtils.GetExpansionPiecesString(CurrentGameSettings.ExpansionPieces));
        }

        public void PlayTargetMove()
        {
            if (null == TargetMove)
            {
                throw new Exception("Please select a valid piece and destination first.");
            }

            if (TargetMove.IsPass)
            {
                Pass();
            }
            else
            {
                SendCommand("play {0}", TargetMove);
            }
        }

        public void Pass()
        {
            SendCommand("pass");
        }

        public void UndoLastMove()
        {
            int moves = CanUndoMoveCount;

            if (moves > 0)
            {
                SendCommand("undo {0}", moves);
            }
        }

        public void FindBestMove()
        {
            if (CurrentGameSettings.BestMoveType == BestMoveType.MaxDepth)
            {
                SendCommand("bestmove depth {0}", CurrentGameSettings.BestMoveMaxDepth);
            }
            else
            {
                StartTimedCommand(CurrentGameSettings.BestMoveMaxTime.Value);
                SendCommand("bestmove time {0}", CurrentGameSettings.BestMoveMaxTime);
            }
        }

        public void SendCommand(string command, params object[] args)
        {
            if (IsIdle)
            {
                IsIdle = false;
                try
                {
                    SendCommandInternal(command, args);
                }
                catch (Exception)
                {
                    IsIdle = true;
                    throw;
                }
            }
        }

        private void SendCommandInternal(string command, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentNullException("command");
            }

            command = string.Format(command, args);

            EngineCommand cmd = IdentifyCommand(command);

            if (cmd == EngineCommand.Exit)
            {
                throw new Exception("Can't send exit command.");
            }

            _inputToProcess.Enqueue(command);

            if (_inputToProcess.Count == 1)
            {
                RunNextCommand();
            }
        }

        private void RunNextCommand()
        {
            if (_inputToProcess.Count > 0)
            {
                IsIdle = false;
                string command = _inputToProcess.Peek();
                EngineTextAppendLine(command);
                _currentlyRunningCommand = IdentifyCommand(command);
                OnEngineInput(command);
            }
            else
            {
                _currentlyRunningCommand = null;
                IsIdle = true;
            }
        }

        private EngineCommand IdentifyCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
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

        private void ProcessEngineOutput(EngineCommand command, string[] outputLines)
        {
            string errorMessage = "";
            string invalidMoveMessage = "";

            foreach (string line in outputLines)
            {
                if (line.StartsWith("err"))
                {
                    errorMessage = line.Substring(line.IndexOf(' ') + 1);
                }
                else if (line.StartsWith("invalidmove"))
                {
                    invalidMoveMessage = line.Substring(line.IndexOf(' ') + 1);
                }
            }

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new EngineException(errorMessage);
            }

            if (!string.IsNullOrWhiteSpace(invalidMoveMessage))
            {
                throw new InvalidMoveException(invalidMoveMessage);
            }

            string firstLine = "";
            string lastLine = "";

            if (null != outputLines && outputLines.Length > 0)
            {
                firstLine = outputLines[0];
                lastLine = outputLines[outputLines.Length - 2]; // ignore the ok line
            }

            // Update other properties
            switch (command)
            {
                case EngineCommand.Board:
                case EngineCommand.NewGame:
                case EngineCommand.Play:
                case EngineCommand.Pass:
                case EngineCommand.Undo:
                    Board = !string.IsNullOrWhiteSpace(firstLine) ? new ViewerBoard(firstLine) : null;
                    break;
                case EngineCommand.ValidMoves:
                    ValidMoves = !string.IsNullOrWhiteSpace(firstLine) ? new MoveSet(firstLine) : null;
                    break;
                case EngineCommand.BestMove:
                    // Update the target move (and potentially auto-play it)
                    ProcessBestMove(lastLine, true);
                    StopTimedCommand();
                    break;
                case EngineCommand.History:
                    BoardHistory = !string.IsNullOrWhiteSpace(firstLine) ? new BoardHistory(firstLine) : null;
                    break;
                case EngineCommand.Info:
                case EngineCommand.Help:
                default:
                    break;
            }
        }

        private void EngineTextAppendLine(string line)
        {
            EngineText += string.Format("{0}{1}", line, Environment.NewLine);
            OnEngineTextUpdate();
        }

        private void ProcessBestMove(string line, bool tryToPlay)
        {
            try
            {
                Move bestMove = new Move(line.Split(';')[0]);

                TargetPiece = bestMove.PieceName;
                TargetPosition = bestMove.Position;
                TargetMove = bestMove;
            }
            catch (Exception)
            {
                TargetPiece = PieceName.INVALID;
            }

            if (tryToPlay && CurrentTurnIsEngineAI && null != TargetMove)
            {
                if (TargetMove.IsPass)
                {
                    SendCommandInternal("pass");
                }
                else
                {
                    SendCommandInternal("play {0}", TargetMove);
                }
            }
        }

        public PieceName GetPieceAt(double cursorX, double cursorY, double hexRadius, HexOrientation hexOrientation)
        {
            Position position = PositionUtils.FromCursor(cursorX, cursorY, hexRadius, hexOrientation);

            return (null != Board) ? Board.GetPieceOnTop(position) : PieceName.INVALID;
        }

        public Position GetTargetPositionAt(double cursorX, double cursorY, double hexRadius, HexOrientation hexOrientation)
        {
            Position bottomPosition = PositionUtils.FromCursor(cursorX, cursorY, hexRadius, hexOrientation);

            PieceName topPiece = (null != Board) ? Board.GetPieceOnTop(bottomPosition) : PieceName.INVALID;

            if (topPiece == PieceName.INVALID)
            {
                // No piece there, return position at bottom of the stack (stack == 0)
                return bottomPosition;
            }
            else
            {
                // Piece present, return position on top of the piece
                return Board.GetPiecePosition(topPiece).GetAbove();
            }
        }

        public bool CanPlayMove(Move move)
        {
            return (GameInProgress && CurrentTurnIsHuman && null != move && null != ValidMoves && ValidMoves.Contains(move));
        }

        private void OnIsIdleUpdate()
        {
            IsIdleUpdated?.Invoke(this, null);
        }

        private void OnBoardUpdate()
        {
            TargetPiece = PieceName.INVALID;
            ValidMoves = null;

            SendCommandInternal("history");

            if (GameInProgress)
            {
                SendCommandInternal("validmoves");
            }

            BoardUpdated?.Invoke(this, null);

            if (CurrentTurnIsEngineAI)
            {
                if (CurrentGameSettings.BestMoveType == BestMoveType.MaxDepth)
                {
                    SendCommandInternal("bestmove depth {0}", CurrentGameSettings.BestMoveMaxDepth);
                }
                else
                {
                    StartTimedCommand(CurrentGameSettings.BestMoveMaxTime.Value);
                    SendCommandInternal("bestmove time {0}", CurrentGameSettings.BestMoveMaxTime);
                }
            }
        }

        private void OnValidMovesUpdate()
        {
            ValidMovesUpdated?.Invoke(this, null);
        }

        private void OnBoardHistoryUpdate()
        {
            BoardHistoryUpdated?.Invoke(this, null);
        }

        private void OnEngineTextUpdate()
        {
            EngineTextUpdated?.Invoke(this, null);
        }

        private void OnTargetPieceUpdate()
        {
            TargetPosition = null;

            TargetPieceUpdated?.Invoke(this, null);
        }

        private void OnTargetPositionUpdate()
        {
            TargetMove = null;
            if (TargetPiece != PieceName.INVALID && null != TargetPosition)
            {
                TargetMove = new Move(TargetPiece, TargetPosition);
            }

            TargetPositionUpdated?.Invoke(this, null);
        }

        private void OnTimedCommandProgressUpdated(bool isRunning, double progress = 1.0)
        {
            TimedCommandProgressUpdated?.Invoke(this, new TimedCommandProgressEventArgs(isRunning, progress));
        }

        private void StartTimedCommand(TimeSpan duration)
        {
            OnTimedCommandProgressUpdated(true, 0.0);

            _timedCommandCTS = new CancellationTokenSource();
            _timedCommandTask = Task.Run(() =>
            {
                Stopwatch sw = Stopwatch.StartNew();
                while (sw.Elapsed <= duration && !_timedCommandCTS.Token.IsCancellationRequested)
                {
                    OnTimedCommandProgressUpdated(true, sw.Elapsed.TotalMilliseconds / duration.TotalMilliseconds);
                    Thread.Sleep(100);
                }
                OnTimedCommandProgressUpdated(true, 1.0);
            });
        }

        private void StopTimedCommand()
        {
            _timedCommandCTS?.Cancel();
            _timedCommandTask?.Wait();
            OnTimedCommandProgressUpdated(false);

            _timedCommandCTS = null;
            _timedCommandTask = null;
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

    public class TimedCommandProgressEventArgs : EventArgs
    {
        public bool IsRunning { get; private set; }
        public double Progress { get; private set; }

        public TimedCommandProgressEventArgs(bool isRunning, double progress)
        {
            IsRunning = isRunning;
            Progress = progress;
        }
    }
}
