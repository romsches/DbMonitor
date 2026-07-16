using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DbMonitorWPF.Helpers
{
  /// <summary>
  /// Stellt Hilfsfunktionen zur Synchronisierung von ObservableCollections bereit.
  /// </summary>
  public static class CollectionHelper
  {
    /// <summary>
    /// Synchronisiert eine ObservableCollection mit einer neuen Liste.
    /// Leere Listen werden ignoriert, um "Flackern" bei kurzzeitigen Fehlern zu vermeiden.
    /// </summary>
    public static void Sync<T>(ObservableCollection<T> collection, List<T> newData)
    {
      if (collection == null) return;

      // Leere Liste ignorieren — alte Daten behalten, um UI-Flackern zu vermeiden
      if (newData == null || newData.Count == 0) return;

      UpdateCollection(collection, newData);
    }

    /// <summary>
    /// Ersetzt den gesamten Inhalt — auch bei leerer Liste.
    /// Nur verwenden, wenn ein leeres Ergebnis ein valider Zustand ist (z.B. bei Locks).
    /// </summary>
    public static void SyncForce<T>(ObservableCollection<T> collection, List<T> newData)
    {
      if (collection == null) return;

      collection.Clear();
      if (newData == null) return;

      foreach (var item in newData)
      {
        collection.Add(item);
      }
    }

    /// <summary>
    /// Interne Hilfsmethode zum Befüllen der Collection.
    /// </summary>
    private static void UpdateCollection<T>(ObservableCollection<T> collection, IEnumerable<T> newData)
    {
      collection.Clear();
      foreach (var item in newData)
      {
        collection.Add(item);
      }
    }
  }
}
