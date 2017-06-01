// 
// MainViewModel.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017 Jon Thysell <http://jonthysell.com>
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

using Mzinga.Core;
using Mzinga.Viewer.Resources;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Mzinga.Viewer.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public AppViewModel AppVM
        {
            get
            {
                return AppViewModel.Instance;
            }
        }

        public string Title
        {
            get
            {
                return AppVM.ProgramTitle;
            }
        }

        public bool IsIdle
        {
            get
            {
                return _isIdle;
            }
            protected set
            {
                _isIdle = value;
                RaisePropertyChanged("IsIdle");
                RaisePropertyChanged("NewGame");
                RaisePropertyChanged("PlayTarget");
                RaisePropertyChanged("Pass");
                RaisePropertyChanged("UndoLastMove");
                RaisePropertyChanged("FindBestMove");
                RaisePropertyChanged("CheckForUpdatesAsync");
            }
        }
        private bool _isIdle;

        public Board Board
        {
            get
            {
                return AppVM.EngineWrapper.Board;
            }
        }

        public string BoardHistory
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (null != AppVM.EngineWrapper.BoardHistory)
                {
                    int count = 1;
                    bool isWhite = true;
                    foreach (BoardHistoryItem item in AppVM.EngineWrapper.BoardHistory)
                    {
                        string countString = count.ToString() + ". ";
                        if (isWhite)
                        {
                            sb.AppendFormat(Strings.BoardHistoryItemFormat, countString, item.ToString(BoardHistoryItemStringFormat.ShortAlgebraic));
                        }
                        else
                        {
                            string spacing = "";

                            for (int i = 0; i < countString.Length; i++)
                            {
                                spacing += " ";
                            }

                            sb.AppendFormat(Strings.BoardHistoryItemFormat, spacing, item.ToString(BoardHistoryItemStringFormat.ShortAlgebraic));
                            count++;
                        }

                        sb.AppendLine();
                        isWhite = !isWhite;
                    }
                }

                return sb.ToString();
            }
        }

        #region Status properties

        public string GameState
        {
            get
            {
                string state = Strings.GameStateNoGame;

                if (null != Board)
                {
                    switch (Board.BoardState)
                    {
                        case BoardState.Draw:
                            state = Strings.GameStateDraw;
                            break;
                        case BoardState.WhiteWins:
                            state = Strings.GameStateWhiteWon;
                            break;
                        case BoardState.BlackWins:
                            state = Strings.GameStateBlackWon;
                            break;
                        default:
                            state = (Board.CurrentTurnColor == Color.White) ? Strings.GameStateWhitesTurn : Strings.GameStateBlacksTurn;
                            break;
                    }
                }

                return state;
            }
        }

        public string ValidMoves
        {
            get
            {
                string moves = "";
                if (null != AppVM.EngineWrapper.ValidMoves)
                {
                    moves = AppVM.EngineWrapper.ValidMoves.Count.ToString();
                }

                return moves;
            }
        }

        public string TargetMove
        {
            get
            {
                string move = "";
                if (null != AppVM.EngineWrapper.TargetMove)
                {
                    move = AppVM.EngineWrapper.TargetMove.ToString();
                }
                else if (AppVM.EngineWrapper.TargetPiece != PieceName.INVALID)
                {
                    move = EnumUtils.GetShortName(AppVM.EngineWrapper.TargetPiece);
                }

                return move;
            }
        }

        #endregion

        #region Canvas properties

        public double CanvasHexRadius
        {
            get
            {
                return _canvasHexRadius;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _canvasHexRadius = value;
                RaisePropertyChanged("CanvasHexRadius");
            }
        }
        private double _canvasHexRadius = 20;

        public double CanvasCursorX
        {
            get
            {
                return _canvasCursorX;
            }
            private set
            {
                _canvasCursorX = value;
                RaisePropertyChanged("CanvasCursorX");
            }
        }
        private double _canvasCursorX;

        public double CanvasCursorY
        {
            get
            {
                return _canvasCursorY;
            }
            private set
            {
                _canvasCursorY = value;
                RaisePropertyChanged("CanvasCursorY");
            }
        }
        private double _canvasCursorY;

        #endregion

        public RelayCommand NewGame
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        Messenger.Default.Send(new NewGameMessage(AppVM.EngineWrapper.CurrentGameSettings, (settings) =>
                            {
                                try
                                {
                                    AppVM.EngineWrapper.NewGame(settings);
                                }
                                catch (Exception ex)
                                {
                                    ExceptionUtils.HandleException(ex);
                                }
                            }));
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, ()=>
                {
                    return IsIdle;
                });
            }
        }

        public RelayCommand PlayTarget
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.PlayTargetMove();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return IsIdle && AppVM.EngineWrapper.CanPlayTargetMove;
                });
            }
        }

        public RelayCommand Pass
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.Pass();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return IsIdle && AppVM.EngineWrapper.CanPass;
                });
            }
        }

        public RelayCommand UndoLastMove
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.UndoLastMove();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return IsIdle && AppVM.EngineWrapper.CanUndoLastMove;
                });
            }
        }

        public RelayCommand FindBestMove
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.FindBestMove();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return IsIdle && AppVM.EngineWrapper.CanFindBestMove;
                });
            }
        }

        public RelayCommand ShowEngineConsole
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        Messenger.Default.Send(new EngineConsoleMessage());
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }

        public RelayCommand CheckForUpdatesAsync
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    try
                    {
                        IsIdle = false;
                        await UpdateUtils.UpdateCheckAsync(true, true);
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                    finally
                    {
                        IsIdle = true;
                    }
                }, () =>
                {
                    return IsIdle && !UpdateUtils.IsCheckingforUpdate;
                });
            }
        }

        public MainViewModel()
        {
            AppVM.EngineWrapper.BoardUpdated += OnBoardUpdated;

            AppVM.EngineWrapper.BoardHistoryUpdated += (boardHistory) =>
            {
                RaisePropertyChanged("BoardHistory");
            };

            AppVM.EngineWrapper.TargetPieceUpdated += (pieceName) =>
            {
                RaisePropertyChanged("TargetMove");
                RaisePropertyChanged("PlayTarget");
            };

            AppVM.EngineWrapper.TargetPositionUpdated += (position) =>
            {
                RaisePropertyChanged("TargetMove");
                RaisePropertyChanged("PlayTarget");
            };

            AppVM.EngineWrapper.IsIdleUpdated += (isIdle) =>
            {
                IsIdle = isIdle;
            };

            IsIdle = true;
        }

        private void OnBoardUpdated(Board board)
        {
            RaisePropertyChanged("Board");
            RaisePropertyChanged("Pass");
            RaisePropertyChanged("PlayBestMove");
            RaisePropertyChanged("FindBestMove");
            RaisePropertyChanged("UndoLastMove");
            RaisePropertyChanged("GameState");
            RaisePropertyChanged("ValidMoves");

            AppVM.DoOnUIThread(() =>
            {
                switch (board.BoardState)
                {
                    case BoardState.WhiteWins:
                        Messenger.Default.Send(new InformationMessage(Strings.GameStateWhiteWon, Strings.GameOverTitle));
                        break;
                    case BoardState.BlackWins:
                        Messenger.Default.Send(new InformationMessage(Strings.GameStateBlackWon, Strings.GameOverTitle));
                        break;
                    case BoardState.Draw:
                        Messenger.Default.Send(new InformationMessage(Strings.GameStateDraw, Strings.GameOverTitle));
                        break;
                }
            });
        }

        internal void CanvasClick(double cursorX, double cursorY)
        {
            if (AppVM.EngineWrapper.CurrentTurnIsHuman)
            {
                CanvasCursorX = cursorX;
                CanvasCursorY = cursorY;

                PieceName clickedPiece = AppVM.EngineWrapper.GetPieceAt(CanvasCursorX, CanvasCursorY, CanvasHexRadius);
                Position clickedPosition = AppVM.EngineWrapper.GetTargetPositionAt(CanvasCursorX, CanvasCursorY, CanvasHexRadius);

                if (AppVM.EngineWrapper.TargetPiece == PieceName.INVALID && clickedPiece != PieceName.INVALID)
                {
                    // No piece seleected, select it
                    AppVM.EngineWrapper.TargetPiece = clickedPiece;
                }
                else if (AppVM.EngineWrapper.TargetPiece != PieceName.INVALID)
                {
                    // Piece is selected
                    if (clickedPiece == AppVM.EngineWrapper.TargetPiece)
                    {
                        // Unselect piece
                        AppVM.EngineWrapper.TargetPiece = PieceName.INVALID;
                    }
                    else
                    {
                        // Get the move with the clicked position
                        Move targetMove = new Move(AppVM.EngineWrapper.TargetPiece, clickedPosition);
                        if (AppVM.EngineWrapper.CanPlayMove(targetMove))
                        {
                            // Move is valid, select position
                            AppVM.EngineWrapper.TargetPosition = clickedPosition;
                        }
                        else
                        {
                            // Move is invalid, (un)select clicked piece
                            AppVM.EngineWrapper.TargetPiece = clickedPiece;
                        }
                    }
                }
            }
        }

        internal void PieceClick(PieceName clickedPiece)
        {
            if (AppVM.EngineWrapper.CurrentTurnIsHuman)
            {
                if (AppVM.EngineWrapper.TargetPiece == clickedPiece)
                {
                    clickedPiece = PieceName.INVALID;
                }

                AppVM.EngineWrapper.TargetPiece = clickedPiece;
            }
        }
    }
}
