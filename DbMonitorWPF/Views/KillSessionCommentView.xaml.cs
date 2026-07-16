using System.Windows;

namespace DbMonitorWPF.Views
{
  /// <summary>
  /// Interaktionslogik für KillSessionCommentView.xaml.
  /// Dient als Eingabedialog für Kommentare beim Beenden einer Sitzung.
  /// </summary>
  public partial class KillSessionCommentView : Window
  {
    #region Eigenschaften

    /// <summary>
    /// Gibt den vom Benutzer eingegebenen Kommentar zurück.
    /// </summary>
    public string Comment => txtComment.Text;

    #endregion

    #region Konstruktor

    /// <summary>
    /// Initialisiert eine neue Instanz der <see cref="KillSessionCommentView"/>-Klasse.
    /// </summary>
    public KillSessionCommentView()
    {
      InitializeComponent();
    }

    #endregion

    #region Event-Handler

    /// <summary>
    /// Schließt den Dialog erfolgreich und setzt das Dialogergebnis auf 'true'.
    /// </summary>
    private void Ok_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
    }

    /// <summary>
    /// Schließt den Dialog abbrechend und setzt das Dialogergebnis auf 'false'.
    /// </summary>
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
    }

    #endregion
  }
}
