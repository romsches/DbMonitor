using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using DbMonitorData;
using DbMonitorWPF.Views;
using Microsoft.Extensions.Logging;
using Prism.Commands;

namespace DbMonitorWPF.ViewModels
{
  /// <summary>
  /// ViewModel zur Verwaltung und Überwachung von Datenbanksitzungen.
  /// </summary>
  public class SitzungenViewModel : SqlBaseViewModel
  {
    #region Felder

    private readonly ILogger _log;
    private readonly DbQuery _dbQuery;
    private string _filterTextSitzungen;
    private string _statusSitzungen;
    private V_SESSION_MONITOR _ausgewaehlteSitzung;

    #endregion

    #region Konstruktor

    public SitzungenViewModel(DbQuery dbQuery, ILogger<SitzungenViewModel> log)
    {
      _log = log;
      _dbQuery = dbQuery;

      Sitzungen = new ObservableCollection<V_SESSION_MONITOR>();
      SitzungenAnsicht = CollectionViewSource.GetDefaultView(Sitzungen);
      SitzungenAnsicht.Filter = FilterSitzungenDelegate;

      SitzungenAktualisierenBefehl = new DelegateCommand(SitzungenAktualisierenAusfuehren);
      SitzungBeendenBefehl = new DelegateCommand(SitzungBeendenAusfuehren, () => AusgewaehlteSitzung != null);
      KopiereSitzungsSqlBefehl = new DelegateCommand(KopiereSitzungsSqlAusfuehren, () => !string.IsNullOrEmpty(AktuellerSqlText));
    }

    #endregion

    #region Eigenschaften

    public ObservableCollection<V_SESSION_MONITOR> Sitzungen { get; }
    public ICollectionView SitzungenAnsicht { get; }

    public V_SESSION_MONITOR AusgewaehlteSitzung
    {
      get => _ausgewaehlteSitzung;
      set
      {
        _ausgewaehlteSitzung = value;
        OnPropertyChanged();
        AusgewaehlteSitzungSqlLaden();
        SitzungBeendenBefehl.RaiseCanExecuteChanged();
        KopiereSitzungsSqlBefehl.RaiseCanExecuteChanged();
      }
    }

    public string FilterTextSitzungen
    {
      get => _filterTextSitzungen;
      set { _filterTextSitzungen = value; OnPropertyChanged(); SitzungenAnsicht.Refresh(); }
    }

    public string StatusSitzungen
    {
      get => _statusSitzungen;
      set { _statusSitzungen = value; OnPropertyChanged(); }
    }

    public DelegateCommand SitzungenAktualisierenBefehl { get; }
    public DelegateCommand SitzungBeendenBefehl { get; }
    public DelegateCommand KopiereSitzungsSqlBefehl { get; }

    #endregion

    #region Methoden

    private void SitzungenAktualisierenAusfuehren()
    {
      var vorherigeSid = AusgewaehlteSitzung?.SID;
      var vorherigeSerial = AusgewaehlteSitzung?.SERIAL;

      var liste = _dbQuery.GetAktuelleSessionsList();

      Application.Current.Dispatcher.Invoke(() =>
      {
        Sitzungen.Clear();
        foreach (var item in liste) Sitzungen.Add(item);

        if (vorherigeSid.HasValue)
        {
          var gefunden = Sitzungen.FirstOrDefault(s => s.SID == vorherigeSid && s.SERIAL == vorherigeSerial);
          if (gefunden != null)
          {
            AusgewaehlteSitzung = gefunden;
          }
        }

        StatusSitzungen = $"{Sitzungen.Count} Sitzungen";
      });
    }

    private void AusgewaehlteSitzungSqlLaden()
    {
      if (AusgewaehlteSitzung == null)
      {
        SetSqlData(string.Empty);
      }
      else
      {
        var ergebnis = _dbQuery.GetCurrentSqlText(AusgewaehlteSitzung.SID.Value, AusgewaehlteSitzung.SERIAL.Value);
        SetSqlData(ergebnis?.SqlText);
      }
      KopiereSitzungsSqlBefehl.RaiseCanExecuteChanged();
    }

    private void KopiereSitzungsSqlAusfuehren()
    {
      if (!string.IsNullOrEmpty(AktuellerSqlText))
      {
        Clipboard.SetText(AktuellerSqlText);
      }
    }

    private bool FilterSitzungenDelegate(object obj)
    {
      if (string.IsNullOrWhiteSpace(FilterTextSitzungen)) return true;
      if (obj is V_SESSION_MONITOR s)
      {
        string f = FilterTextSitzungen.ToLower();
        return (s.USERNAME ?? "").ToLower().Contains(f) ||
               (s.PROGRAM ?? "").ToLower().Contains(f);
      }
      return false;
    }

    private void SitzungBeendenAusfuehren()
    {
      if (AusgewaehlteSitzung == null) return;

      var commentWindow = new KillSessionCommentView { Owner = Application.Current.MainWindow };
      if (commentWindow.ShowDialog() != true) return;

      string grund = commentWindow.Comment;

      var result = MessageBox.Show(
          $"Вы действительно хотите завершить сессию SID={AusgewaehlteSitzung.SID}?\n" +
          "Это действие будет записано в Audit-Log.",
          "Bestätigung", MessageBoxButton.YesNo, MessageBoxImage.Warning);

      if (result != MessageBoxResult.Yes) return;

      try
      {
        _dbQuery.KillSession(
            AusgewaehlteSitzung.SID.Value,
            AusgewaehlteSitzung.SERIAL.Value,
            grund,
            AktuellerSqlText,
            AusgewaehlteSitzung.PROGRAM,
            AusgewaehlteSitzung.OSUSER,
            AusgewaehlteSitzung.TERMINAL
        );

        MessageBox.Show("Сессия успешно завершена.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
        SitzungenAktualisierenAusfuehren();
      }
      catch (Exception ex)
      {
        _log.LogError(ex, "Ошибка при удалении сессии.");
        MessageBox.Show($"Ошибка: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    public void AktualisierenIntern(List<V_SESSION_MONITOR> daten)
    {
      var vorherigeSid = AusgewaehlteSitzung?.SID;
      var vorherigeSerial = AusgewaehlteSitzung?.SERIAL;

      Application.Current.Dispatcher.Invoke(() =>
      {
        Sitzungen.Clear();
        foreach (var item in daten) Sitzungen.Add(item);

        if (vorherigeSid.HasValue)
        {
          var gefunden = Sitzungen.FirstOrDefault(s => s.SID == vorherigeSid && s.SERIAL == vorherigeSerial);
          if (gefunden != null) AusgewaehlteSitzung = gefunden;
        }

        SitzungenAnsicht?.Refresh();
        StatusSitzungen = $"{Sitzungen.Count} Sitzungen";
      });
    }

    #endregion
  }
}
