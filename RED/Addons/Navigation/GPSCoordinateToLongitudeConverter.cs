﻿using System;
using System.Windows.Data;

namespace RED.Addons.Navigation
{
    public class GPSCoordinateToLongitudeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((GPSCoordinate)value).Longitude;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}