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
using Microsoft.Extensions.Logging;
using Prism.Commands;

namespace DbMonitorWPF.ViewModels
{
  public class AuditLogViewModel : ViewModelBase
  {
    private readonly DbQuery _dbQuery;
    private string _filterText = string.Empty;
    private string _status;

    public ObservableCollection<AuditLogEntry> AuditEntries { get; } = new ObservableCollection<AuditLogEntry>();
    public ICollectionView AuditLogAnsicht { get; }

    public AuditLogViewModel(DbQuery dbQuery)
    {
      _dbQuery = dbQuery;
      AuditLogAnsicht = CollectionViewSource.GetDefaultView(AuditEntries);
      AuditLogAnsicht.Filter = FilterAuditLog;
      RefreshCommand = new DelegateCommand(LoadData);
    }

    public string FilterText
    {
      get => _filterText;
      set { _filterText = value; OnPropertyChanged(); AuditLogAnsicht?.Refresh(); }
    }

    public string Status
    {
      get => _status;
      set { _status = value; OnPropertyChanged(); }
    }

    public ICommand RefreshCommand { get; }

    public void LoadData()
    {
      var data = _dbQuery.GetAuditLogEntries() ?? new List<AuditLogEntry>();

      Application.Current.Dispatcher.Invoke(() =>
      {
        AuditEntries.Clear();
        foreach (var item in data) AuditEntries.Add(item);
        Status = $"{AuditEntries.Count} Audit Logs";
      });
    }

    private bool FilterAuditLog(object obj)
    {
      if (string.IsNullOrWhiteSpace(FilterText)) return true;
      if (obj is AuditLogEntry entry)
      {
        string f = FilterText.Trim().ToLower();
        return (entry.DAL_USERNAME ?? "").ToLower().Contains(f) ||
               (entry.DAL_REASON ?? "").ToLower().Contains(f) ||
               (entry.DAL_PROGRAM ?? "").ToLower().Contains(f) ||
               (entry.DAL_TERMINAL ?? "").ToLower().Contains(f) ||
               (entry.DAL_OSUSER ?? "").ToLower().Contains(f);
      }
      return false;
    }

    public void AktualisierenIntern(List<AuditLogEntry> daten)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        AuditEntries.Clear();
        foreach (var item in daten) AuditEntries.Add(item);
        AuditLogAnsicht?.Refresh();
        Status = $"{AuditEntries.Count} Audit Logs";
      });
    }
  }
}
