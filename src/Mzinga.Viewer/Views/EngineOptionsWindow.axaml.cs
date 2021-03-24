// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Mzinga.Viewer.ViewModels;

namespace Mzinga.Viewer.Views
{
    public class EngineOptionsWindow : Window
    {
        public EngineOptionsViewModel VM
        {
            get
            {
                return (EngineOptionsViewModel)DataContext;
            }
            set
            {
                DataContext = value;
                value.RequestClose += (s, e) => Close();
            }
        }

        public EngineOptionsWindow()
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
