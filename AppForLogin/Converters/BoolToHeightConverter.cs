using System.Globalization;

namespace AppForLogin.Converters
{
    public class BoolToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded && isExpanded)
            {
                // When expanded, return auto or a specific height
                return double.Parse(parameter?.ToString() ?? "200");
            }
            
            // When collapsed, return a small height to maintain layout space
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}