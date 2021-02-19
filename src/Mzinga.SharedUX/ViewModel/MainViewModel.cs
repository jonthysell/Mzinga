// 
// MainViewModel.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017, 2018, 2019, 2021 Jon Thysell <http://jonthysell.com>
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
using System.ComponentModel;
using System.Threading.Tasks;

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
                string title = AppVM.ProgramTitle;

                if (IsReviewMode)
                {
                    string fileName = AppVM.EngineWrapper.CurrentGameSettings?.GameRecording?.FileName;

                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        title = $"{fileName} - {title}";
                    }
                }

                return title;
            }
        }

        public bool IsIdle
        {
            get
            {
                return _isIdle;
            }
            set
            {
                _isIdle = value;
                RaisePropertyChanged(nameof(IsIdle));
                RaisePropertyChanged(nameof(IsBusy));

                NewGame.RaiseCanExecuteChanged();
                LoadGame.RaiseCanExecuteChanged();
                SaveGame.RaiseCanExecuteChanged();

                PlayTarget.RaiseCanExecuteChanged();
                Pass.RaiseCanExecuteChanged();
                UndoLastMove.RaiseCanExecuteChanged();

                MoveToStart.RaiseCanExecuteChanged();
                MoveBack.RaiseCanExecuteChanged();
                MoveForward.RaiseCanExecuteChanged();
                MoveToEnd.RaiseCanExecuteChanged();

                SwitchToPlayMode.RaiseCanExecuteChanged();
                ShowGameMetadata.RaiseCanExecuteChanged();
                SwitchToReviewMode.RaiseCanExecuteChanged();

                FindBestMove.RaiseCanExecuteChanged();
                ShowEngineOptions.RaiseCanExecuteChanged();
                ShowViewerConfig.RaiseCanExecuteChanged();

                CopyHistoryToClipboard.RaiseCanExecuteChanged();
                CheckForUpdatesAsync.RaiseCanExecuteChanged();
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
                RaisePropertyChanged(nameof(IsRunningTimedCommand));
                RaisePropertyChanged(nameof(IsRunningIndeterminateCommand));
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
                _timedCommandProgress = Math.Max(0.0, Math.Min(100, value * 100));
                RaisePropertyChanged(nameof(TimedCommandProgress));
            }
        }
        private double _timedCommandProgress = 0.0;

        public bool IsPlayMode
        {
            get
            {
                return null != AppVM.EngineWrapper.CurrentGameSettings && AppVM.EngineWrapper.CurrentGameSettings.GameMode == GameMode.Play;
            }
        }

        public bool IsReviewMode
        {
            get
            {
                return null != AppVM.EngineWrapper.CurrentGameSettings && AppVM.EngineWrapper.CurrentGameSettings.GameMode == GameMode.Review;
            }
        }

        public bool ShowMenu => AppInfo.IsWindows || AppInfo.IsLinux;

        public ViewerConfig ViewerConfig => AppVM.ViewerConfig;

        public GameBoard Board
        {
            get
            {
                return AppVM.EngineWrapper.Board;
            }
        }

        public GameBoard ReviewBoard
        {
            get
            {
                return AppVM.EngineWrapper.ReviewBoard;
            }
        }

        public ObservableBoardHistory BoardHistory
        {
            get
            {
                return _boardHistory;
            }
            private set
            {
                _boardHistory = value;
                RaisePropertyChanged(nameof(BoardHistory));
                CopyHistoryToClipboard.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(CurrentMoveCommentary));
            }
        }
        private ObservableBoardHistory _boardHistory = null;

        public RelayCommand CopyHistoryToClipboard
        {
            get
            {
                return _copyHistoryToClipboard ?? (_copyHistoryToClipboard = new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.TextToClipboard(BoardHistory.Text);
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return IsIdle && (null != BoardHistory) && BoardHistory.CurrentMoveIndex >= 0;
                }));
            }
        }
        private RelayCommand _copyHistoryToClipboard = null;

        public Action RequestClose;

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
                RaisePropertyChanged(nameof(CanvasHexRadius));
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
                RaisePropertyChanged(nameof(CanvasCursorX));
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
                RaisePropertyChanged(nameof(CanvasCursorY));
            }
        }
        private double _canvasCursorY;

        public bool CanRaiseStackedPieces
        {
            get
            {
                return _canRaiseStackedPieces;
            }
            internal set
            {
                _canRaiseStackedPieces = value;
                RaisePropertyChanged(nameof(CanRaiseStackedPieces));
            }
        }
        private bool _canRaiseStackedPieces = false;

        #endregion

        #region File

        public RelayCommand NewGame
        {
            get
            {
                return _newGame ?? (_newGame = new RelayCommand(() =>
                {
                    try
                    {
                        Messenger.Default.Send(new NewGameMessage(AppVM.EngineWrapper.CurrentGameSettings, true, (settings) =>
                        {
                            try
                            {
                                AppVM.EngineWrapper.NewGame(settings);
                                RaisePropertyChanged(nameof(Title));
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

        public RelayCommand LoadGame
        {
            get
            {
                return _loadGame ?? (_loadGame = new RelayCommand(() =>
                {
                    try
                    {
                        IsIdle = false;
                        Messenger.Default.Send(new LoadGameMessage((gameRecording) =>
                        {
                            try
                            {
                                if (null != gameRecording)
                                {
                                    AppVM.EngineWrapper.LoadGame(gameRecording);
                                    RaisePropertyChanged(nameof(Title));
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtils.HandleException(ex);
                            }
                            finally
                            {
                                IsIdle = true;
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
        private RelayCommand _loadGame = null;

        public RelayCommand SaveGame
        {
            get
            {
                return _saveGame ?? (_saveGame = new RelayCommand(() =>
                {
                    try
                    {
                        Messenger.Default.Send(new GameMetadataMessage(AppVM.EngineWrapper.CurrentGameSettings.Metadata, (metadata) =>
                        {
                            try
                            {
                                AppVM.EngineWrapper.CurrentGameSettings.Metadata.Clear();
                                AppVM.EngineWrapper.CurrentGameSettings.Metadata.CopyFrom(metadata);

                                Messenger.Default.Send(new SaveGameMessage(AppVM.EngineWrapper.CurrentGameSettings.GameRecording, (fileName) =>
                                {
                                    if (IsReviewMode)
                                    {
                                        AppVM.EngineWrapper.CurrentGameSettings.GameRecording.FileName = fileName;
                                        RaisePropertyChanged(nameof(Title));
                                    }
                                }));
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
                    return IsIdle && (AppVM.EngineWrapper.GameInProgress || AppVM.EngineWrapper.GameIsOver);
                }));
            }
        }
        private RelayCommand _saveGame = null;

        public RelayCommand Close
        {
            get
            {
                return _close ??= new RelayCommand(() =>
                {
                    try
                    {
                        RequestClose?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand _close;

        #endregion

        #region Play Mode

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
                    return IsIdle && AppVM.EngineWrapper.GameInProgress && (!ViewerConfig.BlockInvalidMoves || AppVM.EngineWrapper.CanPlayTargetMove) && IsPlayMode;
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
                    return IsIdle && AppVM.EngineWrapper.GameInProgress && (!ViewerConfig.BlockInvalidMoves || AppVM.EngineWrapper.CanPass) && IsPlayMode;
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

        #endregion

        #region Review Mode

        public string CurrentMoveCommentary
        {
            get
            {
                return AppVM.EngineWrapper.CurrentGameSettings?.Metadata.GetMoveCommentary(BoardHistory.CurrentMoveIndex + 1);
            }
            set
            {
                if (null != AppVM.EngineWrapper.CurrentGameSettings)
                {
                    AppVM.EngineWrapper.CurrentGameSettings?.Metadata.SetMoveCommentary(BoardHistory.CurrentMoveIndex + 1, value);
                }
            }
        }

        public RelayCommand MoveToStart
        {
            get
            {
                return _moveToStart ?? (_moveToStart = new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.MoveToStart();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return IsIdle && AppVM.EngineWrapper.CanMoveBack;
                }));
            }
        }
        private RelayCommand _moveToStart = null;

        public RelayCommand MoveBack
        {
            get
            {
                return _moveBack ?? (_moveBack = new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.MoveBack();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return IsIdle && AppVM.EngineWrapper.CanMoveBack;
                }));
            }
        }
        private RelayCommand _moveBack = null;

        public RelayCommand MoveForward
        {
            get
            {
                return _moveForward ?? (_moveForward = new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.MoveForward();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return IsIdle && AppVM.EngineWrapper.CanMoveForward;
                }));
            }
        }
        private RelayCommand _moveForward = null;

        public RelayCommand MoveToEnd
        {
            get
            {
                return _moveToEnd ?? (_moveToEnd = new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.MoveToEnd();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return IsIdle && AppVM.EngineWrapper.CanMoveForward;
                }));
            }
        }
        private RelayCommand _moveToEnd = null;

        public RelayCommand ShowGameMetadata
        {
            get
            {
                return _showGameMetadata ?? (_showGameMetadata = new RelayCommand(() =>
                {
                    try
                    {
                        Messenger.Default.Send(new GameMetadataMessage(AppVM.EngineWrapper.CurrentGameSettings.Metadata, (metadata) =>
                        {
                            try
                            {
                                AppVM.EngineWrapper.CurrentGameSettings.Metadata.Clear();
                                AppVM.EngineWrapper.CurrentGameSettings.Metadata.CopyFrom(metadata);
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
                    return IsIdle && IsReviewMode;
                }));
            }
        }
        private RelayCommand _showGameMetadata = null;

        public RelayCommand SwitchToPlayMode
        {
            get
            {
                return _switchToPlayMode ?? (_switchToPlayMode = new RelayCommand(() =>
                {
                    try
                    {
                        Messenger.Default.Send(new ConfirmationMessage("Switching to play mode starts a new game at the current position. Do you want to continue?", (confirmed) =>
                        {
                            try
                            {
                                if (confirmed)
                                {
                                    string activeGameString = Board.ToGameString();

                                    Messenger.Default.Send(new NewGameMessage(AppVM.EngineWrapper.CurrentGameSettings, false, (settings) =>
                                    {
                                        try
                                        {
                                            AppVM.EngineWrapper.NewGame(settings, activeGameString);
                                        }
                                        catch (Exception ex)
                                        {
                                            ExceptionUtils.HandleException(ex);
                                        }
                                    }));
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
                }, () =>
                {
                    return IsIdle && IsReviewMode && (AppVM.EngineWrapper.GameInProgress || AppVM.EngineWrapper.GameIsOver);
                }));
            }
        }
        private RelayCommand _switchToPlayMode = null;

        public RelayCommand SwitchToReviewMode
        {
            get
            {
                return _switchToReviewMode ?? (_switchToReviewMode = new RelayCommand(() =>
                {
                    try
                    {
                        if (AppVM.EngineWrapper.GameIsOver)
                        {
                            AppVM.EngineWrapper.SwitchToReviewMode();
                        }
                        else
                        {
                            Messenger.Default.Send(new ConfirmationMessage("Switching to review mode will end your game. Do you want to continue?", (confirmed) =>
                            {
                                try
                                {
                                    if (confirmed)
                                    {
                                        AppVM.EngineWrapper.SwitchToReviewMode();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ExceptionUtils.HandleException(ex);
                                }
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return IsIdle && IsPlayMode && (AppVM.EngineWrapper.GameInProgress || AppVM.EngineWrapper.GameIsOver);
                }));
            }
        }
        private RelayCommand _switchToReviewMode = null;

        #endregion

        #region Engine

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
                        AppVM.EngineWrapper.OptionsList(() =>
                        {
                            AppVM.DoOnUIThread(() =>
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
                            });
                        });
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

        #endregion

        #region Viewer

        public bool ShowBoardHistory
        {
            get
            {
                return ViewerConfig.ShowBoardHistory;
            }
            set
            {
                ViewerConfig.ShowBoardHistory = value;
                RaisePropertyChanged(nameof(ShowBoardHistory));
            }
        }

        public bool ShowMoveCommentary
        {
            get
            {
                return ViewerConfig.ShowMoveCommentary;
            }
            set
            {
                ViewerConfig.ShowMoveCommentary = value;
                RaisePropertyChanged(nameof(ShowMoveCommentary));
            }
        }

        public RelayCommand ToggleShowBoardHistory
        {
            get
            {
                return _toggleShowBoardHistory ?? (_toggleShowBoardHistory = new RelayCommand(() =>
                {
                    try
                    {
                        ShowBoardHistory = !ShowBoardHistory;
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }));
            }
        }
        private RelayCommand _toggleShowBoardHistory = null;

        public RelayCommand ToggleShowMoveCommentary
        {
            get
            {
                return _toggleShowMoveCommentary ?? (_toggleShowMoveCommentary = new RelayCommand(() =>
                {
                    try
                    {
                        ShowMoveCommentary = !ShowMoveCommentary;
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }));
            }
        }
        private RelayCommand _toggleShowMoveCommentary = null;

        public RelayCommand ShowViewerConfig
        {
            get
            {
                return _showViewerConfig ?? (_showViewerConfig = new RelayCommand(() =>
                {
                    try
                    {
                        Messenger.Default.Send(new ViewerConfigMessage(ViewerConfig, (config) =>
                        {
                            try
                            {
                                ViewerConfig.CopyFrom(config);

                                RaisePropertyChanged(nameof(ViewerConfig));
                                RaisePropertyChanged(nameof(TargetMove));

                                PlayTarget.RaiseCanExecuteChanged();
                                Pass.RaiseCanExecuteChanged();


                                UpdateBoardHistory();
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

        #endregion

        #region Help

        public RelayCommand ShowLicenses => AppVM.ShowLicenses;

        public RelayCommand LaunchHiveWebsite => AppVM.LaunchHiveWebsite;

        public RelayCommand LaunchMzingaWebsite => AppVM.LaunchMzingaWebsite;
        public RelayCommand CheckForUpdatesAsync => AppVM.CheckForUpdatesAsync;

        #endregion

        public MainViewModel()
        {
            AppVM.EngineWrapper.BoardUpdated += (sender, args) =>
            {
                AppVM.DoOnUIThread(() =>
                {
                    RaisePropertyChanged(nameof(Board));
                    SaveGame.RaiseCanExecuteChanged();

                    PlayTarget.RaiseCanExecuteChanged();
                    Pass.RaiseCanExecuteChanged();
                    UndoLastMove.RaiseCanExecuteChanged();

                    MoveToStart.RaiseCanExecuteChanged();
                    MoveBack.RaiseCanExecuteChanged();
                    MoveForward.RaiseCanExecuteChanged();
                    MoveToEnd.RaiseCanExecuteChanged();

                    FindBestMove.RaiseCanExecuteChanged();
                    RaisePropertyChanged(nameof(GameState));

                    if (AppVM.EngineWrapper.GameIsOver && AppVM.EngineWrapper.CurrentGameSettings.GameMode == GameMode.Play)
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

                    UpdateBoardHistory();
                });
            };

            AppVM.EngineWrapper.ValidMovesUpdated += (sender, args) =>
            {
                AppVM.DoOnUIThread(() =>
                {
                    RaisePropertyChanged(nameof(ValidMoves));
                });
            };

            AppVM.EngineWrapper.TargetPieceUpdated += (sender, args) =>
            {
                AppVM.DoOnUIThread(() =>
                {
                    RaisePropertyChanged(nameof(TargetMove));
                    PlayTarget.RaiseCanExecuteChanged();
                });
            };

            AppVM.EngineWrapper.TargetPositionUpdated += (sender, args) =>
            {
                AppVM.DoOnUIThread(() =>
                {
                    RaisePropertyChanged(nameof(TargetMove));
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

            AppVM.EngineWrapper.GameModeChanged += (sender, args) =>
            {
                RaisePropertyChanged(nameof(IsPlayMode));
                RaisePropertyChanged(nameof(IsReviewMode));
            };

            PropertyChanged += MainViewModel_PropertyChanged;
        }

        public void OnLoaded()
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    AppVM.DoOnUIThread(() =>
                    {
                        IsIdle = false;
                    });

                    if (ViewerConfig.FirstRun)
                    {
                        FirstRun();
                    }

                    if (ViewerConfig.CheckUpdateOnStart && UpdateUtils.IsConnectedToInternet)
                    {
                        await UpdateUtils.UpdateCheckAsync(true, false);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
                finally
                {
                    AppVM.DoOnUIThread(() =>
                    {
                        IsIdle = true;
                    });
                }
            });
        }

        private void FirstRun()
        {
            AppVM.DoOnUIThread(() =>
            {
                // Turn off first-run so it doesn't run next time
                ViewerConfig.FirstRun = false;

                Messenger.Default.Send(new ConfirmationMessage(string.Join(Environment.NewLine + Environment.NewLine, $"Welcome to {AppInfo.Name}!", $"Would you like to check for updates when {AppInfo.Name} starts?", "You can change your mind later in Viewer Options."), (enableAutoUpdate) =>
                {
                    try
                    {
                        ViewerConfig.CheckUpdateOnStart = enableAutoUpdate;
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }));
            });
        }

        private void MainViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IsPlayMode):
                case nameof(IsReviewMode):
                    AppVM.DoOnUIThread(() =>
                    {
                        RaisePropertyChanged(nameof(Title));

                        PlayTarget.RaiseCanExecuteChanged();
                        Pass.RaiseCanExecuteChanged();
                        UndoLastMove.RaiseCanExecuteChanged();

                        MoveToStart.RaiseCanExecuteChanged();
                        MoveBack.RaiseCanExecuteChanged();
                        MoveForward.RaiseCanExecuteChanged();
                        MoveToEnd.RaiseCanExecuteChanged();

                        SwitchToPlayMode.RaiseCanExecuteChanged();
                        ShowGameMetadata.RaiseCanExecuteChanged();
                        SwitchToReviewMode.RaiseCanExecuteChanged();

                        UpdateBoardHistory();
                    });
                    break;
                case nameof(ViewerConfig):
                    AppVM.DoOnUIThread(() =>
                    {
                        RaisePropertyChanged(nameof(ShowBoardHistory));
                        RaisePropertyChanged(nameof(ShowMoveCommentary));
                    });
                    break;
            }
        }

        private void UpdateBoardHistory()
        {
            ObservableBoardHistory boardHistory = null;

            if (null != Board)
            {
                if (IsPlayMode)
                {
                    boardHistory = new ObservableBoardHistory(Board.BoardHistory);
                }
                else if (IsReviewMode)
                {
                    boardHistory = new ObservableBoardHistory(ReviewBoard.BoardHistory, Board.BoardHistory, (moveNum) =>
                    {
                        try
                        {
                            AppVM.EngineWrapper.MoveToMoveNumber(moveNum);
                        }
                        catch (Exception ex)
                        {
                            ExceptionUtils.HandleException(ex);
                        }
                    });
                }
            }

            BoardHistory = boardHistory;
        }

        internal void CanvasClick(double cursorX, double cursorY)
        {
            if (AppVM.EngineWrapper.CurrentTurnIsHuman)
            {
                CanvasCursorX = cursorX;
                CanvasCursorY = cursorY;

                PieceName clickedPiece = AppVM.EngineWrapper.GetPieceAt(CanvasCursorX, CanvasCursorY, CanvasHexRadius, ViewerConfig.HexOrientation);
                Position clickedPosition = AppVM.EngineWrapper.GetTargetPositionAt(CanvasCursorX, CanvasCursorY, CanvasHexRadius, ViewerConfig.HexOrientation);

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
                        if (!ViewerConfig.BlockInvalidMoves || AppVM.EngineWrapper.CanPlayMove(targetMove))
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
    }
}
