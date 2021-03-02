// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

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
