// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;

namespace Mzinga.Viewer.ViewModels
{
    public abstract class ObservableGameMetadataTag : ObservableObject
    {
        public string Key
        {
            get
            {
                return _key;
            }
            protected set
            {
                _key = !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentNullException(nameof(value));
                RaisePropertyChanged(nameof(Key));
            }
        }
        private string _key = "";

        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                RaisePropertyChanged(nameof(Value));
            }
        }
        private string _value = "";

        public bool IsReadOnly
        {
            get
            {
                return !CanEdit;
            }
        }

        public bool CanEdit
        {
            get
            {
                return _canEdit;
            }
            set
            {
                _canEdit = value;
                RaisePropertyChanged(nameof(CanEdit));
                RaisePropertyChanged(nameof(IsReadOnly));
            }
        }
        private bool _canEdit = true;

        public ObservableGameMetadataTag(string key, string value)
        {
            _key = !string.IsNullOrWhiteSpace(key) ? key : throw new ArgumentNullException(nameof(key));
            _value = value;
        }
    }

    public class ObservableGameMetadataStringTag : ObservableGameMetadataTag
    {
        public ObservableGameMetadataStringTag(string key, string value) : base(key, value) { }
    }

    public class ObservableGameMetadataEnumTag : ObservableGameMetadataTag
    {
        public ObservableCollection<string> PossibleValues { get; private set; } = new ObservableCollection<string>();

        public ObservableGameMetadataEnumTag(string key, string value, IEnumerable<string> possibleValues) : base(key, value)
        {
            foreach (string pv in possibleValues)
            {
                PossibleValues.Add(pv);
            }
        }
    }
}
