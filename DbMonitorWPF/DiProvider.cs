using DbMonitorWPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DbMonitorWPF
{
  /// <summary>
  /// Stellt Erweiterungsmethoden zur Registrierung der WPF-spezifischen Abhängigkeiten bereit.
  /// </summary>
  public static class DiProvider
  {
    /// <summary>
    /// Registriert alle notwendigen ViewModels und WPF-Dienste für das Modul <c>DbMonitorWPF</c>.
    /// </summary>
    /// <param name="services">Die <see cref="IServiceCollection"/> zur Registrierung.</param>
    public static void InitDependencyInjectionForDbMonitorWPF(this IServiceCollection services)
    {
      if (services == null)
      {
        return;
      }

      // ViewModels
      // Als Transient registriert, um bei jeder Navigation eine frische Instanz zu erhalten.
      services.AddTransient<MainViewModel>();
      services.AddTransient<AuditLogViewModel>();

      // Hinweis: Falls weitere ViewModels hinzugefügt werden, 
      // können diese hier analog registriert werden.
    }
  }
}
