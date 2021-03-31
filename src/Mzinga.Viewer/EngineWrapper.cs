// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Mzinga.Core;

namespace Mzinga.Viewer
{
    public abstract class EngineWrapper
    {
        public string ID { get; private set; }

        public GameSettings CurrentGameSettings { get; private set; }

        public Board Board
        {
            get
            {
                return CurrentGameSettings?.CurrentBoard;
            }
            private set
            {
                if (null == CurrentGameSettings)
                {
                    // Just in case
                    CurrentGameSettings = new GameSettings() { WhitePlayerType = PlayerType.Human, BlackPlayerType = PlayerType.Human };
                }

                CurrentGameSettings.CurrentBoard = value;
                OnBoardUpdate();
            }
        }

        public Board ReviewBoard
        {
            get
            {
                return CurrentGameSettings?.GameRecording?.Board;
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
                        ((Board.CurrentColor == PlayerColor.White && CurrentGameSettings.WhitePlayerType == PlayerType.Human) ||
                         (Board.CurrentColor == PlayerColor.Black && CurrentGameSettings.BlackPlayerType == PlayerType.Human)));
            }
        }

        public bool CurrentTurnIsEngineAI
        {
            get
            {
                return (GameInProgress &&
                        ((Board.CurrentColor == PlayerColor.White && CurrentGameSettings.WhitePlayerType == PlayerType.EngineAI) ||
                         (Board.CurrentColor == PlayerColor.Black && CurrentGameSettings.BlackPlayerType == PlayerType.EngineAI)));
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

        public Position? TargetPosition
        {
            get
            {
                return _targetPosition;
            }
            set
            {
                var oldValue = _targetPosition;

                _targetPosition = value;

                if (oldValue != value)
                {
                    OnTargetPositionUpdate();
                }
            }
        }
        private Position? _targetPosition = null;

        public Move? TargetMove { get; private set; }

        public bool CanPlayTargetMove
        {
            get
            {
                return CanPlayMove(TargetMove) && TargetMove != Move.PassMove && CurrentGameSettings.GameMode == GameMode.Play;
            }
        }

        public bool CanPass
        {
            get
            {
                return GameInProgress && CurrentTurnIsHuman && null != ValidMoves && ValidMoves.Contains(Move.PassMove) && CurrentGameSettings.GameMode == GameMode.Play;
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

                int historyCount = null != Board ? Board.BoardHistory.Count : 0;

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
                return null != CurrentGameSettings && CurrentGameSettings.GameMode == GameMode.Review && Board.BoardHistory.Count > 0;
            }
        }

        public bool CanMoveForward
        {
            get
            {
                return null != CurrentGameSettings && CurrentGameSettings.GameMode == GameMode.Review && Board.BoardHistory.Count < ReviewBoard.BoardHistory.Count;
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

        private readonly Queue<Action> _commandCallbacks;

        private readonly List<string> _outputLines;

        private readonly Queue<string> _inputToProcess;
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

        protected abstract void OnCancelCommand();

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
                catch (EngineErrorException ex)
                {
                    TargetPiece = PieceName.INVALID; // Reset the move
                    ExceptionUtils.HandleException(ex);
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
            CurrentGameSettings = settings ?? throw new ArgumentNullException(nameof(settings));

            SendCommand("newgame {0}", () => { OnGameModeChanged(); }, Enums.GetGameTypeString(CurrentGameSettings.GameType));
        }

        public void NewGame(GameSettings settings, string gameString)
        {
            CurrentGameSettings = settings ?? throw new ArgumentNullException(nameof(settings));

            SendCommand("newgame {0}", () => { OnGameModeChanged(); }, gameString);
        }

        public void LoadGame(GameRecording gameRecording)
        {
            if (null == gameRecording)
            {
                throw new ArgumentNullException(nameof(gameRecording));
            }

            CurrentGameSettings = new GameSettings(gameRecording)
            {
                WhitePlayerType = PlayerType.Human,
                BlackPlayerType = PlayerType.Human,
                GameMode = GameMode.Review,
            };

            SendCommand("newgame {0}", () => { OnGameModeChanged(); }, ReviewBoard.GetGameString());
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

            if (TargetMove == Move.PassMove)
            {
                Pass();
            }
            else
            {
                SendCommand("play {0}", Board.GetMoveString(TargetMove.Value));
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
            OnGameModeChanged();
        }

        public void MoveToStart()
        {
            if (CurrentGameSettings.GameMode != GameMode.Review)
            {
                throw new Exception("Please switch the current game to review mode first.");
            }

            SendCommand("newgame {0}", (new Board(CurrentGameSettings.GameType)).GetGameString());
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

            int targetMoveIndex = Board.BoardHistory.Count;

            BoardHistoryItem targetItem = ReviewBoard.BoardHistory[targetMoveIndex];

            SendCommand("play {0}", targetItem.MoveString);
        }

        public void MoveToEnd()
        {
            if (CurrentGameSettings.GameMode != GameMode.Review)
            {
                throw new Exception("Please switch the current game to review mode first.");
            }

            SendCommand("newgame {0}", ReviewBoard.GetGameString());
        }

        public void MoveToMoveNumber(int moveNum)
        {
            if (CurrentGameSettings.GameMode != GameMode.Review)
            {
                throw new Exception("Please switch the current game to review mode first.");
            }

            if (moveNum == 0)
            {
                MoveToStart();
            }
            else if (moveNum == ReviewBoard.BoardHistory.Count)
            {
                MoveToEnd();
            }
            else
            {
                Board newGame = new Board(ReviewBoard.GameType);

                for (int i = 0; i < moveNum; i++)
                {
                    BoardHistoryItem item = ReviewBoard.BoardHistory[i];
                    newGame.Play(item.Move, item.MoveString);
                }

                SendCommand("newgame {0}", newGame.GetGameString());
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
                throw new ArgumentNullException(nameof(key));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            SendCommand("options set {0} {1}", key, value);
        }

        public void OptionsSet(IDictionary<string, string> options)
        {
            if (null == options)
            {
                throw new ArgumentNullException(nameof(options));
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
                throw new ArgumentNullException(nameof(command));
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

        private static EngineCommand IdentifyCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentNullException(nameof(command));
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

        public void CancelCommand()
        {
            if (!IsIdle)
            {
                OnCancelCommand();
                TryStopTimedCommand();
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
                throw new EngineErrorException(errorMessage.Trim(), outputLines);
            }

            if (!string.IsNullOrWhiteSpace(invalidMoveMessage))
            {
                throw new EngineInvalidMoveException(invalidMoveMessage.Trim(), outputLines);
            }

            string firstLine = "";
            string lastLine = "";

            if (null != outputLines && outputLines.Length > 0)
            {
                firstLine = outputLines[0];
                lastLine = outputLines[^1];
            }

            // Update other properties
            switch (command)
            {
                case EngineCommand.NewGame:
                case EngineCommand.Play:
                case EngineCommand.Pass:
                case EngineCommand.Undo:
                    Board = !string.IsNullOrWhiteSpace(firstLine) ? Board.ParseGameString(firstLine, true) : null;
                    break;
                case EngineCommand.ValidMoves:
                    ValidMoves = !string.IsNullOrWhiteSpace(firstLine) ? MoveSet.ParseMoveList(Board, firstLine) : null;
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
                if (!Board.TryParseMove(line.Split(';')[0], out Move bestMove, out string _))
                {
                    throw new Exception($"Unable to parse '{line}'");
                }

                TargetPiece = bestMove.PieceName;
                TargetPosition = bestMove.Destination;
                TargetMove = bestMove;
            }
            catch (Exception)
            {
                TargetPiece = PieceName.INVALID;
            }

            if (tryToPlay && CurrentTurnIsEngineAI && CurrentGameSettings.GameMode == GameMode.Play && TargetMove.HasValue)
            {
                if (TargetMove == Move.PassMove)
                {
                    SendCommandInternal("pass");
                }
                else
                {
                    SendCommandInternal("play {0}", Board.GetMoveString(TargetMove.Value));
                }
            }
        }

        public PieceName GetPieceAt(double cursorX, double cursorY, double hexRadius, HexOrientation hexOrientation)
        {
            Position position = PositionUtils.FromCursor(cursorX, cursorY, hexRadius, hexOrientation);

            return (null != Board) ? Board.GetPieceOnTopAt(position) : PieceName.INVALID;
        }

        public Position GetTargetPositionAt(double cursorX, double cursorY, double hexRadius, HexOrientation hexOrientation)
        {
            Position bottomPosition = PositionUtils.FromCursor(cursorX, cursorY, hexRadius, hexOrientation);

            PieceName topPiece = (null != Board) ? Board.GetPieceOnTopAt(bottomPosition) : PieceName.INVALID;

            if (topPiece == PieceName.INVALID)
            {
                // No piece there, return position at bottom of the stack (stack == 0)
                return bottomPosition;
            }
            else
            {
                // Piece present, return position on top of the piece
                return Board.GetPosition(topPiece).GetAbove();
            }
        }

        public bool CanPlayMove(Move? move)
        {
            return (GameInProgress && CurrentTurnIsHuman && move.HasValue && null != ValidMoves && ValidMoves.Contains(move.Value));
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
            if (TargetPiece != PieceName.INVALID && TargetPosition.HasValue)
            {
                TargetMove = new Move(TargetPiece, Board.GetPosition(TargetPiece), TargetPosition.Value);
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
                    await Task.Delay(100);
                }
                OnTimedCommandProgressUpdated(true, 1.0);
            }, token);
        }

        private void TryStopTimedCommand()
        {
            try
            {
                if (null != _timedCommandCTS && null != _timedCommandTask)
                {
                    _timedCommandCTS.Cancel();
                    _timedCommandTask.Wait(_timedCommandCTS.Token);
                }
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
