// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Mzinga.Viewer.ViewModels
{
    public class InformationViewModel : ViewModelBase
    {
        public static AppViewModel AppVM
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

        public RelayCommand Accept
        {
            get
            {
                return _accept ??= new RelayCommand(() =>
                {
                    try
                    {
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

        public event EventHandler RequestClose;

        public Action Callback { get; private set; }

        public InformationViewModel(string message, string title, Action callback = null)
        {
            Title = title;
            Message = message;
            Callback = callback;
        }

        public void ProcessClose()
        {
            Callback?.Invoke();
        }
    }
}
