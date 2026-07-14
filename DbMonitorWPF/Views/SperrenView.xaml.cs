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
  public partial class SperrenView : UserControl
  {
    private DataGridCell _lastRightClickedCell;

    public SperrenView()
    {
      InitializeComponent();
      ApplySqlHighlighting(SqlEditorSperren);
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
      if (DataContext is SperrenViewModel vm && vm.SperrenAktualisierenBefehl.CanExecute(null))
        vm.SperrenAktualisierenBefehl.Execute(null);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue is SperrenViewModel oldVm) oldVm.PropertyChanged -= Vm_PropertyChanged;
      if (e.NewValue is SperrenViewModel newVm)
      {
        newVm.PropertyChanged += Vm_PropertyChanged;
        UpdateEditorContent(newVm);
      }
    }

    private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender is SperrenViewModel vm && e.PropertyName == nameof(SqlBaseViewModel.AktuellerSqlText))
        UpdateEditorContent(vm);
    }

    private void UpdateEditorContent(SperrenViewModel vm)
    {
      Dispatcher.Invoke(() => SqlEditorSperren.Text = vm.AktuellerSqlText ?? string.Empty);
    }

    private void ApplySqlHighlighting(TextEditor editor)
    {
      editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("SQL")
                               ?? HighlightingManager.Instance.GetDefinition("TSQL");
    }
  }
}
