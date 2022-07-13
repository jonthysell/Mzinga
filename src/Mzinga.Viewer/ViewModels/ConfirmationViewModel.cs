// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Mzinga.Viewer.ViewModels
{
    public enum ConfirmationResult
    {
        Cancel,
        No,
        Yes,
    }

    public class ConfirmationViewModel : InformationViewModelBase
    {
        public static AppViewModel AppVM
        {
            get
            {
                return AppViewModel.Instance;
            }

        }

        #region Properties

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
                        OnRequestClose();
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
                        OnRequestClose();
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
                        OnRequestClose();
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

        public ConfirmationViewModel(string message, string details = null) : base(message, "Confirmation", details) { }
    }
}
