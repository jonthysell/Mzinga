// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.ComponentModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mzinga.Viewer.ViewModels
{
    public class ViewerConfigViewModel : ObservableObject
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
                OnPropertyChanged(nameof(EngineType));
                OnPropertyChanged(nameof(EngineCommandLine));
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
                OnPropertyChanged(nameof(EngineCommandLine));
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
                OnPropertyChanged(nameof(VisualTheme));
            }
        }

        public bool AutoCenterBoard
        {
            get
            {
                return Config.AutoCenterBoard;
            }
            set
            {
                Config.AutoCenterBoard = value;
                OnPropertyChanged(nameof(AutoCenterBoard));
            }
        }

        public bool AutoZoomBoard
        {
            get
            {
                return Config.AutoZoomBoard;
            }
            set
            {
                Config.AutoZoomBoard = value;
                OnPropertyChanged(nameof(AutoZoomBoard));
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
                OnPropertyChanged(nameof(HexOrientation));
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
                OnPropertyChanged(nameof(PieceStyle));
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
                OnPropertyChanged(nameof(PieceColors));
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
                OnPropertyChanged(nameof(DisablePiecesInHandWithNoMoves));
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
                OnPropertyChanged(nameof(DisablePiecesInPlayWithNoMoves));
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
                OnPropertyChanged(nameof(HighlightTargetMove));
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
                OnPropertyChanged(nameof(HighlightValidMoves));
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
                OnPropertyChanged(nameof(HighlightLastMovePlayed));
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
                OnPropertyChanged(nameof(BlockInvalidMoves));
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
                OnPropertyChanged(nameof(RequireMoveConfirmation));
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
                OnPropertyChanged(nameof(AddPieceNumbers));
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
                OnPropertyChanged(nameof(StackPiecesInHand));
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
                OnPropertyChanged(nameof(ShowBoardHistory));
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
                OnPropertyChanged(nameof(ShowMoveCommentary));
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
                OnPropertyChanged(nameof(PlaySoundEffects));
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
                OnPropertyChanged(nameof(CheckUpdateOnStart));
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
                        AppVM.UpdateVisualTheme(OriginalConfig.VisualTheme);
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

                        OnPropertyChanged(nameof(EngineType));
                        OnPropertyChanged(nameof(EngineCommandLine));
                        OnPropertyChanged(nameof(VisualTheme));
                        OnPropertyChanged(nameof(AutoCenterBoard));
                        OnPropertyChanged(nameof(AutoZoomBoard));
                        OnPropertyChanged(nameof(HexOrientation));
                        OnPropertyChanged(nameof(PieceStyle));
                        OnPropertyChanged(nameof(PieceColors));
                        OnPropertyChanged(nameof(DisablePiecesInHandWithNoMoves));
                        OnPropertyChanged(nameof(DisablePiecesInPlayWithNoMoves));
                        OnPropertyChanged(nameof(HighlightTargetMove));
                        OnPropertyChanged(nameof(HighlightValidMoves));
                        OnPropertyChanged(nameof(HighlightLastMovePlayed));
                        OnPropertyChanged(nameof(BlockInvalidMoves));
                        OnPropertyChanged(nameof(RequireMoveConfirmation));
                        OnPropertyChanged(nameof(AddPieceNumbers));
                        OnPropertyChanged(nameof(StackPiecesInHand));
                        OnPropertyChanged(nameof(PlaySoundEffects));
                        OnPropertyChanged(nameof(CheckUpdateOnStart));

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

        public ViewerConfig OriginalConfig { get; private set; }

        public bool Accepted { get; private set; }

        public event EventHandler RequestClose;

        public Action<ViewerConfig> Callback { get; private set; }

        public ViewerConfigViewModel(ViewerConfig config, Action<ViewerConfig> callback = null)
        {
            OriginalConfig = config?.Clone() ?? throw new ArgumentNullException(nameof(config));
            Config =  config?.Clone() ?? throw new ArgumentNullException(nameof(config));
            Accepted = false;
            Callback = callback;

            PropertyChanged += ViewerConfigViewModel_PropertyChanged;
        }

        private void ViewerConfigViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(VisualTheme):
                    AppVM.UpdateVisualTheme(VisualTheme);
                    break;
            }
        }

        public void ProcessClose()
        {
            if (Callback is not null && Accepted)
            {
                Callback(Config);
            }
        }
    }
}
