using System.Configuration; // Обязательно добавь
using Oracle.ManagedDataAccess.Client;

namespace DbMonitorData
{
  public class OracleConnectionProvider
  {
    public OracleConnection Connection { get; private set; }

    public OracleConnectionProvider()
    {
      Connection = new OracleConnection();
    }

    // Этот метод берет строку из App.config по имени ключа
    public void ConnectFromConfig(string connectionStringName)
    {
      string connString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
      SetConnectionString(connString);
    }

    public void SetConnectionString(string connectionString)
    {
      if (Connection.State == System.Data.ConnectionState.Open)
        Connection.Close();

      Connection.ConnectionString = connectionString;
      Connection.Open();
    }

    public bool IsConnected => Connection != null && Connection.State == System.Data.ConnectionState.Open;
  }
}
