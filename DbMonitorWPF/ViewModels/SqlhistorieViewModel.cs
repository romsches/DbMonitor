using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using DbMonitorData;
using DbMonitorWPF.Helpers;
using Microsoft.Extensions.Logging;
using Prism.Commands;

namespace DbMonitorWPF.ViewModels
{
  /// <summary>
  /// ViewModel zur Verwaltung und Analyse der SQL-Historie.
  /// </summary>
  public class SqlhistorieViewModel : SqlBaseViewModel
  {
    #region Felder

    private readonly DbQuery _dbQuery;
    private V_SQL _ausgewaehltesSql;
    private string _filterTextSql = string.Empty;
    private string _statusSql = string.Empty;
    private bool _istTop20SqlModus = false;
    private CancellationTokenSource _ladeCts;

    #endregion

    #region Konstruktor

    public SqlhistorieViewModel(DbQuery dbQuery)
    {
      _dbQuery = dbQuery;
      SqlVerlauf = new ObservableCollection<V_SQL>();
      SqlVerlaufAnsicht = CollectionViewSource.GetDefaultView(SqlVerlauf);
      SqlVerlaufAnsicht.Filter = FilterSqlDelegate;

      SqlAktualisierenBefehl = new DelegateCommand(SqlAktualisierenAusfuehren);
      LadeTopSqlBefehl = new DelegateCommand(LadeTopSqlAusfuehren);
      AlleSqlLadenBefehl = new DelegateCommand(LadeAlleSqlAusfuehren);
      KopiereHistorieSqlBefehl = new DelegateCommand(KopiereHistorieSqlAusfuehren, () => AusgewaehltesSql != null);
    }

    #endregion

    #region Eigenschaften

    public ObservableCollection<V_SQL> SqlVerlauf { get; }
    public ICollectionView SqlVerlaufAnsicht { get; }

    public ICommand SqlAktualisierenBefehl { get; }
    public ICommand LadeTopSqlBefehl { get; }
    public ICommand AlleSqlLadenBefehl { get; }
    public ICommand KopiereHistorieSqlBefehl { get; }

    public V_SQL AusgewaehltesSql
    {
      get => _ausgewaehltesSql;
      set
      {
        if (SetProperty(ref _ausgewaehltesSql, value))
        {
          AktualisiereHistorieSqlAnsicht();
          ((DelegateCommand)KopiereHistorieSqlBefehl).RaiseCanExecuteChanged();
        }
      }
    }

    public string FilterTextSql
    {
      get => _filterTextSql;
      set
      {
        if (SetProperty(ref _filterTextSql, value))
          SqlVerlaufAnsicht?.Refresh();
      }
    }

    public string StatusSql
    {
      get => _statusSql;
      set => SetProperty(ref _statusSql, value);
    }

    #endregion

    #region Methoden

    public void LadeTopSqlAusfuehren()
    {
      _istTop20SqlModus = true;
      LadeSqlDaten(true);
    }

    private void LadeAlleSqlAusfuehren()
    {
      _istTop20SqlModus = false;
      LadeSqlDaten(false);
    }

    private void SqlAktualisierenAusfuehren() => LadeSqlDaten(_istTop20SqlModus);

    private void LadeSqlDaten(bool nurTop20)
    {
      string vorherigeSqlId = AusgewaehltesSql?.SQL_ID;

      _ladeCts?.Cancel();
      _ladeCts = new CancellationTokenSource();
      var token = _ladeCts.Token;

      StatusSql = "Daten werden geladen...";

      Task.Run(() =>
      {
        var data = nurTop20
            ? (_dbQuery.GetTopExecutedSqlList() ?? new List<V_SQL>())
            : (_dbQuery.GetAllSqlList() ?? new List<V_SQL>());

        if (token.IsCancellationRequested) return;

        Application.Current.Dispatcher.Invoke(() =>
        {
          if (token.IsCancellationRequested) return;

          CollectionHelper.SyncForce(SqlVerlauf, data);

          if (!string.IsNullOrEmpty(vorherigeSqlId))
          {
            AusgewaehltesSql = SqlVerlauf.FirstOrDefault(s => s.SQL_ID == vorherigeSqlId);
          }

          SqlVerlaufAnsicht?.Refresh();
          StatusSql = $"{SqlVerlauf.Count} SQL-Einträge gefunden";
        });
      }, token);
    }

    public void AktualisierenIntern(List<V_SQL> daten)
    {
      string vorherigeSqlId = AusgewaehltesSql?.SQL_ID;

      Application.Current.Dispatcher.Invoke(() =>
      {
        CollectionHelper.SyncForce(SqlVerlauf, daten);

        if (!string.IsNullOrEmpty(vorherigeSqlId))
        {
          AusgewaehltesSql = SqlVerlauf.FirstOrDefault(s => s.SQL_ID == vorherigeSqlId);
        }

        SqlVerlaufAnsicht?.Refresh();
        StatusSql = $"{SqlVerlauf.Count} SQL-Einträge gefunden";
      });
    }

    private void KopiereHistorieSqlAusfuehren()
    {
      if (!string.IsNullOrEmpty(AktuellerSqlText))
        Clipboard.SetText(AktuellerSqlText);
    }

    private bool FilterSqlDelegate(object obj)
    {
      if (string.IsNullOrWhiteSpace(FilterTextSql)) return true;
      if (obj is V_SQL s)
      {
        string filter = FilterTextSql.ToUpper();
        return (s.SQL_ID?.ToUpper().Contains(filter) ?? false) ||
               (s.SQL_FULLTEXT?.ToUpper().Contains(filter) ?? false);
      }
      return false;
    }

    private void AktualisiereHistorieSqlAnsicht()
    {
      if (_ausgewaehltesSql == null || string.IsNullOrWhiteSpace(_ausgewaehltesSql.SQL_FULLTEXT))
      {
        SetSqlData(string.Empty);
      }
      else
      {
        string raw = _ausgewaehltesSql.SQL_FULLTEXT;
        int index = raw.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);
        string cleanSql = (index > 0) ? raw.Substring(index).Trim() : raw;
        SetSqlData(cleanSql);
      }
    }

    public void SortiereNachAusfuehrungen() => LadeTopSqlAusfuehren();

    #endregion
  }
}
