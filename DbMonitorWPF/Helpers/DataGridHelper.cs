using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DbMonitorWPF.Helpers
{
  /// <summary>
  /// Stellt Hilfsfunktionen für DataGrid-Operationen bereit.
  /// </summary>
  public static class DataGridHelper
  {
    public static readonly RoutedUICommand CopyRowCommand = new RoutedUICommand(
        "Zeile kopieren", "CopyRow", typeof(DataGridHelper));

    /// <summary>
    /// Kopiert die ausgewählte Zeile des DataGrids in die Zwischenablage.
    /// </summary>
    public static void CopySelectedRow(DataGrid dataGrid)
    {
      if (dataGrid == null) return;

      var originalUnit = dataGrid.SelectionUnit;
      dataGrid.SelectionUnit = DataGridSelectionUnit.FullRow;
      dataGrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;

      ApplicationCommands.Copy.Execute(null, dataGrid);

      dataGrid.SelectionUnit = originalUnit;
    }

    /// <summary>
    /// Extrahiert den Text aus einer DataGrid-Zelle.
    /// </summary>
    public static string GetCellText(DataGridCell cell)
    {
      if (cell == null) return null;

      if (cell.Content is TextBlock tb) return tb.Text;

      if (cell.Content is ContentPresenter cp)
      {
        // Anwendung des Templates erzwingen, falls noch nicht geschehen
        cp.ApplyTemplate();
        var innerTb = FindVisualChild<TextBlock>(cp);
        return innerTb?.Text;
      }

      return cell.Content?.ToString();
    }

    /// <summary>
    /// Sucht das erste übergeordnete Element vom Typ T im VisualTree.
    /// </summary>
    public static T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
      DependencyObject parent = VisualTreeHelper.GetParent(child);
      while (parent != null)
      {
        if (parent is T typedParent) return typedParent;
        parent = VisualTreeHelper.GetParent(parent);
      }
      return null;
    }

    /// <summary>
    /// Sucht rekursiv das erste untergeordnete Element vom Typ T im VisualTree.
    /// </summary>
    public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
      if (parent == null) return null;

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
