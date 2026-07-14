using Microsoft.Extensions.DependencyInjection;

namespace DbMonitorData
{
  /// <summary>
  /// Stellt Erweiterungsmethoden zur Registrierung von Abhängigkeiten im Dependency Injection Container bereit.
  /// </summary>
  public static class DiProvider
  {
    /// <summary>
    /// Registriert alle notwendigen Datenbank-Dienste für das Modul <c>DbMonitorData</c>.
    /// </summary>
    /// <param name="services">Die <see cref="IServiceCollection"/> zur Registrierung.</param>
    public static void InitDependencyInjectionForDbMonitorData(this IServiceCollection services)
    {
      if (services == null)
      {
        return;
      }

      // Registrierung als Transient, da DbQuery-Instanzen bei Bedarf neu erzeugt werden sollten,
      // um Datenbankverbindungen korrekt zu handhaben.
      services.AddTransient<DbQuery>();
    }
  }
}
