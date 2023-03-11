// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Platform.Storage.FileIO;

using CommunityToolkit.Mvvm.Messaging;

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
            StrongReferenceMessenger.Default.Register<ExceptionMessage>(recipient, async (recipient, message) => await ShowExceptionAsync(message));
            StrongReferenceMessenger.Default.Register<InformationMessage>(recipient, async (recipient, message) => await ShowInformationAsync(message));
            StrongReferenceMessenger.Default.Register<ConfirmationMessage>(recipient, async (recipient, message) => await ShowConfirmationAsync(message));
            StrongReferenceMessenger.Default.Register<LaunchUrlMessage>(recipient, async (recipient, message) => await LaunchUrlAsync(message));
            StrongReferenceMessenger.Default.Register<ShowAboutMessage>(recipient, async (recipient, message) => await ShowAboutAsync(message));
            StrongReferenceMessenger.Default.Register<NewGameMessage>(recipient, async (recipient, message) => await ShowNewGameAsync(message));
            StrongReferenceMessenger.Default.Register<LoadGameMessage>(recipient, async (recipient, message) => await ShowLoadGameAsync(message));
            StrongReferenceMessenger.Default.Register<SaveGameMessage>(recipient, async (recipient, message) => await ShowSaveGameAsync(message));
            StrongReferenceMessenger.Default.Register<GameMetadataMessage>(recipient, async (recipient, message) => await ShowGameMetadataAsync(message));
            StrongReferenceMessenger.Default.Register<ViewerConfigMessage>(recipient, async (recipient, message) => await ShowViewerConfigAsync(message));
            StrongReferenceMessenger.Default.Register<EngineOptionsMessage>(recipient, async (recipient, message) => await ShowEngineOptionsAsync(message));
            StrongReferenceMessenger.Default.Register<EngineConsoleMessage>(recipient, (recipient, message) => ShowEngineConsole(message));
        }

        public static void UnregisterMessageHandlers(object recipient)
        {
            StrongReferenceMessenger.Default.Unregister<ExceptionMessage>(recipient);
            StrongReferenceMessenger.Default.Unregister<InformationMessage>(recipient);
            StrongReferenceMessenger.Default.Unregister<ConfirmationMessage>(recipient);
            StrongReferenceMessenger.Default.Unregister<ShowAboutMessage>(recipient);
            StrongReferenceMessenger.Default.Unregister<LaunchUrlMessage>(recipient);
            StrongReferenceMessenger.Default.Unregister<NewGameMessage>(recipient);
            StrongReferenceMessenger.Default.Unregister<LoadGameMessage>(recipient);
            StrongReferenceMessenger.Default.Unregister<SaveGameMessage>(recipient);
            StrongReferenceMessenger.Default.Unregister<GameMetadataMessage>(recipient);
            StrongReferenceMessenger.Default.Unregister<ViewerConfigMessage>(recipient);
            StrongReferenceMessenger.Default.Unregister<EngineOptionsMessage>(recipient);
            StrongReferenceMessenger.Default.Unregister<EngineConsoleMessage>(recipient);
        }

        private static async Task ShowExceptionAsync(ExceptionMessage message)
        {
            try
            {
                Trace.TraceInformation($"ShowExceptionAsync: { message.ExceptionVM.Exception }");

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

        // Code dispatched to Avalonia's UI thread sometimes runs multiple times, which causes us to display multiple game over messages
        private static volatile InformationWindow _currentInformationWindow = null;

        private static async Task ShowInformationAsync(InformationMessage message)
        {
            try
            {
                Trace.TraceInformation($"ShowInformationAsync: { message.InformationVM.Message }");

                if (_currentInformationWindow is null)
                {
                    _currentInformationWindow = new InformationWindow
                    {
                        VM = message.InformationVM,
                    };

                    await _currentInformationWindow.ShowDialog(MainWindow);
                    _currentInformationWindow = null;
                }
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

        private static async Task ShowAboutAsync(ShowAboutMessage message)
        {
            try
            {
                var window = new AboutWindow()
                {
                    VM = message.AboutVM,
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
                var options = new FilePickerOpenOptions()
                {
                    AllowMultiple = false,
                    Title = "Open Game",
                    FileTypeFilter = GetFilters(true),
                };

                var files = await MainWindow.StorageProvider.OpenFilePickerAsync(options);

                if (files is not null && files.Count > 0 && files[0].CanOpenRead)
                {
                    using Stream inputStream = await files[0].OpenReadAsync();
                    gr = GameRecording.Load(inputStream, files[0].Path);
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
            Uri oldFileUri = message.GameRecording.FileUri;
            Uri newFileUri = null;

            try
            {
                var options = new FilePickerSaveOptions()
                {
                    Title = "Save Game",
                    DefaultExtension = ".pgn",
                    FileTypeChoices = GetFilters(false),
                    SuggestedStartLocation = oldFileUri is null ? null : await MainWindow.StorageProvider.TryGetFolderFromPath(oldFileUri),
                    SuggestedFileName = oldFileUri is null ? null : (await MainWindow.StorageProvider.TryGetFileFromPath(oldFileUri))?.Name,
                    ShowOverwritePrompt = true,
                };

                var file = await MainWindow.StorageProvider.SaveFilePickerAsync(options);

                if (file is not null && file.CanOpenWrite)
                {
                    using Stream outputStream = await file.OpenWriteAsync();
                    message.GameRecording.SavePGN(outputStream);
                    newFileUri = file.Path;
                }
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
            finally
            {
                message.Process(newFileUri);
            }
        }

        private static List<FilePickerFileType> GetFilters(bool open)
        {
            var filters = new List<FilePickerFileType>();

            if (open)
            {
                filters.Add(new FilePickerFileType("All Supported Files")
                {
                    Patterns = new List<string>() { "*.pgn", "*.sgf" }
                });
            }

            filters.Add(new FilePickerFileType("Portable Game Notation")
            {
                Patterns = new List<string>() { "*.pgn" }
            });

            if (open)
            {
                filters.Add(new FilePickerFileType("BoardSpace Smart Game Format")
                {
                    Patterns = new List<string>() { "*.sgf" }
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
