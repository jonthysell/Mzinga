﻿// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Mzinga.Viewer.ViewModels
{
    public class ViewerConfigViewModel : ViewModelBase
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
                return "Viewer Options";
            }
        }

        public EngineType EngineType
        {
            get
            {
                return Config.EngineType;
            }
            set
            {
                Config.EngineType = value;
                RaisePropertyChanged(nameof(EngineType));
                RaisePropertyChanged(nameof(EngineCommandLine));
            }
        }

        public string EngineCommandLine
        {
            get
            {
                return Config.EngineCommandLine;
            }
            set
            {
                Config.EngineCommandLine = value;
                RaisePropertyChanged(nameof(EngineCommandLine));
            }
        }

        public VisualTheme VisualTheme
        {
            get
            {
                return Config.VisualTheme;
            }
            set
            {
                Config.VisualTheme = value;
                RaisePropertyChanged(nameof(VisualTheme));
            }
        }

        public HexOrientation HexOrientation
        {
            get
            {
                return Config.HexOrientation;
            }
            set
            {
                Config.HexOrientation = value;
                RaisePropertyChanged(nameof(HexOrientation));
            }
        }

        public PieceStyle PieceStyle
        {
            get
            {
                return Config.PieceStyle;
            }
            set
            {
                Config.PieceStyle = value;
                RaisePropertyChanged(nameof(PieceStyle));
            }
        }

        public bool PieceColors
        {
            get
            {
                return Config.PieceColors;
            }
            set
            {
                Config.PieceColors = value;
                RaisePropertyChanged(nameof(PieceColors));
            }
        }

        public bool DisablePiecesInHandWithNoMoves
        {
            get
            {
                return Config.DisablePiecesInHandWithNoMoves;
            }
            set
            {
                Config.DisablePiecesInHandWithNoMoves = value;
                RaisePropertyChanged(nameof(DisablePiecesInHandWithNoMoves));
            }
        }

        public bool DisablePiecesInPlayWithNoMoves
        {
            get
            {
                return Config.DisablePiecesInPlayWithNoMoves;
            }
            set
            {
                Config.DisablePiecesInPlayWithNoMoves = value;
                RaisePropertyChanged(nameof(DisablePiecesInPlayWithNoMoves));
            }
        }

        public bool HighlightTargetMove
        {
            get
            {
                return Config.HighlightTargetMove;
            }
            set
            {
                Config.HighlightTargetMove = value;
                RaisePropertyChanged(nameof(HighlightTargetMove));
            }
        }

        public bool HighlightValidMoves
        {
            get
            {
                return Config.HighlightValidMoves;
            }
            set
            {
                Config.HighlightValidMoves = value;
                RaisePropertyChanged(nameof(HighlightValidMoves));
            }
        }

        public bool HighlightLastMovePlayed
        {
            get
            {
                return Config.HighlightLastMovePlayed;
            }
            set
            {
                Config.HighlightLastMovePlayed = value;
                RaisePropertyChanged(nameof(HighlightLastMovePlayed));
            }
        }

        public bool BlockInvalidMoves
        {
            get
            {
                return Config.BlockInvalidMoves;
            }
            set
            {
                Config.BlockInvalidMoves = value;
                RaisePropertyChanged(nameof(BlockInvalidMoves));
            }
        }

        public bool RequireMoveConfirmation
        {
            get
            {
                return Config.RequireMoveConfirmation;
            }
            set
            {
                Config.RequireMoveConfirmation = value;
                RaisePropertyChanged(nameof(RequireMoveConfirmation));
            }
        }

        public bool AddPieceNumbers
        {
            get
            {
                return Config.AddPieceNumbers;
            }
            set
            {
                Config.AddPieceNumbers = value;
                RaisePropertyChanged(nameof(AddPieceNumbers));
            }
        }

        public bool StackPiecesInHand
        {
            get
            {
                return Config.StackPiecesInHand;
            }
            set
            {
                Config.StackPiecesInHand = value;
                RaisePropertyChanged(nameof(StackPiecesInHand));
            }
        }

        public bool ShowBoardHistory
        {
            get
            {
                return Config.ShowBoardHistory;
            }
            set
            {
                Config.ShowBoardHistory = value;
                RaisePropertyChanged(nameof(ShowBoardHistory));
            }
        }

        public bool ShowMoveCommentary
        {
            get
            {
                return Config.ShowMoveCommentary;
            }
            set
            {
                Config.ShowMoveCommentary = value;
                RaisePropertyChanged(nameof(ShowMoveCommentary));
            }
        }

        public bool PlaySoundEffects
        {
            get
            {
                return Config.PlaySoundEffects;
            }
            set
            {
                Config.PlaySoundEffects = value;
                RaisePropertyChanged(nameof(PlaySoundEffects));
            }
        }

        public bool CheckUpdateOnStart
        {
            get
            {
                return Config.CheckUpdateOnStart;
            }
            set
            {
                Config.CheckUpdateOnStart = value;
                RaisePropertyChanged(nameof(CheckUpdateOnStart));
            }
        }

        public RelayCommand<string> ToggleRadioButton
        {
            get
            {
                return _toggleRadioButton ??= new RelayCommand<string>((parameter) =>
                {
                    try
                    {
                        string[] split = parameter.Split(".", StringSplitOptions.RemoveEmptyEntries);
                        switch (split[0])
                        {
                            case nameof(EngineType):
                                EngineType = (EngineType)Enum.Parse(typeof(EngineType), split[1]);
                                break;
                            case nameof(VisualTheme):
                                VisualTheme = (VisualTheme)Enum.Parse(typeof(VisualTheme), split[1]);
                                break;
                            case nameof(HexOrientation):
                                HexOrientation = (HexOrientation)Enum.Parse(typeof(HexOrientation), split[1]);
                                break;
                            case nameof(PieceStyle):
                                PieceStyle = (PieceStyle)Enum.Parse(typeof(PieceStyle), split[1]);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand<string> _toggleRadioButton = null;

        public RelayCommand Accept
        {
            get
            {
                return _accept ??= new RelayCommand(() =>
                {
                    try
                    {
                        Accepted = true;
                        RequestClose?.Invoke(this, null);
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand _accept = null;

        public RelayCommand Reject
        {
            get
            {
                return _reject ??= new RelayCommand(() =>
                {
                    try
                    {
                        Accepted = false;
                        RequestClose?.Invoke(this, null);
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand _reject = null;

        public RelayCommand Reset
        {
            get
            {
                return _reset ??= new RelayCommand(() =>
                {
                    try
                    {
                        ViewerConfig newConfig = new ViewerConfig()
                        {
                            FirstRun = Config.FirstRun,
                            InternalEngineConfig = Config.InternalEngineConfig
                        };
                        Config = newConfig;

                        RaisePropertyChanged(nameof(EngineType));
                        RaisePropertyChanged(nameof(EngineCommandLine));
                        RaisePropertyChanged(nameof(VisualTheme));
                        RaisePropertyChanged(nameof(HexOrientation));
                        RaisePropertyChanged(nameof(PieceStyle));
                        RaisePropertyChanged(nameof(PieceColors));
                        RaisePropertyChanged(nameof(DisablePiecesInHandWithNoMoves));
                        RaisePropertyChanged(nameof(DisablePiecesInPlayWithNoMoves));
                        RaisePropertyChanged(nameof(HighlightTargetMove));
                        RaisePropertyChanged(nameof(HighlightValidMoves));
                        RaisePropertyChanged(nameof(HighlightLastMovePlayed));
                        RaisePropertyChanged(nameof(BlockInvalidMoves));
                        RaisePropertyChanged(nameof(RequireMoveConfirmation));
                        RaisePropertyChanged(nameof(AddPieceNumbers));
                        RaisePropertyChanged(nameof(StackPiecesInHand));
                        RaisePropertyChanged(nameof(PlaySoundEffects));
                        RaisePropertyChanged(nameof(CheckUpdateOnStart));

                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand _reset = null;

        public ViewerConfig Config { get; private set; }

        public bool Accepted { get; private set; }

        public event EventHandler RequestClose;

        public Action<ViewerConfig> Callback { get; private set; }

        public ViewerConfigViewModel(ViewerConfig config, Action<ViewerConfig> callback = null)
        {
            Config =  config?.Clone() ?? throw new ArgumentNullException(nameof(config));
            Accepted = false;
            Callback = callback;
        }

        public void ProcessClose()
        {
            if (null != Callback && Accepted)
            {
                Callback(Config);
            }
        }
    }
}
