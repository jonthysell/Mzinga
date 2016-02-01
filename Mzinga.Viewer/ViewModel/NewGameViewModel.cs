// 
// NewGameViewModel.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016 Jon Thysell <http://jonthysell.com>
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

using Mzinga.Viewer.Resources;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

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

        public RelayCommand Accept
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        Accepted = true;
                        if (null != RequestClose)
                        {
                            RequestClose();
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }

        public RelayCommand Reject
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        Accepted = false;
                        if (null != RequestClose)
                        {
                            RequestClose();
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }

        public GameSettings Settings { get; private set; }

        public bool Accepted { get; private set; }

        public event Action RequestClose;

        public Action<GameSettings> Callback { get; private set; }

        public NewGameViewModel(GameSettings settings = null, Action<GameSettings> callback = null)
        {
            Settings =  null != settings.Clone() ? settings : new GameSettings();
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
