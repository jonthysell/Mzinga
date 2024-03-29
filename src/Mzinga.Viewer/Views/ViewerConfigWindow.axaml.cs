﻿// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using Mzinga.Viewer.ViewModels;

namespace Mzinga.Viewer.Views
{
    public partial class ViewerConfigWindow : Window
    {
        public ViewerConfigViewModel VM
        {
            get
            {
                return (ViewerConfigViewModel)DataContext;
            }
            set
            {
                DataContext = value;
                value.RequestClose += (s, e) => Close();
            }
        }

        public ViewerConfigWindow()
        {
            InitializeComponent();
        }
    }
}
