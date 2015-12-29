// 
// MainViewModel.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015 Jon Thysell <http://jonthysell.com>
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

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Mzinga.Viewer.ViewModel
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

        public string EngineOutputText
        {
            get
            {
                return AppVM.EngineText;
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

        public string StatusText
        {
            get
            {
                return _statusText;
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    value = " ";
                }
                _statusText = value;
                RaisePropertyChanged("StatusText");
            }
        }
        private string _statusText = " ";

        public RelayCommand SendEngineCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.SendCommand(EngineInputText);
                        EngineInputText = "";                        
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return !String.IsNullOrWhiteSpace(EngineInputText);
                });
            }
        }

        public MainViewModel()
        {
            AppVM.PropertyChanged += AppVM_PropertyChanged;
        }

        private void AppVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "EngineText")
            {
                RaisePropertyChanged("EngineOutputText");
            }
            else if (e.PropertyName == "BoardString")
            {
                StatusText = AppVM.BoardString;
            }
        }
    }
}
