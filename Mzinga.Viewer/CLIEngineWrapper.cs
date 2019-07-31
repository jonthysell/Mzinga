// 
// CLIEngineWrapper.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2018, 2019 Jon Thysell <http://jonthysell.com>
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
using System.Runtime.InteropServices;
using System.Text;

using Mzinga.SharedUX;

namespace Mzinga.Viewer
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

            string programName = "";
            string arguments = "";

            if (engineCommand.StartsWith("\""))
            {
                // Program is in quotes
                int endQuoteIndex = engineCommand.IndexOf('"', 1);
                programName = engineCommand.Substring(0, endQuoteIndex).Trim('"').Trim();
                arguments = endQuoteIndex + 1 < engineCommand.Length ? engineCommand.Substring(endQuoteIndex + 1).Trim() : "";
            }
            else
            {
                programName = engineCommand;

                int firstSpaceIndex = engineCommand.IndexOf(' ');
                if (firstSpaceIndex > 0)
                {
                    programName = engineCommand.Substring(0, firstSpaceIndex).Trim();
                    arguments = firstSpaceIndex + 1 < engineCommand.Length ? engineCommand.Substring(firstSpaceIndex + 1).Trim() : "";
                }
            }

            _process = new Process();

            _process.StartInfo.FileName = programName;
            _process.StartInfo.Arguments = arguments;

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
            try
            {
                _process.CancelOutputRead();
                _writer.WriteLine("exit");
                _process.WaitForExit(WaitForExitTimeoutMS);
                _process.Close();
            }
            catch (Exception) { }

            _process = null;
            _writer = null;
        }

        protected override void OnEngineInput(string command)
        {
            _writer.WriteLine(command);
            _writer.Flush();
        }

        protected override void OnCancelCommand()
        {
            if (NativeMethods.AttachConsole((uint)_process.Id))
            {
                NativeMethods.SetConsoleCtrlHandler(null, true);
                NativeMethods.GenerateConsoleCtrlEvent(NativeMethods.CtrlTypes.CTRL_C_EVENT, 0);

                NativeMethods.FreeConsole();

                _process.WaitForExit(WaitForCancelTimeoutMS);

                NativeMethods.SetConsoleCtrlHandler(null, false);
            }
        }

        private const int WaitForExitTimeoutMS = 10 * 1000;
        private const int WaitForCancelTimeoutMS = 500;
    }

    // Adapted from https://stackoverflow.com/questions/813086/can-i-send-a-ctrl-c-sigint-to-an-application-on-windows
    internal static partial class NativeMethods
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        internal static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        internal static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);

        internal delegate bool ConsoleCtrlDelegate(CtrlTypes CtrlType);

        // Enumerated type for the control messages sent to the handler routine
        internal enum CtrlTypes : uint
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GenerateConsoleCtrlEvent(CtrlTypes dwCtrlEvent, uint dwProcessGroupId);
    }
}
