// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Mzinga.Viewer
{
    public class SoundUtils
    {
        public static void PlaySound(GameSound gameSound)
        {
            string fileName = Path.Combine(AppContext.BaseDirectory, "Resources", $"{ gameSound }sfx.wav".ToLower());
            if (AppInfo.IsWindows)
            {
                WindowsPlaySound(fileName);
            }
            else if (AppInfo.IsMacOS)
            {
                MacOSPlaySound(fileName);
            }
            else if (AppInfo.IsLinux)
            {
                LinuxPlaySound(fileName);
            }
        }

        private static void WindowsPlaySound(string fileName)
        {
            NativeMethods.PlaySound(fileName, IntPtr.Zero, NativeMethods.SoundFlags.SND_ASYNC | NativeMethods.SoundFlags.SND_FILENAME);
        }

        private static void MacOSPlaySound(string fileName)
        {
            StartBashProcess($"afplay '{ fileName }'");
        }

        private static void LinuxPlaySound(string fileName)
        {
            StartBashProcess($"aplay -q '{ fileName }'");
        }

        // Adapted from https://github.com/mobiletechtracker/NetCoreAudio
        private static void StartBashProcess(string command)
        {
            var escapedArgs = command.Replace("\"", "\\\"");
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
        }
    }

    public enum GameSound
    {
        Move = 0,
        Undo,
        GameOver,
    }

    internal static partial class NativeMethods
    {
        [DllImport("winmm.dll", SetLastError = true)]
        internal static extern bool PlaySound(string pszSound, IntPtr hmod, SoundFlags fdwSound);

        [Flags]
        internal enum SoundFlags
        {
            SND_ASYNC = 0x0001,
            SND_FILENAME = 0x00020000,
        }
    }
}
