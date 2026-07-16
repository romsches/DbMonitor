using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DbMonitorWPF.ViewModels;
using DbMonitorWPF.Helpers;

namespace DbMonitorWPF.Views
{
  /// <summary>
  /// Interaktionslogik für JobsView.xaml.
  /// </summary>
  public partial class JobsView : UserControl
  {
    #region Felder

    private DataGridCell _letzteRechtsgeklickteZelle;

    #endregion

    #region Konstruktor

    public JobsView()
    {
      InitializeComponent();
      this.Loaded += OnViewLoaded;
    }

    #endregion

    #region Event-Handler

    /// <summary>
    /// Initialisiert die Daten beim Laden der View.
    /// </summary>
    private void OnViewLoaded(object sender, RoutedEventArgs e)
    {
      if (DataContext is JobsViewModel viewModel)
      {
        if (viewModel.JobsAktualisierenBefehl.CanExecute(null))
        {
          viewModel.JobsAktualisierenBefehl.Execute(null);
        }
      }
    }

    /// <summary>
    /// Speichert die Zelle, auf die mit der rechten Maustaste geklickt wurde.
    /// </summary>
    private void DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      _letzteRechtsgeklickteZelle = DataGridHelper.FindParent<DataGridCell>(e.OriginalSource as DependencyObject);
    }

    /// <summary>
    /// Kopiert den Inhalt der aktuell gewählten Zelle in die Zwischenablage.
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
