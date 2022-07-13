// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using CommunityToolkit.Mvvm.ComponentModel;

namespace Mzinga.Viewer.ViewModels
{
    public class ObservableAboutTabItem : ObservableObject
    {
        public string Header { get; private set; }

        public string Body { get; private set; }

        public ObservableAboutTabItem(string header, string body)
        {
            Header = string.IsNullOrWhiteSpace(header) ? throw new ArgumentNullException(nameof(header)) : header.Trim();
            Body = string.IsNullOrWhiteSpace(body) ? throw new ArgumentNullException(nameof(body)) : body.Trim();
        }
    }
}
