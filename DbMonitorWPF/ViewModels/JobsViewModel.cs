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
  public class JobsViewModel : ViewModelBase
  {
    private readonly DbQuery _dbQuery;
    private readonly ILogger _log;
    private string _filterTextJobs = string.Empty;
    private string _statusJobs;
    private string _statusJobProtokolle;
    private V_JOB _ausgewaehlterJob;

    public JobsViewModel(DbQuery dbQuery, ILogger<JobsViewModel> log)
    {
      _dbQuery = dbQuery;
      _log = log;

      Jobs = new ObservableCollection<V_JOB>();
      AuffaelligeJobs = new ObservableCollection<V_JOB>();
      AusgewaehlteJobProtokolle = new ObservableCollection<V_JOB_RUN_DETAILS>();

      JobsAnsicht = CollectionViewSource.GetDefaultView(Jobs);
      JobsAnsicht.Filter = FilterJobsDelegate;

      JobsAktualisierenBefehl = new DelegateCommand(JobsAktualisierenAusfuehren);
      FilterFehlerhafteJobsCommand = new DelegateCommand(FilterFehlerhafteAusfuehren);
    }

    public ObservableCollection<V_JOB> Jobs { get; }
    public ObservableCollection<V_JOB> AuffaelligeJobs { get; }
    public ObservableCollection<V_JOB_RUN_DETAILS> AusgewaehlteJobProtokolle { get; }
    public ICollectionView JobsAnsicht { get; }

    public V_JOB AusgewaehlterJob
    {
      get => _ausgewaehlterJob;
      set
      {
        _ausgewaehlterJob = value;
        OnPropertyChanged();
        if (value != null) JobProtokolleLaden(value);
      }
    }

    public string FilterTextJobs
    {
      get => _filterTextJobs;
      set { _filterTextJobs = value; OnPropertyChanged(); JobsAnsicht?.Refresh(); }
    }

    public string StatusJobs
    {
      get => _statusJobs;
      set { _statusJobs = value; OnPropertyChanged(); }
    }

    public string StatusJobProtokolle
    {
      get => _statusJobProtokolle;
      set { _statusJobProtokolle = value; OnPropertyChanged(); }
    }

    public ICommand JobsAktualisierenBefehl { get; }
    public ICommand FilterFehlerhafteJobsCommand { get; }

    private void JobsAktualisierenAusfuehren()
    {
      var alleJobs = _dbQuery.GetAlleJobList() ?? new List<V_JOB>();
      var auffaelligJobs = _dbQuery.GetAuffaelligeJobList() ?? new List<V_JOB>();

      Application.Current.Dispatcher.Invoke(() =>
      {
        Jobs.Clear();
        foreach (var j in alleJobs) Jobs.Add(j);

        AuffaelligeJobs.Clear();
        foreach (var j in auffaelligJobs) AuffaelligeJobs.Add(j);

        JobsAnsicht?.Refresh();
        StatusJobs = $"{alleJobs.Count} Jobs ({auffaelligJobs.Count} auffällig)";
      });
    }

    private void JobProtokolleLaden(V_JOB job)
    {
      var protokolle = _dbQuery.GetJobLog(job.JOB_NAME) ?? new List<V_JOB_RUN_DETAILS>();

      Application.Current.Dispatcher.Invoke(() =>
      {
        AusgewaehlteJobProtokolle.Clear();
        foreach (var p in protokolle) AusgewaehlteJobProtokolle.Add(p);

        StatusJobProtokolle = protokolle.Count > 0
            ? $"{protokolle.Count} Protokolle für {job.JOB_NAME}"
            : $"Kein Protokoll für {job.JOB_NAME}";
      });
    }

    private void FilterFehlerhafteAusfuehren()
    {
      JobsAnsicht.Filter = (obj) =>
      {
        if (obj is V_JOB j) return j.FAILURE_COUNT > 0 || j.STATUS != "SUCCEEDED";
        return false;
      };
    }

    private bool FilterJobsDelegate(object obj)
    {
      if (string.IsNullOrWhiteSpace(_filterTextJobs)) return true;
      if (obj is V_JOB j)
      {
        string f = _filterTextJobs.ToLower();
        return (j.JOB_NAME ?? "").ToLower().Contains(f) ||
               (j.JOB_TYPE ?? "").ToLower().Contains(f) ||
               (j.JOB_ACTION ?? "").ToLower().Contains(f) ||
               (j.STATE ?? "").ToLower().Contains(f) ||
               (j.COMMENTS ?? "").ToLower().Contains(f);
      }
      return false;
    }

    public void AktualisierenIntern(List<V_JOB> daten)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        Jobs.Clear();
        foreach (var item in daten) Jobs.Add(item);
        JobsAnsicht?.Refresh();
        StatusJobs = $"{Jobs.Count} Jobs";
      });
    }
    public void AktualisierenIntern(List<V_JOB> alle, List<V_JOB> auffaellig)
    {
      Application.Current.Dispatcher.Invoke(() =>
      {
        Jobs.Clear();
        foreach (var item in alle) Jobs.Add(item);

        AuffaelligeJobs.Clear();
        foreach (var item in auffaellig) AuffaelligeJobs.Add(item);

        JobsAnsicht?.Refresh();
        StatusJobs = $"{Jobs.Count} Jobs ({AuffaelligeJobs.Count} auffällig)";
      });
    }
  }
}
