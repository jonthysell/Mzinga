﻿// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

using Mzinga.Viewer.ViewModels;

namespace Mzinga.Viewer.Views
{
    public partial class EngineConsoleWindow : Window
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
        }

        private void EngineConsoleWindow_Closed(object sender, EventArgs e)
        {
            Instance = null;
        }

        private void EngineConsoleInput_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
        {
            (sender as InputElement)?.Focus();
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

        private void EngineConsoleOutput_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == TextBox.TextProperty)
            {
                if (sender is TextBox tb && e.NewValue is string text)
                {
                    tb.CaretIndex = text.Length;
                }
            }
        }
    }
}
