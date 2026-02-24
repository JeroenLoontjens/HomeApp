using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AppForLogin.Converters
{
    public class ProgressToStarConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progress)
            {
                progress = Math.Clamp(progress, 0, 1);
                return new GridLength(progress, GridUnitType.Star);
            }

            return new GridLength(0, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }


}
