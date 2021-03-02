// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Mzinga.SharedUX.ViewModel;

namespace Mzinga.Viewer.Views
{
    public class InformationWindow : Window
    {
        public InformationViewModel VM
        {
            get
            {
                return (InformationViewModel)DataContext;
            }
            set
            {
                DataContext = value;
                value.RequestClose += (s, e) => Close();
            }
        }

        public InformationWindow()
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
