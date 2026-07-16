using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
  /// Verwaltet die Ansicht und Aktualisierung der Datenbankprozesse.
  /// </summary>
  public class ProzesseViewModel : ViewModelBase
  {
    #region Felder

    private readonly ILogger _log;
    private readonly DbQuery _dbQuery;
    private V_PROCESS _ausgewaehlterProzess;
    private string _filterTextProzesse = string.Empty;
    private string _statusProzesse;
    private bool _istTop20Modus;

    #endregion

    #region Konstruktor

    public ProzesseViewModel(DbQuery dbQuery, ILogger<ProzesseViewModel> log)
    {
      _dbQuery = dbQuery;
      _log = log;

      Prozesse = new ObservableCollection<V_PROCESS>();
      ProzesseAnsicht = CollectionViewSource.GetDefaultView(Prozesse);
      ProzesseAnsicht.Filter = FilterProzesseDelegate;

      AlleProzesseLadenBefehl = new DelegateCommand(LadeAlleProzesseAusfuehren);
      LadeTopProzesseBefehl = new DelegateCommand(LadeTopProzesseAusfuehren);
      ProzesseAktualisierenBefehl = new DelegateCommand(ProzesseAktualisierenAusfuehren);
    }

    #endregion

    #region Eigenschaften

    public ObservableCollection<V_PROCESS> Prozesse { get; }
    public ICollectionView ProzesseAnsicht { get; }

    public bool IstTop20Modus => _istTop20Modus;

    public V_PROCESS AusgewaehlterProzess
    {
      get => _ausgewaehlterProzess;
      set => SetProperty(ref _ausgewaehlterProzess, value);
    }

    public string FilterTextProzesse
    {
      get => _filterTextProzesse;
      set
      {
        if (SetProperty(ref _filterTextProzesse, value))
          ProzesseAnsicht?.Refresh();
      }
    }

    public string StatusProzesse
    {
      get => _statusProzesse;
      set => SetProperty(ref _statusProzesse, value);
    }

    public ICommand AlleProzesseLadenBefehl { get; }
    public ICommand LadeTopProzesseBefehl { get; }
    public ICommand ProzesseAktualisierenBefehl { get; }

    #endregion

    #region Methoden

    public void SortiereNachSpeicher() => LadeTopProzesseAusfuehren();

    private void ProzesseAktualisierenAusfuehren()
    {
      if (_istTop20Modus) LadeTopProzesseAusfuehren();
      else LadeAlleProzesseAusfuehren();
    }

    public void LadeTopProzesseAusfuehren()
    {
      _istTop20Modus = true;
      try
      {
        var liste = _dbQuery.GetTopMemoryProcessesList() ?? new List<V_PROCESS>();
        UpdateCollection(liste, "Top-Prozesse (nach Speicher)");
      }
      catch (Exception ex)
      {
        _log.LogError(ex, "Fehler beim Laden der Top-Prozesse.");
        StatusProzesse = "Fehler beim Laden.";
      }
    }

    public void LadeAlleProzesseAusfuehren()
    {
      _istTop20Modus = false;
      var liste = _dbQuery.GetAllProcessesList() ?? new List<V_PROCESS>();
      UpdateCollection(liste, "Prozesse");
    }

    public void AktualisierenIntern(List<V_PROCESS> daten)
    {
      if (_istTop20Modus) return; // Modus beibehalten

      Application.Current.Dispatcher.Invoke(() =>
      {
        CollectionHelper.SyncForce(Prozesse, daten);
        StatusProzesse = $"{Prozesse.Count} Prozesse";
      });
    }

    private void UpdateCollection(List<V_PROCESS> liste, string statusBase)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        CollectionHelper.SyncForce(Prozesse, liste);
        ProzesseAnsicht?.Refresh();
        StatusProzesse = $"{liste.Count} {statusBase}";
      });
    }

    private bool FilterProzesseDelegate(object obj)
    {
      if (string.IsNullOrWhiteSpace(FilterTextProzesse)) return true;
      if (obj is V_PROCESS p)
      {
        string f = FilterTextProzesse.ToLower().Trim();
        return (p.USERNAME ?? "").ToLower().Contains(f) ||
               (p.PROGRAM ?? "").ToLower().Contains(f) ||
               (p.BACKGROUND ?? "").ToLower().Contains(f) ||
               (p.SPID?.ToString() ?? "").Contains(f) ||
               (p.ADDR ?? "").ToLower().Contains(f);
      }
      return false;
    }

    #endregion
  }
}
