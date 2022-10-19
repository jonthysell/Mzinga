// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

using Mzinga.Core;
using Mzinga.Viewer;

namespace Mzinga.Viewer
{
    public static class ColorUtils
    {
        public static readonly Color[] BugColors;

        public static readonly SolidColorBrush[] BugColorBrushes;

        static ColorUtils()
        {
            BugColors = new Color[(int)BugType.NumBugTypes];
            BugColorBrushes = new SolidColorBrush[(int)BugType.NumBugTypes];

            for (int bt = 0; bt < (int)BugType.NumBugTypes; bt++)
            {
                BugColors[bt] = (Color)App.Current.FindResource($"{(BugType)bt}Color");
                BugColorBrushes[bt] = new SolidColorBrush(BugColors[bt]);
            }
        }

        public static Color MixColors(Color c1, Color c2)
        {
            return Color.FromArgb((byte)((c1.A + c2.A) / 2),
                                       (byte)((c1.R + c2.R) / 2),
                                       (byte)((c1.G + c2.G) / 2),
                                       (byte)((c1.B + c2.B) / 2));
        }
        public static SolidColorBrush MixSolidColorBrushes(SolidColorBrush b1, SolidColorBrush b2)
        {
            return new SolidColorBrush(MixColors(b1.Color, b2.Color));
        }
    }
}
