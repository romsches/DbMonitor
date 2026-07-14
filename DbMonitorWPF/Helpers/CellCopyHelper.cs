using System.Windows;
using System.Windows.Controls;

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
    /// <param name="sender">Das auslösende MenuItem.</param>
    /// <param name="e">Die Routing-Event-Argumente.</param>
    public static void CopyCell_Click(object sender, RoutedEventArgs e)
    {
      if (sender is MenuItem menuItem &&
          menuItem.Parent is ContextMenu contextMenu &&
          contextMenu.PlacementTarget is DataGridCell cell)
      {
        string text = ExtractCellContent(cell);

        if (!string.IsNullOrEmpty(text))
        {
          CopyToClipboard(text);
        }
      }
    }

    /// <summary>
    /// Extrahiert den Textinhalt aus einer DataGrid-Zelle.
    /// </summary>
    private static string ExtractCellContent(DataGridCell cell)
    {
      // Fall 1: Standardmäßiger TextBlock innerhalb der Zelle
      if (cell.Content is TextBlock textBlock)
      {
        return textBlock.Text;
      }

      // Fall 2: Andere Steuerelemente oder ContentPresenter (Konvertierung zum String)
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
