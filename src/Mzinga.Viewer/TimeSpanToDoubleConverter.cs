// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Globalization;

using Avalonia.Data.Converters;

namespace Mzinga.Viewer
{
    public class TimeSpanToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan ts)
            {
                return ts.TotalSeconds;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
            {
                return TimeSpan.FromSeconds(d);
            }

            return TimeSpan.Zero;
        }
    } 
}
