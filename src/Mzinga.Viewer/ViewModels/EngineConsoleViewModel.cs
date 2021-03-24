﻿// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Mzinga.Viewer.ViewModels
{
    public class EngineConsoleViewModel : ViewModelBase
    {
        internal static readonly EngineConsoleViewModel Instance = new EngineConsoleViewModel();

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
                return "Engine Console";
            }
        }

        public bool IsIdle
        {
            get
            {
                return _isIdle;
            }
            protected set
            {
                _isIdle = value;
                RaisePropertyChanged(nameof(IsIdle));
                SendEngineCommand.RaiseCanExecuteChanged();
                CancelEngineCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isIdle = true;

        public static string EngineOutputText
        {
            get
            {
                return AppVM.EngineWrapper.EngineText;
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
                RaisePropertyChanged(nameof(EngineInputText));
                SendEngineCommand.RaiseCanExecuteChanged();
            }
        }
        private string _engineInputText = "";

        public RelayCommand SendEngineCommand
        {
            get
            {
                return _sendEngineCommand ??= new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.SendCommand(EngineInputText);                      
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                    finally
                    {
                        EngineInputText = "";
                    }
                }, () =>
                {
                    return IsIdle && !string.IsNullOrWhiteSpace(EngineInputText);
                });
            }
        }
        private RelayCommand _sendEngineCommand = null;

        public RelayCommand CancelEngineCommand
        {
            get
            {
                return _cancelEngineCommand ??= new RelayCommand(() =>
                {
                    try
                    {
                        AppVM.EngineWrapper.CancelCommand();
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }, () =>
                {
                    return !IsIdle;
                });
            }
        }
        private RelayCommand _cancelEngineCommand = null;

        private EngineConsoleViewModel()
        {
            _isIdle = AppVM.EngineWrapper.IsIdle;

            AppVM.EngineWrapper.EngineTextUpdated += (sender, args) =>
            {
                AppVM.DoOnUIThread(() =>
                {
                    RaisePropertyChanged(nameof(EngineOutputText));
                });
            };

            AppVM.EngineWrapper.IsIdleUpdated += (sender, args) =>
            {
                AppVM.DoOnUIThread(() =>
                {
                    IsIdle = AppVM.EngineWrapper.IsIdle;
                });
            };
        }
    }
}
