using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DbMonitorWPF.Converters
{
  /// <summary>
  /// Konvertiert einen booleschen Wert in eine Visibility (true -> Visible, false -> Collapsed).
  /// </summary>
  [ValueConversion(typeof(bool), typeof(Visibility))]
  public class BoolToCollapsedConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool boolValue)
      {
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
      }

      // Wenn der Wert nicht bool ist, signalisieren wir dem Binding, 
      // dass keine Konvertierung möglich ist.
      return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is Visibility visibility)
      {
        return visibility == Visibility.Visible;
      }

      return false;
    }
  }
}
