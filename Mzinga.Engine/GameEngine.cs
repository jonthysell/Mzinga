// 
// GameEngine.cs
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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Mzinga.Core;
using Mzinga.Core.AI;

namespace Mzinga.Engine
{
    public class GameEngine
    {
        public string ID { get; private set; }
        public ConsoleOut ConsoleOut { get; private set; }
        public GameEngineConfig Config { get; private set; }

        public bool ExitRequested { get; private set; }

        public event EventHandler StartAsyncCommand;
        public event EventHandler EndAsyncCommand;

        private GameBoard _gameBoard;

        private GameAI _gameAI;

        private CancellationTokenSource _asyncCommandCTS = null;

        private Task _ponderTask = null;
        private CancellationTokenSource _ponderCTS = null;
        private volatile bool _isPondering = false;

        public GameEngine(string id, GameEngineConfig config, ConsoleOut consoleOut)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }

            if (null == config)
            {
                throw new ArgumentNullException("config");
            }

            if (null == consoleOut)
            {
                throw new ArgumentNullException("consoleOut");
            }

            ID = id;
            Config = config;
            ConsoleOut = consoleOut;

            _gameAI = Config.GetGameAI();
            _gameAI.BestMoveFound += OnBestMoveFound;

            ExitRequested = false;
        }

        private void OnBestMoveFound(object sender, BestMoveFoundEventArgs args)
        {
            if (null == args || null == args.Move)
            {
                ConsoleOut("");
            }
            else
            {
                ConsoleOut("{0};{1};{2:0.00}", args.Move, args.Depth, args.Score);
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
                throw new ArgumentNullException("command");
            }

            string[] split = command.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            try
            {
                string cmd = split[0].ToLower();

                int paramCount = split.Length - 1;
#if !DEBUG
                StopPonder();
#endif

                switch (cmd)
                {
                    case "info":
                        Info();
                        break;
                    case "?":
                    case "help":
                        Help();
                        break;
                    case "board":
                        PrintBoard();
                        break;
                    case "newgame":
                        if (paramCount == 0)
                        {
                            NewGame(ExpansionPieces.None);
                        }
                        else
                        {
                            NewGame(EnumUtils.ParseExpansionPieces(split[1]));
                        }
                        break;
                    case "play":
                        if (paramCount < 1)
                        {
                            throw new CommandException();
                        }
                        else
                        {
                            Play(split[1]);
                        }
                        break;
                    case "pass":
                        Pass();
                        break;
                    case "validmoves":
                        if (paramCount == 0)
                        {
                            ValidMoves();
                        }
                        else
                        {
                            ValidMoves(split[1]);
                        }
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
                    case "history":
                        History();
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
                        break;
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
#if !DEBUG
            StartPonder();
#endif
        }

        private void ErrorOut(Exception ex)
        {
            ConsoleOut("err {0}", ex.Message.Replace("\r\n", " "));
#if DEBUG
            ConsoleOut(ex.StackTrace.Replace("\r\n", " "));
#endif

            if (null != ex.InnerException)
            {
                ErrorOut(ex.InnerException);
            }
        }

        private void Info()
        {
            ConsoleOut("id {0}", ID);
        }

        private void Help()
        {
            ConsoleOut("Available commands: ");
            ConsoleOut("info");
            ConsoleOut("help");
            ConsoleOut("board");
            ConsoleOut("newgame");
            ConsoleOut("play");
            ConsoleOut("pass");
            ConsoleOut("validmoves");
            ConsoleOut("bestmove");
            ConsoleOut("undo");
            ConsoleOut("history");
            ConsoleOut("perft");
            ConsoleOut("exit");
        }

        private void PrintBoard()
        {
            if (null == _gameBoard)
            {
                throw new NoBoardException();
            }

            ConsoleOut(_gameBoard.ToString());
        }

        private void NewGame(ExpansionPieces expansionPieces)
        {
            _gameBoard = new GameBoard(expansionPieces);

            _gameAI.ResetCaches();

            ConsoleOut(_gameBoard.ToString());
        }

        private void Play(string moveString)
        {
            if (null == _gameBoard)
            {
                throw new NoBoardException();
            }

            if (_gameBoard.GameIsOver)
            {
                throw new GameIsOverException();
            }

            _gameBoard.Play(new Move(moveString));
            ConsoleOut(_gameBoard.ToString());
        }

        private void Pass()
        {
            if (null == _gameBoard)
            {
                throw new NoBoardException();
            }

            if (_gameBoard.GameIsOver)
            {
                throw new GameIsOverException();
            }

            _gameBoard.Pass();
            ConsoleOut(_gameBoard.ToString());
        }

        private void ValidMoves()
        {
            if (null == _gameBoard)
            {
                throw new NoBoardException();
            }

            if (_gameBoard.GameIsOver)
            {
                throw new GameIsOverException();
            }

            MoveSet validMoves = _gameBoard.GetValidMoves();
            ConsoleOut(validMoves.ToString());
        }

        private void ValidMoves(string pieceName)
        {
            if (null == _gameBoard)
            {
                throw new NoBoardException();
            }

            if (string.IsNullOrWhiteSpace(pieceName))
            {
                throw new ArgumentNullException(pieceName);
            }

            if (_gameBoard.GameIsOver)
            {
                throw new GameIsOverException();
            }

            MoveSet validMoves = _gameBoard.GetValidMoves(EnumUtils.ParseShortName(pieceName));
            ConsoleOut(validMoves.ToString());
        }

        private void BestMove()
        {
            if (null == _gameBoard)
            {
                throw new NoBoardException();
            }

            if (_gameBoard.GameIsOver)
            {
                throw new GameIsOverException();
            }

            CancellationToken token = OnStartAsyncCommand();

            Task<Move> task = _gameAI.GetBestMoveAsync(_gameBoard, Config.MaxHelperThreads, token);
            task.Wait();

            OnEndAsyncCommand();
        }

        private void BestMove(int maxDepth)
        {
            if (null == _gameBoard)
            {
                throw new NoBoardException();
            }

            if (_gameBoard.GameIsOver)
            {
                throw new GameIsOverException();
            }

            CancellationToken token = OnStartAsyncCommand();

            Task<Move> task = _gameAI.GetBestMoveAsync(_gameBoard, maxDepth, Config.MaxHelperThreads, token);
            task.Wait();

            OnEndAsyncCommand();
        }

        private void BestMove(TimeSpan maxTime)
        {
            if (null == _gameBoard)
            {
                throw new NoBoardException();
            }

            if (_gameBoard.GameIsOver)
            {
                throw new GameIsOverException();
            }

            CancellationToken token = OnStartAsyncCommand();

            if (maxTime < TimeSpan.MaxValue)
            {
                _asyncCommandCTS.CancelAfter(maxTime);
            }

            Task<Move> task = _gameAI.GetBestMoveAsync(_gameBoard, maxTime, Config.MaxHelperThreads, token);
            task.Wait();

            OnEndAsyncCommand();
        }

        private void Undo(int moves = 1)
        {
            if (null == _gameBoard)
            {
                throw new NoBoardException();
            }

            if (moves < 1 || moves > _gameBoard.BoardHistoryCount)
            {
                throw new UndoInvalidNumberOfMovesException(moves);
            }

            for (int i = 0; i < moves; i++)
            {
                _gameBoard.UndoLastMove();
            }
            ConsoleOut(_gameBoard.ToString());
        }

        private void History()
        {
            if (null == _gameBoard)
            {
                throw new NoBoardException();
            }

            BoardHistory history = new BoardHistory(_gameBoard.BoardHistory);
            ConsoleOut(history.ToString());
        }

        private void Perft(int maxDepth = Int32.MaxValue)
        {
            if (null == _gameBoard)
            {
                throw new NoBoardException();
            }

            if (maxDepth < 0)
            {
                throw new PerftInvalidDepthException(maxDepth);
            }

            for (int depth = 0; depth <= maxDepth; depth++)
            {
                Stopwatch sw = Stopwatch.StartNew();
                long nodes = _gameBoard.ParallelPerft(depth, Environment.ProcessorCount);
                sw.Stop();

                ConsoleOut("{0,-9} = {1,16:#,##0} in {2,16:#,##0} ms. {3,8:#,##0.0} KN/s", string.Format("perft({0})", depth), nodes, sw.ElapsedMilliseconds, Math.Round(nodes / (double)sw.ElapsedMilliseconds, 1));
            }
        }

        private void StartPonder()
        {
            if (Config.PonderDuringIdle != PonderDuringIdleType.Disabled && !_isPondering && null != _gameBoard && _gameBoard.GameInProgress)
            {
                _gameAI.BestMoveFound -= OnBestMoveFound;

                _ponderCTS = new CancellationTokenSource();
                _ponderTask = Task.Factory.StartNew(async () => await _gameAI.GetBestMoveAsync(_gameBoard.Clone(), Config.PonderDuringIdle == PonderDuringIdleType.MultiThreaded ? Config.MaxHelperThreads : 0, _ponderCTS.Token));

                _isPondering = true;
            }
        }

        private void StopPonder()
        {
            if (_isPondering)
            {
                _ponderCTS.Cancel();
                _ponderTask.Wait();

                _ponderCTS = null;
                _ponderTask = null;

                _gameAI.BestMoveFound += OnBestMoveFound;
                _isPondering = false;
            }
        }

        private void Exit()
        {
            ExitRequested = true;
        }

        private CancellationToken OnStartAsyncCommand()
        {
            _asyncCommandCTS = new CancellationTokenSource();

            StartAsyncCommand?.Invoke(this, null);

            return _asyncCommandCTS.Token;
        }

        private void OnEndAsyncCommand()
        {
            _asyncCommandCTS = null;

            EndAsyncCommand?.Invoke(this, null);
        }
    }

    public delegate void ConsoleOut(string format, params object[] arg);

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
