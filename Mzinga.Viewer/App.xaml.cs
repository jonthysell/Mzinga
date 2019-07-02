// 
// App.xaml.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2017, 2018, 2019 Jon Thysell <http://jonthysell.com>
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
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

using Mzinga.Engine;

using Mzinga.SharedUX;
using Mzinga.SharedUX.ViewModel;

namespace Mzinga.Viewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
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

        public App(string configFile)
        {
            MessageHandlers.RegisterMessageHandlers(this);

#if PORTABLE
            ViewerConfigPath = !string.IsNullOrWhiteSpace(configFile) ? configFile : DefaultViewerConfigFileName;
#else
            ViewerConfigPath = !string.IsNullOrWhiteSpace(configFile) ? configFile : GetAppDataViewerConfigPath();
#endif

            AppViewModelParameters parameters = new AppViewModelParameters()
            {
                ProgramTitle = string.Format("{0} v{1}", Assembly.GetEntryAssembly().GetName().Name, Assembly.GetEntryAssembly().GetName().Version.ToString()),
                FullVersion = Assembly.GetEntryAssembly().GetName().Version.ToString(),
                ViewerConfig = LoadConfig(),
                DoOnUIThread = (action) => { Dispatcher.Invoke(action); },
                InternalGameEngineConfig = InternalGameEngineConfig, // Should be the unmodified defaults
            };

            if (parameters.ViewerConfig.EngineType == EngineType.CommandLine)
            {
                parameters.EngineWrapper = new CLIEngineWrapper(parameters.ViewerConfig.EngineCommandLine);
            }

            AppViewModel.Init(parameters);

            Exit += App_Exit;
        }

        private void App_Exit(object sender, ExitEventArgs e)
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

        private string GetAppDataViewerConfigPath()
        {
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            userFolder = Path.Combine(userFolder, "Mzinga");
            if (!Directory.Exists(userFolder))
            {
                Directory.CreateDirectory(userFolder);
            }

            return Path.Combine(userFolder, DefaultViewerConfigFileName);
        }

        private void SaveConfig()
        {
            using (FileStream outputStream = new FileStream(ViewerConfigPath, FileMode.Create))
            {
                AppVM.ViewerConfig.InternalGameEngineConfig.CopyOptionsFrom(InternalGameEngineConfig.GetOptionsClone()); // Repopulate with current engine values
                AppVM.ViewerConfig.SaveConfig(outputStream);
            }
        }

        private const string DefaultViewerConfigFileName = "Mzinga.Viewer.xml";
    }
}
