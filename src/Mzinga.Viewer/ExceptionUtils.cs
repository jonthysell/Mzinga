// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using GalaSoft.MvvmLight.Messaging;

using Mzinga.Viewer.ViewModels;

namespace Mzinga.Viewer
{
    public class ExceptionUtils
    {
        public static AppViewModel AppVM
        {
            get
            {
                return AppViewModel.Instance;
            }
        }

        public static void HandleException(Exception exception)
        {
            AppVM.DoOnUIThread(() =>
            {
                Messenger.Default.Send(new ExceptionMessage(exception));
            });
        }
    }
}
