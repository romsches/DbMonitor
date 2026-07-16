using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DbMonitorWPF.Helpers
{
  /// <summary>
  /// Stellt Hilfsfunktionen zum Kopieren von Zellinhalten aus einem DataGrid in die Zwischenablage bereit.
  /// </summary>
  public static class CellCopyHelper
  {
    /// <summary>
    /// Behandelt das Click-Event eines Kontextmenü-Eintrags, um den Inhalt der ausgewählten Zelle zu kopieren.
    /// </summary>
    public static void CopyCell_Click(object sender, RoutedEventArgs e)
    {
      if (sender is MenuItem menuItem &&
          menuItem.Parent is ContextMenu contextMenu &&
          contextMenu.PlacementTarget is DataGridCell cell)
      {
        string text = ExtractCellContent(cell);

        if (!string.IsNullOrWhiteSpace(text))
        {
          CopyToClipboard(text);
        }
      }
    }

    /// <summary>
    /// Extrahiert den Textinhalt aus einer DataGrid-Zelle.
    /// Durchläuft bei Bedarf den VisualTree, um Steuerelemente zu finden.
    /// </summary>
    private static string ExtractCellContent(DataGridCell cell)
    {
      // 1. Direkte Prüfung
      if (cell.Content is TextBlock textBlock) return textBlock.Text;

      // 2. Prüfung auf ContentPresenter (Standard in DataGrid-Templates)
      if (cell.Content is ContentPresenter cp)
      {
        var content = cp.Content;
        return content?.ToString() ?? string.Empty;
      }

      // 3. Fallback
      return cell.Content?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Versucht, den gegebenen Text in die Zwischenablage zu schreiben.
    /// </summary>
    private static void CopyToClipboard(string text)
    {
      try
      {
        Clipboard.SetText(text);
      }
      catch
      {
        // Fehler bei Zugriff auf die Zwischenablage (z.B. durch andere Prozesse belegt) werden ignoriert
      }
    }
  }
}
