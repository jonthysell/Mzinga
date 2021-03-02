// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Globalization;

using Avalonia.Data.Converters;

// Class adapted from http://wpftutorial.net/RadioButton.html

namespace Mzinga.SharedUX
{
    public class EnumMatchToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (null == value || null == parameter)
            {
                return false;
            }

            string checkValue = value.ToString();
            string targetValue = parameter.ToString();

            return checkValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (null == value || null == parameter)
            {
                return null;
            }

            bool? useValue = value as bool?;

            if (useValue.HasValue && useValue.Value)
            {
                return Enum.Parse(targetType, parameter.ToString());
            }

            return null;
        }
    } 
}
