using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DbMonitorWPF.ViewModels
{
  /// <summary>
  /// Basisklasse für alle ViewModels, implementiert INotifyPropertyChanged.
  /// </summary>
  public abstract class ViewModelBase : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Löst das PropertyChanged-Event aus.
    /// </summary>
    /// <param name="propertyName">Der Name der Property, die sich geändert hat.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Aktualisiert den Wert eines Feldes und löst das PropertyChanged-Event aus, 
    /// falls sich der Wert tatsächlich geändert hat.
    /// </summary>
    /// <typeparam name="T">Der Typ der Property.</typeparam>
    /// <param name="field">Das zu aktualisierende Feld.</param>
    /// <param name="value">Der neue Wert.</param>
    /// <param name="propertyName">Der Name der Property (automatisch via CallerMemberName).</param>
    /// <returns>True, wenn der Wert geändert wurde, andernfalls False.</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
      if (EqualityComparer<T>.Default.Equals(field, value))
      {
        return false;
      }

      field = value;
      OnPropertyChanged(propertyName);
      return true;
    }
  }
}
