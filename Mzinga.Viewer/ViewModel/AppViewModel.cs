// 
// AppViewModel.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015 Jon Thysell <http://jonthysell.com>
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
using System.Reflection;

using GalaSoft.MvvmLight;

namespace Mzinga.Viewer.ViewModel
{
    public delegate void DoOnUIThread(Action action);

    public class AppViewModel : ViewModelBase
    {
        public static AppViewModel Instance { get; private set; }

        public string ProgramTitle
        {
            get
            {
                AssemblyName name = Assembly.GetEntryAssembly().GetName();
                return String.Format("{0} v{1}", name.Name, name.Version.ToString());
            }
        }

        public DoOnUIThread DoOnUIThread { get; private set; }

        public EngineWrapper EngineWrapper { get; private set; }

        public static void Init(DoOnUIThread doOnUIThread)
        {
            if (null != Instance)
            {
                throw new NotSupportedException();
            }

            Instance = new AppViewModel(doOnUIThread);
        }

        private AppViewModel(DoOnUIThread doOnUIThread)
        {
            if (null == doOnUIThread)
            {
                throw new ArgumentNullException("doOnUIThread");
            }

            DoOnUIThread = doOnUIThread;

            EngineWrapper = new EngineWrapper("Mzinga.Engine.exe");
        }
    }
}
