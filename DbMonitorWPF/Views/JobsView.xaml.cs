using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DbMonitorWPF.ViewModels;
using DbMonitorWPF.Helpers;

namespace DbMonitorWPF.Views
{
  /// <summary>
  /// Code-Behind für die JobsView.
  /// </summary>
  public partial class JobsView : UserControl
  {
    private DataGridCell _lastRightClickedCell;

    public JobsView()
    {
      InitializeComponent();
      this.Loaded += OnViewLoaded;
    }

    private void OnViewLoaded(object sender, RoutedEventArgs e)
    {
      if (DataContext is JobsViewModel vm)
        ((System.Windows.Input.ICommand)vm.JobsAktualisierenBefehl)?.Execute(null);
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

    private void CopyRow_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      if (sender is DataGrid dg)
      {
        DataGridHelper.CopySelectedRow(dg);
      }
    }
  }
}
