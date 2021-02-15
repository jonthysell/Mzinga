// 
// LicensesViewModel.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2019, 2021 Jon Thysell <http://jonthysell.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Mzinga.SharedUX.ViewModel
{
    public class LicensesViewModel : ViewModelBase
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
                return $"About {AppVM.ProgramTitle} v{AppVM.FullVersion}";
            }
        }

        public ObservableCollection<ObservableLicense> Licenses { get; private set; }

        public RelayCommand Accept
        {
            get
            {
                return _accept ?? (_accept = new RelayCommand(() =>
                {
                    try
                    {
                        RequestClose?.Invoke(this, null);
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                }));
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
#if AVALONIAUI
                GetAvaloniaLicense(),
#endif
                GetMvvmLightLicense(),
            };
        }

        private ObservableLicense GetHiveLicense()
        {
            return new ObservableLicense("Hive", "Copyright © 2016 Gen42 Games", "", "Mzinga is in no way associated with or endorsed by Gen42 Games.");
        }

        private ObservableLicense GetMzingaLicense()
        {
            return new ObservableLicense(AppInfo.Product, AppInfo.Copyright, AppInfo.MitLicenseName, AppInfo.MitLicenseBody);
        }

#if AVALONIAUI
        private ObservableLicense GetAvaloniaLicense()
        {
            return new ObservableLicense("Avalonia", "Copyright © .NET Foundation and Contributors", AppInfo.MitLicenseName, AppInfo.MitLicenseBody);
        }
#endif

        private ObservableLicense GetMvvmLightLicense()
        {
            return new ObservableLicense("MVVM Light", "Copyright © 2009-2018 Laurent Bugnion", AppInfo.MitLicenseName, AppInfo.MitLicenseBody);
        }

        public void ProcessClose()
        {
            Callback?.Invoke();
        }
    }
}
