using System;

namespace DbMonitorWPF.ViewModels
{
  public class DashboardViewModel : ViewModelBase
  {
    private readonly SitzungenViewModel _sitzungenVm;
    private readonly SperrenViewModel _sperrenVm;
    private readonly JobsViewModel _jobsVm;

    public DashboardViewModel(
        SitzungenViewModel sitzungenVm,
        SperrenViewModel sperrenVm,
        JobsViewModel jobsVm)
    {
      _sitzungenVm = sitzungenVm;
      _sperrenVm = sperrenVm;
      _jobsVm = jobsVm;
    }

    public int AnzahlSitzungen => _sitzungenVm?.Sitzungen?.Count ?? 0;
    public int AnzahlSperren => _sperrenVm?.Sperren?.Count ?? 0;
    public int AnzahlAuffaelligeJobs => _jobsVm?.AuffaelligeJobs?.Count ?? 0;

    public void Refresh()
    {
      OnPropertyChanged(nameof(AnzahlSitzungen));
      OnPropertyChanged(nameof(AnzahlSperren));
      OnPropertyChanged(nameof(AnzahlAuffaelligeJobs));
    }
  }
}
