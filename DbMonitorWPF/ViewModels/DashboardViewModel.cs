using System;
using System.Windows.Input;
using DbMonitorData;
using DbMonitorWPF.Helpers;
using Microsoft.Extensions.Logging;
using System.Windows.Threading;

namespace DbMonitorWPF.ViewModels
{
  /// <summary>
  /// Das Dashboard-ViewModel zeigt aggregierte Statusinformationen an.
  /// Es greift lesend auf andere ViewModels zu.
  /// </summary>
  public class DashboardViewModel : ViewModelBase
  {
    // Ссылки на ViewModels, которые нужны для отображения
    public SitzungenViewModel SitzungenVm { get; set; }
    public SperrenViewModel SperrenVm { get; set; }
    public JobsViewModel JobsVm { get; set; }

    public DashboardViewModel() { } // Пустой конструктор

    public int AnzahlSitzungen => SitzungenVm?.Sitzungen?.Count ?? 0;
    public int AnzahlSperren => SperrenVm?.Sperren?.Count ?? 0;
    public int AnzahlAuffaelligeJobs => JobsVm?.AuffaelligeJobs?.Count ?? 0;

    public void Refresh()
    {
      OnPropertyChanged(nameof(AnzahlSitzungen));
      OnPropertyChanged(nameof(AnzahlSperren));
      OnPropertyChanged(nameof(AnzahlAuffaelligeJobs));
    }
  }
}
