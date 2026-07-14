using System;
using System.Windows;
using DbMonitorData;
using DbMonitorWPF.ViewModels;
using DbMonitorWPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DbMonitorWPF
{
  public partial class App : Application
  {
    public static IServiceProvider ServiceProvider { get; private set; }

    private void ConfigureServices(ServiceCollection services)
    {
      services.AddLogging(configure => configure.AddConsole());

      services.AddSingleton<OracleConnectionProvider>();
      services.AddTransient<DbQuery>();

      // Tab-ViewModels als Singleton — damit MainViewModel immer dieselbe Instanz bekommt
      services.AddSingleton<SitzungenViewModel>();
      services.AddSingleton<SperrenViewModel>();
      services.AddSingleton<ProzesseViewModel>();
      services.AddSingleton<JobsViewModel>();
      services.AddSingleton<SqlhistorieViewModel>();
      services.AddSingleton<AuditLogViewModel>();
      services.AddSingleton<DashboardViewModel>();

      // MainViewModel bekommt alle Tab-VMs per DI injiziert
      services.AddSingleton<MainViewModel>();

      services.AddTransient<MainView>();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      var serviceCollection = new ServiceCollection();
      ConfigureServices(serviceCollection);
      ServiceProvider = serviceCollection.BuildServiceProvider();

      try
      {
        var connectionProvider = ServiceProvider.GetRequiredService<OracleConnectionProvider>();
        connectionProvider.ConnectFromConfig("ctssConnectionString");

        var window = new Window();
        window.Content = ServiceProvider.GetRequiredService<MainView>();
        window.DataContext = ServiceProvider.GetRequiredService<MainViewModel>();
        window.Title = "DB Monitor";
        window.Width = 1200;
        window.Height = 700;
        window.Show();
      }
      catch (Exception ex)
      {
        MessageBox.Show(
            $"Fehler beim Start: {ex.Message}",
            "Kritischer Fehler",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        Shutdown();
      }
    }
  }
}
