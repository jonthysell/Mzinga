// 
// GameEngine.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016 Jon Thysell <http://jonthysell.com>
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
using System.Text;

using Mzinga.Core.AI;

namespace Mzinga.Core
{
    public delegate void ConsoleOut(string format, params object[] arg);

    public class GameEngine
    {
        public string ID { get; private set; }
        public ConsoleOut ConsoleOut { get; private set; }

        private GameBoard GameBoard;

        private IGameAI _gameAI;

        private Move _cachedBestMove;

        public bool ExitRequested { get; private set; }

        public GameEngine(string id, ConsoleOut consoleOut)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("id");
            }

            if (null == consoleOut)
            {
                throw new ArgumentNullException("consoleOut");
            }

            ID = id;
            ConsoleOut = consoleOut;

            InitGameAI();

            ExitRequested = false;
        }

        public void ParseCommand(string command)
        {
            if (String.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentNullException("command");
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
                        Help();
                        break;
                    case "board":
                        PrintBoard();
                        break;
                    case "newgame":
                        NewGame();
                        break;
                    case "play":
                        if (paramCount == 0)
                        {
                            PlayBestMove();
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
                        BestMove();
                        break;
                    case "undo":
                        if (paramCount == 0)
                        {
                            Undo();
                        }
                        else
                        {
                            Undo(Int32.Parse(split[1]));
                        }
                        
                        break;
                    case "history":
                        History();
                        break;
                    case "exit":
                        Exit();
                        break;
                    default:
                        throw new Exception("Unknown command.");
                }
            }
            catch (InvalidMoveException ex)
            {
                ConsoleOut("invalidmove {0}", ex.Message);
            }
            catch (Exception ex)
            {
                ConsoleOut("err {0}", ex.Message.Replace("\r\n", " "));
            }
            ConsoleOut("ok");
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
            ConsoleOut("exit");
        }

        private void PrintBoard()
        {
            if (null == GameBoard)
            {
                throw new NoBoardException();
            }

            ConsoleOut(GameBoard.ToString());
        }

        private void NewGame()
        {
            GameBoard = new GameBoard();
            _cachedBestMove = null;

            GameBoard.BoardChanged += () =>
            {
                _cachedBestMove = null;
            };

            _gameAI.ClearTranspositionTables();

            ConsoleOut(GameBoard.ToString());
        }

        private void PlayBestMove()
        {
            if (null == GameBoard)
            {
                throw new NoBoardException();
            }

            if (GameBoard.GameIsOver)
            {
                throw new GameIsOverException();
            }

            Move bestMove = GetBestMove();

            GameBoard.Play(bestMove);
            ConsoleOut(GameBoard.ToString());
        }

        private void Play(string moveString)
        {
            if (null == GameBoard)
            {
                throw new NoBoardException();
            }

            if (GameBoard.GameIsOver)
            {
                throw new GameIsOverException();
            }

            GameBoard.Play(new Move(moveString));
            ConsoleOut(GameBoard.ToString());
        }

        private void Pass()
        {
            if (null == GameBoard)
            {
                throw new NoBoardException();
            }

            if (GameBoard.GameIsOver)
            {
                throw new GameIsOverException();
            }

            GameBoard.Pass();
            ConsoleOut(GameBoard.ToString());
        }

        private void ValidMoves()
        {
            if (null == GameBoard)
            {
                throw new NoBoardException();
            }

            if (GameBoard.GameIsOver)
            {
                throw new GameIsOverException();
            }

            MoveSet validMoves = GameBoard.GetValidMoves();
            ConsoleOut(validMoves.ToString());
        }

        private void ValidMoves(string pieceName)
        {
            if (null == GameBoard)
            {
                throw new NoBoardException();
            }

            if (String.IsNullOrWhiteSpace(pieceName))
            {
                throw new ArgumentNullException(pieceName);
            }

            if (GameBoard.GameIsOver)
            {
                throw new GameIsOverException();
            }

            MoveSet validMoves = GameBoard.GetValidMoves(EnumUtils.ParseShortName(pieceName));
            ConsoleOut(validMoves.ToString());
        }

        private void BestMove()
        {
            if (null == GameBoard)
            {
                throw new NoBoardException();
            }

            if (GameBoard.GameIsOver)
            {
                throw new GameIsOverException();
            }

            Move bestMove = GetBestMove();

            if (null != bestMove)
            {
                ConsoleOut(bestMove.ToString());
            }
        }

        private void Undo(int moves = 1)
        {
            if (null == GameBoard)
            {
                throw new NoBoardException();
            }

            if (moves < 1)
            {
                throw new UndoTooFewMoves();
            }
            else if (moves > GameBoard.BoardHistoryCount)
            {
                throw new UndoTooManyMoves();
            }

            for (int i = 0; i < moves; i++)
            {
                GameBoard.UndoLastMove();
            }
            ConsoleOut(GameBoard.ToString());
        }

        private void History()
        {
            if (null == GameBoard)
            {
                throw new NoBoardException();
            }

            BoardHistory history = new BoardHistory(GameBoard.BoardHistory);
            ConsoleOut(history.ToString());
        }

        private void Exit()
        {
            ExitRequested = true;
        }

        private Move GetBestMove()
        {
            if (null == GameBoard)
            {
                throw new NoBoardException();
            }

            if (null == _cachedBestMove)
            {
                _cachedBestMove = _gameAI.GetBestMove(GameBoard);
            }

            return _cachedBestMove;
        }

        private void InitGameAI()
        {
            GameAI ai = new GameAI();

            ai.MaxDepth = GameAI.IterativeDepth;
            ai.MaxTime = TimeSpan.FromSeconds(1.0);

            ai.AlphaBetaPruning = true;
            ai.TranspositionTable = true;

            MetricWeights mw = ai.MetricWeights;

            // Basic Queen Management
            mw.Set(Player.Maximizing, BugType.QueenBee, BugTypeWeight.NeighborWeight, -5.0); // Your Queen has company!
            mw.Set(Player.Maximizing, BugType.QueenBee, BugTypeWeight.IsPinnedWeight, -42.0); // Your Queen is pinned!

            mw.Set(Player.Minimizing, BugType.QueenBee, BugTypeWeight.NeighborWeight, 7.0); // Surround the enemy Queen!
            mw.Set(Player.Minimizing, BugType.QueenBee, BugTypeWeight.IsPinnedWeight, 23.0); // The enemy Queen is pinned!

            // Give edge to opening up more moves
            mw.Set(Player.Maximizing, PlayerWeight.ValidMoveWeight, 0.001);
            mw.Set(Player.Minimizing, PlayerWeight.ValidMoveWeight, -0.01);

            _gameAI = ai;
        }
    }

    public class NoBoardException : Exception
    {
        public NoBoardException() : base("You must start a game before you can do that.") { }
    }

    public class GameIsOverException : Exception
    {
        public GameIsOverException() : base("You can't do that, the game is over.") { }
    }

    public class UndoTooFewMoves : Exception
    {
        public UndoTooFewMoves() : base("You must undo at least one move.") { }
    }

    public class UndoTooManyMoves : Exception
    {
        public UndoTooManyMoves() : base("You cannot undo that many moves.") { }
    }
}
