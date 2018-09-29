// 
// ViewerConfigViewModel.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2018 Jon Thysell <http://jonthysell.com>
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

namespace Mzinga.SharedUX.ViewModel
{
    public class ViewerConfigViewModel : ViewModelBase
    {
        public string Title
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
                RaisePropertyChanged("EngineType");
                RaisePropertyChanged("EngineCommandLine");
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
                RaisePropertyChanged("EngineCommandLine");
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
                RaisePropertyChanged("HexOrientation");
                RaisePropertyChanged("NotationType");
            }
        }

        public NotationType NotationType
        {
            get
            {
                return Config.NotationType;
            }
            set
            {
                Config.NotationType = value;
                RaisePropertyChanged("NotationType");
                RaisePropertyChanged("HexOrientation");
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
                RaisePropertyChanged("DisablePiecesInHandWithNoMoves");
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
                RaisePropertyChanged("DisablePiecesInPlayWithNoMoves");
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
                RaisePropertyChanged("HighlightTargetMove");
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
                RaisePropertyChanged("HighlightValidMoves");
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
                RaisePropertyChanged("HighlightLastMovePlayed");
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
                RaisePropertyChanged("BlockInvalidMoves");
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
                RaisePropertyChanged("RequireMoveConfirmation");
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
                RaisePropertyChanged("StackPiecesInHand");
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

        public RelayCommand Reset
        {
            get
            {
                return _reset ?? (_reset = new RelayCommand(() =>
                {
                    try
                    {
                        Config = new ViewerConfig();
                        RaisePropertyChanged("EngineType");
                        RaisePropertyChanged("EngineCommandLine");
                        RaisePropertyChanged("HexOrientation");
                        RaisePropertyChanged("NotationType");
                        RaisePropertyChanged("DisablePiecesInHandWithNoMoves");
                        RaisePropertyChanged("DisablePiecesInPlayWithNoMoves");
                        RaisePropertyChanged("HighlightTargetMove");
                        RaisePropertyChanged("HighlightValidMoves");
                        RaisePropertyChanged("HighlightLastMovePlayed");
                        RaisePropertyChanged("BlockInvalidMoves");
                        RaisePropertyChanged("RequireMoveConfirmation");
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }));
            }
        }
        private RelayCommand _reset = null;

        public ViewerConfig Config { get; private set; }

        public bool Accepted { get; private set; }

        public event EventHandler RequestClose;

        public Action<ViewerConfig> Callback { get; private set; }

        public ViewerConfigViewModel(ViewerConfig config = null, Action<ViewerConfig> callback = null)
        {
            Config =  null != config ? config.Clone() : new ViewerConfig();
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
