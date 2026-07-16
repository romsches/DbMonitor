using System;
using System.Configuration;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace DbMonitorData
{
  /// <summary>
  /// Verwaltet die Oracle-Datenbankverbindung mit sicherem Ressourcen-Management.
  /// </summary>
  public class OracleConnectionProvider : IDisposable
  {
    private OracleConnection _connection;

    public OracleConnection Connection => _connection;

    public OracleConnectionProvider()
    {
      _connection = new OracleConnection();
    }

    /// <summary>
    /// Baut die Verbindung basierend auf einem Connection-String aus der App.config auf.
    /// </summary>
    public void ConnectFromConfig(string connectionStringName)
    {
      var settings = ConfigurationManager.ConnectionStrings[connectionStringName];
      if (settings == null)
        throw new ConfigurationErrorsException($"Connection-String '{connectionStringName}' nicht in Config gefunden.");

      SetConnectionString(settings.ConnectionString);
    }

    public void SetConnectionString(string connectionString)
    {
      if (_connection.State == ConnectionState.Open)
        _connection.Close();

      _connection.ConnectionString = connectionString;
      _connection.Open();
    }

    public bool IsConnected => _connection != null && _connection.State == ConnectionState.Open;

    /// <summary>
    /// Gibt die Datenbankressourcen explizit frei.
    /// </summary>
    public void Dispose()
    {
      if (_connection != null)
      {
        if (_connection.State == ConnectionState.Open)
          _connection.Close();

        _connection.Dispose();
        _connection = null;
      }
      GC.SuppressFinalize(this);
    }
  }
}
