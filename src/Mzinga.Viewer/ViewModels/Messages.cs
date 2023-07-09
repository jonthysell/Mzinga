// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using Mzinga.Core;

namespace Mzinga.Viewer.ViewModels
{
    public class ExceptionMessage
    {
        public ExceptionViewModel ExceptionVM { get; private set; }

        public ExceptionMessage(Exception exception)
        {
            ExceptionVM = new ExceptionViewModel(exception);
        }
    }

    public class InformationMessage
    {
        public InformationViewModel InformationVM { get; private set; }

        public InformationMessage(string message, string title = "Mzinga", string details = null, Action callback = null)
        {
            InformationVM = new InformationViewModel(message, title, details, callback);
        }

        public void Process()
        {
            InformationVM.ProcessClose();
        }
    }

    public class ConfirmationMessage
    {
        public ConfirmationViewModel ConfirmationVM { get; private set; }

        private readonly Action<bool> Callback;

        public ConfirmationMessage(string message, Action<bool> callback)
        {
            ConfirmationVM = new ConfirmationViewModel(message);
            Callback = callback;
        }

        public ConfirmationMessage(string message, string details, Action<bool> callback)
        {
            ConfirmationVM = new ConfirmationViewModel(message, details);
            Callback = callback;
        }

        public void Process(bool confirmation)
        {
            Callback(confirmation);
        }
    }

    public class LaunchUrlMessage
    {
        public string Url { get; private set; }

        public LaunchUrlMessage(string url)
        {
            Url = url;
        }
    }

    public class ShowAboutMessage
    {
        public AboutViewModel AboutVM { get; private set; }

        public ShowAboutMessage(Action callback = null)
        {
            AboutVM = new AboutViewModel(callback);
        }

        public void Process()
        {
            AboutVM.ProcessClose();
        }
    }

    public class NewGameMessage
    {
        public NewGameViewModel NewGameVM { get; private set; }

        public NewGameMessage(GameSettings settings, bool enableGameType, Action<GameSettings> callback)
        {
            NewGameVM = new NewGameViewModel(settings, enableGameType, callback);
        }

        public void Process()
        {
            NewGameVM.ProcessClose();
        }
    }

    public class LoadGameMessage
    {
        private readonly Action<GameRecording> Callback;

        public LoadGameMessage(Action<GameRecording> callback = null)
        {
            Callback = callback;
        }

        public void Process(GameRecording gameRecording)
        {
            Callback?.Invoke(gameRecording);
        }
    }

    public class SaveGameMessage
    {
        private readonly Action<Uri> Callback;

        public GameRecording GameRecording { get; private set; }

        public SaveGameMessage(GameRecording gameRecording, Action<Uri> callback = null)
        {
            GameRecording = gameRecording;
            Callback = callback;
        }

        public void Process(Uri fileUri)
        {
            Callback?.Invoke(fileUri);
        }
    }

    public class GameMetadataMessage
    {
        public GameMetadataViewModel GameMetadataVM { get; private set; }

        public GameMetadataMessage(GameMetadata metadata = null, Action<GameMetadata> callback = null)
        {
            GameMetadataVM = new GameMetadataViewModel(metadata, callback);
        }

        public void Process()
        {
            GameMetadataVM.ProcessClose();
        }
    }

    public class ViewerConfigMessage
    {
        public ViewerConfigViewModel ViewerConfigVM { get; private set; }

        public ViewerConfigMessage(ViewerConfig config, Action<ViewerConfig> callback = null)
        {
            ViewerConfigVM = new ViewerConfigViewModel(config, callback);
        }

        public void Process()
        {
            ViewerConfigVM.ProcessClose();
        }
    }

    public class EngineOptionsMessage
    {
        public EngineOptionsViewModel EngineOptionsVM { get; private set; }

        public EngineOptionsMessage(EngineOptions options = null, Action<IDictionary<string, string>> callback = null)
        {
            EngineOptionsVM = new EngineOptionsViewModel(options, callback);
        }

        public void Process()
        {
            EngineOptionsVM.ProcessClose();
        }
    }

    public class EngineConsoleMessage
    {
        public EngineConsoleViewModel EngineConsoleVM { get; private set; }

        public EngineConsoleMessage()
        {
            EngineConsoleVM = EngineConsoleViewModel.Instance;
        }
    }
}
