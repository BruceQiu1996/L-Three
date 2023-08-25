using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ThreeL.Client.Win.Converters
{
    public class Int2BooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value <= 0) 
            {
                return false;
            }
            else 
            {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
