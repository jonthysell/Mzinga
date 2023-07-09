// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

using Mzinga.Viewer.ViewModels;

namespace Mzinga.Viewer.Views
{
    public partial class NewGameWindow : Window
    {
        public NewGameViewModel VM
        {
            get
            {
                return (NewGameViewModel)DataContext;
            }
            set
            {
                DataContext = value;
                value.RequestClose += (s, e) => Close();
            }
        }

        public NewGameWindow()
        {
            InitializeComponent();
        }

        private void ToggleOption_Click(object sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                if (sender == WhitePlayerTile)
                {
                    VM.WhitePlayerType = (PlayerType)(1 - (int)VM.WhitePlayerType);
                }
                else if (sender == BlackPlayerTile)
                {
                    VM.BlackPlayerType = (PlayerType)(1 - (int)VM.BlackPlayerType);
                }
                else if (sender == MosquitoCanvas)
                {
                    VM.IncludeMosquito = !VM.IncludeMosquito;
                }
                else if (sender == LadybugCanvas)
                {
                    VM.IncludeLadybug = !VM.IncludeLadybug;
                }
                else if (sender == PillbugCanvas)
                {
                    VM.IncludePillbug = !VM.IncludePillbug;
                }
                e.Handled = true;
            }
        }
    }
}
