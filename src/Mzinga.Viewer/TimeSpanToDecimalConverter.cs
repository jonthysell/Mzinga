// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Globalization;

using Avalonia.Data.Converters;

namespace Mzinga.Viewer
{
    public class TimeSpanToDecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan ts)
            {
                return (decimal)ts.TotalSeconds;
            }

            return (decimal)0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal d)
            {
                return TimeSpan.FromSeconds((double)d);
            }

            return TimeSpan.Zero;
        }
    } 
}
