// 
// MessageHandlers.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017, 2018, 2019 Jon Thysell <http://jonthysell.com>
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
using System.IO;
using System.Windows;
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
            Messenger.Default.Register<ShowLicensesMessage>(recipient, (message) => ShowLicense(message));
            Messenger.Default.Register<NewGameMessage>(recipient, (message) => ShowNewGame(message));
            Messenger.Default.Register<LoadGameMessage>(recipient, (message) => ShowLoadGame(message));
            Messenger.Default.Register<SaveGameMessage>(recipient, (message) => ShowSaveGame(message));
            Messenger.Default.Register<GameMetadataMessage>(recipient, (message) => ShowGameMetadata(message));
            Messenger.Default.Register<ViewerConfigMessage>(recipient, (message) => ShowViewerConfig(message));
            Messenger.Default.Register<EngineOptionsMessage>(recipient, (message) => ShowEngineOptions(message));
            Messenger.Default.Register<EngineConsoleMessage>(recipient, (message) => ShowEngineConsole(message));
        }

        public static void UnregisterMessageHandlers(object recipient)
        {
            Messenger.Default.Unregister<ExceptionMessage>(recipient);
            Messenger.Default.Unregister<InformationMessage>(recipient);
            Messenger.Default.Unregister<ConfirmationMessage>(recipient);
            Messenger.Default.Unregister<ShowLicensesMessage>(recipient);
            Messenger.Default.Unregister<LaunchUrlMessage>(recipient);
            Messenger.Default.Unregister<NewGameMessage>(recipient);
            Messenger.Default.Unregister<LoadGameMessage>(recipient);
            Messenger.Default.Unregister<SaveGameMessage>(recipient);
            Messenger.Default.Unregister<GameMetadataMessage>(recipient);
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
                MessageBoxResult result = System.Windows.MessageBox.Show(message.Message, "Mzinga", MessageBoxButton.YesNo);
                message.Process(result == MessageBoxResult.Yes);
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

        private static void ShowLicense(ShowLicensesMessage message)
        {
            LicensesWindow window = new LicensesWindow();

            message.LicensesVM.Licenses.Add(GetExtendedWpfToolkitLicense());

            window.DataContext = message.LicensesVM;
            message.LicensesVM.RequestClose += () =>
            {
                window.Close();
            };
            window.ShowDialog();
            message.Process();
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

        private static void ShowLoadGame(LoadGameMessage message)
        {
            GameRecording gr = null;
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();

                dialog.Title = "Open Game";
                dialog.DefaultExt = ".pgn";
                dialog.Filter = "All Supported Files|*.pgn;*.sgf|Portable Game Notation|*.pgn|Smart Game Format|*.sgf";
                dialog.AddExtension = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (Stream inputStream = dialog.OpenFile())
                    {
                        gr = Path.GetExtension(dialog.SafeFileName).ToLower() == ".sgf" ? GameRecording.LoadSGF(inputStream) : GameRecording.LoadPGN(inputStream);
                    }
                }

            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
            finally
            {
                message.Process(gr);
            }
        }

        private static void ShowSaveGame(SaveGameMessage message)
        {
            try
            {
                SaveFileDialog dialog = new SaveFileDialog();

                dialog.Title = "Save Game";
                dialog.DefaultExt = ".pgn";
                dialog.Filter = "Portable Game Notation|*.pgn";
                dialog.AddExtension = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (Stream outputStream = dialog.OpenFile())
                    {
                        message.GameRecording.SavePGN(outputStream);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
        }

        private static void ShowGameMetadata(GameMetadataMessage message)
        {
            try
            {
                GameMetadataWindow window = new GameMetadataWindow
                {
                    DataContext = message.GameMetadataVM
                };
                message.GameMetadataVM.RequestClose += (sender, e) =>
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

        private static ObservableLicense GetExtendedWpfToolkitLicense()
        {
            return new ObservableLicense("Extended WPF Toolkit", "Copyright © 2010-2019 Xceed Software Inc", "Microsoft Public License (Ms-PL)", string.Join(Environment.NewLine + Environment.NewLine, _msPlLicense));
        }

        private static readonly string[] _msPlLicense = {
            @"This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.",
            @"1. Definitions",
            @"The terms ""reproduce,"" ""reproduction,"" ""derivative works,"" and ""distribution"" have the same meaning here as under U.S. copyright law.",
            @"A ""contribution"" is the original software, or any additions or changes to the software.",
            @"A ""contributor"" is any person that distributes its contribution under this license.",
            @"""Licensed patents"" are a contributor's patent claims that read directly on its contribution.",
            @"2. Grant of Rights",
            @"(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.",
            @"(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.",
            @"3. Conditions and Limitations",
            @"(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.",
            @"(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.",
            @"(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.",
            @"(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.",
            @"(E) The software is licensed ""as-is."" You bear the risk of using it. The contributors give no express warranties, guarantees or conditions.You may have additional consumer rights under your local laws which this license cannot change.To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.",
        };
    }
}
