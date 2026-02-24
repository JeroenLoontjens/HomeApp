using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AppForLogin.Converters
{
    public class LevelToIndentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => new Thickness((int)value * 20, 0, 0, 0);

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
