// 
// AppViewModel.cs
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

using GalaSoft.MvvmLight;

using Mzinga.Engine;

namespace Mzinga.SharedUX.ViewModel
{
    public class AppViewModel : ViewModelBase
    {
        public static AppViewModel Instance { get; private set; }

        public string ProgramTitle { get; private set;  }

        public string FullVersion { get; private set;  }

        public ViewerConfig ViewerConfig { get; private set; }

        public DoOnUIThread DoOnUIThread { get; private set; }

        public TextToClipboard TextToClipboard { get; private set; }

        public EngineWrapper EngineWrapper { get; private set; }

        public GameEngineConfig InternalGameEngineConfig { get; private set; }

        public Exception EngineExceptionOnStart { get; private set; } = null;

        public static void Init(AppViewModelParameters parameters)
        {
            if (null != Instance)
            {
                throw new NotSupportedException();
            }

            Instance = new AppViewModel(parameters);
        }

        private AppViewModel(AppViewModelParameters parameters)
        {
            if (null == parameters)
            {
                throw new ArgumentNullException("parameters");
            }

            ProgramTitle = parameters.ProgramTitle;
            FullVersion = parameters.FullVersion;
            ViewerConfig = parameters.ViewerConfig ?? throw new ArgumentNullException("viewerConfig");
            DoOnUIThread = parameters.DoOnUIThread ?? throw new ArgumentNullException("doOnUIThread");
            TextToClipboard = parameters.TextToClipboard ?? throw new ArgumentNullException("textToClipboard");
            EngineWrapper = parameters.EngineWrapper;
            InternalGameEngineConfig = parameters.InternalGameEngineConfig;

            try
            {
                EngineWrapper?.StartEngine();
            }
            catch (Exception ex)
            {
                EngineWrapper?.StopEngine();
                EngineWrapper = null;
                EngineExceptionOnStart = ex;
            }

            if (null == EngineWrapper)
            {
                // No engine started, use an internal one
                EngineWrapper = new InternalEngineWrapper(ProgramTitle, InternalGameEngineConfig);
                EngineWrapper.StartEngine();
            }

            // Now that the engine is started, load user options from viewer config
            InternalGameEngineConfig.CopyOptionsFrom(ViewerConfig.InternalGameEngineConfig);
        }
    }

    public delegate void DoOnUIThread(Action action);
    public delegate void TextToClipboard(string text);

    public class AppViewModelParameters
    {
        public string ProgramTitle;
        public string FullVersion;
        public ViewerConfig ViewerConfig;
        public DoOnUIThread DoOnUIThread;
        public TextToClipboard TextToClipboard;
        public EngineWrapper EngineWrapper;
        public GameEngineConfig InternalGameEngineConfig;
    }
}
