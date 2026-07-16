using System.ComponentModel;
using DbMonitorWPF.Helpers;

namespace DbMonitorWPF.ViewModels
{
  /// <summary>
  /// Basisklasse für alle ViewModels, die SQL-Abfragen anzeigen und formatieren.
  /// </summary>
  public abstract class SqlBaseViewModel : ViewModelBase
  {
    #region Felder

    private bool _istSqlFormatiert = false;
    private string _aktuellerSqlText;

    protected string _rawSql;
    protected string _formattedSql;

    #endregion

    #region Eigenschaften

    /// <summary>
    /// Gibt an, ob der SQL-Text formatiert angezeigt werden soll.
    /// </summary>
    public bool IstSqlFormatiert
    {
      get => _istSqlFormatiert;
      set
      {
        if (SetProperty(ref _istSqlFormatiert, value))
        {
          UpdateSqlDisplay();
        }
      }
    }

    /// <summary>
    /// Der aktuell angezeigte SQL-Text (abhängig vom Formatierungs-Status).
    /// </summary>
    public string AktuellerSqlText
    {
      get => _aktuellerSqlText;
      set => SetProperty(ref _aktuellerSqlText, value);
    }

    #endregion

    #region Methoden

    /// <summary>
    /// Aktualisiert die Anzeige des SQL-Textes basierend auf dem Formatierungs-Status.
    /// </summary>
    protected void UpdateSqlDisplay()
    {
      AktuellerSqlText = !IstSqlFormatiert ? _formattedSql : _rawSql;
    }

    /// <summary>
    /// Setzt die SQL-Rohdaten und bereitet die formatierte Ansicht vor.
    /// </summary>
    /// <param name="rawSql">Der unformatierte SQL-Text aus der Datenbank.</param>
    protected void SetSqlData(string rawSql)
    {
      _rawSql = rawSql ?? string.Empty;
      _formattedSql = SqlFormatterHelper.Format(_rawSql);
      UpdateSqlDisplay();
    }

    #endregion
  }
}
