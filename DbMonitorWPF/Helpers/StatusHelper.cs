using System;

namespace DbMonitorWPF.Helpers
{
  /// <summary>
  /// Stellt Hilfsmethoden zur Formatierung von Statusanzeigen in der Benutzeroberfläche bereit.
  /// </summary>
  public static class StatusHelper
  {
    /// <summary>
    /// Erstellt eine standardisierte Status-Zeichenfolge für die Anzeige in der UI.
    /// </summary>
    /// <param name="count">Die Anzahl der gefundenen Elemente.</param>
    /// <param name="entityName">Der Name der Entität (z. B. "Sitzungen").</param>
    /// <returns>Ein formatierter String mit Elementanzahl und aktuellem Zeitstempel.</returns>
    public static string FormatStatus(int count, string entityName)
    {
      // Die Verwendung von 'G' für das Zeitformat ist ebenfalls eine Option, 
      // aber 'HH:mm:ss' ist für Statuszeilen im Dashboard ideal.
      string timeStamp = DateTime.Now.ToString("HH:mm:ss");

      return $"Gefunden: {count} {entityName} | Stand: {timeStamp}";
    }
  }
}
