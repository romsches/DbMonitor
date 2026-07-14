using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using DbMonitorData;
using Microsoft.Extensions.Logging;
using Prism.Commands;

namespace DbMonitorWPF.ViewModels
{
  public class ProzesseViewModel : ViewModelBase
  {
    private readonly ILogger _log;
    private readonly DbQuery _dbQuery;
    private V_PROCESS _ausgewaehlterProzess;
    private string _filterTextProzesse;
    private string _statusProzesse;
    private bool _istTop20Modus = false;

    public bool IstTop20Modus => _istTop20Modus;

    public ICommand AlleProzesseLadenBefehl { get; }
    public ICommand LadeTopProzesseBefehl { get; }
    public ICommand ProzesseAktualisierenBefehl { get; }

    public ObservableCollection<V_PROCESS> Prozesse { get; }
    public ICollectionView ProzesseAnsicht { get; }

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

    // --- ДОБАВЛЕННЫЙ МЕТОД для MainViewModel ---
    public void SortiereNachSpeicher()
    {
      LadeTopProzesseAusfuehren();
    }

    public V_PROCESS AusgewaehlterProzess
    {
      get => _ausgewaehlterProzess;
      set { _ausgewaehlterProzess = value; OnPropertyChanged(); }
    }

    public string FilterTextProzesse
    {
      get => _filterTextProzesse;
      set { _filterTextProzesse = value; OnPropertyChanged(); ProzesseAnsicht?.Refresh(); }
    }

    public string StatusProzesse
    {
      get => _statusProzesse;
      set { _statusProzesse = value; OnPropertyChanged(); }
    }

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

    private void UpdateCollection(List<V_PROCESS> liste, string statusBase)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        Prozesse.Clear();
        foreach (var p in liste) Prozesse.Add(p);
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

    public void AktualisierenIntern(List<V_PROCESS> daten)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        // Если мы в режиме "Топ", не перезаписываем его на "Все" при авто-апдейте
        if (!_istTop20Modus)
        {
          Prozesse.Clear();
          foreach (var item in daten) Prozesse.Add(item);
          ProzesseAnsicht?.Refresh();
          StatusProzesse = $"{Prozesse.Count} Prozesse";
        }
      });
    }
  }
}
