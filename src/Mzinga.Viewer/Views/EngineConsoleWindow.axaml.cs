// 
// EngineConsoleWindow.axaml.cs
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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

using Mzinga.SharedUX.ViewModel;

namespace Mzinga.Viewer.Views
{
    public class EngineConsoleWindow : Window
    {
        public static EngineConsoleWindow Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new EngineConsoleWindow()
                    {
                        VM = new EngineConsoleViewModel(),
                    };
                }
                return _instance;
            }
        }
        private static EngineConsoleWindow _instance;

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
