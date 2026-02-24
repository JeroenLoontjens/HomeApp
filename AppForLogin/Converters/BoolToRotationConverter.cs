using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AppForLogin.Converters
{
    public class BoolToRotationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? 180 : 0;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
             => throw new NotImplementedException();
    }

}
