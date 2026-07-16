using System;

namespace DbMonitorData
{
  /// <summary>
  /// Ergebnis-DTO für SQL-Abfrageoperationen.
  /// Kapselt den SQL-Rohtext, die formatierte Version sowie Bind-Variablen.
  /// </summary>
  public class SqlTextErgebnis
  {
    #region Eigenschaften

    /// <summary>Unformatierter SQL-Rohtext (Original aus V$SQL).</summary>
    public string RohSql { get; set; }

    /// <summary>Formatierter SQL-Text (Pretty-Print).</summary>
    public string FormatierterSql { get; set; }

    /// <summary>Die mit dem SQL verknüpften Bind-Variablen als lesbare Textliste.</summary>
    public string BindVariablen { get; set; }

    /// <summary>
    /// Kurzbezeichner für die Anzeige im Dashboard.
    /// Priorisiert den formatierten SQL-Text, sofern verfügbar.
    /// </summary>
    public string SqlText => string.IsNullOrWhiteSpace(FormatierterSql) ? RohSql : FormatierterSql;

    /// <summary>
    /// Gibt an, ob ein gültiger SQL-Text vorliegt (kein Hinweis oder Fehler).
    /// </summary>
    public bool HatSql => !string.IsNullOrWhiteSpace(RohSql)
                       && !RohSql.StartsWith("Hinweis:", StringComparison.OrdinalIgnoreCase)
                       && !RohSql.StartsWith("Fehler:", StringComparison.OrdinalIgnoreCase);

    #endregion

    #region Konstruktor

    /// <summary>
    /// Initialisiert eine neue Instanz des <see cref="SqlTextErgebnis"/>.
    /// </summary>
    public SqlTextErgebnis(string rohSql, string formatierterSql, string bindVariablen)
    {
      RohSql = rohSql ?? string.Empty;
      FormatierterSql = formatierterSql ?? string.Empty;
      BindVariablen = bindVariablen ?? string.Empty;
    }

    #endregion

    #region Factory Methoden

    /// <summary>
    /// Erstellt eine leere Instanz, optional mit einem Hinweis-Text.
    /// </summary>
    public static SqlTextErgebnis Leer(string hinweis = "") =>
        new SqlTextErgebnis(hinweis, string.Empty, string.Empty);

    #endregion
  }
}
