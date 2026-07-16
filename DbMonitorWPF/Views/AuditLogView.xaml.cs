using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DbMonitorWPF.ViewModels;
using DbMonitorWPF.Helpers;

namespace DbMonitorWPF.Views
{
  /// <summary>
  /// Interaktionslogik für AuditLogView.xaml
  /// </summary>
  public partial class AuditLogView : UserControl
  {
    #region Felder

    private DataGridCell _letzteRechtsgeklickteZelle;

    #endregion

    #region Konstruktor

    public AuditLogView()
    {
      InitializeComponent();
      this.Loaded += OnViewLoaded;
    }

    #endregion

    #region Event-Handler

    /// <summary>
    /// Initialisiert das Audit-Log beim Laden der View.
    /// </summary>
    private void OnViewLoaded(object sender, RoutedEventArgs e)
    {
      if (DataContext is AuditLogViewModel viewModel)
      {
        if (viewModel.RefreshCommand.CanExecute(null))
        {
          viewModel.RefreshCommand.Execute(null);
        }
      }
    }

    /// <summary>
    /// Speichert die Zelle, auf die mit der rechten Maustaste geklickt wurde, für Kontextmenü-Aktionen.
    /// </summary>
    private void DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      _letzteRechtsgeklickteZelle = DataGridHelper.FindParent<DataGridCell>(e.OriginalSource as DependencyObject);
    }

    /// <summary>
    /// Kopiert den Inhalt der ausgewählten Zelle in die Zwischenablage.
    /// </summary>
    private void CopyCell_Click(object sender, RoutedEventArgs e)
    {
      if (_letzteRechtsgeklickteZelle == null) return;

      string text = DataGridHelper.GetCellText(_letzteRechtsgeklickteZelle);
      if (!string.IsNullOrEmpty(text))
      {
        Clipboard.SetText(text);
      }
    }

    /// <summary>
    /// Kopiert die gesamte Zeile in die Zwischenablage.
    /// </summary>
    private void CopyRow_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      if (sender is DataGrid dataGrid)
      {
        DataGridHelper.CopySelectedRow(dataGrid);
      }
    }

    #endregion
  }
}
