using System;
using System.Windows.Input;
using DbMonitorData;
using Microsoft.Extensions.DependencyInjection;
using Prism.Commands;

namespace DbMonitorWPF.ViewModels
{
  public class MainViewModel : ViewModelBase
  {
    #region Felder

    private readonly IServiceProvider _serviceProvider;
    private int _aktiverTabIndex;
    private string _globalerStatusText = string.Empty;
    private System.Timers.Timer _aktualisierungsTimer;

    #endregion

    #region Konstruktor

    public MainViewModel(
        IServiceProvider serviceProvider,
        SitzungenViewModel sitzungenVm,
        SperrenViewModel sperrenVm,
        JobsViewModel jobsVm,
        AuditLogViewModel auditLogVm,
        SqlhistorieViewModel sqlhistorieVm,
        ProzesseViewModel prozesseVm,
        DashboardViewModel dashboardVm)
    {
      _serviceProvider = serviceProvider;

      SitzungenVm = sitzungenVm;
      SperrenVm = sperrenVm;
      JobsVm = jobsVm;
      AuditLogVm = auditLogVm;
      SqlhistorieVm = sqlhistorieVm;
      ProzesseVm = prozesseVm;
      DashboardVm = dashboardVm;

      WechsleZuSitzungenBefehl = new DelegateCommand(() => AktiverTabIndex = 1);
      WechsleZuSqlBefehl = new DelegateCommand(() => AktiverTabIndex = 2);
      WechsleZuProzesseBefehl = new DelegateCommand(() => AktiverTabIndex = 3);
      WechsleZuJobsBefehl = new DelegateCommand(() => AktiverTabIndex = 4);
      WechsleZuSperrenBefehl = new DelegateCommand(() => AktiverTabIndex = 5);

      LoadedBefehl = new DelegateCommand(OnLoaded);
      UnloadedBefehl = new DelegateCommand(OnUnloaded);

      SitzungenVm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(SitzungenVm.StatusSitzungen)) GlobalenStatusTextAktualisieren(); };
      SperrenVm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(SperrenVm.StatusSperren)) GlobalenStatusTextAktualisieren(); };
      JobsVm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(JobsVm.StatusJobs)) GlobalenStatusTextAktualisieren(); };
      AuditLogVm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(AuditLogVm.Status)) GlobalenStatusTextAktualisieren(); };
      SqlhistorieVm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(SqlhistorieVm.StatusSql)) GlobalenStatusTextAktualisieren(); };
      ProzesseVm.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(ProzesseVm.StatusProzesse)) GlobalenStatusTextAktualisieren(); };

      _aktualisierungsTimer = new System.Timers.Timer(30_000);
      _aktualisierungsTimer.Elapsed += (_, __) => TimerZyklusAusfuehren();
      _aktualisierungsTimer.AutoReset = true;
    }

    #endregion

    #region Tab-ViewModels

    public SitzungenViewModel SitzungenVm { get; }
    public SperrenViewModel SperrenVm { get; }
    public JobsViewModel JobsVm { get; }
    public AuditLogViewModel AuditLogVm { get; }
    public SqlhistorieViewModel SqlhistorieVm { get; }
    public ProzesseViewModel ProzesseVm { get; }
    public DashboardViewModel DashboardVm { get; }

    #endregion

    #region Befehle

    public ICommand WechsleZuSitzungenBefehl { get; }
    public ICommand WechsleZuSqlBefehl { get; }
    public ICommand WechsleZuProzesseBefehl { get; }
    public ICommand WechsleZuJobsBefehl { get; }
    public ICommand WechsleZuSperrenBefehl { get; }
    public ICommand LoadedBefehl { get; }
    public ICommand UnloadedBefehl { get; }

    #endregion

    #region Eigenschaften

    public int AktiverTabIndex
    {
      get => _aktiverTabIndex;
      set { SetProperty(ref _aktiverTabIndex, value); GlobalenStatusTextAktualisieren(); }
    }

    public string GlobalerStatusText
    {
      get => _globalerStatusText;
      set => SetProperty(ref _globalerStatusText, value);
    }

    #endregion

    #region Private Methoden – Lifecycle

    private void OnLoaded()
    {
      AlleTabsAktualisieren();
      _aktualisierungsTimer.Start();
      GlobalenStatusTextAktualisieren();
    }

    private void OnUnloaded()
    {
      _aktualisierungsTimer.Stop();
      _aktualisierungsTimer.Dispose();
    }

    #endregion

    #region Private Methoden – Daten

    private void AlleTabsAktualisieren()
    {
      ((System.Windows.Input.ICommand)SitzungenVm?.SitzungenAktualisierenBefehl)?.Execute(null);
      ((System.Windows.Input.ICommand)SperrenVm?.SperrenAktualisierenBefehl)?.Execute(null);
      ((System.Windows.Input.ICommand)JobsVm?.JobsAktualisierenBefehl)?.Execute(null);
      AuditLogVm?.RefreshCommand?.Execute(null);
      ((System.Windows.Input.ICommand)SqlhistorieVm?.SqlAktualisierenBefehl)?.Execute(null);
      ((System.Windows.Input.ICommand)ProzesseVm?.ProzesseAktualisierenBefehl)?.Execute(null);
      DashboardVm?.Refresh();
    }
    private void TimerZyklusAusfuehren()
    {
      try
      {
        var db = _serviceProvider.GetService<DbQuery>();
        if (db == null) return;

        var sitzungen = db.GetAktuelleSessionsList();
        var sperren = db.GetLockList();
        var alleJobs = db.GetAlleJobList();
        var auffaelligJobs = db.GetAuffaelligeJobList();
        var auditLogs = db.GetAuditLogEntries();
        var sqlDaten = db.GetAllSqlList();
        var prozesse = db.GetAllProcessesList();

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
          SitzungenVm?.AktualisierenIntern(sitzungen);
          SperrenVm?.AktualisierenIntern(sperren);
          JobsVm?.AktualisierenIntern(alleJobs, auffaelligJobs);
          AuditLogVm?.AktualisierenIntern(auditLogs);
          SqlhistorieVm?.AktualisierenIntern(sqlDaten);
          ProzesseVm?.AktualisierenIntern(prozesse);
          DashboardVm?.Refresh();
          GlobalenStatusTextAktualisieren();
        });
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Timer-Fehler: {ex.Message}");
      }
    }

    #endregion

    #region Private Methoden – Status

    private void GlobalenStatusTextAktualisieren()
    {
      string status = null;
      switch (_aktiverTabIndex)
      {
        case 1: status = SitzungenVm?.StatusSitzungen; break;
        case 2: status = SqlhistorieVm?.StatusSql; break;
        case 3: status = ProzesseVm?.StatusProzesse; break;
        case 4: status = JobsVm?.StatusJobs; break;
        case 5: status = SperrenVm?.StatusSperren; break;
        case 6: status = AuditLogVm?.Status; break;
      }
      GlobalerStatusText = string.IsNullOrEmpty(status) ? "Bereit" : status;
    }

    #endregion
  }
}
