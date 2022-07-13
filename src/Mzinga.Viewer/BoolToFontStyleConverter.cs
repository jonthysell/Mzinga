// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Mzinga.Viewer
{
    public class BoolToItalicFontStyleConverter : BoolToFontStyleConverter
    {
        public BoolToItalicFontStyleConverter() : base(FontStyle.Italic) { }
    }

    public abstract class BoolToFontStyleConverter : IValueConverter
    {
        public BoolToFontStyleConverter(FontStyle fontStyle)
        {
            _fontStyle = fontStyle;
        }

        private readonly FontStyle _fontStyle;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
            {
                return _fontStyle;
            }

            return FontStyle.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}