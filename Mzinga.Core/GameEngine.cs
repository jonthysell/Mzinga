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
using System.Text;

namespace Mzinga.Core
{
    public delegate void ConsoleOut(string format, params object[] arg);

    public class GameEngine
    {
        public string ID { get; private set; }
        public ConsoleOut ConsoleOut { get; private set; }

        private GameBoard GameBoard;

        private Random Random;

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
            Random = new Random();
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
                            Play();
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
                        Undo();
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

            GameBoard.BoardChanged += () =>
            {
                _cachedBestMove = null;
            };

            ConsoleOut(GameBoard.ToString());
        }

        private void Play()
        {
            if (null == GameBoard)
            {
                throw new NoBoardException();
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

            GameBoard.Play(new Move(moveString));
            ConsoleOut(GameBoard.ToString());
        }

        private void Pass()
        {
            if (null == GameBoard)
            {
                throw new NoBoardException();
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

            MoveSet validMoves = GameBoard.GetValidMoves(EnumUtils.ParseShortName(pieceName));
            ConsoleOut(validMoves.ToString());
        }

        private void BestMove()
        {
            if (null == GameBoard)
            {
                throw new NoBoardException();
            }

            Move bestMove = GetBestMove();

            if (null != bestMove)
            {
                ConsoleOut(bestMove.ToString());
            }
        }

        private void Undo()
        {
            if (null == GameBoard)
            {
                throw new NoBoardException();
            }

            GameBoard.UndoLastMove();
            ConsoleOut(GameBoard.ToString());
        }

        private void History()
        {
            if (null == GameBoard)
            {
                throw new NoBoardException();
            }

            StringBuilder sb = new StringBuilder();

            foreach (Move move in GameBoard.MoveHistory)
            {
                sb.AppendFormat("{0}{1}", move.ToString(), MoveSet.MoveStringSeparator);
            }

            string history = sb.ToString().TrimEnd(MoveSet.MoveStringSeparator);

            if (String.IsNullOrWhiteSpace(history))
            {
                throw new Exception("You must move before you can see any move history.");
            }

            ConsoleOut(history);
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
                MoveSet validMoves = GameBoard.GetValidMoves();

                int randIndex = Random.Next(validMoves.Count);

                Move bestMove = null;
                Move randomMove = null;

                int index = 0;
                foreach (Move move in validMoves)
                {
                    if (index == randIndex)
                    {
                        randomMove = move;
                    }

                    BoardState resultState = GameBoard.TryMove(move);
                    if ((GameBoard.CurrentTurnColor == Color.White && resultState == BoardState.WhiteWins) ||
                        (GameBoard.CurrentTurnColor == Color.Black && resultState == BoardState.BlackWins))
                    {
                        bestMove = move;
                        break;
                    }

                    index++;
                }

                _cachedBestMove = bestMove != null ? bestMove : randomMove;
            }

            return _cachedBestMove;
        }
    }

    public class NoBoardException : Exception
    {
        public NoBoardException() : base("You must start a game before you can do that.") { }
    }
}
