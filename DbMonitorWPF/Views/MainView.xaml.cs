using System.Windows;
using System.Windows.Controls;
using DbMonitorWPF.Helpers; // Добавили твой CellCopyHelper

namespace DbMonitorWPF.Views
{
  /// <summary>
  /// Code-Behind für die MainView.
  /// </summary>
  public partial class MainView : UserControl
  {
    public MainView()
    {
      InitializeComponent();
    }

    // Мы используем CellCopyHelper, который ты создал в самом начале, 
    // чтобы не дублировать логику копирования здесь.
    private void CopySelectedCell_Click(object sender, RoutedEventArgs e)
    {
      CellCopyHelper.CopyCell_Click(sender, e);
    }
  }
}
