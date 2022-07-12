// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Mzinga.Viewer
{
    public class BoolToBoldFontWeightConverter : BoolToFontWeightConverter
    {
        public BoolToBoldFontWeightConverter() : base(FontWeight.Bold) { }
    }

    public abstract class BoolToFontWeightConverter : IValueConverter
    {
        public BoolToFontWeightConverter(FontWeight fontWeight)
        {
            _fontWeight = fontWeight;
        }

        private FontWeight _fontWeight;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
            {
                return _fontWeight;
            }

            return FontWeight.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}