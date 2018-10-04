// 
// MainViewModel.cs
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
using System.Text;

using Mzinga.Core;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Mzinga.SharedUX.ViewModel
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
                RaisePropertyChanged("IsBusy");
                NewGame.RaiseCanExecuteChanged();
                PlayTarget.RaiseCanExecuteChanged();
                Pass.RaiseCanExecuteChanged();
                UndoLastMove.RaiseCanExecuteChanged();
                FindBestMove.RaiseCanExecuteChanged();
                ShowViewerConfig.RaiseCanExecuteChanged();
#if WINDOWS_WPF
                CheckForUpdatesAsync.RaiseCanExecuteChanged();
#endif
            }
        }
        private bool _isIdle = true;

        public bool IsBusy
        {
            get
            {
                return !IsIdle;
            }
        }

        public bool IsRunningTimedCommand
        {
            get
            {
                return _isRunningTimeCommand;
            }
            private set
            {
                _isRunningTimeCommand = value;
                RaisePropertyChanged("IsRunningTimedCommand");
                RaisePropertyChanged("IsRunningIndeterminateCommand");
            }
        }
        private bool _isRunningTimeCommand = false;

        public bool IsRunningIndeterminateCommand
        {
            get
            {
                return !IsRunningTimedCommand;
            }
        }

        public double TimedCommandProgress
        {
            get
            {
                return _timedCommandProgress;
            }
            private set
            {
                _timedCommandProgress = Math.Max(0.0, Math.Min(1.0, value));
                RaisePropertyChanged("TimedCommandProgress");
            }
        }
        private double _timedCommandProgress = 0.0;

        public ViewerConfig ViewerConfig
        {
            get
            {
                return AppVM.ViewerConfig;
            }
        }

        public ViewerBoard Board
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
                    foreach (Tuple<ViewerBoard, BoardHistoryItem> item in AppVM.EngineWrapper.BoardHistory.EnumerateWithBoard(Board))
                    {
                        string countString = count.ToString() + ". ";
                        if (isWhite)
                        {
                            sb.AppendFormat("{0}{1}", countString, ViewerConfig.NotationType == NotationType.BoardSpace ? NotationUtils.ToBoardSpaceMoveString(item.Item1, item.Item2.Move) : item.Item2.Move.ToString());
                        }
                        else
                        {
                            string spacing = "";

                            for (int i = 0; i < countString.Length; i++)
                            {
                                spacing += " ";
                            }

                            sb.AppendFormat("{0}{1}", spacing, ViewerConfig.NotationType == NotationType.BoardSpace ? NotationUtils.ToBoardSpaceMoveString(item.Item1, item.Item2.Move) : item.Item2.Move.ToString());
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
                string state = "A game has not been started.";

                if (null != Board)
                {
                    switch (Board.BoardState)
                    {
                        case BoardState.Draw:
                            state = "The game is a draw.";
                            break;
                        case BoardState.WhiteWins:
                            state = "White has won the game.";
                            break;
                        case BoardState.BlackWins:
                            state = "Black has won the game.";
                            break;
                        default:
                            state = (Board.CurrentTurnColor == PlayerColor.White) ? "It's white's turn." : "It's black's turn.";
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
                    move = ViewerConfig.NotationType == NotationType.BoardSpace ? NotationUtils.ToBoardSpaceMoveString(Board, AppVM.EngineWrapper.TargetMove) : AppVM.EngineWrapper.TargetMove.ToString();
                }
                else if (AppVM.EngineWrapper.TargetPiece != PieceName.INVALID)
                {
                    move = ViewerConfig.NotationType == NotationType.BoardSpace ? NotationUtils.ToBoardSpacePieceName(AppVM.EngineWrapper.TargetPiece) : EnumUtils.GetShortName(AppVM.EngineWrapper.TargetPiece);
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
                return _newGame ?? (_newGame = new RelayCommand(() =>
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
                }));
            }
        }
        private RelayCommand _newGame = null;

        public RelayCommand PlayTarget
        {
            get
            {
                return _playTarget ?? (_playTarget = new RelayCommand(() =>
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
                    return IsIdle && AppVM.EngineWrapper.GameInProgress && (!AppVM.ViewerConfig.BlockInvalidMoves || AppVM.EngineWrapper.CanPlayTargetMove);
                }));
            }
        }
        private RelayCommand _playTarget = null;

        public RelayCommand Pass
        {
            get
            {
                return _pass ?? (_pass = new RelayCommand(() =>
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
                    return IsIdle && AppVM.EngineWrapper.GameInProgress && (!AppVM.ViewerConfig.BlockInvalidMoves || AppVM.EngineWrapper.CanPass);
                }));
            }
        }
        private RelayCommand _pass = null;

        public RelayCommand UndoLastMove
        {
            get
            {
                return _undoLastMove ?? (_undoLastMove = new RelayCommand(() =>
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
                }));
            }
        }
        private RelayCommand _undoLastMove = null;

        public RelayCommand FindBestMove
        {
            get
            {
                return _findBestMove ?? (_findBestMove = new RelayCommand(() =>
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
                }));
            }
        }
        private RelayCommand _findBestMove = null;

        public RelayCommand ShowEngineConsole
        {
            get
            {
                return _showEngineConsole ?? (_showEngineConsole = new RelayCommand(() =>
                {
                    try
                    {
                        Messenger.Default.Send(new EngineConsoleMessage());
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }));
            }
        }
        private RelayCommand _showEngineConsole = null;

        public RelayCommand ShowEngineOptions
        {
            get
            {
                return _showEngineOptions ?? (_showEngineOptions = new RelayCommand(() =>
                {
                    try
                    {
                        Messenger.Default.Send(new EngineOptionsMessage(AppVM.EngineWrapper.EngineOptions, (changedOptions) =>
                        {
                            try
                            {
                                AppVM.EngineWrapper.OptionsSet(changedOptions);
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
                }, () =>
                {
                    return IsIdle;
                }));
            }
        }
        private RelayCommand _showEngineOptions = null;

        public RelayCommand ShowViewerConfig
        {
            get
            {
                return _showViewerConfig ?? (_showViewerConfig = new RelayCommand(() =>
                {
                    try
                    {
                        Messenger.Default.Send(new ViewerConfigMessage(AppVM.ViewerConfig, (config) =>
                        {
                            try
                            {
                                AppVM.ViewerConfig.CopyFrom(config);
                                RaisePropertyChanged("ViewerConfig");
                                RaisePropertyChanged("BoardHistory");
                                RaisePropertyChanged("TargetMove");
                                PlayTarget.RaiseCanExecuteChanged();
                                Pass.RaiseCanExecuteChanged();
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
                }, () =>
                {
                    return IsIdle;
                }));
            }
        }
        private RelayCommand _showViewerConfig = null;

        public RelayCommand ShowAbout
        {
            get
            {
                return _showAbout ?? (_showAbout = new RelayCommand(() =>
                {
                    try
                    {
                        StringBuilder sb = new StringBuilder();

                        sb.AppendLine(Title);
                        sb.AppendLine();

                        sb.AppendLine("Mzinga Copyright (c) 2015-2018 Jon Thysell");
                        sb.AppendLine("MVVM Light Toolkit Copyright (c) 2009-2018 Laurent Bugnion");

                        sb.AppendLine();

                        sb.AppendLine(string.Join(Environment.NewLine + Environment.NewLine, _license));

                        sb.AppendLine();

                        sb.Append("Hive Copyright (c) 2016 Gen42 Games. Mzinga is in no way associated with or endorsed by Gen42 Games. To learn more about Hive, see https://gen42.com/games/hive.");

                        Messenger.Default.Send(new InformationMessage(sb.ToString(), "About Mzinga"));
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }));
            }
        }
        private RelayCommand _showAbout = null;

        public RelayCommand LaunchHiveWebsite
        {
            get
            {
                return _launchHiveWebsite ?? (_launchHiveWebsite = new RelayCommand(() =>
                {
                    try
                    {
                        Messenger.Default.Send(new ConfirmationMessage("This will open the official Hive website in your browser. Do you want to continue?", (confirmed) =>
                        {
                            try
                            {
                                if (confirmed)
                                {
                                    Messenger.Default.Send(new LaunchUrlMessage("https://gen42.com/games/hive"));
                                }
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
                }));
            }
        }
        private RelayCommand _launchHiveWebsite;

#if WINDOWS_WPF
        public RelayCommand CheckForUpdatesAsync
        {
            get
            {
                return _checkForUpdatesAsync ?? (_checkForUpdatesAsync = new RelayCommand(async () =>
                {
                    try
                    {
                        IsIdle = false;
                        await Viewer.UpdateUtils.UpdateCheckAsync(true, true);
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
                    return IsIdle && !Viewer.UpdateUtils.IsCheckingforUpdate;
                }));
            }
        }
        private RelayCommand _checkForUpdatesAsync = null;
#endif

        public MainViewModel()
        {
            AppVM.EngineWrapper.BoardUpdated += (sender, args) =>
            {
                AppVM.DoOnUIThread(() =>
                {
                    RaisePropertyChanged("Board");
                    PlayTarget.RaiseCanExecuteChanged();
                    Pass.RaiseCanExecuteChanged();
                    FindBestMove.RaiseCanExecuteChanged();
                    UndoLastMove.RaiseCanExecuteChanged();
                    RaisePropertyChanged("GameState");

                    if (AppVM.EngineWrapper.GameIsOver)
                    {
                        if (ViewerConfig.PlaySoundEffects)
                        {
                            SoundUtils.PlaySound(GameSound.GameOver);
                        }

                        switch (Board.BoardState)
                        {
                            case BoardState.WhiteWins:
                                Messenger.Default.Send(new InformationMessage("White has won the game.", "Game Over"));
                                break;
                            case BoardState.BlackWins:
                                Messenger.Default.Send(new InformationMessage("Black has won the game.", "Game Over"));
                                break;
                            case BoardState.Draw:
                                Messenger.Default.Send(new InformationMessage("The game is a draw.", "Game Over"));
                                break;
                        }
                    }
                });
            };

            AppVM.EngineWrapper.ValidMovesUpdated += (sender, args) =>
            {
                AppVM.DoOnUIThread(() =>
                {
                    RaisePropertyChanged("ValidMoves");
                });
            };

            AppVM.EngineWrapper.BoardHistoryUpdated += (sender, args) =>
            {
                AppVM.DoOnUIThread(() =>
                {
                    RaisePropertyChanged("BoardHistory");
                });
            };

            AppVM.EngineWrapper.TargetPieceUpdated += (sender, args) =>
            {
                AppVM.DoOnUIThread(() =>
                {
                    RaisePropertyChanged("TargetMove");
                    PlayTarget.RaiseCanExecuteChanged();
                });
            };

            AppVM.EngineWrapper.TargetPositionUpdated += (sender, args) =>
            {
                AppVM.DoOnUIThread(() =>
                {
                    RaisePropertyChanged("TargetMove");
                    PlayTarget.RaiseCanExecuteChanged();

                    if (!ViewerConfig.RequireMoveConfirmation)
                    {
                        if (PlayTarget.CanExecute(null) && null != AppVM.EngineWrapper.TargetMove)
                        {
                            // Only fast-play if a move is selected
                            PlayTarget.Execute(null);
                        }
                        else if (Pass.CanExecute(null) && AppVM.EngineWrapper.CanPass)
                        {
                            // Only fast-pass if pass is available
                            Pass.Execute(null);
                        }
                    }
                });
            };

            AppVM.EngineWrapper.IsIdleUpdated += (sender, args) =>
            {
                AppVM.DoOnUIThread(() =>
                {
                    IsIdle = AppVM.EngineWrapper.IsIdle;
                });
            };

            AppVM.EngineWrapper.TimedCommandProgressUpdated += (sender, args) =>
            {
                AppVM.DoOnUIThread(() =>
                {
                    IsRunningTimedCommand = args.IsRunning;
                    TimedCommandProgress = args.Progress;
                });
            };

            AppVM.EngineWrapper.MovePlaying += (sender, args) =>
            {
                if (ViewerConfig.PlaySoundEffects)
                {
                    SoundUtils.PlaySound(GameSound.Move);
                }
            };

            AppVM.EngineWrapper.MoveUndoing += (sender, args) =>
            {
                if (ViewerConfig.PlaySoundEffects)
                {
                    SoundUtils.PlaySound(GameSound.Undo);
                }
            };
        }

        internal void CanvasClick(double cursorX, double cursorY)
        {
            if (AppVM.EngineWrapper.CurrentTurnIsHuman)
            {
                CanvasCursorX = cursorX;
                CanvasCursorY = cursorY;

                PieceName clickedPiece = AppVM.EngineWrapper.GetPieceAt(CanvasCursorX, CanvasCursorY, CanvasHexRadius, AppVM.ViewerConfig.HexOrientation);
                Position clickedPosition = AppVM.EngineWrapper.GetTargetPositionAt(CanvasCursorX, CanvasCursorY, CanvasHexRadius, AppVM.ViewerConfig.HexOrientation);

                // Make sure the first move is on the origin, no matter what
                if (Board.BoardState == BoardState.NotStarted && AppVM.EngineWrapper.TargetPiece != PieceName.INVALID)
                {
                    if (AppVM.EngineWrapper.TargetPosition == Position.Origin)
                    {
                        AppVM.EngineWrapper.TargetPiece = PieceName.INVALID;
                    }
                    else
                    {
                        clickedPosition = Position.Origin;
                    }
                }

                if (AppVM.EngineWrapper.TargetPiece == PieceName.INVALID && clickedPiece != PieceName.INVALID)
                {
                    // No piece selected, select it
                    AppVM.EngineWrapper.TargetPiece = clickedPiece;
                }
                else if (AppVM.EngineWrapper.TargetPiece != PieceName.INVALID)
                {
                    // Piece is selected
                    if (clickedPiece == AppVM.EngineWrapper.TargetPiece || clickedPosition == AppVM.EngineWrapper.TargetPosition)
                    {
                        // Unselect piece
                        AppVM.EngineWrapper.TargetPiece = PieceName.INVALID;
                    }
                    else
                    {
                        // Get the move with the clicked position
                        Move targetMove = new Move(AppVM.EngineWrapper.TargetPiece, clickedPosition);
                        if (!AppVM.ViewerConfig.BlockInvalidMoves || AppVM.EngineWrapper.CanPlayMove(targetMove))
                        {
                            // Move is selectable, select position
                            AppVM.EngineWrapper.TargetPosition = clickedPosition;
                        }
                        else
                        {
                            // Move is not selectable, (un)select clicked piece
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

        internal void CancelClick()
        {
            if (AppVM.EngineWrapper.CurrentTurnIsHuman)
            {
                AppVM.EngineWrapper.TargetPiece = PieceName.INVALID;
            }
        }

        private static string[] _license = {
            @"Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the ""Software""), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:",
            @"The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.",
            @"THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE."
        };
    }
}
