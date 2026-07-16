using DbMonitorWPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DbMonitorWPF
{
  /// <summary>
  /// Stellt Erweiterungsmethoden zur Registrierung der WPF-spezifischen Abhängigkeiten bereit.
  /// </summary>
  public static class DiProvider
  {
    #region Methoden

    /// <summary>
    /// Registriert alle notwendigen ViewModels und WPF-Dienste für das Modul <c>DbMonitorWPF</c>.
    /// </summary>
    /// <param name="services">Die <see cref="IServiceCollection"/> zur Registrierung.</param>
    public static void InitDependencyInjectionForDbMonitorWPF(this IServiceCollection services)
    {
      if (services == null) return;

      // ViewModels
      // Registriert als Transient, um bei jeder Navigation eine frische Instanz zu erhalten.
      RegisterViewModels(services);

      // Weitere Services (falls vorhanden)
      RegisterServices(services);
    }

    private static void RegisterViewModels(IServiceCollection services)
    {
      services.AddTransient<MainViewModel>();
      services.AddTransient<AuditLogViewModel>();
      services.AddTransient<DashboardViewModel>();
      services.AddTransient<SitzungenViewModel>();
      services.AddTransient<SqlhistorieViewModel>();
      services.AddTransient<ProzesseViewModel>();
      services.AddTransient<JobsViewModel>();
      services.AddTransient<SperrenViewModel>();
    }

    private static void RegisterServices(IServiceCollection services)
    {
      // Hier können später zentrale Dienste wie Logging, 
      // Datenbankverbindungs-Manager oder Event-Aggregator als Singleton registriert werden.
    }

    #endregion
  }
}
