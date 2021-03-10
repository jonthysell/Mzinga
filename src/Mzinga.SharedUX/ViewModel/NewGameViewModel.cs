// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using Mzinga.Core;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Mzinga.SharedUX.ViewModel
{
    public class NewGameViewModel : ViewModelBase
    {
        public static AppViewModel AppVM
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
                return IsNewGame ? "New Game" : "Continue Game";
            }
        }

        public PlayerType WhitePlayerType
        {
            get
            {
                return Settings.WhitePlayerType;
            }
            set
            {
                try
                {
                    Settings.WhitePlayerType = value;
                    RaisePropertyChanged(nameof(WhitePlayerType));
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
            }
        }

        public PlayerType BlackPlayerType
        {
            get
            {
                return Settings.BlackPlayerType;
            }
            set
            {
                try
                {
                    Settings.BlackPlayerType = value;
                    RaisePropertyChanged(nameof(BlackPlayerType));
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
            }
        }

        public static bool EnableMosquito
        {
            get
            {
                return AppVM.EngineWrapper.EngineCapabilities.Mosquito;
            }
        }

        public bool IncludeMosquito
        {
            get
            {
                return Enums.BugTypeIsEnabledForGameType(BugType.Mosquito, Settings.GameType);
            }
            set
            {
                try
                {
                    Settings.GameType = Enums.EnableBugType(BugType.Mosquito, Settings.GameType, EnableMosquito && value);
                    RaisePropertyChanged(nameof(IncludeMosquito));
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
            }
        }

        public static bool EnableLadybug
        {
            get
            {
                return AppVM.EngineWrapper.EngineCapabilities.Ladybug;
            }
        }

        public bool IncludeLadybug
        {
            get
            {
                return Enums.BugTypeIsEnabledForGameType(BugType.Ladybug, Settings.GameType);
            }
            set
            {
                try
                {
                    Settings.GameType = Enums.EnableBugType(BugType.Ladybug, Settings.GameType, EnableLadybug && value);
                    RaisePropertyChanged(nameof(IncludeLadybug));
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
            }
        }

        public static bool EnablePillbug
        {
            get
            {
                return AppVM.EngineWrapper.EngineCapabilities.Pillbug;
            }
        }

        public bool IncludePillbug
        {
            get
            {
                return Enums.BugTypeIsEnabledForGameType(BugType.Pillbug, Settings.GameType);
            }
            set
            {
                try
                {
                    Settings.GameType = Enums.EnableBugType(BugType.Pillbug, Settings.GameType, EnablePillbug && value);
                    RaisePropertyChanged(nameof(IncludePillbug));
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
            }
        }

        public BestMoveType BestMoveType
        {
            get
            {
                return Settings.BestMoveType;
            }
            set
            {
                try
                {
                    Settings.BestMoveType = value;
                    RaisePropertyChanged(nameof(BestMoveType));
                    RaisePropertyChanged(nameof(EnableBestMoveMaxDepthValue));
                    RaisePropertyChanged(nameof(BestMoveMaxDepthValue));
                    RaisePropertyChanged(nameof(EnableBestMoveMaxTimeValue));
                    RaisePropertyChanged(nameof(BestMoveMaxTimeValue));
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
            }
        }

        public bool EnableBestMoveMaxDepthValue
        {
            get
            {
                return Settings.BestMoveType == BestMoveType.MaxDepth;
            }
        }

        public int? BestMoveMaxDepthValue
        {
            get
            {
                return Settings.BestMoveMaxDepth;
            }
            set
            {
                Settings.BestMoveMaxDepth = value;
                RaisePropertyChanged(nameof(BestMoveMaxDepthValue));
            }
        }

        public bool EnableBestMoveMaxTimeValue
        {
            get
            {
                return Settings.BestMoveType == BestMoveType.MaxTime;
            }
        }

        public TimeSpan? BestMoveMaxTimeValue
        {
            get
            {
                return Settings.BestMoveMaxTime;
            }
            set
            {
                Settings.BestMoveMaxTime = value;
                RaisePropertyChanged(nameof(BestMoveMaxTimeValue));
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
                            case nameof(WhitePlayerType):
                                WhitePlayerType = (PlayerType)Enum.Parse(typeof(PlayerType), split[1]);
                                break;
                            case nameof(BlackPlayerType):
                                BlackPlayerType = (PlayerType)Enum.Parse(typeof(PlayerType), split[1]);
                                break;
                            case nameof(BestMoveType):
                                BestMoveType = (BestMoveType)Enum.Parse(typeof(BestMoveType), split[1]);
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

        public GameSettings Settings { get; private set; }

        public bool IsNewGame { get; private set; }

        public bool Accepted { get; private set; }

        public event EventHandler RequestClose;

        public Action<GameSettings> Callback { get; private set; }

        public NewGameViewModel(GameSettings settings, bool isNewGame, Action<GameSettings> callback)
        {
            Settings = settings?.Clone() ?? new GameSettings();

            IsNewGame = isNewGame;

            Accepted = false;
            Callback = callback;
        }

        private GameSettings GetNewGameSettings()
        {
            GameSettings gs = GameSettings.CreateNewFromExisting(Settings);

            gs.Metadata.SetTag("White", gs.WhitePlayerType == PlayerType.Human ? Environment.UserName : AppVM.EngineWrapper.ID);
            gs.Metadata.SetTag("Black", gs.BlackPlayerType == PlayerType.Human ? Environment.UserName : AppVM.EngineWrapper.ID);
            gs.Metadata.SetTag("Date", DateTime.Today.ToString("yyyy.MM.dd"));

            gs.GameMode = GameMode.Play;

            return gs;
        }

        public void ProcessClose()
        {
            if (null != Callback && Accepted)
            {
                Callback(GetNewGameSettings());
            }
        }
    }
}
