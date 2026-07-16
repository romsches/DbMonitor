using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DbMonitorWPF.Helpers;
using DbMonitorWPF.ViewModels;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace DbMonitorWPF.Views
{
  /// <summary>
  /// Interaktionslogik für SitzungenView.xaml.
  /// Verantwortlich für die Anzeige der aktiven Sitzungen und deren SQL-Details.
  /// </summary>
  public partial class SitzungenView : UserControl
  {
    #region Felder

    private DataGridCell _letzteRechtsgeklickteZelle;

    #endregion

    #region Konstruktor

    public SitzungenView()
    {
      InitializeComponent();
      ApplySqlHighlighting(SqlEditorSitzungen);

      this.Loaded += OnViewLoaded;
      this.DataContextChanged += OnDataContextChanged;
    }

    #endregion

    #region Event-Handler

    private void OnViewLoaded(object sender, RoutedEventArgs e)
    {
      if (DataContext is SitzungenViewModel viewModel)
      {
        if (viewModel.SitzungenAktualisierenBefehl.CanExecute())
        {
          viewModel.SitzungenAktualisierenBefehl.Execute();
        }
      }
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue is SitzungenViewModel oldVm)
      {
        oldVm.PropertyChanged -= Vm_PropertyChanged;
      }

      if (e.NewValue is SitzungenViewModel newVm)
      {
        newVm.PropertyChanged += Vm_PropertyChanged;
        UpdateEditorContent(newVm);
      }
    }

    private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender is SitzungenViewModel vm && e.PropertyName == nameof(SqlBaseViewModel.AktuellerSqlText))
      {
        UpdateEditorContent(vm);
      }
    }

    #endregion

    #region Methoden

    private void UpdateEditorContent(SitzungenViewModel viewModel)
    {
      Dispatcher.Invoke(() => SqlEditorSitzungen.Text = viewModel.AktuellerSqlText ?? string.Empty);
    }

    private void ApplySqlHighlighting(TextEditor editor)
    {
      editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("SQL")
                               ?? HighlightingManager.Instance.GetDefinition("TSQL");
    }

    private void DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      _letzteRechtsgeklickteZelle = DataGridHelper.FindParent<DataGridCell>(e.OriginalSource as DependencyObject);
    }

    private void CopyRow_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      if (sender is DataGrid dataGrid)
      {
        DataGridHelper.CopySelectedRow(dataGrid);
      }
    }

    private void CopyCell_Click(object sender, RoutedEventArgs e)
    {
      if (_letzteRechtsgeklickteZelle == null) return;

      string text = DataGridHelper.GetCellText(_letzteRechtsgeklickteZelle);

      if (!string.IsNullOrEmpty(text))
      {
        Clipboard.SetText(text);
      }
    }

    #endregion
  }
}
