// 
// App.axaml.cs
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

using System;
using System.ComponentModel;
using System.IO;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

using Mzinga.Engine;

using Mzinga.SharedUX;
using Mzinga.SharedUX.ViewModel;

namespace Mzinga.Viewer
{
    public class App : Application
    {
        public AppViewModel AppVM
        {
            get
            {
                return AppViewModel.Instance;
            }
        }

        public GameEngineConfig InternalGameEngineConfig { get; private set; } = GameEngineConfig.GetDefaultEngineConfig(); // This should be the only place we load the config in the Viewer

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

            ViewerConfigPath = e.Args.Length > 0 && !string.IsNullOrWhiteSpace(e.Args[0]) ? e.Args[0] : DefaultViewerConfigFileName;

            AppViewModelParameters parameters = new AppViewModelParameters()
            {
                ProgramTitle = AppInfo.Name,
                FullVersion = AppInfo.Version,
                ViewerConfig = LoadConfig(),
                DoOnUIThread = (action) => { Avalonia.Threading.Dispatcher.UIThread.Post(action); },
                TextToClipboard = TextToClipboard,
                InternalGameEngineConfig = InternalGameEngineConfig, // Should be the unmodified defaults
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
            using (FileStream inputStream = new FileStream(ViewerConfigPath, FileMode.OpenOrCreate))
            {
                ViewerConfig viewerConfig = new ViewerConfig();
                viewerConfig.InternalGameEngineConfig = InternalGameEngineConfig.GetOptionsClone(); // Create clone to store user values

                try
                {
                    viewerConfig.LoadConfig(inputStream);
                }
                catch (Exception) { }

                return viewerConfig;
            }
        }

        private void SaveConfig()
        {
            using (FileStream outputStream = new FileStream(ViewerConfigPath, FileMode.Create))
            {
                AppVM.ViewerConfig.InternalGameEngineConfig.CopyOptionsFrom(InternalGameEngineConfig.GetOptionsClone()); // Repopulate with current engine values
                AppVM.ViewerConfig.SaveConfig(outputStream);
            }
        }

        private void TextToClipboard(string text)
        {
            Clipboard.SetTextAsync(text);
        }

        private const string DefaultViewerConfigFileName = "MzingaViewerConfig.xml";
    }
}
