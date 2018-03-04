// 
// CLIEngineWrapper.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2018 Jon Thysell <http://jonthysell.com>
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
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Mzinga.Viewer.ViewModel
{
    public class CLIEngineWrapper : EngineWrapper
    {
        private Process _process;
        private StreamWriter _writer;

        public CLIEngineWrapper(string engineCommand) : base()
        {
            if (string.IsNullOrWhiteSpace("engineCommand"))
            {
                throw new ArgumentNullException("engineCommand");
            }

            _process = new Process();
            _process.StartInfo.FileName = engineCommand;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            _process.OutputDataReceived += (sender, e) =>
            {
                OnEngineOutput(e.Data);
            };
        }

        public override void StartEngine()
        {
            IsIdle = false;

            _process.Start();
            _process.BeginOutputReadLine();

            _writer = _process.StandardInput;
        }

        public override void StopEngine()
        {
            _process.CancelOutputRead();
            _writer.WriteLine("exit");
            _process.WaitForExit();
            _process.Close();
            _process = null;
        }

        protected override void OnEngineInput(string command)
        {
            _writer.WriteLine(command);
            _writer.Flush();
        }
    }
}
