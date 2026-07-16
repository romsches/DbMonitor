using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DbMonitorWPF.Converters
{
  /// <summary>
  /// Konvertiert einen booleschen Wert in eine Visibility (true -> Collapsed, false -> Visible).
  /// </summary>
  [ValueConversion(typeof(bool), typeof(Visibility))]
  public class BoolInvertedToCollapsedConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool boolValue)
      {
        return boolValue ? Visibility.Collapsed : Visibility.Visible;
      }

      // Standardverhalten, falls der Wert kein bool ist
      return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is Visibility visibility)
      {
        return visibility != Visibility.Visible;
      }

      return false;
    }
  }
}
