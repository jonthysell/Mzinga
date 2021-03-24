// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Mzinga.Viewer.ViewModels
{
    public enum ConfirmationResult
    {
        Cancel,
        No,
        Yes,
    }

    public class ConfirmationViewModel : ViewModelBase
    {
        public static AppViewModel AppVM
        {
            get
            {
                return AppViewModel.Instance;
            }


        }
        #region Properties

        public static string Title => "Confirmation";

        public string Message { get; private set; }

        public ConfirmationResult Result { get; private set; } = ConfirmationResult.Cancel;

        #endregion

        #region Commands

        public RelayCommand Yes
        {
            get
            {
                return _yes ??= new RelayCommand(() =>
                {
                    try
                    {
                        Result = ConfirmationResult.Yes;
                        RequestClose?.Invoke(this, null);
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand _yes;

        public RelayCommand No
        {
            get
            {
                return _no ??= new RelayCommand(() =>
                {
                    try
                    {
                        Result = ConfirmationResult.No;
                        RequestClose?.Invoke(this, null);
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand _no;

        public RelayCommand Cancel
        {
            get
            {
                return _cancel ??= new RelayCommand(() =>
                {
                    try
                    {
                        Result = ConfirmationResult.Cancel;
                        RequestClose?.Invoke(this, null);
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand _cancel;

        #endregion

        public event EventHandler RequestClose;

        public ConfirmationViewModel(string message) : base()
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }
    }
}
