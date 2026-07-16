using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DbMonitorWPF.Helpers;
using DbMonitorWPF.ViewModels;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace DbMonitorWPF.Views
{
  /// <summary>
  /// Interaktionslogik für SperrenView.xaml.
  /// Verantwortlich für die Anzeige von Sperren und die Details der blockierenden SQL-Statements.
  /// </summary>
  public partial class SperrenView : UserControl
  {
    #region Felder

    private DataGridCell _letzteRechtsgeklickteZelle;

    #endregion

    #region Konstruktor

    public SperrenView()
    {
      InitializeComponent();
      ApplySqlHighlighting(SqlEditorSperren);

      this.Loaded += OnViewLoaded;
      this.DataContextChanged += OnDataContextChanged;
    }

    #endregion

    #region Event-Handler

    private void OnViewLoaded(object sender, RoutedEventArgs e)
    {
      if (DataContext is SperrenViewModel viewModel)
      {
        if (viewModel.SperrenAktualisierenBefehl.CanExecute(null))
        {
          viewModel.SperrenAktualisierenBefehl.Execute(null);
        }
      }
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue is SperrenViewModel oldVm)
      {
        oldVm.PropertyChanged -= Vm_PropertyChanged;
      }

      if (e.NewValue is SperrenViewModel newVm)
      {
        newVm.PropertyChanged += Vm_PropertyChanged;
        UpdateEditorContent(newVm);
      }
    }

    private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender is SperrenViewModel viewModel && e.PropertyName == nameof(SqlBaseViewModel.AktuellerSqlText))
      {
        UpdateEditorContent(viewModel);
      }
    }

    private void DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      _letzteRechtsgeklickteZelle = DataGridHelper.FindParent<DataGridCell>(e.OriginalSource as DependencyObject);
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

    private void CopyRow_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      if (sender is DataGrid dataGrid)
      {
        DataGridHelper.CopySelectedRow(dataGrid);
      }
    }

    #endregion

    #region Methoden

    private void UpdateEditorContent(SperrenViewModel viewModel)
    {
      Dispatcher.Invoke(() => SqlEditorSperren.Text = viewModel.AktuellerSqlText ?? string.Empty);
    }

    private void ApplySqlHighlighting(TextEditor editor)
    {
      editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("SQL")
                               ?? HighlightingManager.Instance.GetDefinition("TSQL");
    }

    #endregion
  }
}
