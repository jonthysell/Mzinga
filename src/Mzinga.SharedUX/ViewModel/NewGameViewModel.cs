// 
// NewGameViewModel.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017, 2018, 2019, 2021 Jon Thysell <http://jonthysell.com>
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

using Mzinga.Core;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Mzinga.SharedUX.ViewModel
{
    public class NewGameViewModel : ViewModelBase
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

        public bool EnableMosquito
        {
            get
            {
                return EnumUtils.IsEnabled(BugType.Mosquito, AppVM.EngineWrapper.EngineCapabilities.ExpansionPieces);
            }
        }

        public bool IncludeMosquito
        {
            get
            {
                return (Settings.ExpansionPieces & ExpansionPieces.Mosquito) == ExpansionPieces.Mosquito;
            }
            set
            {
                try
                {
                    if (EnableMosquito && value)
                    {
                        Settings.ExpansionPieces |= ExpansionPieces.Mosquito;
                    }
                    else
                    {
                        Settings.ExpansionPieces &= ~ExpansionPieces.Mosquito; 
                    }
                    RaisePropertyChanged(nameof(IncludeMosquito));
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
            }
        }

        public bool EnableLadybug
        {
            get
            {
                return EnumUtils.IsEnabled(BugType.Ladybug, AppVM.EngineWrapper.EngineCapabilities.ExpansionPieces);
            }
        }

        public bool IncludeLadybug
        {
            get
            {
                return (Settings.ExpansionPieces & ExpansionPieces.Ladybug) == ExpansionPieces.Ladybug;
            }
            set
            {
                try
                {
                    if (EnableLadybug && value)
                    {
                        Settings.ExpansionPieces |= ExpansionPieces.Ladybug;
                    }
                    else
                    {
                        Settings.ExpansionPieces &= ~ExpansionPieces.Ladybug;
                    }
                    RaisePropertyChanged(nameof(IncludeLadybug));
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
            }
        }

        public bool EnablePillbug
        {
            get
            {
                return EnumUtils.IsEnabled(BugType.Pillbug, AppVM.EngineWrapper.EngineCapabilities.ExpansionPieces);
            }
        }

        public bool IncludePillbug
        {
            get
            {
                return (Settings.ExpansionPieces & ExpansionPieces.Pillbug) == ExpansionPieces.Pillbug;
            }
            set
            {
                try
                {
                    if (EnablePillbug && value)
                    {
                        Settings.ExpansionPieces |= ExpansionPieces.Pillbug;
                    }
                    else
                    {
                        Settings.ExpansionPieces &= ~ExpansionPieces.Pillbug;
                    }
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
