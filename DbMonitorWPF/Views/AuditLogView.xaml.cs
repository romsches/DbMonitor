using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DbMonitorWPF.ViewModels;
using DbMonitorWPF.Helpers;

namespace DbMonitorWPF.Views
{
  public partial class AuditLogView : UserControl
  {
    private DataGridCell _lastRightClickedCell;

    public AuditLogView()
    {
      InitializeComponent();
      this.Loaded += OnViewLoaded;
    }

    private void OnViewLoaded(object sender, RoutedEventArgs e)
    {
      if (DataContext is AuditLogViewModel vm)
      {
        if (vm.RefreshCommand.CanExecute(null))
          vm.RefreshCommand.Execute(null);
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
