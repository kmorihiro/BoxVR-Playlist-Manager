﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace BoxVR_Playlist_Manager
{
    public class IsEnabledColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isEnabled = (bool)value;
            return isEnabled ? parameter.ToString() : "#ccc";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
