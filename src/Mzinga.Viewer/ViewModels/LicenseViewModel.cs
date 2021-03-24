// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Mzinga.Viewer.ViewModels
{
    public class LicensesViewModel : ViewModelBase
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
                return $"About {AppVM.ProgramTitle} v{AppVM.FullVersion}";
            }
        }

        public ObservableCollection<ObservableLicense> Licenses { get; private set; }

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
        private RelayCommand _accept;

        public event EventHandler RequestClose;

        public Action Callback { get; private set; }

        public LicensesViewModel(Action callback = null)
        {
            Callback = callback;

            Licenses = new ObservableCollection<ObservableLicense>
            {
                GetHiveLicense(),
                GetMzingaLicense(),
                GetAvaloniaLicense(),
                GetMvvmLightLicense(),
            };
        }

        private static ObservableLicense GetHiveLicense()
        {
            return new ObservableLicense("Hive", "Copyright © 2016 Gen42 Games", "", "Mzinga is in no way associated with or endorsed by Gen42 Games.");
        }

        private static ObservableLicense GetMzingaLicense()
        {
            return new ObservableLicense(AppInfo.Product, AppInfo.Copyright, AppInfo.MitLicenseName, AppInfo.MitLicenseBody);
        }

        private static ObservableLicense GetAvaloniaLicense()
        {
            return new ObservableLicense("Avalonia", "Copyright © .NET Foundation and Contributors", AppInfo.MitLicenseName, AppInfo.MitLicenseBody);
        }

        private static ObservableLicense GetMvvmLightLicense()
        {
            return new ObservableLicense("MVVM Light", "Copyright © 2009-2018 Laurent Bugnion", AppInfo.MitLicenseName, AppInfo.MitLicenseBody);
        }

        public void ProcessClose()
        {
            Callback?.Invoke();
        }
    }
}
