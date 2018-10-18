// 
// MessageHandlers.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017, 2018 Jon Thysell <http://jonthysell.com>
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
using System.Diagnostics;
using System.Windows.Forms;

using GalaSoft.MvvmLight.Messaging;

using Mzinga.SharedUX;
using Mzinga.SharedUX.ViewModel;

namespace Mzinga.Viewer
{
    public class MessageHandlers
    {
        public static void RegisterMessageHandlers(object recipient)
        {
            Messenger.Default.Register<ExceptionMessage>(recipient, (message) => ShowException(message));
            Messenger.Default.Register<InformationMessage>(recipient, (message) => ShowInformation(message));
            Messenger.Default.Register<ConfirmationMessage>(recipient, (message) => ShowConfirmation(message));
            Messenger.Default.Register<LaunchUrlMessage>(recipient, (message) => LaunchUrl(message));
            Messenger.Default.Register<NewGameMessage>(recipient, (message) => ShowNewGame(message));
            Messenger.Default.Register<ViewerConfigMessage>(recipient, (message) => ShowViewerConfig(message));
            Messenger.Default.Register<EngineOptionsMessage>(recipient, (message) => ShowEngineOptions(message));
            Messenger.Default.Register<EngineConsoleMessage>(recipient, (message) => ShowEngineConsole(message));
        }

        public static void UnregisterMessageHandlers(object recipient)
        {
            Messenger.Default.Unregister<ExceptionMessage>(recipient);
            Messenger.Default.Unregister<InformationMessage>(recipient);
            Messenger.Default.Unregister<ConfirmationMessage>(recipient);
            Messenger.Default.Unregister<LaunchUrlMessage>(recipient);
            Messenger.Default.Unregister<NewGameMessage>(recipient);
            Messenger.Default.Unregister<ViewerConfigMessage>(recipient);
            Messenger.Default.Unregister<EngineOptionsMessage>(recipient);
            Messenger.Default.Unregister<EngineConsoleMessage>(recipient);
        }

        private static void ShowException(ExceptionMessage message)
        {
            ExceptionWindow window = new ExceptionWindow
            {
                DataContext = message.ExceptionVM
            };
            message.ExceptionVM.RequestClose += (sender, e) =>
            {
                window.Close();
            };
            window.ShowDialog();
        }

        private static void ShowInformation(InformationMessage message)
        {
            try
            {
                InformationWindow window = new InformationWindow
                {
                    DataContext = message.InformationVM
                };
                message.InformationVM.RequestClose += (sender, e) =>
                {
                    window.Close();
                };
                window.Closed += (sender, args) =>
                {
                    message.Process();
                };
                window.Show();
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
        }

        private static void ShowConfirmation(ConfirmationMessage message)
        {
            try
            {
                DialogResult dialogResult = MessageBox.Show(message.Message, "Mzinga", MessageBoxButtons.YesNo);
                message.Process(dialogResult == DialogResult.Yes);
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
}

        private static void LaunchUrl(LaunchUrlMessage message)
        {
            try
            {
                Process.Start(message.Url, null);
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
        }

        private static void ShowNewGame(NewGameMessage message)
        {
            try
            {
                NewGameWindow window = new NewGameWindow
                {
                    DataContext = message.NewGameVM
                };
                message.NewGameVM.RequestClose += (sender, e) =>
                {
                    window.Close();
                };
                window.ShowDialog();
                message.Process();
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
        }

        private static void ShowViewerConfig(ViewerConfigMessage message)
        {
            try
            {
                ViewerConfigWindow window = new ViewerConfigWindow
                {
                    DataContext = message.ViewerConfigVM
                };
                message.ViewerConfigVM.RequestClose += (sender, e) =>
                {
                    window.Close();
                };
                window.ShowDialog();
                message.Process();
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
        }

        private static void ShowEngineOptions(EngineOptionsMessage message)
        {
            try
            {
                if (message.EngineOptionsVM.Options.Count == 0)
                {
                    throw new Exception("This engine exposes no options to set.");
                }

                EngineOptionsWindow window = new EngineOptionsWindow
                {
                    DataContext = message.EngineOptionsVM
                };
                message.EngineOptionsVM.RequestClose += (sender, e) =>
                {
                    window.Close();
                };
                window.ShowDialog();
                message.Process();
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
        }

        private static void ShowEngineConsole(EngineConsoleMessage message)
        {
            try
            {
                EngineConsoleWindow window = EngineConsoleWindow.Instance;

                window.Show();

                if (!window.IsActive)
                {
                    window.Activate();
                }
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
        }
    }
}
