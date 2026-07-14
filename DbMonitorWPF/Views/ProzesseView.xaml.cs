using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DbMonitorWPF.ViewModels;
using DbMonitorWPF.Helpers;

namespace DbMonitorWPF.Views
{
  /// <summary>
  /// Code-Behind für die ProzesseView.
  /// Verantwortlich für die Anzeige der aktiven Oracle-Prozesse.
  /// </summary>
  public partial class ProzesseView : UserControl
  {
    private DataGridCell _lastRightClickedCell;

    public ProzesseView()
    {
      InitializeComponent();
      this.Loaded += OnViewLoaded;
    }

    private void OnViewLoaded(object sender, RoutedEventArgs e)
    {
      if (DataContext is ProzesseViewModel vm)
      {
        if (vm.ProzesseAktualisierenBefehl.CanExecute(null))
          vm.ProzesseAktualisierenBefehl.Execute(null);
      }
    }

    // Обработчик для сохранения ячейки при клике
    private void DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      _lastRightClickedCell = DataGridHelper.FindParent<DataGridCell>(e.OriginalSource as DependencyObject);
    }

    // Обработчик для копирования текста ячейки
    private void CopyCell_Click(object sender, RoutedEventArgs e)
    {
      if (_lastRightClickedCell == null) return;

      string text = DataGridHelper.GetCellText(_lastRightClickedCell);
      if (!string.IsNullOrEmpty(text)) Clipboard.SetText(text);
    }

    // Handler für das Kopieren der gesamten Zeile
    private void CopyRow_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      if (sender is DataGrid dg)
      {
        DataGridHelper.CopySelectedRow(dg);
      }
    }
  }
}
