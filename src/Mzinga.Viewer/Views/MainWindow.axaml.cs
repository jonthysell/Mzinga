// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.ComponentModel;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

using Mzinga.Viewer;
using Mzinga.Viewer.ViewModels;

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
                if (MainViewModel.AppVM.EngineExceptionOnStart is not null)
                {
                    throw new Exception("Unable to start the external engine so used the internal one instead.", MainViewModel.AppVM.EngineExceptionOnStart);
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
            EngineConsoleWindow.Instance?.Close();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void MainWindow_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Z:
                    ReZoomButton_Click(sender, e);
                    break;
                case Key.X:
                    LiftButton_Click(sender, e);
                    break;
                case Key.C:
                    ReCenterButton_Click(sender, e);
                    break;
            }
        }

        private void ReZoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (!VM.AutoZoomBoard)
            {
                BoardRenderer.TryRedraw(false, true);
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

        private void ReCenterButton_Click(object sender, RoutedEventArgs e)
        {
            if (!VM.AutoCenterBoard)
            {
                BoardRenderer.TryRedraw(true, false);
                e.Handled = true;
            }
        }

        private void BoardHistoryListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox lb)
            {
                lb.ScrollIntoView(Math.Max(VM.BoardHistory.CurrentMoveIndex, 0));
            }
        }
    }
}
