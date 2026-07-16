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
  /// <summary>
  /// ViewModel für die Verwaltung und Überwachung von Datenbank-Jobs.
  /// </summary>
  public class JobsViewModel : ViewModelBase
  {
    #region Felder

    private readonly DbQuery _dbQuery;
    private readonly ILogger _log;

    private string _filterTextJobs = string.Empty;
    private string _statusJobs;
    private string _statusJobProtokolle;
    private V_JOB _ausgewaehlterJob;

    #endregion

    #region Konstruktor

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

    #endregion

    #region Eigenschaften

    public ObservableCollection<V_JOB> Jobs { get; }
    public ObservableCollection<V_JOB> AuffaelligeJobs { get; }
    public ObservableCollection<V_JOB_RUN_DETAILS> AusgewaehlteJobProtokolle { get; }
    public ICollectionView JobsAnsicht { get; }

    public V_JOB AusgewaehlterJob
    {
      get => _ausgewaehlterJob;
      set
      {
        if (SetProperty(ref _ausgewaehlterJob, value) && value != null)
        {
          JobProtokolleLaden(value);
        }
      }
    }

    public string FilterTextJobs
    {
      get => _filterTextJobs;
      set
      {
        if (SetProperty(ref _filterTextJobs, value))
        {
          JobsAnsicht?.Refresh();
        }
      }
    }

    public string StatusJobs
    {
      get => _statusJobs;
      set => SetProperty(ref _statusJobs, value);
    }

    public string StatusJobProtokolle
    {
      get => _statusJobProtokolle;
      set => SetProperty(ref _statusJobProtokolle, value);
    }

    #endregion

    #region Befehle

    public ICommand JobsAktualisierenBefehl { get; }
    public ICommand FilterFehlerhafteJobsCommand { get; }

    #endregion

    #region Methoden

    private void JobsAktualisierenAusfuehren()
    {
      var alleJobs = _dbQuery.GetAlleJobList() ?? new List<V_JOB>();
      var auffaelligJobs = _dbQuery.GetAuffaelligeJobList() ?? new List<V_JOB>();

      Application.Current.Dispatcher.Invoke(() =>
      {
        CollectionHelper.SyncForce(Jobs, alleJobs);
        CollectionHelper.SyncForce(AuffaelligeJobs, auffaelligJobs);

        JobsAnsicht?.Refresh();
        StatusJobs = $"{Jobs.Count} Jobs ({AuffaelligeJobs.Count} auffällig)";
      });
    }

    private void JobProtokolleLaden(V_JOB job)
    {
      var protokolle = _dbQuery.GetJobLog(job.JOB_NAME) ?? new List<V_JOB_RUN_DETAILS>();

      Application.Current.Dispatcher.Invoke(() =>
      {
        CollectionHelper.SyncForce(AusgewaehlteJobProtokolle, protokolle);

        StatusJobProtokolle = protokolle.Count > 0
            ? $"{protokolle.Count} Protokolle für {job.JOB_NAME}"
            : $"Kein Protokoll für {job.JOB_NAME}";
      });
    }

    private void FilterFehlerhafteAusfuehren()
    {
      JobsAnsicht.Filter = obj => obj is V_JOB j && (j.FAILURE_COUNT > 0 || j.STATUS != "SUCCEEDED");
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

    public void AktualisierenIntern(List<V_JOB> alleJobs, List<V_JOB> auffaelligJobs)
    {
      if (alleJobs == null || alleJobs.Count == 0) return;

      string ausgewaehlterName = _ausgewaehlterJob?.JOB_NAME;

      Application.Current.Dispatcher.Invoke(() =>
      {
        CollectionHelper.SyncForce(Jobs, alleJobs);
        CollectionHelper.SyncForce(AuffaelligeJobs, auffaelligJobs ?? new List<V_JOB>());

        JobsAnsicht?.Refresh();

        if (ausgewaehlterName != null)
        {
          AusgewaehlterJob = Jobs.FirstOrDefault(j => j.JOB_NAME == ausgewaehlterName);
        }

        StatusJobs = StatusHelper.FormatStatus(Jobs.Count, "Jobs") +
                     $" ({AuffaelligeJobs.Count} auffällig)";
      });
    }

    #endregion
  }
}
