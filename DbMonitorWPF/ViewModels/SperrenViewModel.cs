using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using DbMonitorData;
using DbMonitorWPF.Helpers;
using DbMonitorWPF.Views;
using Microsoft.Extensions.Logging;
using Prism.Commands;

namespace DbMonitorWPF.ViewModels
{
  public class SperrenViewModel : SqlBaseViewModel
  {
    private readonly ILogger _log;
    private readonly DbQuery _dbQuery;
    private string _filterTextSperren = string.Empty;
    private string _statusSperren = string.Empty;
    private V_LOCK _ausgewaehlteSperre;

    public ObservableCollection<V_LOCK> Sperren { get; }
    public ICollectionView SperrenAnsicht { get; }

    public ICommand SperrenAktualisierenBefehl { get; }
    public ICommand SperreBeendenBefehl { get; }
    public ICommand KopiereSperrenSqlBefehl { get; }

    public SperrenViewModel(DbQuery dbQuery, ILogger<SperrenViewModel> log)
    {
      _dbQuery = dbQuery;
      _log = log;

      Sperren = new ObservableCollection<V_LOCK>();
      SperrenAnsicht = CollectionViewSource.GetDefaultView(Sperren);
      SperrenAnsicht.Filter = FilterSperrenDelegate;

      SperrenAktualisierenBefehl = new DelegateCommand(SperrenAktualisierenAusfuehren);
      SperreBeendenBefehl = new DelegateCommand(SperreBeendenAusfuehren, () => AusgewaehlteSperre != null);
      KopiereSperrenSqlBefehl = new DelegateCommand(KopiereSperrenSqlAusfuehren, () => !string.IsNullOrEmpty(AktuellerSqlText));
    }

    public V_LOCK AusgewaehlteSperre
    {
      get => _ausgewaehlteSperre;
      set
      {
        _ausgewaehlteSperre = value;
        OnPropertyChanged();
        AusgewaehlteSperreSqlLaden();
        ((DelegateCommand)SperreBeendenBefehl).RaiseCanExecuteChanged();
        ((DelegateCommand)KopiereSperrenSqlBefehl).RaiseCanExecuteChanged();
      }
    }

    public bool IsSperreSqlVerfuegbar => !string.IsNullOrWhiteSpace(AktuellerSqlText);

    public string FilterTextSperren
    {
      get => _filterTextSperren;
      set { _filterTextSperren = value; OnPropertyChanged(); SperrenAnsicht?.Refresh(); }
    }

    public string StatusSperren
    {
      get => _statusSperren;
      set { _statusSperren = value; OnPropertyChanged(); }
    }

    private void SperrenAktualisierenAusfuehren()
    {
      var vorherigeSid = AusgewaehlteSperre?.SID;
      var vorherigeSerial = AusgewaehlteSperre?.SERIAL;

      var liste = _dbQuery.GetLockList() ?? new List<V_LOCK>();

      Application.Current.Dispatcher.Invoke(() =>
      {
        Sperren.Clear();
        foreach (var item in liste) Sperren.Add(item);

        if (vorherigeSid.HasValue)
        {
          AusgewaehlteSperre = Sperren.FirstOrDefault(s => s.SID == vorherigeSid && s.SERIAL == vorherigeSerial);
        }

        SperrenAnsicht?.Refresh();
        StatusSperren = $"{Sperren.Count} aktive Sperren";
      });
    }

    private void AusgewaehlteSperreSqlLaden()
    {
      if (_ausgewaehlteSperre == null)
      {
        SetSqlData(string.Empty);
        return;
      }
      var ergebnis = _dbQuery.GetCurrentSqlText(_ausgewaehlteSperre.SID, _ausgewaehlteSperre.SERIAL);
      SetSqlData(ergebnis?.SqlText);
      ((DelegateCommand)KopiereSperrenSqlBefehl).RaiseCanExecuteChanged();
    }

    private void KopiereSperrenSqlAusfuehren()
    {
      if (!string.IsNullOrEmpty(AktuellerSqlText))
        Clipboard.SetText(AktuellerSqlText);
    }

    private void SperreBeendenAusfuehren()
    {
      if (_ausgewaehlteSperre == null) return;

      var commentDialog = new KillSessionCommentView { Owner = Application.Current.MainWindow };
      if (commentDialog.ShowDialog() != true) return;

      var result = MessageBox.Show(
          $"Вы действительно хотите снять блокировку SID={_ausgewaehlteSperre.SID}?",
          "Sperrung beenden", MessageBoxButton.YesNo, MessageBoxImage.Warning);

      if (result != MessageBoxResult.Yes) return;

      try
      {
        _dbQuery.KillSession(
            _ausgewaehlteSperre.SID,
            _ausgewaehlteSperre.SERIAL,
            commentDialog.Comment,
            AktuellerSqlText,
            _ausgewaehlteSperre.PROGRAM,
            _ausgewaehlteSperre.OSUSER,
            _ausgewaehlteSperre.TERMINAL,
            "KILL_LOCK"
        );

        MessageBox.Show("Sperrung erfolgreich beendet.", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
        SperrenAktualisierenAusfuehren();
      }
      catch (Exception ex)
      {
        _log.LogError(ex, "Fehler beim Beenden der Sperre.");
        MessageBox.Show($"Fehler: {ex.Message}");
      }
    }

    private bool FilterSperrenDelegate(object obj)
    {
      if (string.IsNullOrWhiteSpace(FilterTextSperren)) return true;
      if (obj is V_LOCK l)
      {
        string f = FilterTextSperren.ToLower().Trim();
        return (l.SID.ToString().Contains(f)) || (l.USERNAME ?? "").ToLower().Contains(f) ||
               (l.OSUSER ?? "").ToLower().Contains(f) || (l.PROGRAM ?? "").ToLower().Contains(f);
      }
      return false;
    }

    public void AktualisierenIntern(List<V_LOCK> daten)
    {
      var vorherigeSid = AusgewaehlteSperre?.SID;
      var vorherigeSerial = AusgewaehlteSperre?.SERIAL;

      Application.Current.Dispatcher.Invoke(() =>
      {
        Sperren.Clear();
        foreach (var item in daten) Sperren.Add(item);

        if (vorherigeSid.HasValue)
        {
          AusgewaehlteSperre = Sperren.FirstOrDefault(s => s.SID == vorherigeSid && s.SERIAL == vorherigeSerial);
        }

        SperrenAnsicht?.Refresh();
        StatusSperren = $"{Sperren.Count} aktive Sperren";
      });
    }
  }
}
