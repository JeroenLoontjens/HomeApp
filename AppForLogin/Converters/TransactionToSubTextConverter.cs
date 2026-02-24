using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using DataAccess.Model;

namespace AppForLogin.Converters
{
    public class TransactionToSubTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Transaction t)
            {
                var date = t.Date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
                var category = t.Category?.Name?.Trim();
                return !string.IsNullOrWhiteSpace(category)
                    ? $"{date} \u2022 {category}"
                    : date;
            }

            if (value is DateTime dt)
            {
                return dt.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
            }

            if (value is string s)
            {
                return s;
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
