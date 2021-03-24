﻿// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

using GalaSoft.MvvmLight.Messaging;

using Mzinga.Viewer;
using Mzinga.Viewer.ViewModels;

using Mzinga.Viewer.Views;

namespace Mzinga.Viewer
{
    public class MessageHandlers
    {
        public static Window MainWindow => (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;

        public static void RegisterMessageHandlers(object recipient)
        {
            Messenger.Default.Register<ExceptionMessage>(recipient, async (message) => await ShowExceptionAsync(message));
            Messenger.Default.Register<InformationMessage>(recipient, async (message) => await ShowInformationAsync(message));
            Messenger.Default.Register<ConfirmationMessage>(recipient, async (message) => await ShowConfirmationAsync(message));
            Messenger.Default.Register<LaunchUrlMessage>(recipient, async (message) => await LaunchUrlAsync(message));
            Messenger.Default.Register<ShowLicensesMessage>(recipient, async (message) => await ShowLicenseAsync(message));
            Messenger.Default.Register<NewGameMessage>(recipient, async (message) => await ShowNewGameAsync(message));
            Messenger.Default.Register<LoadGameMessage>(recipient, async (message) => await ShowLoadGameAsync(message));
            Messenger.Default.Register<SaveGameMessage>(recipient, async (message) => await ShowSaveGameAsync(message));
            Messenger.Default.Register<GameMetadataMessage>(recipient, async (message) => await ShowGameMetadataAsync(message));
            Messenger.Default.Register<ViewerConfigMessage>(recipient, async (message) => await ShowViewerConfigAsync(message));
            Messenger.Default.Register<EngineOptionsMessage>(recipient, async (message) => await ShowEngineOptionsAsync(message));
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

        private static async Task ShowExceptionAsync(ExceptionMessage message)
        {
            try
            {
                InformationWindow window = new InformationWindow
                {
                    VM = message.ExceptionVM,
                };

                await window.ShowDialog(MainWindow);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

        private static async Task ShowInformationAsync(InformationMessage message)
        {
            try
            {
                InformationWindow window = new InformationWindow
                {
                   VM = message.InformationVM,
                };

                await window.ShowDialog(MainWindow);
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
            finally
            {
                message.Process();
            }
        }

        private static async Task ShowConfirmationAsync(ConfirmationMessage message)
        {
            bool confirmation = false;
            try
            {
                var window = new ConfirmationWindow()
                {
                    VM = message.ConfirmationVM,
                };

                await window.ShowDialog(MainWindow);
                confirmation = message.ConfirmationVM.Result == ConfirmationResult.Yes;
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
            finally
            {
                message.Process(confirmation);
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

        private static async Task ShowLicenseAsync(ShowLicensesMessage message)
        {
            try
            {
                var window = new LicensesWindow()
                {
                    VM = message.LicensesVM,
                };

                await window.ShowDialog(MainWindow);
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
            finally
            {
                message.Process();
            }
        }

        private static async Task ShowNewGameAsync(NewGameMessage message)
        {
            try
            {
                var window = new NewGameWindow
                {
                    VM = message.NewGameVM,
                };

                await window.ShowDialog(MainWindow);
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
            finally
            {
                message.Process();
            }
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

                if (filenames is not null && filenames.Length > 0 && !string.IsNullOrWhiteSpace(filenames[0]))
                {
                    string fileName = filenames[0].Trim();
                    using Stream inputStream = File.OpenRead(fileName);
                    gr = Path.GetExtension(fileName).ToLower() == ".sgf" ? GameRecording.LoadSGF(inputStream, fileName) : GameRecording.LoadPGN(inputStream, fileName);
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
                    using Stream outputStream = File.OpenWrite(fileName);
                    message.GameRecording.SavePGN(outputStream);
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

        private static async Task ShowGameMetadataAsync(GameMetadataMessage message)
        {
            try
            {
                var window = new GameMetadataWindow
                {
                    VM = message.GameMetadataVM,
                };

                await window.ShowDialog(MainWindow);
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
            finally
            {
                message.Process();
            }
        }

        private static async Task ShowViewerConfigAsync(ViewerConfigMessage message)
        {
            try
            {
                var window = new ViewerConfigWindow
                {
                    VM = message.ViewerConfigVM,
                };

                await window.ShowDialog(MainWindow);
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
            finally
            {
                message.Process();
            }
        }

        private static async Task ShowEngineOptionsAsync(EngineOptionsMessage message)
        {
            try
            {
                if (message.EngineOptionsVM.Options.Count == 0)
                {
                    throw new Exception("This engine exposes no options to set.");
                }

                var window = new EngineOptionsWindow
                {
                    VM = message.EngineOptionsVM,
                };

                await window.ShowDialog(MainWindow);
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
            finally
            {
                message.Process();
            }
        }

        private static void ShowEngineConsole(EngineConsoleMessage message)
        {
            try
            {
                var window = EngineConsoleWindow.Instance ??= new EngineConsoleWindow()
                {
                    VM = message.EngineConsoleVM,
                };

                window.Show(MainWindow);
                window.Activate();
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
        }
    }
}
