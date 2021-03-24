// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Mzinga.Viewer.ViewModels;

namespace Mzinga.Viewer.Views
{
    public class LicensesWindow : Window
    {
        public LicensesViewModel VM
        {
            get
            {
                return (LicensesViewModel)DataContext;
            }
            set
            {
                DataContext = value;
                value.RequestClose += (s, e) => Close();
            }
        }

        public LicensesWindow()
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
