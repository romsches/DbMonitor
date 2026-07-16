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
    /// Interaktionslogik für SqlhistorieView.xaml.
    /// Verantwortlich für die Anzeige und Historie der ausgeführten SQL-Statements.
    /// </summary>
    public partial class SqlhistorieView : UserControl
    {
        #region Felder

        private DataGridCell _letzteRechtsgeklickteZelle;

        #endregion

        #region Konstruktor

        public SqlhistorieView()
        {
            InitializeComponent();
            ApplySqlHighlighting(SqlHistorieEditor);

            this.Loaded += OnViewLoaded;
            this.DataContextChanged += OnDataContextChanged;
        }

    #endregion

    #region Event-Handler

    private void OnViewLoaded(object sender, RoutedEventArgs e)
    {
      if (DataContext is SqlhistorieViewModel viewModel)
      {
        if (viewModel.SqlAktualisierenBefehl.CanExecute(null))
        {
          viewModel.SqlAktualisierenBefehl.Execute(null);
        }
      }
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is SqlhistorieViewModel oldVm)
            {
                oldVm.PropertyChanged -= Vm_PropertyChanged;
            }

            if (e.NewValue is SqlhistorieViewModel newVm)
            {
                newVm.PropertyChanged += Vm_PropertyChanged;
                UpdateEditorContent(newVm);
            }
        }

        private void Vm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is SqlhistorieViewModel viewModel && e.PropertyName == nameof(SqlBaseViewModel.AktuellerSqlText))
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

        private void UpdateEditorContent(SqlhistorieViewModel viewModel)
        {
            Dispatcher.Invoke(() => SqlHistorieEditor.Text = viewModel.AktuellerSqlText ?? string.Empty);
        }

        private void ApplySqlHighlighting(TextEditor editor)
        {
            editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("SQL")
                                     ?? HighlightingManager.Instance.GetDefinition("TSQL");
        }

        #endregion
    }
}
