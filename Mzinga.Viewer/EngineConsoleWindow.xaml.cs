// 
// EngineConsoleWindow.xaml.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2016, 2017, 2019 Jon Thysell <http://jonthysell.com>
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
using System.Windows;

namespace Mzinga.Viewer
{
    /// <summary>
    /// Interaction logic for EngineConsoleWindow.xaml
    /// </summary>
    public partial class EngineConsoleWindow : Window
    {
        public static EngineConsoleWindow Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new EngineConsoleWindow();
                    _instance.Owner = Application.Current.MainWindow;
                }
                return _instance;
            }
        }
        private static EngineConsoleWindow _instance;

        private EngineConsoleWindow()
        {
            InitializeComponent();
            Closed += EngineConsoleWindow_Closed;
        }

        private void EngineConsoleWindow_Closed(object sender, EventArgs e)
        {
            _instance = null;
        }
    }
}
