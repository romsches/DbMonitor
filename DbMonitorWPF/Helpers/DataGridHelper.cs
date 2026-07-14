using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DbMonitorWPF.Helpers
{
  public static class DataGridHelper
  {
    public static readonly RoutedUICommand CopyRowCommand = new RoutedUICommand(
        "Zeile kopieren", "CopyRow", typeof(DataGridHelper));

    // Метод для копирования строки
    public static void CopySelectedRow(DataGrid dataGrid)
    {
      var originalUnit = dataGrid.SelectionUnit;
      dataGrid.SelectionUnit = DataGridSelectionUnit.FullRow;
      dataGrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
      ApplicationCommands.Copy.Execute(null, dataGrid);
      dataGrid.SelectionUnit = originalUnit;
    }

    // Универсальный метод получения текста из ячейки
    public static string GetCellText(DataGridCell cell)
    {
      if (cell == null) return null;

      var content = cell.Content;
      if (content is TextBlock tb) return tb.Text;
      if (content is ContentPresenter cp)
      {
        var innerTb = FindVisualChild<TextBlock>(cp);
        return innerTb?.Text;
      }
      return content?.ToString();
    }

    // Вспомогательные методы визуального дерева (теперь они здесь!)
    public static T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
      DependencyObject parent = VisualTreeHelper.GetParent(child);
      while (parent != null && !(parent is T))
        parent = VisualTreeHelper.GetParent(parent);
      return parent as T;
    }

    public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
      {
        var child = VisualTreeHelper.GetChild(parent, i);
        if (child is T result) return result;
        var found = FindVisualChild<T>(child);
        if (found != null) return found;
      }
      return null;
    }
  }
}
