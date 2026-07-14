using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DbMonitorWPF.Converters
{
    public class BoolToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? Visibility.Visible : Visibility.Collapsed;
            }

            if (value == null)
                return Visibility.Collapsed;

            // Try to convert
            try
            {
                var parsed = System.Convert.ToBoolean(value);
                return parsed ? Visibility.Visible : Visibility.Collapsed;
            }
            catch
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v)
                return v == Visibility.Visible;

            return false;
        }
    }
}
