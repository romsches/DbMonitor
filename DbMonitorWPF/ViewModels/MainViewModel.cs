using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
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
    private readonly Timer _aktualisierungsTimer;
    private int _aktiverTabIndex;
    private string _globalerStatusText = "Bereit";

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

      InitializeCommands();
      SubscribeToStatusChanges();

      _aktualisierungsTimer = new Timer(30_000);
      _aktualisierungsTimer.AutoReset = true;
      _aktualisierungsTimer.Elapsed += (s, e) => TimerZyklusAusfuehren();
    }

    #endregion

    #region Eigenschaften

    public SitzungenViewModel SitzungenVm { get; }
    public SperrenViewModel SperrenVm { get; }
    public JobsViewModel JobsVm { get; }
    public AuditLogViewModel AuditLogVm { get; }
    public SqlhistorieViewModel SqlhistorieVm { get; }
    public ProzesseViewModel ProzesseVm { get; }
    public DashboardViewModel DashboardVm { get; }

    public int AktiverTabIndex
    {
      get => _aktiverTabIndex;
      set
      {
        if (SetProperty(ref _aktiverTabIndex, value))
          GlobalenStatusTextAktualisieren();
      }
    }

    public string GlobalerStatusText
    {
      get => _globalerStatusText;
      set => SetProperty(ref _globalerStatusText, value);
    }

    #endregion

    #region Befehle

    public ICommand WechsleZuSitzungenBefehl { get; private set; }
    public ICommand WechsleZuSqlBefehl { get; private set; }
    public ICommand WechsleZuProzesseBefehl { get; private set; }
    public ICommand WechsleZuJobsBefehl { get; private set; }
    public ICommand WechsleZuSperrenBefehl { get; private set; }
    public ICommand LadeTopSqlBefehl { get; private set; }
    public ICommand LadeTopProzesseBefehl { get; private set; }
    public ICommand LoadedBefehl { get; private set; }
    public ICommand UnloadedBefehl { get; private set; }

    private void InitializeCommands()
    {
      WechsleZuSitzungenBefehl = new DelegateCommand(() => AktiverTabIndex = 1);
      WechsleZuSqlBefehl = new DelegateCommand(() => AktiverTabIndex = 2);
      WechsleZuProzesseBefehl = new DelegateCommand(() => AktiverTabIndex = 3);
      WechsleZuJobsBefehl = new DelegateCommand(() => AktiverTabIndex = 4);
      WechsleZuSperrenBefehl = new DelegateCommand(() => AktiverTabIndex = 5);

      LadeTopSqlBefehl = new DelegateCommand(LadeTopSqlAusfuehren);
      LadeTopProzesseBefehl = new DelegateCommand(LadeTopProzesseAusfuehren);

      LoadedBefehl = new DelegateCommand(OnLoaded);
      UnloadedBefehl = new DelegateCommand(OnUnloaded);
    }

    #endregion

    #region Methoden – Lifecycle & Daten

    private void OnLoaded()
    {
      AlleTabsAktualisieren();
      _aktualisierungsTimer.Start();
    }

    private void OnUnloaded()
    {
      _aktualisierungsTimer.Stop();
      _aktualisierungsTimer.Dispose();
    }

    private void AlleTabsAktualisieren()
    {
      ((ICommand)SitzungenVm?.SitzungenAktualisierenBefehl)?.Execute(null);
      ((ICommand)SperrenVm?.SperrenAktualisierenBefehl)?.Execute(null);
      ((ICommand)JobsVm?.JobsAktualisierenBefehl)?.Execute(null);
      AuditLogVm?.RefreshCommand?.Execute(null);
      ((ICommand)SqlhistorieVm?.SqlAktualisierenBefehl)?.Execute(null);
      ((ICommand)ProzesseVm?.ProzesseAktualisierenBefehl)?.Execute(null);
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

        Application.Current.Dispatcher.Invoke(() =>
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
        System.Diagnostics.Debug.WriteLine(ex.Message);
      }
    }

    #endregion

    #region Status-Logik

    private void SubscribeToStatusChanges()
    {
      SitzungenVm.PropertyChanged += HandleStatus;
      SperrenVm.PropertyChanged += HandleStatus;
      JobsVm.PropertyChanged += HandleStatus;
      AuditLogVm.PropertyChanged += HandleStatus;
      SqlhistorieVm.PropertyChanged += HandleStatus;
      ProzesseVm.PropertyChanged += HandleStatus;
    }

    private void HandleStatus(object sender, PropertyChangedEventArgs e)
    {
      GlobalenStatusTextAktualisieren();
    }

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

      GlobalerStatusText = string.IsNullOrWhiteSpace(status) ? "Bereit" : status;
    }

    private void LadeTopSqlAusfuehren() { SqlhistorieVm?.LadeTopSqlBefehl?.Execute(null); AktiverTabIndex = 2; }
    private void LadeTopProzesseAusfuehren() { ProzesseVm?.LadeTopProzesseBefehl?.Execute(null); AktiverTabIndex = 3; }

    #endregion
  }
}
