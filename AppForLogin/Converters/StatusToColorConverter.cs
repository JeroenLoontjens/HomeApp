using System.Globalization;
using Microsoft.Maui.Graphics;

namespace AppForLogin.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status)
                {
                    case "Over Budget":
                        return Colors.Red;
                    case "Under Budget":
                        return Colors.Green;
                    case "On Track":
                        return Colors.Yellow;
                    default:
                        return Colors.Transparent;
                }
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
