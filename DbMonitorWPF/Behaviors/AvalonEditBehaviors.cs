using System.Windows;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace DbMonitorWPF.Behaviors
{
  /// <summary>
  /// Stellt Attached-Properties bereit, um AvalonEdit-Editoren via XAML zu konfigurieren.
  /// </summary>
  public static class AvalonEditBehaviors
  {
    #region Dependency Properties

    /// <summary>
    /// Attached-Property für das Syntax-Highlighting im TextEditor.
    /// </summary>
    public static readonly DependencyProperty SyntaxHighlightingProperty =
        DependencyProperty.RegisterAttached(
            "SyntaxHighlighting",
            typeof(string),
            typeof(AvalonEditBehaviors),
            new PropertyMetadata(null, OnSyntaxHighlightingChanged));

    #endregion

    #region Getter & Setter

    public static void SetSyntaxHighlighting(UIElement element, string value)
        => element.SetValue(SyntaxHighlightingProperty, value);

    public static string GetSyntaxHighlighting(UIElement element)
        => (string)element.GetValue(SyntaxHighlightingProperty);

    #endregion

    #region Private Event-Handler

    /// <summary>
    /// Reagiert auf Änderungen des Syntax-Highlighting-Namens.
    /// </summary>
    private static void OnSyntaxHighlightingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (!(d is TextEditor editor))
      {
        return;
      }

      if (e.NewValue is string highlightingName)
      {
        // Lädt die Definition aus dem HighlightingManager
        editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition(highlightingName);
      }
      else
      {
        editor.SyntaxHighlighting = null;
      }
    }

    #endregion
  }
}
