// 
// SoundUtils.cs
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

#if WINDOWS_WPF
using System;
using System.Media;
using System.Windows;
using System.Windows.Resources;
#endif

namespace Mzinga.SharedUX
{
    public class SoundUtils
    {
        public static void PlaySound(GameSound sound)
        {
#if WINDOWS_WPF
            string resPath = "pack://application:,,,/Resources/";

            switch (sound)
            {
                case GameSound.Move:
                    resPath += "movesfx.wav";
                    break;
                case GameSound.Undo:
                    resPath += "undosfx.wav";
                    break;
            }

            StreamResourceInfo sri = Application.GetResourceStream(new Uri(resPath));

            if (null != sri)
            {
                using (sri.Stream)
                {
                    SoundPlayer player = new SoundPlayer(sri.Stream);
                    player.Load();
                    player.Play();
                }
            }
#endif
        }
    }

    public enum GameSound
    {
        Move = 0,
        Undo,
        GameOver,
    }
}
