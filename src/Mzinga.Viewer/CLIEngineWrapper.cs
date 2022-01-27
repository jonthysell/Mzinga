// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

using Mono.Unix.Native;

namespace Mzinga.Viewer
{
    public sealed class CLIEngineWrapper : EngineWrapper, IDisposable
    {
        private readonly Process _process;
        private StreamWriter _writer;

        public CLIEngineWrapper(string engineCommand) : base()
        {
            if (string.IsNullOrWhiteSpace(engineCommand))
            {
                throw new ArgumentNullException(nameof(engineCommand));
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

#if INSTALLED
            // Can't expect relative paths when packaged, so do nothing
#else
            _process.StartInfo.WorkingDirectory = AppInfo.IsMacOS && AppContext.BaseDirectory.EndsWith(".app/Contents/MacOS/") ? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../")) : AppInfo.EntryAssemblyPath;
#endif
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
                // Stop processing output
                _process.CancelOutputRead();

                // Try to cancel a running command
                if (!IsIdle)
                {
                    OnCancelCommand();
                }

                _writer.WriteLine("exit");
                _writer.Flush();

                _process.WaitForExit(WaitForExitTimeoutMS);
            }
            catch (Exception) { }
            finally
            {
                try
                {
                    _process.Kill(true);
                }
                catch (Exception) { }
                _process?.Close();
                _writer?.Close();
            }
        }

        protected override void OnEngineInput(string command)
        {
            _writer.WriteLine(command);
            _writer.Flush();
        }

        protected override void OnCancelCommand()
        {
            if (AppInfo.IsWindows)
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
            else if (AppInfo.IsLinux || AppInfo.IsMacOS)
            {
                if (Syscall.kill(_process.Id, Signum.SIGINT) != Stdlib.EXIT_SUCCESS)
                {
                    throw new Exception($"Cancel failed with error code {Stdlib.GetLastError()}.");
                }
            }
        }

        public void Dispose()
        {
            ((IDisposable)_process).Dispose();
            ((IDisposable)_writer)?.Dispose();
        }

        private const int WaitForExitTimeoutMS = 3 * 1000;
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
