// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Mzinga.Engine;

namespace Mzinga.Viewer.ViewModels
{
    public class AppViewModel : ObservableObject
    {
        public static AppViewModel Instance { get; private set; }

        public string ProgramTitle { get; private set;  }

        public string FullVersion { get; private set;  }

        public ViewerConfig ViewerConfig { get; private set; }

        public DoOnUIThread DoOnUIThread { get; private set; }

        public TextToClipboard TextToClipboard { get; private set; }

        public EngineWrapper EngineWrapper { get; private set; }

        public EngineConfig InternalEngineConfig { get; private set; }

        public Exception EngineExceptionOnStart { get; private set; } = null;

        public MainViewModel MainVM { get; private set; }

        #region Help

        public RelayCommand ShowLicenses
        {
            get
            {
                return _showLicenses ??= new RelayCommand(() =>
                {
                    try
                    {
                        StrongReferenceMessenger.Default.Send(new ShowAboutMessage());
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand _showLicenses = null;

        public RelayCommand LaunchHiveWebsite
        {
            get
            {
                return _launchHiveWebsite ??= new RelayCommand(() =>
                {
                    try
                    {
                        StrongReferenceMessenger.Default.Send(new ConfirmationMessage("This will open the official Hive website in your browser. Do you want to continue?", (confirmed) =>
                        {
                            try
                            {
                                if (confirmed)
                                {
                                    StrongReferenceMessenger.Default.Send(new LaunchUrlMessage("https://gen42.com/games/hive"));
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtils.HandleException(ex);
                            }
                        }));
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand _launchHiveWebsite;

        public RelayCommand LaunchMzingaWebsite
        {
            get
            {
                return _launchMzingaWebsite ??= new RelayCommand(() =>
                {
                    try
                    {
                        StrongReferenceMessenger.Default.Send(new ConfirmationMessage("This will open the Mzinga website in your browser. Do you want to continue?", (confirmed) =>
                        {
                            try
                            {
                                if (confirmed)
                                {
                                    StrongReferenceMessenger.Default.Send(new LaunchUrlMessage("http://mzinga.jonthysell.com"));
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtils.HandleException(ex);
                            }
                        }));
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand _launchMzingaWebsite;

        public RelayCommand<string> LaunchURL
        {
            get
            {
                return _launchURL ??= new RelayCommand<string>((url) =>
                {
                    try
                    {
                        StrongReferenceMessenger.Default.Send(new ConfirmationMessage($"This will open \"{ url }\" in your browser. Do you want to continue?", (confirmed) =>
                        {
                            try
                            {
                                if (confirmed)
                                {
                                    StrongReferenceMessenger.Default.Send(new LaunchUrlMessage(url));
                                }
                            }
                            catch (Exception ex)
                            {
                                ExceptionUtils.HandleException(ex);
                            }
                        }));
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand<string> _launchURL;

#if UPDATES
        public static bool CheckForUpdatesEnabled => true;
#else
        public static bool CheckForUpdatesEnabled => false;
#endif
        public RelayCommand CheckForUpdatesAsync
        {
            get
            {
                return _checkForUpdatesAsync ??= new RelayCommand(async () =>
                {
                    try
                    {
                        MainVM.IsIdle = false;
                        await UpdateUtils.UpdateCheckAsync(true, true);
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                    finally
                    {
                        MainVM.IsIdle = true;
                    }
                });
            }
        }
        private RelayCommand _checkForUpdatesAsync;

#endregion

        public static void Init(AppViewModelParameters parameters)
        {
            if (Instance is not null)
            {
                throw new NotSupportedException();
            }

#pragma warning disable IDE0017 // Simplify object initialization
            Instance = new AppViewModel(parameters);
#pragma warning restore IDE0017 // Simplify object initialization
            Instance.MainVM = new MainViewModel();
        }

        private AppViewModel(AppViewModelParameters parameters)
        {
            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            ProgramTitle = parameters.ProgramTitle;
            FullVersion = parameters.FullVersion;
            ViewerConfig = parameters.ViewerConfig;
            DoOnUIThread = parameters.DoOnUIThread;
            TextToClipboard = parameters.TextToClipboard;
            InternalEngineConfig = parameters.InternalEngineConfig;

            if (ViewerConfig.EngineType == EngineType.CommandLine)
            {
                try
                {
                    EngineWrapper = new CLIEngineWrapper(ViewerConfig.EngineCommandLine);
                    EngineWrapper.StartEngine();
                }
                catch (Exception ex)
                {
                    EngineWrapper?.StopEngine();
                    EngineWrapper = null;
                    EngineExceptionOnStart = ex;
                }
            }

            if (EngineWrapper is null)
            {
                // No engine started, use an internal one
                EngineWrapper = new InternalEngineWrapper($"{ProgramTitle} v{FullVersion}", InternalEngineConfig);
                EngineWrapper.StartEngine();
            }

            // Now that the engine is started, load user options from viewer config
            InternalEngineConfig.CopyOptionsFrom(ViewerConfig.InternalEngineConfig);
        }
    }

    public delegate void DoOnUIThread(Action action);

    public delegate void TextToClipboard(string text);

    public class AppViewModelParameters
    {
        public string ProgramTitle;

        public string FullVersion;

        public ViewerConfig ViewerConfig
        {
            get
            {
                return _viewerConfig;
            }
            set
            {
                _viewerConfig = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        private ViewerConfig _viewerConfig;

        public DoOnUIThread DoOnUIThread
        {
            get
            {
                return _doOnUIThread;
            }
            set
            {
                _doOnUIThread = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        private DoOnUIThread _doOnUIThread;

        public TextToClipboard TextToClipboard
        {
            get
            {
                return _textToClipboard;
            }
            set
            {
                _textToClipboard = value ?? throw new ArgumentNullException(nameof(value));
            }
        }
        private TextToClipboard _textToClipboard;

        public EngineConfig InternalEngineConfig;
    }
}
