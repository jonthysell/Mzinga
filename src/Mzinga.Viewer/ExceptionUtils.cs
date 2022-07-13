// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using CommunityToolkit.Mvvm.Messaging;

using Mzinga.Viewer.ViewModels;

namespace Mzinga.Viewer
{
    public static class ExceptionUtils
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
                StrongReferenceMessenger.Default.Send(new ExceptionMessage(exception));
            });
        }
    }
}
