using System;

namespace DbMonitorWPF.ViewModels
{
  /// <summary>
  /// Zentrales ViewModel für das Dashboard.
  /// Dient als Aggregator für verschiedene Unter-ViewModels und stellt zusammenfassende Kennzahlen bereit.
  /// </summary>
  public class DashboardViewModel : ViewModelBase
  {
    #region Felder

    private readonly SitzungenViewModel _sitzungenVm;
    private readonly SperrenViewModel _sperrenVm;
    private readonly JobsViewModel _jobsVm;

    #endregion

    #region Konstruktor

    public DashboardViewModel(
        SitzungenViewModel sitzungenVm,
        SperrenViewModel sperrenVm,
        JobsViewModel jobsVm)
    {
      _sitzungenVm = sitzungenVm ?? throw new ArgumentNullException(nameof(sitzungenVm));
      _sperrenVm = sperrenVm ?? throw new ArgumentNullException(nameof(sperrenVm));
      _jobsVm = jobsVm ?? throw new ArgumentNullException(nameof(jobsVm));
    }

    #endregion

    #region Eigenschaften

    /// <summary>Ruft die Anzahl der aktuell aktiven Sitzungen ab.</summary>
    public int AnzahlSitzungen => _sitzungenVm.Sitzungen?.Count ?? 0;

    /// <summary>Ruft die Anzahl der aktuell bestehenden Sperren ab.</summary>
    public int AnzahlSperren => _sperrenVm.Sperren?.Count ?? 0;

    /// <summary>Ruft die Anzahl der auffälligen Datenbank-Jobs ab.</summary>
    public int AnzahlAuffaelligeJobs => _jobsVm.AuffaelligeJobs?.Count ?? 0;

    #endregion

    #region Methoden

    /// <summary>
    /// Aktualisiert die Anzeige aller aggregierten Kennzahlen im Dashboard.
    /// </summary>
    public void Refresh()
    {
      OnPropertyChanged(nameof(AnzahlSitzungen));
      OnPropertyChanged(nameof(AnzahlSperren));
      OnPropertyChanged(nameof(AnzahlAuffaelligeJobs));
    }

    #endregion
  }
}
