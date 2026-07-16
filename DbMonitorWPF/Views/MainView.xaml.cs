using System.Windows;
using System.Windows.Controls;
using DbMonitorWPF.Helpers;

namespace DbMonitorWPF.Views
{
  /// <summary>
  /// Interaktionslogik für MainView.xaml.
  /// Dient als Haupt-Container für die Anwendungsnavigation und Statusanzeige.
  /// </summary>
  public partial class MainView : UserControl
  {
    #region Konstruktor

    public MainView()
    {
      InitializeComponent();
    }

    #endregion

    #region Event-Handler

    /// <summary>
    /// Delegiert den Kopiervorgang der ausgewählten Zelle an den <see cref="CellCopyHelper"/>.
    /// Dadurch wird eine zentrale Logik-Verwaltung sichergestellt.
    /// </summary>
    private void CopySelectedCell_Click(object sender, RoutedEventArgs e)
    {
      CellCopyHelper.CopyCell_Click(sender, e);
    }

    #endregion
  }
}
