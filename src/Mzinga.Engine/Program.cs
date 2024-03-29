﻿// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text;

namespace Mzinga.Engine
{
    public class Program
    {
        static string ID => $"{AppInfo.Name} v{AppInfo.Version}";

        static string LicensesText => string.Join(Environment.NewLine + Environment.NewLine,
            AppInfo.FormattedLicensesText,
            $"## Mono.Unix ##", AppInfo.MitLicenseName, "Copyright © 2021 Mono Project", AppInfo. MitLicenseBody);

        private static Engine? _engine;

        private static volatile bool _interceptCancel = false;

        private static SigIntMonitor? _sigIntMonitor;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            EngineConfig config = LoadConfig(args.Length > 0 ? args[0] : null);

            _engine = new Engine(ID, config, PrintLine, LicensesText);
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
                string? command = Console.ReadLine();
                if (command is null)
                {
                    break;
                }
                else if (!string.IsNullOrWhiteSpace(command))
                {
                    _engine.ParseCommand(command);
                }
            }

            _sigIntMonitor?.Stop();
        }

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            if (_interceptCancel)
            {
                _engine?.TryCancelAsyncCommand();
                e.Cancel = true;
            }
        }

        private static void SigIntMonitor_SigIntReceived(object? sender, EventArgs e)
        {
            if (_interceptCancel)
            {
                _engine?.TryCancelAsyncCommand();
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

        static EngineConfig LoadConfig(string? configPath)
        {
            EngineConfig? result = null;

            // Try loading specified file
            if (!string.IsNullOrWhiteSpace(configPath) && !TryLoadConfig(configPath, out result))
            {
                // Try loading default file
                TryLoadConfig(Path.Combine(AppInfo.EntryAssemblyPath, DefaultEngineConfigFileName), out result);
            }

            // If no config, load default from embedded resource
            return result ?? EngineConfig.GetDefaultEngineConfig();
        }

        private static bool TryLoadConfig(string configPath, out EngineConfig? result)
        {
            try
            {
                using FileStream fs = new FileStream(configPath, FileMode.Open);
                result = new EngineConfig(fs);
                return true;
            }
            catch (Exception) { }

            result = default;
            return false;
        }

        private const string DefaultEngineConfigFileName = "MzingaEngineConfig.xml";
    }
}
