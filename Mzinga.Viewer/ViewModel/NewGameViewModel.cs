// 
// NewGameViewModel.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017, 2018 Jon Thysell <http://jonthysell.com>
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
using Mzinga.Viewer.Resources;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Mzinga.Viewer.ViewModel
{
    public class NewGameViewModel : ViewModelBase
    {
        public string Title
        {
            get
            {
                return Strings.NewGameTitle;
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
                    RaisePropertyChanged("WhitePlayerType");
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
                    RaisePropertyChanged("BlackPlayerType");
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
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
                    if (value)
                    {
                        Settings.ExpansionPieces |= ExpansionPieces.Mosquito;
                    }
                    else
                    {
                        Settings.ExpansionPieces &= ~ExpansionPieces.Mosquito; 
                    }
                    RaisePropertyChanged("IncludeMosquito");
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
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
                    if (value)
                    {
                        Settings.ExpansionPieces |= ExpansionPieces.Ladybug;
                    }
                    else
                    {
                        Settings.ExpansionPieces &= ~ExpansionPieces.Ladybug;
                    }
                    RaisePropertyChanged("IncludeLadybug");
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
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
                    if (value)
                    {
                        Settings.ExpansionPieces |= ExpansionPieces.Pillbug;
                    }
                    else
                    {
                        Settings.ExpansionPieces &= ~ExpansionPieces.Pillbug;
                    }
                    RaisePropertyChanged("IncludePillbug");
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
                    RaisePropertyChanged("BestMoveType");
                    RaisePropertyChanged("BestMoveMaxValue");
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
            }
        }

        public string BestMoveMaxValue
        {
            get
            {
                if (Settings.BestMoveType == BestMoveType.MaxDepth)
                {
                    return Settings.BestMoveMaxDepth.Value.ToString();
                }
                else
                {
                    return Settings.BestMoveMaxTime.Value.ToString(@"hh\:mm\:ss");
                }
            }
            set
            {
                try
                {
                    if (Settings.BestMoveType == BestMoveType.MaxDepth)
                    {
                        Settings.BestMoveMaxDepth = int.Parse(value);
                    }
                    else
                    {
                        Settings.BestMoveMaxTime = TimeSpan.Parse(value);
                    }

                    RaisePropertyChanged("BestMoveMaxValue");
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
            }
        }

        public RelayCommand Accept
        {
            get
            {
                return _accept ?? (_accept = new RelayCommand(() =>
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
                }));
            }
        }
        private RelayCommand _accept = null;

        public RelayCommand Reject
        {
            get
            {
                return _reject ?? (_reject = new RelayCommand(() =>
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
                }));
            }
        }
        private RelayCommand _reject = null;

        public GameSettings Settings { get; private set; }

        public bool Accepted { get; private set; }

        public event EventHandler RequestClose;

        public Action<GameSettings> Callback { get; private set; }

        public NewGameViewModel(GameSettings settings = null, Action<GameSettings> callback = null)
        {
            Settings =  null != settings ? settings.Clone() : new GameSettings();
            Accepted = false;
            Callback = callback;
        }

        public void ProcessClose()
        {
            if (null != Callback && Accepted)
            {
                Callback(Settings);
            }
        }
    }
}
