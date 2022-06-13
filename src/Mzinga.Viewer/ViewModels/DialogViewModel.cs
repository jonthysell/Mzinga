// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using GalaSoft.MvvmLight;

namespace Mzinga.Viewer.ViewModels
{
    public abstract class DialogViewModel : ViewModelBase
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
                return _title;
            }
            protected set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _title = value;
            }
        }
        private string _title;

        public string Message
        {
            get
            {
                return _message;
            }
            protected set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }
                _message = value;
            }
        }
        private string _message;

        public string Details { get; protected set; }

        public virtual bool ShowDetails => !string.IsNullOrWhiteSpace(Details);

        public event EventHandler RequestClose;

        public DialogViewModel(string message, string title, string details = null)
        {
            Title = title;
            Message = message;
            Details = details;
        }

        protected void OnRequestClose()
        {
            RequestClose?.Invoke(this, null);
        }
    }
}
