using DataAccess.Model;
using Microsoft.Maui.Controls;
using System;
using System.Globalization;


namespace AppForLogin.Converters
{
    public class StatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StatusTrans status)
            {
                return status switch
                {
                    StatusTrans.Manual => "hand48.png",     // ✍️
                    StatusTrans.Imported => "csv48.png",   // 📥
                    StatusTrans.Pending => "pending48.png",    // ⏳
                    StatusTrans.Approved => "approved48.png",   // ✔
                    StatusTrans.Rejected => "cross48.png",   // ❌
                    _ => "icon_unknown.png"
                };
            }

            return "icon_unknown.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}

