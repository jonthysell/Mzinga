// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Mzinga.Core;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Mzinga.Viewer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public static AppViewModel AppVM
        {
            get
            {
                return AppViewModel.Instance;
            }
        }

        public static string Title
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

        public static bool IsPlayMode
        {
            get
            {
                return AppVM.EngineWrapper.CurrentGameSettings is null || AppVM.EngineWrapper.CurrentGameSettings.GameMode == GameMode.Play;
            }
        }

        public static bool IsReviewMode
        {
            get
            {
                return AppVM.EngineWrapper.CurrentGameSettings is not null && AppVM.EngineWrapper.CurrentGameSettings.GameMode == GameMode.Review;
            }
        }

        public static bool ShowMenu => AppInfo.IsWindows || AppInfo.IsLinux;

        public static ViewerConfig ViewerConfig => AppVM.ViewerConfig;

        public static Board Board
        {
            get
            {
                return AppVM.EngineWrapper.Board;
            }
        }

        public static bool BoardIsLoaded
        {
            get
            {
                return Board is not null;
            }
        }

        public static Board ReviewBoard
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
                return _copyHistoryToClipboard ??= new RelayCommand(() =>
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
                    return IsIdle && (BoardHistory is not null) && BoardHistory.CurrentMoveIndex >= 0;
                });
            }
        }
        private RelayCommand _copyHistoryToClipboard = null;

        public Action RequestClose;

        #region Status properties

        public static string GameState
        {
            get
            {
                string state = "A game has not been started.";

                if (Board is not null)
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
                            state = (Board.CurrentColor == PlayerColor.White) ? "It's white's turn." : "It's black's turn.";
                            break;
                    }
                }

                return state;
            }
        }

        public static string ValidMoves
        {
            get
            {
                string moves = "";
                if (AppVM.EngineWrapper.ValidMoves is not null)
                {
                    moves = AppVM.EngineWrapper.ValidMoves.Count.ToString();
                }

                return moves;
            }
        }

        public static string TargetMove
        {
            get
            {
                var targetMove = AppVM.EngineWrapper.TargetMove;
                if (targetMove.HasValue && Board.TryGetMoveString(targetMove.Value, out string move))
                {
                    return move;
                }

                var targetPiece = AppVM.EngineWrapper.TargetPiece;
                if (targetPiece != PieceName.INVALID)
                {
                    return targetPiece.ToString();
                }

                return "";
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
                    throw new ArgumentOutOfRangeException(nameof(value));
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

        public bool CanCenterBoard
        {
            get
            {
                return (AppVM.EngineWrapper.GameInProgress || AppVM.EngineWrapper.GameIsOver) && !ViewerConfig.AutoCenterBoard;
            }
        }

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

        public bool CanZoomBoard
        {
            get
            {
                return (AppVM.EngineWrapper.GameInProgress || AppVM.EngineWrapper.GameIsOver) && !ViewerConfig.AutoZoomBoard;
            }
        }

        #endregion

        #region File

        public RelayCommand NewGame
        {
            get
            {
                return _newGame ??= new RelayCommand(() =>
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
                });
            }
        }
        private RelayCommand _newGame = null;

        public RelayCommand LoadGame
        {
            get
            {
                return _loadGame ??= new RelayCommand(() =>
                {
                    try
                    {
                        IsIdle = false;
                        Messenger.Default.Send(new LoadGameMessage((gameRecording) =>
                        {
                            try
                            {
                                if (gameRecording is not null)
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
                });
            }
        }
        private RelayCommand _loadGame = null;

        public RelayCommand SaveGame
        {
            get
            {
                return _saveGame ??= new RelayCommand(() =>
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
                });
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
                return _playTarget ??= new RelayCommand(() =>
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
                    return IsIdle && AppVM.EngineWrapper.GameInProgress && ViewerConfig.RequireMoveConfirmation && (!ViewerConfig.BlockInvalidMoves || AppVM.EngineWrapper.CanPlayTargetMove) && IsPlayMode;
                });
            }
        }
        private RelayCommand _playTarget = null;

        public RelayCommand Pass
        {
            get
            {
                return _pass ??= new RelayCommand(() =>
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
                    return IsIdle && AppVM.EngineWrapper.GameInProgress && ViewerConfig.RequireMoveConfirmation && (!ViewerConfig.BlockInvalidMoves || AppVM.EngineWrapper.CanPass) && IsPlayMode;
                });
            }
        }
        private RelayCommand _pass = null;

        public RelayCommand UndoLastMove
        {
            get
            {
                return _undoLastMove ??= new RelayCommand(() =>
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
                if (AppVM.EngineWrapper.CurrentGameSettings is not null)
                {
                    AppVM.EngineWrapper.CurrentGameSettings?.Metadata.SetMoveCommentary(BoardHistory.CurrentMoveIndex + 1, value);
                }
            }
        }

        public RelayCommand MoveToStart
        {
            get
            {
                return _moveToStart ??= new RelayCommand(() =>
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
                });
            }
        }
        private RelayCommand _moveToStart = null;

        public RelayCommand MoveBack
        {
            get
            {
                return _moveBack ??= new RelayCommand(() =>
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
                });
            }
        }
        private RelayCommand _moveBack = null;

        public RelayCommand MoveForward
        {
            get
            {
                return _moveForward ??= new RelayCommand(() =>
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
                });
            }
        }
        private RelayCommand _moveForward = null;

        public RelayCommand MoveToEnd
        {
            get
            {
                return _moveToEnd ??= new RelayCommand(() =>
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
                });
            }
        }
        private RelayCommand _moveToEnd = null;

        public RelayCommand ShowGameMetadata
        {
            get
            {
                return _showGameMetadata ??= new RelayCommand(() =>
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
                });
            }
        }
        private RelayCommand _showGameMetadata = null;

        public RelayCommand SwitchToPlayMode
        {
            get
            {
                return _switchToPlayMode ??= new RelayCommand(() =>
                {
                    try
                    {
                        Messenger.Default.Send(new ConfirmationMessage("Switching to play mode starts a new game at the current position. Do you want to continue?", (confirmed) =>
                        {
                            try
                            {
                                if (confirmed)
                                {
                                    string activeGameString = Board.GetGameString();

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
                });
            }
        }
        private RelayCommand _switchToPlayMode = null;

        public RelayCommand SwitchToReviewMode
        {
            get
            {
                return _switchToReviewMode ??= new RelayCommand(() =>
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
                });
            }
        }
        private RelayCommand _switchToReviewMode = null;

        #endregion

        #region Engine

        public string EngineId => AppVM.EngineWrapper.ID;

        public RelayCommand FindBestMove
        {
            get
            {
                return _findBestMove ??= new RelayCommand(() =>
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
        private RelayCommand _findBestMove = null;

        public RelayCommand ShowEngineConsole
        {
            get
            {
                return _showEngineConsole ??= new RelayCommand(() =>
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
        private RelayCommand _showEngineConsole = null;

        public RelayCommand ShowEngineOptions
        {
            get
            {
                return _showEngineOptions ??= new RelayCommand(() =>
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
                });
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

        public bool AutoCenterBoard
        {
            get
            {
                return ViewerConfig.AutoCenterBoard;
            }
            set
            {
                ViewerConfig.AutoCenterBoard = value;
                RaisePropertyChanged(nameof(AutoCenterBoard));
                RaisePropertyChanged(nameof(CanCenterBoard));
            }
        }

        public bool AutoZoomBoard
        {
            get
            {
                return ViewerConfig.AutoZoomBoard;
            }
            set
            {
                ViewerConfig.AutoZoomBoard = value;
                RaisePropertyChanged(nameof(AutoZoomBoard));
                RaisePropertyChanged(nameof(CanZoomBoard));
            }
        }

        public RelayCommand ToggleShowBoardHistory
        {
            get
            {
                return _toggleShowBoardHistory ??= new RelayCommand(() =>
                {
                    try
                    {
                        ShowBoardHistory = !ShowBoardHistory;
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand _toggleShowBoardHistory = null;

        public RelayCommand ToggleShowMoveCommentary
        {
            get
            {
                return _toggleShowMoveCommentary ??= new RelayCommand(() =>
                {
                    try
                    {
                        ShowMoveCommentary = !ShowMoveCommentary;
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand _toggleShowMoveCommentary = null;

        public RelayCommand ToggleAutoCenterBoard
        {
            get
            {
                return _toggleAutoCenterBoard ??= new RelayCommand(() =>
                {
                    try
                    {
                        AutoCenterBoard = !AutoCenterBoard;
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand _toggleAutoCenterBoard = null;

        public RelayCommand ToggleAutoZoomBoard
        {
            get
            {
                return _toggleAutoZoomBoard ??= new RelayCommand(() =>
                {
                    try
                    {
                        AutoZoomBoard = !AutoZoomBoard;
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand _toggleAutoZoomBoard = null;

        public RelayCommand ShowViewerConfig
        {
            get
            {
                return _showViewerConfig ??= new RelayCommand(() =>
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
                });
            }
        }
        private RelayCommand _showViewerConfig = null;

        #endregion

        #region Help

        public static RelayCommand ShowLicenses => AppVM.ShowLicenses;

        public static RelayCommand LaunchHiveWebsite => AppVM.LaunchHiveWebsite;

        public static RelayCommand LaunchMzingaWebsite => AppVM.LaunchMzingaWebsite;

        public static bool CheckForUpdatesEnabled => AppViewModel.CheckForUpdatesEnabled;

        public static RelayCommand CheckForUpdatesAsync => AppVM.CheckForUpdatesAsync;

        #endregion

        public MainViewModel()
        {
            AppVM.EngineWrapper.BoardUpdated += (sender, args) =>
            {
                AppVM.DoOnUIThread(() =>
                {
                    RaisePropertyChanged(nameof(Board));
                    RaisePropertyChanged(nameof(BoardIsLoaded));
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

                    RaisePropertyChanged(nameof(CanCenterBoard));
                    RaisePropertyChanged(nameof(CanZoomBoard));

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

            AppVM.EngineWrapper.TargetMoveUpdated += (sender, args) =>
            {
                AppVM.DoOnUIThread(() =>
                {
                    RaisePropertyChanged(nameof(TargetMove));
                    PlayTarget.RaiseCanExecuteChanged();

                    if (AppVM.EngineWrapper.CurrentTurnIsHuman && IsPlayMode && !ViewerConfig.RequireMoveConfirmation)
                    {
                        try
                        {
                            if (AppVM.EngineWrapper.TargetMove is not null)
                            {
                                // Only fast-play if a move is selected
                                AppVM.EngineWrapper.PlayTargetMove();
                            }
                            else if (AppVM.EngineWrapper.CanPass)
                            {
                                // Only fast-pass if pass is available
                                AppVM.EngineWrapper.Pass();
                            }
                        }
                        catch (Exception ex)
                        {
                            ExceptionUtils.HandleException(ex);
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
            Task.Run(async () =>
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

                    if (AppViewModel.CheckForUpdatesEnabled && ViewerConfig.CheckUpdateOnStart && UpdateUtils.IsConnectedToInternet)
                    {
                        //AppVM.DoOnUIThread(async () =>
                        //{
                            await UpdateUtils.UpdateCheckAsync(true, false);
                        //});
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

        private static void FirstRun()
        {
            AppVM.DoOnUIThread(() =>
            {
                // Turn off first-run so it doesn't run next time
                ViewerConfig.FirstRun = false;

                if (!AppViewModel.CheckForUpdatesEnabled)
                {
                    Messenger.Default.Send(new InformationMessage($"Welcome to {AppInfo.Name}!"));
                }
                else
                {
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
                }
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
                        RaisePropertyChanged(nameof(AutoCenterBoard));
                        RaisePropertyChanged(nameof(CanCenterBoard));
                        RaisePropertyChanged(nameof(AutoZoomBoard));
                        RaisePropertyChanged(nameof(CanZoomBoard));
                    });
                    break;
            }
        }

        private void UpdateBoardHistory()
        {
            if (Board is null)
            {
                BoardHistory = null;
            }
            else if (IsPlayMode)
            {
                // Replace the BoardHistory and move on
                if (BoardHistory is not null)
                {
                    BoardHistory.PropertyChanged -= BoardHistory_PropertyChanged;
                }
                BoardHistory = new ObservableBoardHistory(Board.BoardHistory);
            }
            else if (IsReviewMode)
            {
                if (BoardHistory?.BoardHistory == ReviewBoard.BoardHistory)
                {
                    BoardHistory.CurrentMoveIndex = Board.BoardHistory.Count - 1;
                }
                else
                {
                    // Replace the BoardHistory
                    BoardHistory = new ObservableBoardHistory(ReviewBoard.BoardHistory, Board.BoardHistory.Count - 1);
                    BoardHistory.PropertyChanged += BoardHistory_PropertyChanged;
                }
            }
        }

        private void BoardHistory_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ObservableBoardHistory.CurrentMoveIndex))
            {
                try
                {
                    AppVM.EngineWrapper.MoveToMoveNumber(BoardHistory.CurrentMoveIndex + 1);
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
            }
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
                    if (AppVM.EngineWrapper.TargetPosition == Position.OriginPosition)
                    {
                        AppVM.EngineWrapper.TargetPiece = PieceName.INVALID;
                    }
                    else
                    {
                        clickedPosition = Position.OriginPosition;
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
                        Move targetMove = new Move(AppVM.EngineWrapper.TargetPiece, Board.GetPosition(AppVM.EngineWrapper.TargetPiece), clickedPosition);
                        if (IsPlayMode && (!ViewerConfig.BlockInvalidMoves || AppVM.EngineWrapper.CanPlayMove(targetMove)))
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

        internal static bool TryPieceClick(PieceName clickedPiece)
        {
            if (AppVM.EngineWrapper.CurrentTurnIsHuman)
            {
                if (AppVM.EngineWrapper.TargetPiece == clickedPiece)
                {
                    clickedPiece = PieceName.INVALID;
                }

                AppVM.EngineWrapper.TargetPiece = clickedPiece;
                return true;
            }
            return false;
        }

        internal static bool TryCancelClick()
        {
            if (AppVM.EngineWrapper.CurrentTurnIsHuman)
            {
                AppVM.EngineWrapper.TargetPiece = PieceName.INVALID;
                return true;
            }

            return false;
        }
    }
}
