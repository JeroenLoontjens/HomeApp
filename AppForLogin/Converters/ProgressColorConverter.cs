using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace AppForLogin.Converters
{
    
   
        public class ProgressColorConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is double progress)
                { 

                    if (progress < 0.75)
                        return Colors.Green;
                     else if (progress <  0.9)
                        return Colors.Yellow;
                     else
                        return  Colors.Red;             
                                    
                }
                return Colors.Green;
            }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
                => throw new NotImplementedException();
        }
    
}