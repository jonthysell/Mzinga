// 
// GameMetadataViewModel.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using Mzinga.Core;

namespace Mzinga.SharedUX.ViewModel
{
    public class GameMetadataViewModel : ViewModelBase
    {
        public string Title
        {
            get
            {
                return "Edit Metadata";
            }
        }

        public ObservableCollection<ObservableGameMetadataTag> StandardTags
        {
            get
            {
                return _standardTags;
            }
        }
        private ObservableCollection<ObservableGameMetadataTag> _standardTags;

        public ObservableCollection<ObservableGameMetadataTag> OptionalTags
        {
            get
            {
                return _optionalTags;
            }
        }
        private ObservableCollection<ObservableGameMetadataTag> _optionalTags;

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
                        LoadTags();
                        RaisePropertyChanged("StandardTags");
                        RaisePropertyChanged("OptionalTags");
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }));
            }
        }
        private RelayCommand _reset = null;

        private readonly GameMetadata _originalMetadata;

        public bool Accepted { get; private set; } = false;

        public event EventHandler RequestClose;

        public Action<GameMetadata> Callback { get; private set; }

        public GameMetadataViewModel(GameMetadata metadata = null, Action<GameMetadata> callback = null)
        {
            _originalMetadata =  null != metadata ? metadata.Clone() : new GameMetadata();

            LoadTags();

            Callback = callback;
        }

        private void LoadTags()
        {
            _standardTags = new ObservableCollection<ObservableGameMetadataTag>();

            _standardTags.Add(new ObservableGameMetadataTag("GameType", EnumUtils.GetExpansionPiecesString(_originalMetadata.GameType)) { CanEdit = false });

            _standardTags.Add(new ObservableGameMetadataTag("Event", _originalMetadata.Event));
            _standardTags.Add(new ObservableGameMetadataTag("Site", _originalMetadata.Site));
            _standardTags.Add(new ObservableGameMetadataTag("Date", _originalMetadata.Date));
            _standardTags.Add(new ObservableGameMetadataTag("Round", _originalMetadata.Round));
            _standardTags.Add(new ObservableGameMetadataTag("White", _originalMetadata.White));
            _standardTags.Add(new ObservableGameMetadataTag("Black", _originalMetadata.Black));

            _standardTags.Add(new ObservableGameMetadataTag("Result", _originalMetadata.Result.ToString()));

            _optionalTags = new ObservableCollection<ObservableGameMetadataTag>();
            foreach (KeyValuePair<string, string> kvp in _originalMetadata.OptionalTags)
            {
                _optionalTags.Add(new ObservableGameMetadataTag(kvp.Key, kvp.Value));
            }


        }

        public void ProcessClose()
        {
            if (null != Callback && Accepted)
            {
                GameMetadata metadata = new GameMetadata();

                foreach (ObservableGameMetadataTag tag in StandardTags)
                {
                    try
                    {
                        metadata.SetTag(tag.Key, tag.Value);
                    }
                    catch (Exception)
                    {
                        metadata.SetTag(tag.Key, _originalMetadata.GetTag(tag.Key));
                    }
                }

                foreach (ObservableGameMetadataTag tag in OptionalTags)
                {
                    try
                    {
                        metadata.SetTag(tag.Key, tag.Value);
                    }
                    catch (Exception)
                    {
                        metadata.SetTag(tag.Key, _originalMetadata.GetTag(tag.Key));
                    }
                }

                Callback(metadata);
            }
        }
    }
}
