// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Mzinga.Viewer.ViewModels
{
    public class EngineOptionsViewModel : ViewModelBase
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
                return "Engine Options";
            }
        }

        public ObservableCollection<ObservableEngineOption> Options
        {
            get
            {
                return _options;
            }
        }
        private ObservableCollection<ObservableEngineOption> _options;

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
                        LoadOptions(true);
                        RaisePropertyChanged(nameof(Options));
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand _reset = null;

        private readonly EngineOptions _originalOptions;

        public bool Accepted { get; private set; } = false;

        public event EventHandler RequestClose;

        public Action<IDictionary<string, string>> Callback { get; private set; }

        public EngineOptionsViewModel(EngineOptions options = null, Action<IDictionary<string, string>> callback = null)
        {
            _originalOptions =  null != options ? options.Clone() : new EngineOptions();

            LoadOptions(false);

            Callback = callback;
        }

        private void LoadOptions(bool resetToDefaults)
        {
            _options = new ObservableCollection<ObservableEngineOption>();

            foreach (EngineOption eo in _originalOptions)
            {
                if (eo is BooleanEngineOption beo)
                {
                    _options.Add(new ObservableBooleanEngineOption(beo, resetToDefaults));
                }
                else if (eo is IntegerEngineOption ieo)
                {
                    _options.Add(new ObservableIntegerEngineOption(ieo, resetToDefaults));
                }
                else if (eo is DoubleEngineOption deo)
                {
                    _options.Add(new ObservableDoubleEngineOption(deo, resetToDefaults));
                }
                else if (eo is EnumEngineOption eeo)
                {
                    _options.Add(new ObservableEnumEngineOption(eeo, resetToDefaults));
                }
            }
        }

        public void ProcessClose()
        {
            if (null != Callback && Accepted)
            {
                Dictionary<string, string> changedOptions = new Dictionary<string, string>();

                foreach (ObservableEngineOption oeo in Options)
                {
                    if (oeo is ObservableBooleanEngineOption obeo)
                    {
                        if (((BooleanEngineOption)_originalOptions[oeo.Key]).Value != obeo.Value)
                        {
                            changedOptions.Add(oeo.Key, obeo.Value.ToString());
                        }
                    }
                    else if (oeo is ObservableIntegerEngineOption oieo)
                    {
                        if (((IntegerEngineOption)_originalOptions[oeo.Key]).Value != oieo.Value)
                        {
                            changedOptions.Add(oeo.Key, oieo.Value.ToString());
                        }
                    }
                    else if (oeo is ObservableDoubleEngineOption odeo)
                    {
                        if (((DoubleEngineOption)_originalOptions[oeo.Key]).Value != odeo.Value)
                        {
                            changedOptions.Add(oeo.Key, odeo.Value.ToString());
                        }
                    }
                    else if (oeo is ObservableEnumEngineOption oeeo)
                    {
                        if (((EnumEngineOption)_originalOptions[oeo.Key]).Value != oeeo.Value)
                        {
                            changedOptions.Add(oeo.Key, oeeo.Value);
                        }
                    }
                }

                Callback(changedOptions);
            }
        }
    }
}
