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
  public partial class SitzungenView : UserControl
  {
    private DataGridCell _lastRightClickedCell;

    public SitzungenView()
    {
      InitializeComponent();
      ApplySqlHighlighting(SqlEditorSitzungen);
      this.Loaded += OnViewLoaded;
      this.DataContextChanged += OnDataContextChanged;
    }

    private void OnViewLoaded(object sender, RoutedEventArgs e)
    {
      if (DataContext is SitzungenViewModel vm)
        if (((System.Windows.Input.ICommand)vm.SitzungenAktualisierenBefehl).CanExecute(null))
          ((System.Windows.Input.ICommand)vm.SitzungenAktualisierenBefehl).Execute(null);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue is SitzungenViewModel oldVm)
        oldVm.PropertyChanged -= Vm_PropertyChanged;

      if (e.NewValue is SitzungenViewModel newVm)
      {
        newVm.PropertyChanged += Vm_PropertyChanged;
        UpdateEditorContent(newVm);
      }
    }

    private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (sender is SitzungenViewModel vm && e.PropertyName == nameof(SqlBaseViewModel.AktuellerSqlText))
        UpdateEditorContent(vm);
    }

    private void UpdateEditorContent(SitzungenViewModel vm)
    {
      Dispatcher.Invoke(() => SqlEditorSitzungen.Text = vm.AktuellerSqlText ?? string.Empty);
    }

    private void ApplySqlHighlighting(TextEditor editor)
    {
      editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("SQL")
                               ?? HighlightingManager.Instance.GetDefinition("TSQL");
    }

    private void DataGrid_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
      _lastRightClickedCell = FindParent<DataGridCell>(e.OriginalSource as DependencyObject);
    }

    private void CopyRow_Executed(object sender, ExecutedRoutedEventArgs e)
    {
      if (sender is DataGrid dg)
        DataGridHelper.CopySelectedRow(dg);
    }

    private void CopyCell_Click(object sender, RoutedEventArgs e)
    {
      if (_lastRightClickedCell == null) return;

      string text = null;
      var content = _lastRightClickedCell.Content;

      if (content is TextBlock tb)
        text = tb.Text;
      else if (content is ContentPresenter cp)
      {
        var innerTb = FindVisualChild<TextBlock>(cp);
        if (innerTb != null) text = innerTb.Text;
      }
      else
        text = content?.ToString();

      if (!string.IsNullOrEmpty(text))
        Clipboard.SetText(text);
    }

    private T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
      DependencyObject parent = VisualTreeHelper.GetParent(child);
      while (parent != null && !(parent is T))
        parent = VisualTreeHelper.GetParent(parent);
      return parent as T;
    }

    private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
      {
        var child = VisualTreeHelper.GetChild(parent, i);
        if (child is T result) return result;
        var found = FindVisualChild<T>(child);
        if (found != null) return found;
      }
      return null;
    }
  }
}
