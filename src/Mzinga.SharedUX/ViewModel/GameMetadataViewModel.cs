// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

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
                        LoadTags();
                        RaisePropertyChanged(nameof(StandardTags));
                        RaisePropertyChanged(nameof(OptionalTags));
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
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
            _standardTags = new ObservableCollection<ObservableGameMetadataTag>
            {
                new ObservableGameMetadataStringTag("GameType", Enums.GetGameTypeString(_originalMetadata.GameType)) { CanEdit = false },

                new ObservableGameMetadataStringTag("Date", _originalMetadata.Date),
                new ObservableGameMetadataStringTag("Event", _originalMetadata.Event),
                new ObservableGameMetadataStringTag("Site", _originalMetadata.Site),
                new ObservableGameMetadataStringTag("Round", _originalMetadata.Round),
                new ObservableGameMetadataStringTag("White", _originalMetadata.White),
                new ObservableGameMetadataStringTag("Black", _originalMetadata.Black),

                new ObservableGameMetadataEnumTag("Result", _originalMetadata.Result.ToString(), Enum.GetNames(typeof(BoardState)))
            };

            _optionalTags = new ObservableCollection<ObservableGameMetadataTag>();
            foreach (KeyValuePair<string, string> kvp in _originalMetadata.OptionalTags)
            {
                _optionalTags.Add(new ObservableGameMetadataStringTag(kvp.Key, kvp.Value));
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
