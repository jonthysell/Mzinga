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

            GameBoard.BoardChanged += () =>
            {
                _cachedBestMove = null;
            };

            ConsoleOut(GameBoard.ToString());
        }

        private void PlayBestMove()
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
                MoveSet validMoves = GameBoard.GetValidMoves();

                EvaluatedMoveCollection evaluatedMoves = new EvaluatedMoveCollection();
                BoardMetrics metricsBeforeMove = GameBoard.GetBoardMetrics();
                double scoreBeforeMove = CalculateBoardScore(metricsBeforeMove);

                foreach (Move move in validMoves)
                {
                    EvaluatedMove evaluatedMove = Evaluate(move, metricsBeforeMove, scoreBeforeMove);
                    evaluatedMoves.Add(evaluatedMove);
                }

                List<EvaluatedMove> bestMoves = new List<EvaluatedMove>(evaluatedMoves.GetBestMoves());

                int randIndex = Random.Next(bestMoves.Count);
                _cachedBestMove = bestMoves[randIndex].Move;
            }

            return _cachedBestMove;
        }

        private EvaluatedMove Evaluate(Move move, BoardMetrics metricsBeforeMove, double scoreBeforeMove)
        {
            if (null == move)
            {
                throw new ArgumentNullException("move");
            }

            if (null == metricsBeforeMove)
            {
                throw new ArgumentNullException("metricsBeforeMove");
            }

            BoardMetrics metricsAfterMove = GameBoard.TryMove(move);
            double scoreAfterMove = CalculateBoardScore(metricsAfterMove);

            return new EvaluatedMove(move, scoreBeforeMove, scoreAfterMove);
        }

        private double CalculateBoardScore(BoardMetrics boardMetrics)
        {
            if (null == boardMetrics)
            {
                throw new ArgumentNullException("boardMetrics");
            }

            Color currentTurnColor = GameBoard.CurrentTurnColor;
            Color opponentTurnColor = (Color)(1 - (int)currentTurnColor);

            PieceName currentQueen = currentTurnColor == Color.White ? PieceName.WhiteQueenBee : PieceName.BlackQueenBee;
            PieceName opponentQueen = opponentTurnColor == Color.White ? PieceName.WhiteQueenBee : PieceName.BlackQueenBee;

            BoardState boardState = boardMetrics.BoardState;

            if ((currentTurnColor == Color.White && boardState == BoardState.WhiteWins) ||
                (currentTurnColor == Color.Black && boardState == BoardState.BlackWins))
            {
                return Double.MaxValue;
            }
            else if ((currentTurnColor == Color.White && boardState == BoardState.BlackWins) ||
                     (currentTurnColor == Color.Black && boardState == BoardState.WhiteWins))
            {
                return Double.MinValue;
            }
            else if (boardState == BoardState.Draw)
            {
                return 0;
            }

            double score = 0;

            int currentQueenNeighbors = boardMetrics.TurnMetrics[currentTurnColor].PieceMetrics[currentQueen].NeighborCount;
            int opponentQueenNeighbors = boardMetrics.TurnMetrics[opponentTurnColor].PieceMetrics[opponentQueen].NeighborCount;

            score += 10.0 * opponentQueenNeighbors; // Attack the enemy Queen!
            score -= 5.0 * currentQueenNeighbors; // Protect your Queen!

            return score;
        }
    }

    public class NoBoardException : Exception
    {
        public NoBoardException() : base("You must start a game before you can do that.") { }
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
