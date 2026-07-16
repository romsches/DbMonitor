using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using DbMonitorData;
using Prism.Commands;

namespace DbMonitorWPF.ViewModels
{
  /// <summary>
  /// ViewModel zur Verwaltung und Filterung des Audit-Logs.
  /// </summary>
  public class AuditLogViewModel : ViewModelBase
  {
    #region Felder

    private readonly DbQuery _dbQuery;
    private string _filterText = string.Empty;
    private string _status = string.Empty;

    #endregion

    #region Eigenschaften

    public ObservableCollection<AuditLogEntry> AuditEntries { get; } = new ObservableCollection<AuditLogEntry>();

    public ICollectionView AuditLogAnsicht { get; }

    public string FilterText
    {
      get => _filterText;
      set
      {
        if (SetProperty(ref _filterText, value))
        {
          AuditLogAnsicht?.Refresh();
        }
      }
    }

    public string Status
    {
      get => _status;
      set => SetProperty(ref _status, value);
    }

    public ICommand RefreshCommand { get; }

    #endregion

    #region Konstruktor

    public AuditLogViewModel(DbQuery dbQuery)
    {
      _dbQuery = dbQuery;

      AuditLogAnsicht = CollectionViewSource.GetDefaultView(AuditEntries);
      AuditLogAnsicht.Filter = FilterAuditLog;

      RefreshCommand = new DelegateCommand(LoadData);
    }

    #endregion

    #region Methoden

    /// <summary>
    /// Lädt die Audit-Daten aus der Datenbank.
    /// </summary>
    public void LoadData()
    {
      var data = _dbQuery.GetAuditLogEntries() ?? new List<AuditLogEntry>();
      AktualisierenIntern(data);
    }

    /// <summary>
    /// Aktualisiert die UI-Liste thread-sicher.
    /// </summary>
    public void AktualisierenIntern(List<AuditLogEntry> daten)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        AuditEntries.Clear();
        foreach (var item in daten)
        {
          AuditEntries.Add(item);
        }

        AuditLogAnsicht?.Refresh();
        Status = $"{AuditEntries.Count} Audit-Einträge gefunden";
      });
    }

    private bool FilterAuditLog(object obj)
    {
      if (string.IsNullOrWhiteSpace(FilterText)) return true;

      if (obj is AuditLogEntry entry)
      {
        string filter = FilterText.Trim().ToLower();

        return (entry.DAL_USERNAME ?? "").ToLower().Contains(filter) ||
               (entry.DAL_REASON ?? "").ToLower().Contains(filter) ||
               (entry.DAL_PROGRAM ?? "").ToLower().Contains(filter) ||
               (entry.DAL_TERMINAL ?? "").ToLower().Contains(filter) ||
               (entry.DAL_OSUSER ?? "").ToLower().Contains(filter);
      }

      return false;
    }

    #endregion
  }
}
