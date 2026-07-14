using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit;
using DbMonitorWPF.ViewModels;
using DbMonitorWPF.Helpers;

namespace DbMonitorWPF.Views
{
  public partial class SqlhistorieView : UserControl
  {
    private DataGridCell _lastRightClickedCell;

    public SqlhistorieView()
    {
      InitializeComponent();
      ApplySqlHighlighting(SqlHistorieEditor);
      this.Loaded += OnViewLoaded;
      this.DataContextChanged += OnDataContextChanged;
    }

    private void DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      _lastRightClickedCell = DataGridHelper.FindParent<DataGridCell>(e.OriginalSource as DependencyObject);
    }

    private void CopyCell_Click(object sender, RoutedEventArgs e)
    {
      if (_lastRightClickedCell == null) return;
      string text = DataGridHelper.GetCellText(_lastRightClickedCell);
      if (!string.IsNullOrEmpty(text)) Clipboard.SetText(text);
    }

    private void CopyRow_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      if (sender is DataGrid dg) DataGridHelper.CopySelectedRow(dg);
    }

    private void OnViewLoaded(object sender, RoutedEventArgs e)
    {
      if (DataContext is SqlhistorieViewModel vm && vm.SqlAktualisierenBefehl.CanExecute(null))
        vm.SqlAktualisierenBefehl.Execute(null);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue is SqlhistorieViewModel oldVm) oldVm.PropertyChanged -= Vm_PropertyChanged;
      if (e.NewValue is SqlhistorieViewModel newVm)
      {
        newVm.PropertyChanged += Vm_PropertyChanged;
        UpdateEditorContent(newVm);
      }
    }

    private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender is SqlhistorieViewModel vm && e.PropertyName == nameof(SqlBaseViewModel.AktuellerSqlText))
        UpdateEditorContent(vm);
    }

    private void UpdateEditorContent(SqlhistorieViewModel vm)
    {
      Dispatcher.Invoke(() => SqlHistorieEditor.Text = vm.AktuellerSqlText ?? string.Empty);
    }

    private void ApplySqlHighlighting(TextEditor editor)
    {
      editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("SQL")
                               ?? HighlightingManager.Instance.GetDefinition("TSQL");
    }
  }
}
