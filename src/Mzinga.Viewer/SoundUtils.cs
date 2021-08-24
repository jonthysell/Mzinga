// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Mzinga.Viewer
{
    public class SoundUtils
    {
        public static void PlaySound(GameSound gameSound)
        {
            if (AppInfo.IsWindows)
            {
                WindowsPlaySound(gameSound);
            }
        }

        private static void WindowsPlaySound(GameSound gameSound)
        {
            var pszSound = GetSoundFile(gameSound);
            NativeMethods.PlaySound(pszSound, IntPtr.Zero, NativeMethods.SoundFlags.SND_ASYNC | NativeMethods.SoundFlags.SND_FILENAME);
        }

        private static string GetSoundFile(GameSound gameSound)
        {
            string fileName = $"{ gameSound }sfx.wav".ToLower();
            return Path.Combine(Path.GetDirectoryName(AppInfo.Assembly.Location), "Resources", fileName);
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
