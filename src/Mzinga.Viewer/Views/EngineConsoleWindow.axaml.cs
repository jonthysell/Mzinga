// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

using Mzinga.SharedUX.ViewModel;

namespace Mzinga.Viewer.Views
{
    public class EngineConsoleWindow : Window
    {
        public static EngineConsoleWindow Instance { get; set; }

        public EngineConsoleViewModel VM
        {
            get
            {
                return (EngineConsoleViewModel)DataContext;
            }
            set
            {
                DataContext = value;
            }
        }

        public EngineConsoleWindow()
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

        private void EngineConsoleWindow_Closed(object sender, EventArgs e)
        {
            Instance = null;
        }

        private void EngineConsoleInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (VM.SendEngineCommand.CanExecute(null))
                {
                    VM.SendEngineCommand.Execute(null);
                }
            }
        }
    }
}
