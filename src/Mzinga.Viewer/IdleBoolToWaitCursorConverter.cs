// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Input;

namespace Mzinga.Viewer
{
    public class IdleBoolToWaitCursorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((value as bool?) is not null)
            {
                if (!(bool)value)
                {
                    return new Cursor(StandardCursorType.Wait);
                }
            }

            return new Cursor(StandardCursorType.Arrow);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}