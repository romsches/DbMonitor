using System;
using PoorMansTSqlFormatterLib;

namespace DbMonitorWPF.Helpers
{
  /// <summary>
  /// Bietet Funktionalitäten zur professionellen Formatierung von SQL-Strings.
  /// </summary>
  public static class SqlFormatterHelper
  {
    private const string WarningMarker = "--WARNING!";

    /// <summary>
    /// Formatiert einen rohen SQL-String und bereinigt ihn von Warnmeldungen.
    /// </summary>
    public static string Format(string rawSql)
    {
      if (string.IsNullOrWhiteSpace(rawSql))
      {
        return string.Empty;
      }

      string sqlToFormat = CleanSqlContent(rawSql);

      // Die Bibliothek kann Exceptions werfen, wenn das SQL extrem kaputt ist.
      try
      {
        var formatter = new SqlFormattingManager();
        return formatter.Format(sqlToFormat).Trim();
      }
      catch
      {
        // Fallback: Bei Formatierungsfehlern das Original-SQL zurückgeben
        return sqlToFormat.Trim();
      }
    }

    /// <summary>
    /// Bereinigt den SQL-Input von spezifischen Warnhinweisen.
    /// </summary>
    private static string CleanSqlContent(string sql)
    {
      int warningIndex = sql.IndexOf(WarningMarker, StringComparison.OrdinalIgnoreCase);
      if (warningIndex < 0)
      {
        return sql;
      }

      // Suche nach dem nächsten SELECT nach der Warnung
      int selectIndex = sql.IndexOf("SELECT", warningIndex, StringComparison.OrdinalIgnoreCase);

      if (selectIndex > 0)
      {
        return sql.Substring(selectIndex);
      }

      // Sicherer Schnitt: Prüfen, ob noch Restinhalt vorhanden ist
      int startOfActualSql = warningIndex + WarningMarker.Length;
      if (startOfActualSql < sql.Length)
      {
        return sql.Substring(startOfActualSql).TrimStart();
      }

      return sql;
    }
  }
}
