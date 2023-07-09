// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Mzinga.Viewer.ViewModels;

namespace Mzinga.Viewer.Views
{
    public partial class ConfirmationWindow : Window
    {
        public ConfirmationViewModel VM
        {
            get
            {
                return (ConfirmationViewModel)DataContext;
            }
            set
            {
                DataContext = value;
                value.RequestClose += (s, e) => Close();
            }
        }

        public ConfirmationWindow()
        {
            InitializeComponent();
        }
    }
}
