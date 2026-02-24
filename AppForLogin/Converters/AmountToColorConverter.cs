using System;
using System.Globalization;
using Microsoft.Maui.Graphics;

namespace AppForLogin.Converters
{
    public class AmountToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Colors.Black; // Default kleur als de waarde null is

            if (value is decimal amount)
            {
                if (amount < 0)
                    return Colors.Red; // Rood voor negatieve bedragen (uitgaven)
                else if (amount > 0)
                    return Colors.Green; // Groen voor positieve bedragen (inkomsten)
                else
                    return Colors.Black; // Zwart voor nul bedragen
            }

            // Als het type niet decimal is, probeer dan double of float
            if (value is double doubleAmount)
            {
                if (doubleAmount < 0)
                    return Colors.Red;
                else if (doubleAmount > 0)
                    return Colors.Green;
                else
                    return Colors.Black;
            }

            if (value is float floatAmount)
            {
                if (floatAmount < 0)
                    return Colors.Red;
                else if (floatAmount > 0)
                    return Colors.Green;
                else
                    return Colors.Black;
            }

            // Als het type niet herkend wordt, geef een default kleur terug
            return Colors.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
