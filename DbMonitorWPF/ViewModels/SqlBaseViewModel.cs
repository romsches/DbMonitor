using System.ComponentModel;
using DbMonitorWPF.Helpers;

namespace DbMonitorWPF.ViewModels
{
  public abstract class SqlBaseViewModel : ViewModelBase
  {
    private bool _istSqlFormatiert = false; // По умолчанию выключено
    private string _aktuellerSqlText;
    protected string _rawSql;
    protected string _formattedSql;

    public bool IstSqlFormatiert
    {
      get => _istSqlFormatiert;
      set { _istSqlFormatiert = value; OnPropertyChanged(); UpdateSqlDisplay(); }
    }

    public string AktuellerSqlText
    {
      get => _aktuellerSqlText;
      set { _aktuellerSqlText = value; OnPropertyChanged(); }
    }

    // Этот метод обновляет текст в зависимости от чекбокса
    protected void UpdateSqlDisplay()
    {
      AktuellerSqlText = !IstSqlFormatiert ? _formattedSql : _rawSql;
    }

    // Метод, который каждый таб будет вызывать после получения SQL из БД
    protected void SetSqlData(string rawSql)
    {
      _rawSql = rawSql ?? string.Empty;
      _formattedSql = SqlFormatterHelper.Format(_rawSql);
      UpdateSqlDisplay();
    }
  }
}
