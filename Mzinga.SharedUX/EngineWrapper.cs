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

namespace Mzinga.SharedUX
{
    public abstract class EngineWrapper
    {
        public string ID { get; private set; }

        public GameSettings CurrentGameSettings { get; private set; }

        public GameBoard Board
        {
            get
            {
                return CurrentGameSettings?.CurrentGameBoard;
            }
            private set
            {
                if (null == CurrentGameSettings)
                {
                    // Just in case
                    CurrentGameSettings = new GameSettings() { WhitePlayerType = PlayerType.Human, BlackPlayerType = PlayerType.Human };
                }

                CurrentGameSettings.CurrentGameBoard = value;
                OnBoardUpdate();
            }
        }

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

        public EngineOptions EngineOptions { get; private set; }

        public EngineCapabilities EngineCapabilities { get; private set; }

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
                        ((Board.CurrentTurnColor == PlayerColor.White && CurrentGameSettings.WhitePlayerType == PlayerType.Human) ||
                         (Board.CurrentTurnColor == PlayerColor.Black && CurrentGameSettings.BlackPlayerType == PlayerType.Human)));
            }
        }

        public bool CurrentTurnIsEngineAI
        {
            get
            {
                return (GameInProgress &&
                        ((Board.CurrentTurnColor == PlayerColor.White && CurrentGameSettings.WhitePlayerType == PlayerType.EngineAI) ||
                         (Board.CurrentTurnColor == PlayerColor.Black && CurrentGameSettings.BlackPlayerType == PlayerType.EngineAI)));
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
                return CanPlayMove(TargetMove) && !TargetMove.IsPass && CurrentGameSettings.GameMode == GameMode.Play;
            }
        }

        public bool CanPass
        {
            get
            {
                return GameInProgress && CurrentTurnIsHuman && null != ValidMoves && ValidMoves.Contains(Move.Pass) && CurrentGameSettings.GameMode == GameMode.Play;
            }
        }

        public bool CanUndoLastMove
        {
            get
            {
                return null != Board && CanUndoMoveCount > 0 && CurrentGameSettings.GameMode == GameMode.Play;
            }
        }

        public int CanUndoMoveCount
        {
            get
            {
                int moves = 0;

                int historyCount = null != Board ? Board.BoardHistoryCount : 0;

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

        public bool CanMoveBack
        {
            get
            {
                return null != CurrentGameSettings && CurrentGameSettings.GameMode == GameMode.Review && CurrentGameSettings.CurrentGameBoard.BoardHistoryCount > 0;
            }
        }

        public bool CanMoveForward
        {
            get
            {
                return null != CurrentGameSettings && CurrentGameSettings.GameMode == GameMode.Review && CurrentGameSettings.CurrentGameBoard.BoardHistoryCount < CurrentGameSettings.GameRecording.GameBoard.BoardHistoryCount;
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

        public event EventHandler IsIdleUpdated;

        public event EventHandler BoardUpdated;
        public event EventHandler ValidMovesUpdated;
        public event EventHandler EngineTextUpdated;

        public event EventHandler TargetPieceUpdated;
        public event EventHandler TargetPositionUpdated;

        public event EventHandler MovePlaying;
        public event EventHandler MoveUndoing;

        public event EventHandler GameModeChanged;

        public event EventHandler<TimedCommandProgressEventArgs> TimedCommandProgressUpdated;

        private Queue<Action> _commandCallbacks;

        private List<string> _outputLines;

        private Queue<string> _inputToProcess;
        private EngineCommand? _currentlyRunningCommand = null;

        private CancellationTokenSource _timedCommandCTS = null;
        private Task _timedCommandTask = null;

        public EngineWrapper()
        {
            _commandCallbacks = new Queue<Action>();

            _outputLines = new List<string>();
            _inputToProcess = new Queue<string>();
            EngineOptions = new EngineOptions();

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
                    string[] outputLines = new string[_outputLines.Count - 1];
                    _outputLines.CopyTo(0, outputLines, 0, outputLines.Length);
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
            CurrentGameSettings = settings ?? throw new ArgumentNullException("settings");

            SendCommand("newgame {0}", () => { OnGameModeChanged(); }, EnumUtils.GetExpansionPiecesString(CurrentGameSettings.ExpansionPieces));
        }

        public void LoadGame(GameRecording gameRecording)
        {
            if (null == gameRecording)
            {
                throw new ArgumentNullException("gameRecording");
            }

            CurrentGameSettings = new GameSettings(gameRecording)
            {
                WhitePlayerType = PlayerType.Human,
                BlackPlayerType = PlayerType.Human,
                GameMode = GameMode.Review,
            };

            SendCommand("newgame {0}", () => { OnGameModeChanged(); }, CurrentGameSettings.CurrentGameBoard.ToGameString());
        }

        public void PlayTargetMove()
        {
            if (CurrentGameSettings.GameMode != GameMode.Play)
            {
                throw new Exception("Please switch the current game to play mode first.");
            }

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
                SendCommand("play {0}", NotationUtils.ToBoardSpaceMoveString(Board, TargetMove));
            }
        }

        public void Pass()
        {
            if (CurrentGameSettings.GameMode != GameMode.Play)
            {
                throw new Exception("Please switch the current game to play mode first.");
            }

            SendCommand("pass");
        }

        public void UndoLastMove()
        {
            if (CurrentGameSettings.GameMode != GameMode.Play)
            {
                throw new Exception("Please switch the current game to play mode first.");
            }

            int moves = CanUndoMoveCount;

            if (moves > 0)
            {
                SendCommand("undo {0}", moves);
            }
        }

        public void SwitchToReviewMode()
        {
            if (CurrentGameSettings.GameMode == GameMode.Review)
            {
                throw new Exception("The current game is already in review mode.");
            }

            CurrentGameSettings.GameMode = GameMode.Review;
        }

        public void MoveToStart()
        {
            if (CurrentGameSettings.GameMode != GameMode.Review)
            {
                throw new Exception("Please switch the current game to review mode first.");
            }

            SendCommand("newgame {0}", (new GameBoard(CurrentGameSettings.ExpansionPieces)).ToGameString());
        }

        public void MoveBack()
        {
            if (CurrentGameSettings.GameMode != GameMode.Review)
            {
                throw new Exception("Please switch the current game to review mode first.");
            }

            SendCommand("undo");
        }

        public void MoveForward()
        {
            if (CurrentGameSettings.GameMode != GameMode.Review)
            {
                throw new Exception("Please switch the current game to review mode first.");
            }

            int targetMove = CurrentGameSettings.CurrentGameBoard.BoardHistoryCount;

            int currentMove = 0;
            foreach(BoardHistoryItem item in CurrentGameSettings.GameRecording.GameBoard.BoardHistory)
            {
                if (currentMove == targetMove)
                {
                    SendCommand("play {0}", item.MoveString);
                    break;
                }
                currentMove++;
            }
        }

        public void MoveToEnd()
        {
            if (CurrentGameSettings.GameMode != GameMode.Review)
            {
                throw new Exception("Please switch the current game to review mode first.");
            }

            SendCommand("newgame {0}", CurrentGameSettings.GameRecording.GameBoard.ToGameString());
        }

        public void FindBestMove()
        {
            if (CurrentGameSettings.BestMoveType == BestMoveType.MaxDepth)
            {
                SendCommand("bestmove depth {0}", CurrentGameSettings.BestMoveMaxDepth);
            }
            else
            {
                SendCommand("bestmove time {0}", CurrentGameSettings.BestMoveMaxTime);
            }
        }

        public void OptionsList(Action callback = null)
        {
            SendCommand("options", callback);
        }

        public void OptionsSet(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException("value");
            }

            SendCommand("options set {0} {1}", key, value);
        }

        public void OptionsSet(IDictionary<string, string> options)
        {
            if (null == options)
            {
                throw new ArgumentNullException("options");
            }

            if (options.Count > 0)
            {
                if (IsIdle)
                {
                    IsIdle = false;
                    try
                    {
                        foreach (KeyValuePair<string, string> kvp in options)
                        {
                            SendCommandInternal("options set {0} {1}", kvp.Key, kvp.Value);
                        }
                    }
                    catch (Exception)
                    {
                        IsIdle = true;
                        throw;
                    }
                }
            }
        }

        public void SendCommand(string command, params object[] args)
        {
            SendCommand(command, null, args);
        }

        public void SendCommand(string command, Action callback, params object[] args)
        {
            if (IsIdle)
            {
                IsIdle = false;
                try
                {
                    SendCommandInternal(command, callback, args);
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
            SendCommandInternal(command, null, args);
        }

        private void SendCommandInternal(string command, Action callback, params object[] args)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentNullException("command");
            }

            command = string.Format(command, args);

            TryStartTimedCommand(command);

            EngineCommand cmd = IdentifyCommand(command);

            if (cmd == EngineCommand.Exit)
            {
                throw new Exception("Can't send exit command.");
            }

            _inputToProcess.Enqueue(command);

            if (null != callback)
            {
                _commandCallbacks.Enqueue(callback);
            }

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
                OnSendingCommand(_currentlyRunningCommand);
                OnEngineInput(command);
            }
            else
            {
                _currentlyRunningCommand = null;
                IsIdle = true;
                if (_commandCallbacks.Count > 0)
                {
                    _commandCallbacks.Dequeue().Invoke();
                }
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
                case "options":
                    return EngineCommand.Options;
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

            for (int i = 0; i < outputLines.Length; i++)
            {
                if (outputLines[i].StartsWith("err"))
                {
                    errorMessage += outputLines[i].Substring(outputLines[i].IndexOf(' ') + 1) + Environment.NewLine;
                }
                else if (outputLines[i].StartsWith("invalidmove"))
                {
                    invalidMoveMessage += outputLines[i].Substring(outputLines[i].IndexOf(' ') + 1) + Environment.NewLine;
                }
            }

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                throw new EngineException(errorMessage.Trim(), outputLines);
            }

            if (!string.IsNullOrWhiteSpace(invalidMoveMessage))
            {
                throw new InvalidMoveException(invalidMoveMessage.Trim(), outputLines);
            }

            string firstLine = "";
            string lastLine = "";

            if (null != outputLines && outputLines.Length > 0)
            {
                firstLine = outputLines[0];
                lastLine = outputLines[outputLines.Length - 1];
            }

            // Update other properties
            switch (command)
            {
                case EngineCommand.NewGame:
                case EngineCommand.Play:
                case EngineCommand.Pass:
                case EngineCommand.Undo:
                    Board = !string.IsNullOrWhiteSpace(firstLine) ? GameBoard.ParseGameString(firstLine, true) : null;
                    break;
                case EngineCommand.ValidMoves:
                    ValidMoves = !string.IsNullOrWhiteSpace(firstLine) ? NotationUtils.ParseMoveStringList(Board, firstLine) : null;
                    break;
                case EngineCommand.BestMove:
                    // Update the target move (and potentially auto-play it)
                    ProcessBestMove(lastLine, true);
                    TryStopTimedCommand();
                    break;
                case EngineCommand.Options:
                    string[] optionLines = new string[outputLines.Length];
                    Array.Copy(outputLines, optionLines, optionLines.Length);
                    EngineOptions.ParseEngineOptionLines(optionLines);
                    break;
                case EngineCommand.Info:
                    ID = firstLine.StartsWith("id ") ? firstLine.Substring(3).Trim() : "Unknown";
                    EngineCapabilities = new EngineCapabilities(lastLine);
                    break;
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
                Move bestMove = NotationUtils.ParseMoveString(Board, line.Split(';')[0]);

                TargetPiece = bestMove.PieceName;
                TargetPosition = bestMove.Position;
                TargetMove = bestMove;
            }
            catch (Exception)
            {
                TargetPiece = PieceName.INVALID;
            }

            if (tryToPlay && CurrentTurnIsEngineAI && CurrentGameSettings.GameMode == GameMode.Play && null != TargetMove)
            {
                if (TargetMove.IsPass)
                {
                    SendCommandInternal("pass");
                }
                else
                {
                    SendCommandInternal("play {0}", NotationUtils.ToBoardSpaceMoveString(Board, TargetMove));
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

            if (GameInProgress)
            {
                SendCommandInternal("validmoves");
            }

            BoardUpdated?.Invoke(this, null);

            if (CurrentTurnIsEngineAI && CurrentGameSettings.GameMode == GameMode.Play)
            {
                if (CurrentGameSettings.BestMoveType == BestMoveType.MaxDepth)
                {
                    SendCommandInternal("bestmove depth {0}", CurrentGameSettings.BestMoveMaxDepth);
                }
                else
                {
                    SendCommandInternal("bestmove time {0}", CurrentGameSettings.BestMoveMaxTime);
                }
            }
        }

        private void OnValidMovesUpdate()
        {
            ValidMovesUpdated?.Invoke(this, null);
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

        private void OnGameModeChanged()
        {
            GameModeChanged?.Invoke(this, null);
        }

        private void OnSendingCommand(EngineCommand? command)
        {
            switch (command)
            {
                case EngineCommand.Play:
                case EngineCommand.Pass:
                    MovePlaying?.Invoke(this, null);
                    break;
                case EngineCommand.Undo:
                    MoveUndoing?.Invoke(this, null);
                    break;
            }
        }

        private void OnTimedCommandProgressUpdated(bool isRunning, double progress = 1.0)
        {
            TimedCommandProgressUpdated?.Invoke(this, new TimedCommandProgressEventArgs(isRunning, progress));
        }

        private void TryStartTimedCommand(string command)
        {
            EngineCommand cmd = IdentifyCommand(command);

            if (cmd == EngineCommand.BestMove)
            {
                string[] split = command.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length > 2 && split[1] == "time")
                {
                    if (TimeSpan.TryParse(split[2], out TimeSpan ts))
                    {
                        StartTimedCommand(ts);
                    }
                }
            }
        }

        private void StartTimedCommand(TimeSpan duration)
        {
            OnTimedCommandProgressUpdated(true, 0.0);

            _timedCommandCTS = new CancellationTokenSource();
            CancellationToken token = _timedCommandCTS.Token;

            _timedCommandTask = Task.Run(async () =>
            {
                Stopwatch sw = Stopwatch.StartNew();
                while (sw.Elapsed <= duration && !token.IsCancellationRequested)
                {
                    OnTimedCommandProgressUpdated(true, sw.Elapsed.TotalMilliseconds / duration.TotalMilliseconds);
                    await Task.Yield();
                }
                OnTimedCommandProgressUpdated(true, 1.0);
            }, token);
        }

        private void TryStopTimedCommand()
        {
            if (null != _timedCommandCTS && null != _timedCommandTask)
            {
                StopTimedCommand();
            }
        }

        private void StopTimedCommand()
        {
            try
            {
                _timedCommandCTS.Cancel();
                _timedCommandTask.Wait(_timedCommandCTS.Token);
            }
            catch (OperationCanceledException) { }
            finally
            {
                OnTimedCommandProgressUpdated(false);
                _timedCommandCTS = null;
                _timedCommandTask = null;
            }
        }

        private enum EngineCommand
        {
            Unknown = -1,
            Info = 0,
            NewGame,
            Play,
            Pass,
            ValidMoves,
            BestMove,
            Undo,
            Options,
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
