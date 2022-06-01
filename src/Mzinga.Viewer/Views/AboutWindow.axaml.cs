// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Mzinga.Viewer.ViewModels;

namespace Mzinga.Viewer.Views
{
    public class AboutWindow : Window
    {
        public AboutViewModel VM
        {
            get
            {
                return (AboutViewModel)DataContext;
            }
            set
            {
                DataContext = value;
                value.RequestClose += (s, e) => Close();
            }
        }

        public AboutWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
