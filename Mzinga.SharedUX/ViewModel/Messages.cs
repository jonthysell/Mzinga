// 
// Messages.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017, 2018 Jon Thysell <http://jonthysell.com>
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

using GalaSoft.MvvmLight.Messaging;

namespace Mzinga.SharedUX.ViewModel
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

        public InformationMessage(string message, string title = "Mzinga", Action callback = null) : base()
        {
            InformationVM = new InformationViewModel(message, title, callback);
        }

        public void Process()
        {
            InformationVM.ProcessClose();
        }
    }

    public class ConfirmationMessage : MessageBase
    {
        public string Message { get; private set; }

        private readonly Action<bool> Callback;

        public ConfirmationMessage(string message, Action<bool> callback) : base()
        {
            Message = message;
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

    public class NewGameMessage : MessageBase
    {
        public NewGameViewModel NewGameVM { get; private set; }

        public NewGameMessage(GameSettings settings = null, Action<GameSettings> callback = null) : base()
        {
            NewGameVM = new NewGameViewModel(settings, callback);
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
        public GameRecording GameRecording { get; private set; }

        public SaveGameMessage(GameRecording gameRecording) : base()
        {
            GameRecording = gameRecording;
        }
    }

    public class ViewerConfigMessage : MessageBase
    {
        public ViewerConfigViewModel ViewerConfigVM { get; private set; }

        public ViewerConfigMessage(ViewerConfig config = null, Action<ViewerConfig> callback = null) : base()
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
        public EngineConsoleMessage() : base() { }
    }
}
