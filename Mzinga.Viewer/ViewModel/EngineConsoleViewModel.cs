// 
// EngineConsoleViewModel.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017 Jon Thysell <http://jonthysell.com>
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

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Mzinga.Viewer.Resources;

namespace Mzinga.Viewer.ViewModel
{
    public class EngineConsoleViewModel : ViewModelBase
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
                return Strings.EngineConsoleTitle;
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
            }
        }
        private bool _isIdle;

        public string EngineOutputText
        {
            get
            {
                return AppVM.EngineWrapper.EngineText;
            }
        }

        public string EngineInputText
        {
            get
            {
                return _engineInputText;
            }
            set
            {
                _engineInputText = value;
                RaisePropertyChanged("EngineInputText");
                RaisePropertyChanged("SendEngineCommand");
            }
        }
        private string _engineInputText = "";

        public RelayCommand SendEngineCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.SendCommand(EngineInputText);                      
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                    finally
                    {
                        EngineInputText = "";
                    }
                }, () =>
                {
                    return !string.IsNullOrWhiteSpace(EngineInputText);
                });
            }
        }

        public EngineConsoleViewModel()
        {
            AppVM.EngineWrapper.EngineTextUpdated += (engineText) =>
            {
                RaisePropertyChanged("EngineOutputText");
            };

            AppVM.EngineWrapper.IsIdleUpdated += (isIdle) =>
            {
                IsIdle = isIdle;
            };

            IsIdle = true;
        }
    }
}
