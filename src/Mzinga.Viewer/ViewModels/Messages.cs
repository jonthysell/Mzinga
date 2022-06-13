// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using GalaSoft.MvvmLight.Messaging;

namespace Mzinga.Viewer.ViewModels
{
    public class ExceptionMessage : MessageBase
    {
        public ExceptionViewModel ExceptionVM { get; private set; }

        public ExceptionMessage(Exception exception) : base()
        {
            ExceptionVM = new ExceptionViewModel(exception);
        }
    }

    public class InformationMessage : MessageBase
    {
        public InformationViewModel InformationVM { get; private set; }

        public InformationMessage(string message, string title = "Mzinga", string details = null, Action callback = null) : base()
        {
            InformationVM = new InformationViewModel(message, title, details, callback);
        }

        public void Process()
        {
            InformationVM.ProcessClose();
        }
    }

    public class ConfirmationMessage : MessageBase
    {
        public ConfirmationViewModel ConfirmationVM { get; private set; }

        private readonly Action<bool> Callback;

        public ConfirmationMessage(string message, Action<bool> callback) : base()
        {
            ConfirmationVM = new ConfirmationViewModel(message);
            Callback = callback;
        }

        public ConfirmationMessage(string message, string details, Action<bool> callback) : base()
        {
            ConfirmationVM = new ConfirmationViewModel(message, details);
            Callback = callback;
        }

        public void Process(bool confirmation)
        {
            Callback(confirmation);
        }
    }

    public class LaunchUrlMessage : MessageBase
    {
        public string Url { get; private set; }

        public LaunchUrlMessage(string url) : base()
        {
            Url = url;
        }
    }

    public class ShowAboutMessage : MessageBase
    {
        public AboutViewModel AboutVM { get; private set; }

        public ShowAboutMessage(Action callback = null) : base()
        {
            AboutVM = new AboutViewModel(callback);
        }

        public void Process()
        {
            AboutVM.ProcessClose();
        }
    }

    public class NewGameMessage : MessageBase
    {
        public NewGameViewModel NewGameVM { get; private set; }

        public NewGameMessage(GameSettings settings, bool enableGameType, Action<GameSettings> callback) : base()
        {
            NewGameVM = new NewGameViewModel(settings, enableGameType, callback);
        }

        public void Process()
        {
            NewGameVM.ProcessClose();
        }
    }

    public class LoadGameMessage : MessageBase
    {
        private readonly Action<GameRecording> Callback;

        public LoadGameMessage(Action<GameRecording> callback = null) : base()
        {
            Callback = callback;
        }

        public void Process(GameRecording gameRecording)
        {
            Callback?.Invoke(gameRecording);
        }
    }

    public class SaveGameMessage : MessageBase
    {
        private readonly Action<string> Callback;

        public GameRecording GameRecording { get; private set; }

        public SaveGameMessage(GameRecording gameRecording, Action<string> callback = null) : base()
        {
            GameRecording = gameRecording;
            Callback = callback;
        }

        public void Process(string fileName)
        {
            Callback?.Invoke(fileName);
        }
    }

    public class GameMetadataMessage : MessageBase
    {
        public GameMetadataViewModel GameMetadataVM { get; private set; }

        public GameMetadataMessage(GameMetadata metadata = null, Action<GameMetadata> callback = null) : base()
        {
            GameMetadataVM = new GameMetadataViewModel(metadata, callback);
        }

        public void Process()
        {
            GameMetadataVM.ProcessClose();
        }
    }

    public class ViewerConfigMessage : MessageBase
    {
        public ViewerConfigViewModel ViewerConfigVM { get; private set; }

        public ViewerConfigMessage(ViewerConfig config, Action<ViewerConfig> callback = null) : base()
        {
            ViewerConfigVM = new ViewerConfigViewModel(config, callback);
        }

        public void Process()
        {
            ViewerConfigVM.ProcessClose();
        }
    }

    public class EngineOptionsMessage : MessageBase
    {
        public EngineOptionsViewModel EngineOptionsVM { get; private set; }

        public EngineOptionsMessage(EngineOptions options = null, Action<IDictionary<string, string>> callback = null) : base()
        {
            EngineOptionsVM = new EngineOptionsViewModel(options, callback);
        }

        public void Process()
        {
            EngineOptionsVM.ProcessClose();
        }
    }

    public class EngineConsoleMessage : MessageBase
    {
        public EngineConsoleViewModel EngineConsoleVM { get; private set; }

        public EngineConsoleMessage() : base()
        {
            EngineConsoleVM = EngineConsoleViewModel.Instance;
        }
    }
}
