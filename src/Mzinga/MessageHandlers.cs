// 
// MessageHandlers.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017, 2018, 2019, 2021 Jon Thysell <http://jonthysell.com>
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

using GalaSoft.MvvmLight.Messaging;

using Mzinga.SharedUX;
using Mzinga.SharedUX.ViewModel;

namespace Mzinga
{
    public class MessageHandlers
    {
        public static Window MainWindow => (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;

        public static void RegisterMessageHandlers(object recipient)
        {
            Messenger.Default.Register<ExceptionMessage>(recipient, (message) => ShowException(message));
            Messenger.Default.Register<InformationMessage>(recipient, (message) => ShowInformation(message));
            Messenger.Default.Register<ConfirmationMessage>(recipient, (message) => ShowConfirmation(message));
            Messenger.Default.Register<LaunchUrlMessage>(recipient, async (message) => await LaunchUrlAsync(message));
            Messenger.Default.Register<ShowLicensesMessage>(recipient, (message) => ShowLicense(message));
            Messenger.Default.Register<NewGameMessage>(recipient, async (message) => await ShowNewGameAsync(message));
            Messenger.Default.Register<LoadGameMessage>(recipient, async (message) => await ShowLoadGameAsync(message));
            Messenger.Default.Register<SaveGameMessage>(recipient, async (message) => await ShowSaveGameAsync(message));
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
            //ExceptionWindow window = new ExceptionWindow
            //{
            //    DataContext = message.ExceptionVM,
            //    Owner = Application.Current.MainWindow,
            //};
            //message.ExceptionVM.RequestClose += (sender, e) =>
            //{
            //    window.Close();
            //};
            //window.ShowDialog();
        }

        private static void ShowInformation(InformationMessage message)
        {
            try
            {
                //InformationWindow window = new InformationWindow
                //{
                //    DataContext = message.InformationVM,
                //    Owner = Application.Current.MainWindow,
                //};
                //message.InformationVM.RequestClose += (sender, e) =>
                //{
                //    window.Close();
                //};
                //window.Closed += (sender, args) =>
                //{
                //    message.Process();
                //};
                //window.Show();
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
                //MessageBoxResult result = MessageBox.Show(message.Message, "Mzinga", MessageBoxButton.YesNo);
                //message.Process(result == MessageBoxResult.Yes);
                message.Process(true);
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
        }

        private static async Task LaunchUrlAsync(LaunchUrlMessage message)
        {
            string url = message.Url;

            await Task.Run(() =>
            {
                try
                {
                    if (AppInfo.IsWindows)
                    {
                        Process.Start(new ProcessStartInfo(url)
                        {
                            UseShellExecute = true
                        });
                    }
                    else if (AppInfo.IsMacOS)
                    {
                        Process.Start("open", url);
                    }
                    else if (AppInfo.IsLinux)
                    {
                        Process.Start("xdg-open", url);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
            });
        }

        private static void ShowLicense(ShowLicensesMessage message)
        {
            //LicensesWindow window = new LicensesWindow()
            //{
            //    DataContext = message.LicensesVM,
            //    Owner = Application.Current.MainWindow
            //};

            //message.LicensesVM.Licenses.Add(GetExtendedWpfToolkitLicense());
            //message.LicensesVM.RequestClose += () =>
            //{
            //    window.Close();
            //};
            //window.ShowDialog();
            message.Process();
        }

        private static async Task ShowNewGameAsync(NewGameMessage message)
        {
            await Task.Run(() =>
            {
                try
                {
                    //NewGameWindow window = new NewGameWindow
                    //{
                    //    DataContext = message.NewGameVM,
                    //    Owner = Application.Current.MainWindow,
                    //};
                    //message.NewGameVM.RequestClose += (sender, e) =>
                    //{
                    //    window.Close();
                    //};
                    //window.ShowDialog();
                    message.NewGameVM.Accept.Execute(null);
                    message.Process();
                }
                catch (Exception ex)
                {
                    ExceptionUtils.HandleException(ex);
                }
            });
        }

        private static async Task ShowLoadGameAsync(LoadGameMessage message)
        {
            GameRecording gr = null;
            try
            {
                OpenFileDialog dialog = new OpenFileDialog
                {
                    AllowMultiple = false,
                    Title = "Open Game",
                    Filters = GetFilters(true),
                };

                string[] filenames = await dialog.ShowAsync(MainWindow);

                if (null != filenames && filenames.Length > 0 && !string.IsNullOrWhiteSpace(filenames[0]))
                {
                    string fileName = filenames[0].Trim();
                    using (Stream inputStream = File.OpenRead(fileName))
                    {
                        gr = Path.GetExtension(fileName).ToLower() == ".sgf" ? GameRecording.LoadSGF(inputStream, fileName) : GameRecording.LoadPGN(inputStream, fileName);
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

        private static async Task ShowSaveGameAsync(SaveGameMessage message)
        {
            string fileName = null;
            try
            {
                SaveFileDialog dialog = new SaveFileDialog
                {
                    Title = "Save Game",
                    DefaultExtension = ".pgn",
                    Filters = GetFilters(false),
                    Directory = !string.IsNullOrEmpty(message.GameRecording.FileName) ? Path.GetDirectoryName(message.GameRecording.FileName) : null,
                    InitialFileName = !string.IsNullOrEmpty(message.GameRecording.FileName) ? Path.GetFileName(message.GameRecording.FileName) : null,
                };

                string result = await dialog.ShowAsync(MainWindow);

                fileName = result?.Trim();

                if (!string.IsNullOrEmpty(fileName))
                {
                    using (Stream outputStream = File.OpenWrite(fileName))
                    {
                        message.GameRecording.SavePGN(outputStream);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
            finally
            {
                message.Process(fileName);
            }
        }

        private static List<FileDialogFilter> GetFilters(bool open)
        {
            var filters = new List<FileDialogFilter>();

            if (open)
            {
                filters.Add(new FileDialogFilter()
                {
                    Name = "All Supported Files",
                    Extensions = new List<string>() { "pgn", "sgf" }
                });
            }

            filters.Add(new FileDialogFilter()
            {
                Name = "Portable Game Notation",
                Extensions = new List<string>() { "pgn" }
            });

            if (open)
            {
                filters.Add(new FileDialogFilter()
                {
                    Name = "BoardSpace Smart Game Format",
                    Extensions = new List<string>() { "sgf" }
                });
            }

            return filters;
        }

        private static void ShowGameMetadata(GameMetadataMessage message)
        {
            try
            {
                //GameMetadataWindow window = new GameMetadataWindow
                //{
                //    DataContext = message.GameMetadataVM,
                //    Owner = Application.Current.MainWindow,
                //};
                //message.GameMetadataVM.RequestClose += (sender, e) =>
                //{
                //    window.Close();
                //};
                //window.ShowDialog();
                //message.Process();
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
                //ViewerConfigWindow window = new ViewerConfigWindow
                //{
                //    DataContext = message.ViewerConfigVM,
                //    Owner = Application.Current.MainWindow,
                //};
                //message.ViewerConfigVM.RequestClose += (sender, e) =>
                //{
                //    window.Close();
                //};
                //window.ShowDialog();
                //message.Process();
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
                //if (message.EngineOptionsVM.Options.Count == 0)
                //{
                //    throw new Exception("This engine exposes no options to set.");
                //}

                //EngineOptionsWindow window = new EngineOptionsWindow
                //{
                //    DataContext = message.EngineOptionsVM,
                //    Owner = Application.Current.MainWindow,
                //};
                //message.EngineOptionsVM.RequestClose += (sender, e) =>
                //{
                //    window.Close();
                //};
                //window.ShowDialog();
                //message.Process();
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
                //EngineConsoleWindow window = EngineConsoleWindow.Instance;

                //window.Show();

                //if (!window.IsActive)
                //{
                //    window.Activate();
                //}
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
        }
    }
}
