﻿// 
// Program.cs
//  
// Author:
//       Jon Thysell <thysell@gmail.com>
// 
// Copyright (c) 2015, 2016, 2017, 2018, 2019, 2021 Jon Thysell <http://jonthysell.com>
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
using System.Text;

namespace Mzinga.Engine
{
    public class Program
    {
        static string ID => $"{AppInfo.Name} v{AppInfo.Version}";

        private static GameEngine _engine;

        private static volatile bool _interceptCancel = false;

        private static SigIntMonitor _sigIntMonitor;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            GameEngineConfig config = LoadConfig(null != args && args.Length > 0 ? args[0] : null);

            _engine = new GameEngine(ID, config, PrintLine);
            _engine.ParseCommand("info");

            if (AppInfo.IsWindows)
            {
                Console.CancelKeyPress += Console_CancelKeyPress;
            }
            else if (AppInfo.IsLinux || AppInfo.IsMacOS)
            {
                _sigIntMonitor = SigIntMonitor.CreateAndStart(SigIntMonitor_SigIntReceived);
            }

            _engine.StartAsyncCommand += (s, e) =>
            {
                _interceptCancel = true;
            };

            _engine.EndAsyncCommand += (s, e) =>
            {
                _interceptCancel = false;
            };

            while (!_engine.ExitRequested)
            {
                string command = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(command))
                {
                    _engine.ParseCommand(command);
                }
            }

            _sigIntMonitor?.Stop();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (_interceptCancel)
            {
                _engine.TryCancelAsyncCommand();
                e.Cancel = true;
            }
        }

        private static void SigIntMonitor_SigIntReceived(object sender, EventArgs e)
        {
            if (_interceptCancel)
            {
                _engine.TryCancelAsyncCommand();
            }
            else
            {
                _sigIntMonitor?.Stop();
                Environment.Exit(CtrlCExitCode);
            }
        }

        private const int CtrlCExitCode = 130;

        static void PrintLine(string format, params object[] arg)
        {
            Console.Out.WriteLine(format, arg);
        }

        static GameEngineConfig LoadConfig(string configPath)
        {

            // Try loading specified file
            if (!TryLoadConfig(configPath, out GameEngineConfig result))
            {
                // Try loading default file
                if (!TryLoadConfig(DefaultEngineConfigFileName, out result))
                {
                    // Load default from embedded resource
                    result = GameEngineConfig.GetDefaultEngineConfig();
                }
            }

            return result;
        }

        private static bool TryLoadConfig(string configPath, out GameEngineConfig result)
        {
            try
            {
                using FileStream fs = new FileStream(configPath, FileMode.Open);
                result = new GameEngineConfig(fs);
                return true;
            }
            catch (Exception) { }

            result = default;
            return false;
        }

        private const string DefaultEngineConfigFileName = "MzingaEngineConfig.xml";
    }
}
