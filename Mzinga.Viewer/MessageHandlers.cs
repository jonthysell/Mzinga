// 
// MessageHandlers.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016 Jon Thysell <http://jonthysell.com>
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

using GalaSoft.MvvmLight.Messaging;

using Mzinga.Viewer.ViewModel;

namespace Mzinga.Viewer
{
    public class MessageHandlers
    {
        public static void RegisterMessageHandlers(object recipient)
        {
            Messenger.Default.Register<ExceptionMessage>(recipient, (message) => MessageHandlers.ShowException(message));
            Messenger.Default.Register<InformationMessage>(recipient, (message) => MessageHandlers.ShowInformation(message));
            Messenger.Default.Register<EngineConsoleMessage>(recipient, (message) => MessageHandlers.ShowEngineConsole(message));
        }

        public static void UnregisterMessageHandlers(object recipient)
        {
            Messenger.Default.Unregister<ExceptionMessage>(recipient);
            Messenger.Default.Unregister<InformationMessage>(recipient);
            Messenger.Default.Unregister<EngineConsoleMessage>(recipient);
        }

        private static void ShowException(ExceptionMessage message)
        {
            ExceptionWindow window = new ExceptionWindow();
            window.DataContext = message.ExceptionVM;
            message.ExceptionVM.RequestClose += () =>
            {
                window.Close();
            };
            window.ShowDialog();
        }

        private static void ShowInformation(InformationMessage message)
        {
            InformationWindow window = new InformationWindow();
            window.DataContext = message.InformationVM;
            message.InformationVM.RequestClose += () =>
            {
                window.Close();
            };
            window.ShowDialog();
            message.Process();
        }

        private static void ShowEngineConsole(EngineConsoleMessage message)
        {
            EngineConsoleWindow window = EngineConsoleWindow.Instance;

            window.Show();

            if (!window.IsActive)
            {
                window.Activate();
            }
        }
    }
}
