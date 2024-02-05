// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mzinga.Viewer.ViewModels
{
    public abstract class InformationViewModelBase : ObservableObject
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

        public bool ShowDetails => !string.IsNullOrWhiteSpace(Details);

        #region Commands

        public RelayCommand CopyDetailsToClipboard
        {
            get
            {
                return _copyDetailsToClipboard ??= new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.TextToClipboard(Details);
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return ShowDetails;
                });
            }
        }
        private RelayCommand _copyDetailsToClipboard = null;

        #endregion

        public event EventHandler RequestClose;

        public InformationViewModelBase(string message, string title, string details = null)
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
