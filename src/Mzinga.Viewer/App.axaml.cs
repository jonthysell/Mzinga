// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.IO;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

using Mzinga.Engine;

using Mzinga.Viewer.ViewModels;

namespace Mzinga.Viewer
{
    public class App : Application
    {
        public static AppViewModel AppVM
        {
            get
            {
                return AppViewModel.Instance;
            }
        }

        public EngineConfig InternalEngineConfig { get; private set; } = EngineConfig.GetDefaultEngineConfig(); // This should be the only place we load the config in the Viewer

        public string ViewerConfigPath { get; private set; }

        public static IStyle FluentLight { get; private set; }
        public static IStyle FluentDark { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            FluentLight = AvaloniaXamlLoader.Load(new Uri("avares://Avalonia.Themes.Fluent/FluentLight.xaml")) as IStyle;
            FluentDark = AvaloniaXamlLoader.Load(new Uri("avares://Avalonia.Themes.Fluent/FluentDark.xaml")) as IStyle;
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Startup += Desktop_Startup;
                desktop.Exit += Desktop_Exit;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void Desktop_Startup(object sender, ControlledApplicationLifetimeStartupEventArgs e)
        {
            MessageHandlers.RegisterMessageHandlers(this);

            ViewerConfigPath = e.Args.Length > 0 && !string.IsNullOrWhiteSpace(e.Args[0]) ? e.Args[0] : GetDefaultViewerConfigFileName();

            AppViewModelParameters parameters = new AppViewModelParameters()
            {
                ProgramTitle = AppInfo.Name,
                FullVersion = AppInfo.Version,
                ViewerConfig = LoadConfig(),
                DoOnUIThread = (action) => { Avalonia.Threading.Dispatcher.UIThread.Post(action); },
                TextToClipboard = TextToClipboard,
                InternalEngineConfig = InternalEngineConfig, // Should be the unmodified defaults
            };

            if (parameters.ViewerConfig.EngineType == EngineType.CommandLine)
            {
                parameters.EngineWrapper = new CLIEngineWrapper(parameters.ViewerConfig.EngineCommandLine);
            }

            AppViewModel.Init(parameters);
            DataContext = AppVM;

            Current.Styles[0] = AppVM.ViewerConfig.VisualTheme == VisualTheme.Dark ? FluentDark : FluentLight;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = new Views.MainWindow();
                desktop.MainWindow = window;
            }
        }

        private void Desktop_Exit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            try
            {
                SaveConfig();
                AppVM.EngineWrapper.StopEngine();
            }
            catch (Exception ex)
            {
                ExceptionUtils.HandleException(ex);
            }
            finally
            {
                MessageHandlers.UnregisterMessageHandlers(this);
            }
        }

        private ViewerConfig LoadConfig()
        {
            ViewerConfig viewerConfig = new ViewerConfig()
            {
                InternalEngineConfig = InternalEngineConfig.GetOptionsClone() // Create clone to store user values
            };

            try
            {
                using FileStream inputStream = new FileStream(ViewerConfigPath, FileMode.OpenOrCreate);
                viewerConfig.LoadConfig(inputStream);
            }
            catch (Exception) { }

            return viewerConfig;
        }

        private void SaveConfig()
        {
            try
            {
                using FileStream outputStream = new FileStream(ViewerConfigPath, FileMode.Create);
                AppVM.ViewerConfig.InternalEngineConfig.CopyOptionsFrom(InternalEngineConfig.GetOptionsClone()); // Repopulate with current engine values
                AppVM.ViewerConfig.SaveConfig(outputStream);
            }
            catch (Exception) { }
        }

        private void TextToClipboard(string text)
        {
            Clipboard.SetTextAsync(text);
        }

        private string GetDefaultViewerConfigFileName()
        {
#if WINSTORE
            // Only safe place is within the redirected AppData folder
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mzinga", ViewerConfigFileName);
#else
            return Path.Combine(AppInfo.IsMacOS ? AppContext.BaseDirectory : Environment.CurrentDirectory, ViewerConfigFileName);
#endif
        }


        private const string ViewerConfigFileName = "MzingaViewerConfig.xml";
    }
}
