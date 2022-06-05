// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace Mzinga.Viewer.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        public static AppViewModel AppVM
        {
            get
            {
                return AppViewModel.Instance;
            }
        }

        public static string Title
        {
            get
            {
                return $"About {AppVM.ProgramTitle} v{AppVM.FullVersion}";
            }
        }

        public static ObservableCollection<ObservableAboutTabItem> TabItems { get; private set; }

        public RelayCommand Accept
        {
            get
            {
                return _accept ??= new RelayCommand(() =>
                {
                    try
                    {
                        RequestClose?.Invoke(this, null);
                    }
                    catch (Exception ex)
                    {
                        ExceptionUtils.HandleException(ex);
                    }
                });
            }
        }
        private RelayCommand _accept;

        public RelayCommand<string> LaunchURL
        {
            get
            {
                return _launchURL ??= new RelayCommand<string>((url) =>
                {
                    try
                    {
                        Messenger.Default.Send(new ConfirmationMessage("This will open a website in your browser. Do you want to continue?", (confirmed) =>
                        {
                            try
                            {
                                if (confirmed)
                                {
                                    Messenger.Default.Send(new LaunchUrlMessage(url));
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

        public event EventHandler RequestClose;

        public Action Callback { get; private set; }

        public AboutViewModel(Action callback = null)
        {
            Callback = callback;

            TabItems = new ObservableCollection<ObservableAboutTabItem>
            {
                new ObservableAboutTabItem("About", GetAboutText()),
                new ObservableAboutTabItem("Changelog", AssemblyUtils.GetEmbeddedMarkdownText<AboutViewModel>("CHANGELOG.md", true)),
                new ObservableAboutTabItem("Licenses", AssemblyUtils.GetEmbeddedMarkdownText<AboutViewModel>("Licenses.txt", true)),
            };
        }

        private static string GetAboutText()
        {
            return string.Join(Environment.NewLine + Environment.NewLine,
                "## Mzinga ##",
                "Mzinga is a collection of open-source software to play the board game [Hive](https://gen42.com/games/hive), with the primary goal of building a community of developers who create Hive-playing AIs.",
                "To that end, Mzinga proposes a [Universal Hive Protocol](https://github.com/jonthysell/Mzinga/wiki/UniversalHiveProtocol) to support interoperability for Hive-playing software.",
                "For more information on Mzinga and its projects, please check out the [Mzinga Wiki](https://github.com/jonthysell/Mzinga/wiki).",
                "## MzingaViewer ##",
                "MzingaViewer is a graphical application which can drive any engine that implements the specifications of the Universal Hive Protocol.",
                "MzingaViewer is not meant to be graphically impressive or compete with commercial versions of Hive, but rather be a ready-made UI for developers who'd rather focus their time on building a compatible engine and AI.");
        }

        public void ProcessClose()
        {
            Callback?.Invoke();
        }
    }
}
