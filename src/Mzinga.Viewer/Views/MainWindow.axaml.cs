// 
// MainWindow.axaml.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2021 Jon Thysell <http://jonthysell.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.ComponentModel;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

using Mzinga.SharedUX;
using Mzinga.SharedUX.ViewModel;

namespace Mzinga.Viewer.Views
{
    public class MainWindow : Window
    {
        public MainViewModel VM
        {
            get
            {
                return DataContext as MainViewModel;
            }
            private set
            {
                DataContext = value;
                value.RequestClose = Close;
            }
        }

        public Canvas BoardCanvas => _boardCanvas ??= this.FindControl<Canvas>(nameof(BoardCanvas));
        private Canvas _boardCanvas;

        public StackPanel WhiteHandStackPanel => _whiteHandStackPanel ??= this.FindControl<StackPanel>(nameof(WhiteHandStackPanel));
        private StackPanel _whiteHandStackPanel;

        public StackPanel BlackHandStackPanel => _blackHandStackPanel ??= this.FindControl<StackPanel>(nameof(BlackHandStackPanel));
        private StackPanel _blackHandStackPanel;

        public XamlBoardRenderer BoardRenderer { get; private set; }

        public MainWindow()
        {
            VM = AppViewModel.Instance.MainVM;

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            BoardRenderer = new XamlBoardRenderer(VM, BoardCanvas, WhiteHandStackPanel, BlackHandStackPanel);

            Opened += MainWindow_Opened;
            Closing += MainWindow_Closing;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void MainWindow_Opened(object sender, EventArgs e)
        {
            try
            {
                if (null != VM.AppVM.EngineExceptionOnStart)
                {
                    throw new Exception("Unable to start the external engine so used the internal one instead.", VM.AppVM.EngineExceptionOnStart);
                }

                VM.OnLoaded();
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            EngineConsoleWindow.Instance.Close();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (VM.CanRaiseStackedPieces)
            {
                if (e.Key == Key.X)
                {
                    BoardRenderer.RaiseStackedPieces = true;
                    e.Handled = true;
                }
            }
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.X)
            {
                BoardRenderer.RaiseStackedPieces = false;
                e.Handled = true;
            }
        }

        private void LiftButton_Click(object sender, RoutedEventArgs e)
        {
            if (VM.CanRaiseStackedPieces)
            {
                BoardRenderer.RaiseStackedPieces = !BoardRenderer.RaiseStackedPieces;
                e.Handled = true;
            }
        }
    }
}
