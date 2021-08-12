// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Mzinga.Core;
using Mzinga.Core.AI;

namespace Mzinga.Engine
{
    public class Engine
    {
        public string ID { get; private set; }
        public ConsoleOut ConsoleOut { get; private set; }
        public EngineConfig Config { get; private set; }

        public EngineConfig DefaultConfig { get; private set; }

        public bool ExitRequested { get; private set; }

        public event EventHandler? StartAsyncCommand;
        public event EventHandler? EndAsyncCommand;

        private Board? _board;

        private GameAI? _gameAI;

        private CancellationTokenSource? _asyncCommandCTS = null;

        private Task? _ponderTask = null;
        private CancellationTokenSource? _ponderCTS = null;
        private volatile bool _isPondering = false;

        public Engine(string id, EngineConfig config, ConsoleOut consoleOut)
        {
            ID = id;
            Config = config;
            DefaultConfig = config.GetOptionsClone();
            ConsoleOut = consoleOut;

            ExitRequested = false;
        }

        private void InitAI()
        {
            _gameAI = Config.GetGameAI(_board?.GameType ?? GameType.INVALID);
            _gameAI.BestMoveFound += OnBestMoveFound;

            ResetAI(false);
        }

        private void ResetAI(bool resetCaches)
        {
            if (_gameAI is not null)
            {
                if (resetCaches)
                {
                    StopPonder();
                    _gameAI.ResetCaches();
                }
            }
        }

        private void OnBestMoveFound(object? sender, BestMoveFoundEventArgs args)
        {
            if (_board is not null && !_isPondering && Config.ReportIntermediateBestMoves && _board.TryGetMoveString(args.Move, out string? moveStr))
            {
                ConsoleOut("{0};{1};{2:0.00}", moveStr ?? "", args.Depth, args.Score);
            }
        }

        public void TryCancelAsyncCommand()
        {
            _asyncCommandCTS?.Cancel();
        }

        public void ParseCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentNullException(nameof(command));
            }

            string[] split = command.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                string cmd = split[0].ToLower();

                int paramCount = split.Length - 1;

                switch (cmd)
                {
                    case "info":
                        Info();
                        break;
                    case "?":
                    case "help":
                        if (paramCount == 0)
                        {
                            Help();
                        }
                        else
                        {
                            Help(split[1]);
                        }
                        break;
                    case "newgame":
                        if (paramCount == 0)
                        {
                            NewGame(GameType.Base);
                        }
                        else if (Enums.TryParse(split[1], out GameType gameType))
                        {
                            NewGame(gameType);
                        }
                        else
                        {
                            NewGame(string.Join(" ", split, 1, paramCount));
                        }
                        break;
                    case "play":
                        if (paramCount == 0)
                        {
                            throw new CommandException();
                        }
                        else
                        {
                            Play(string.Join(" ", split, 1, paramCount));
                        }
                        break;
                    case "pass":
                        Pass();
                        break;
                    case "validmoves":
                        ValidMoves();
                        break;
                    case "bestmove":
                        if (paramCount == 0)
                        {
                            BestMove();
                        }
                        else if (paramCount >= 2 && split[1].ToLower() == "depth")
                        {
                            BestMove(int.Parse(split[2]));
                        }
                        else if (paramCount >= 2 && split[1].ToLower() == "time")
                        {
                            BestMove(TimeSpan.Parse(split[2]));
                        }
                        else
                        {
                            throw new CommandException();
                        }
                        break;
                    case "undo":
                        if (paramCount == 0)
                        {
                            Undo();
                        }
                        else
                        {
                            Undo(int.Parse(split[1]));
                        }
                        break;
                    case "options":
                        if (paramCount == 0)
                        {
                            OptionsList();
                        }
                        else if (paramCount >= 2 && split[1].ToLower() == "get")
                        {
                            OptionsGet(split[2]);
                        }
                        else if (paramCount >= 3 && split[1].ToLower() == "set")
                        {
                            OptionsSet(split[2], split[3]);
                        }
                        else
                        {
                            throw new CommandException();
                        }
                        break;
                    case "licenses":
                        Licenses();
                        break;
                    case "perft":
                        if (paramCount == 0)
                        {
                            Perft();
                        }
                        else
                        {
                            Perft(int.Parse(split[1]));
                        }
                        break;
                    case "exit":
                        Exit();
                        return; // nothing to do after exiting
#if DEBUG
                    case "break":
                        Break();
                        break;
#endif
                    default:
                        throw new CommandException();
                }
            }
            catch (InvalidMoveException ex)
            {
                ConsoleOut("invalidmove {0}", ex.Message);
            }
            catch (AggregateException ex)
            {
                ErrorOut(ex);
                foreach (Exception innerEx in ex.InnerExceptions)
                {
                    ErrorOut(innerEx);
                }
            }
            catch (Exception ex)
            {
                ErrorOut(ex);
            }

            ConsoleOut("ok");

            StartPonder();
        }

        private void ErrorOut(Exception ex)
        {
            ConsoleOut("err {0}", ex.Message.Replace("\r\n", " "));
#if DEBUG
            if (ex.StackTrace is not null)
            {
                ConsoleOut(ex.StackTrace);
            }
#endif

            if (ex.InnerException is not null)
            {
                ErrorOut(ex.InnerException);
            }
        }

        private void Info()
        {
            ConsoleOut("id {0}", ID);
            ConsoleOut("Mosquito;Ladybug;Pillbug");
        }

        private void Help(string? command = null)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                ConsoleOut("Game commands:");
                ConsoleOut("  newgame");
                ConsoleOut("  play");
                ConsoleOut("  pass");
                ConsoleOut("  validmoves");
                ConsoleOut("  bestmove");
                ConsoleOut("  undo");

                ConsoleOut("");

                ConsoleOut("Engine commands:");
                ConsoleOut("  info");
                ConsoleOut("  help");
                ConsoleOut("  options");                
                ConsoleOut("  exit");

                ConsoleOut("");

                ConsoleOut("  Advanced commands:");
                ConsoleOut("  licenses");
                ConsoleOut("  perft");

#if DEBUG
                ConsoleOut("");

                ConsoleOut("Debug commands:");
                ConsoleOut("  break");
#endif
                ConsoleOut("");

                ConsoleOut("  Try 'help Command' to see help for a particular Command.");
            }
            else
            {
                string cmd = command.Trim().ToLower();

                switch (cmd)
                {
                    case "info":
                        ConsoleOut("  info");
                        ConsoleOut("");
                        ConsoleOut("  Displays the identifier string of the engine and list of its capabilities.");
                        ConsoleOut("  See https://github.com/jonthysell/Mzinga/wiki/UniversalHiveProtocol#info.");
                        break;
                    case "?":
                    case "help":
                        ConsoleOut("  help [Command]");
                        ConsoleOut("");
                        ConsoleOut("  Displays the list of available commands. If a Command is specified, displays the help for that Command.");
                        break;
                    case "newgame":
                        ConsoleOut("  newgame [GameTypeString|GameString]");
                        ConsoleOut("");
                        ConsoleOut("  Starts a new Base game with no expansion pieces. If GameTypeString is specified, start a game of that type. If a GameString is specified, load it as the current game.");
                        ConsoleOut("  See https://github.com/jonthysell/Mzinga/wiki/UniversalHiveProtocol#newgame.");
                        break;
                    case "play":
                        ConsoleOut("  play MoveString");
                        ConsoleOut("");
                        ConsoleOut("  Play the specified MoveString in the current game.");
                        ConsoleOut("  See https://github.com/jonthysell/Mzinga/wiki/UniversalHiveProtocol#play.");
                        break;
                    case "pass":
                        ConsoleOut("  pass");
                        ConsoleOut("");
                        ConsoleOut("  Play a passing move in the current game.");
                        ConsoleOut("  See https://github.com/jonthysell/Mzinga/wiki/UniversalHiveProtocol#pass.");
                        break;
                    case "validmoves":
                        ConsoleOut("  validmoves");
                        ConsoleOut("");
                        ConsoleOut("  Display a list of every valid move in the current game.");
                        ConsoleOut("  See https://github.com/jonthysell/Mzinga/wiki/UniversalHiveProtocol#validmoves.");
                        break;
                    case "bestmove":
                        ConsoleOut("  bestmove time MaxTime");
                        ConsoleOut("  bestmove depth MaxTime");
                        ConsoleOut("");
                        ConsoleOut("  Search for the best move for the current game. Use 'time' to limit the search by time in hh:mm:ss or use 'depth' to limit the number of turns to look into the future.");
                        ConsoleOut("  See https://github.com/jonthysell/Mzinga/wiki/UniversalHiveProtocol#bestmove.");
                        break;
                    case "undo":
                        ConsoleOut("  undo [MovesToUndo]");
                        ConsoleOut("");
                        ConsoleOut("  Undo the last move in the current game. If MovesToUndo is specified, undo that many moves.");
                        ConsoleOut("  See https://github.com/jonthysell/Mzinga/wiki/UniversalHiveProtocol#undo.");
                        break;
                    case "options":
                        ConsoleOut("  options");
                        ConsoleOut("  options get OptionName");
                        ConsoleOut("  options set OptionName OptionValue");
                        ConsoleOut("");
                        ConsoleOut("  Display the available options for the engine. Use 'get' to get the specified OptionName or 'set' to set the specified OptionName to OptionValue.");
                        ConsoleOut("  See https://github.com/jonthysell/Mzinga/wiki/UniversalHiveProtocol#options.");
                        break;
                    case "licenses":
                        ConsoleOut("  licenses");
                        ConsoleOut("");
                        ConsoleOut("  Displays the engine licenses.");
                        break;
                    case "perft":
                        ConsoleOut("  perft [MaxDepth]");
                        ConsoleOut("");
                        ConsoleOut("  Calculates the perft result for each depth starting with the current game. If MaxDepth is specified, stop after that reaching that depth.");
                        ConsoleOut("  See https://github.com/jonthysell/Mzinga/wiki/Perft.");
                        break;
                    case "exit":
                        ConsoleOut("  exit");
                        ConsoleOut("");
                        ConsoleOut("  Exit the engine.");
                        break;
#if DEBUG
                    case "break":
                        ConsoleOut("  break");
                        ConsoleOut("");
                        ConsoleOut("  Break into the debugger if it's attached.");
                        break;
#endif
                    default:
                        throw new CommandException();
                }
            }
        }

        private void NewGame(GameType gameType)
        {
            StopPonder();

            _board = new Board(gameType);

            InitAI();

            ConsoleOut(_board.GetGameString());
        }

        private void NewGame(string gameString)
        {
            Board parsed = Board.ParseGameString(gameString, false);

            StopPonder();

            _board = parsed;

            InitAI();

            ConsoleOut(_board.GetGameString());
        }

        private void Play(string moveStr)
        {
            if (_board is null)
            {
                throw new NoBoardException();
            }

            if (_board.GameIsOver)
            {
                throw new GameIsOverException();
            }

            if (!_board.TryParseMove(moveStr, out Move move, out string moveString))
            {
                throw new InvalidMoveException(move, $"Unable to parse move \"{moveStr}\".");
            }

            _board.Play(move, moveString);

            StopPonder();

            ConsoleOut(_board.GetGameString());
        }

        private void Pass()
        {
            Play(Move.PassString);
        }

        private void ValidMoves()
        {
            if (_board is null)
            {
                throw new NoBoardException();
            }

            if (_board.GameIsOver)
            {
                throw new GameIsOverException();
            }

            var validMoves = _board.GetValidMoves();

            var sb = new StringBuilder();
            bool first = true;
            foreach (var validMove in validMoves)
            {
                string moveStr = _board.GetMoveString(validMove);
                sb.Append(first ? moveStr : $";{moveStr}");
                first = false;
            }

            ConsoleOut(sb.ToString());
        }

        private void BestMove()
        {
            if (_board is null)
            {
                throw new NoBoardException();
            }

            if (_board.GameIsOver)
            {
                throw new GameIsOverException();
            }

            StopPonder();

            CancellationToken token = OnStartAsyncCommand();

            if (_gameAI is not null)
            {
                Task<Move> task = _gameAI.GetBestMoveAsync(_board.Clone(), Config.MaxHelperThreads, token).AsTask();
                task.Wait();

                if (!_board.TryGetMoveString(task.Result, out string result))
                {
                    throw new Exception("Invalid best move returned!");
                }

                ConsoleOut(result);
            }

            OnEndAsyncCommand();
        }

        private void BestMove(int maxDepth)
        {
            if (_board is null)
            {
                throw new NoBoardException();
            }

            if (_board.GameIsOver)
            {
                throw new GameIsOverException();
            }

            StopPonder();

            CancellationToken token = OnStartAsyncCommand();

            if (_gameAI is not null)
            {
                Task<Move> task = _gameAI.GetBestMoveAsync(_board.Clone(), maxDepth, Config.MaxHelperThreads, token).AsTask();
                task.Wait();

                if (!_board.TryGetMoveString(task.Result, out string result))
                {
                    throw new Exception("Invalid best move returned!");
                }

                ConsoleOut(result);
            }

            OnEndAsyncCommand();
        }

        private void BestMove(TimeSpan maxTime)
        {
            if (_board is null)
            {
                throw new NoBoardException();
            }

            if (_board.GameIsOver)
            {
                throw new GameIsOverException();
            }

            StopPonder();

            CancellationToken token = OnStartAsyncCommand();

            if (maxTime < TimeSpan.MaxValue)
            {
                _asyncCommandCTS?.CancelAfter(maxTime);
            }

            if (_gameAI is not null)
            {
                Task<Move> task = _gameAI.GetBestMoveAsync(_board.Clone(), maxTime, Config.MaxHelperThreads, token).AsTask();
                task.Wait();

                if (!_board.TryGetMoveString(task.Result, out string result))
                {
                    throw new Exception("Invalid best move returned!");
                }

                ConsoleOut(result);
            }

            OnEndAsyncCommand();
        }

        private void Undo(int moves = 1)
        {
            if (_board is null)
            {
                throw new NoBoardException();
            }

            if (moves < 1 || moves > _board.CurrentTurn)
            {
                throw new UndoInvalidNumberOfMovesException(moves);
            }

            StopPonder();

            for (int i = 0; i < moves; i++)
            {
                _board.TryUndoLastMove();
            }

            ConsoleOut(_board.GetGameString());
        }

        private void OptionsList()
        {
            OptionsGet("MaxBranchingFactor");
            OptionsGet("MaxHelperThreads");
            OptionsGet("PonderDuringIdle");
            OptionsGet("TranspositionTableSizeMB");
            OptionsGet("ReportIntermediateBestMoves");
        }

        private void OptionsGet(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            key = key.Trim();

            string defaultValue;
            string value;
            string type;
            string values;

            switch (key)
            {
                case "MaxBranchingFactor":
                    DefaultConfig.GetMaxBranchingFactorValue(out _, out defaultValue, out _);
                    Config.GetMaxBranchingFactorValue(out type, out value, out values);
                    break;
                case "MaxHelperThreads":
                    DefaultConfig.GetMaxHelperThreadsValue(out _, out defaultValue, out _);
                    Config.GetMaxHelperThreadsValue(out type, out value, out values);
                    break;
                case "PonderDuringIdle":
                    DefaultConfig.GetPonderDuringIdleValue(out _, out defaultValue, out _);
                    Config.GetPonderDuringIdleValue(out type, out value, out values);
                    break;
                case "TranspositionTableSizeMB":
                    DefaultConfig.GetTranspositionTableSizeMBValue(out _, out defaultValue, out _);
                    Config.GetTranspositionTableSizeMBValue(out type, out value, out values);
                    break;
                case "ReportIntermediateBestMoves":
                    DefaultConfig.GetReportIntermediateBestMovesValue(out _, out defaultValue, out _);
                    Config.GetReportIntermediateBestMovesValue(out type, out value, out values);
                    break;
                default:
                    throw new ArgumentException(string.Format("The option \"{0}\" is not valid.", key));
            }

            if (string.IsNullOrWhiteSpace(values))
            {
                ConsoleOut(string.Format("{0};{1};{2};{3}", key, type, value, defaultValue));
            }
            else
            {
                ConsoleOut(string.Format("{0};{1};{2};{3};{4}", key, type, value, defaultValue, values));
            }
        }

        private void OptionsSet(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            key = key.Trim();

            bool refreshAI = false;
            bool resetCaches = false;

            switch (key)
            {
                case "MaxBranchingFactor":
                    Config.ParseMaxBranchingFactorValue(value);
                    refreshAI = true;
                    resetCaches = true;
                    break;
                case "MaxHelperThreads":
                    Config.ParseMaxHelperThreadsValue(value);
                    break;
                case "PonderDuringIdle":
                    Config.ParsePonderDuringIdleValue(value);
                    break;
                case "TranspositionTableSizeMB":
                    Config.ParseTranspositionTableSizeMBValue(value);
                    refreshAI = true;
                    resetCaches = true;
                    break;
                case "ReportIntermediateBestMoves":
                    Config.ParseReportIntermediateBestMovesValue(value);
                    refreshAI = true;
                    break;
                default:
                    throw new ArgumentException(string.Format("The option \"{0}\" is not valid.", key));
            }

            OptionsGet(key);

            if (refreshAI)
            {
                ResetAI(resetCaches);
            }
        }

        private void Licenses()
        {
            ConsoleOut(string.Format("# {0} #", AppInfo.HiveProduct));
            ConsoleOut("");
            ConsoleOut(string.Join(Environment.NewLine + Environment.NewLine, AppInfo.HiveCopyright, AppInfo.HiveLicense));

            ConsoleOut("");

            ConsoleOut(string.Format("# {0} #", AppInfo.Product));
            ConsoleOut("");
            ConsoleOut(string.Join(Environment.NewLine + Environment.NewLine, AppInfo.MitLicenseName, AppInfo.Copyright, AppInfo.MitLicenseBody));
        }

        private void Perft(int maxDepth = Int32.MaxValue)
        {
            if (_board is null)
            {
                throw new NoBoardException();
            }

            if (maxDepth < 0)
            {
                throw new PerftInvalidDepthException(maxDepth);
            }

            StopPonder();

            CancellationToken token = OnStartAsyncCommand();

            for (int depth = 0; depth <= maxDepth; depth++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                Task<long?> task = _board.CalculatePerftAsync(depth, token).AsTask();
                task.Wait();
                sw.Stop();

                if (!task.Result.HasValue)
                {
                    break;
                }

                ConsoleOut("{0,-9} = {1,16:#,##0} in {2,16:#,##0} ms. {3,8:#,##0.0} KN/s", string.Format("perft({0})", depth), task.Result.Value, sw.ElapsedMilliseconds, Math.Round(task.Result.Value / (double)sw.ElapsedMilliseconds, 1));
            }

            OnEndAsyncCommand();
        }

        private void StartPonder()
        {
            if (Config.PonderDuringIdle != PonderDuringIdleType.Disabled && !_isPondering && _board is not null && _board.GameInProgress && _gameAI is not null)
            {
                _ponderCTS = new CancellationTokenSource();
                _ponderTask = Task.Factory.StartNew(async () => await _gameAI.GetBestMoveAsync(_board.Clone(), Config.PonderDuringIdle == PonderDuringIdleType.MultiThreaded ? Config.MaxHelperThreads : 0, _ponderCTS.Token));

                _isPondering = true;
            }
        }

        private void StopPonder()
        {
            if (_isPondering)
            {
                _ponderCTS?.Cancel();
                _ponderTask?.Wait();

                _ponderCTS = null;
                _ponderTask = null;

                _isPondering = false;
            }
        }

        private void Exit()
        {
            StopPonder();

            _board = null;
            ExitRequested = true;
        }

#if DEBUG
        private static void Break()
        {
            if (!Debugger.IsAttached)
            {
                throw new Exception("Please attach a debugger before using the 'break' command.");
            }

            Debugger.Break();
        }
#endif

        private CancellationToken OnStartAsyncCommand()
        {
            _asyncCommandCTS = new CancellationTokenSource();

            StartAsyncCommand?.Invoke(this, EventArgs.Empty);

            return _asyncCommandCTS.Token;
        }

        private void OnEndAsyncCommand()
        {
            _asyncCommandCTS = null;

            EndAsyncCommand?.Invoke(this, EventArgs.Empty);
        }
    }

    public delegate void ConsoleOut(string format, params object[] arg);

    [Serializable]
    public class CommandException : Exception
    {
        public CommandException(string message) : base(message) { }
        public CommandException() : base("Invalid command. Try 'help' to see a list of valid commands.") { }
    }

    public class NoBoardException : CommandException
    {
        public NoBoardException() : base("No game in progress. Try 'newgame' to start a new game.") { }
    }

    public class GameIsOverException : CommandException
    {
        public GameIsOverException() : base("The game is over. Try 'newgame' to start a new game.") { }
    }

    public class UndoInvalidNumberOfMovesException : CommandException
    {
        public UndoInvalidNumberOfMovesException(int moves) : base(string.Format("Unable to undo {0} moves.", moves)) { }
    }

    public class PerftInvalidDepthException : CommandException
    {
        public PerftInvalidDepthException(int depth) : base(string.Format("Unable to calculate perft({0}).", depth)) { }
    }
}
