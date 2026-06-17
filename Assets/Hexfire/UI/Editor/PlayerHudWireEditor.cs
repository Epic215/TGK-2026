#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Hexfire.UI.Editor
{
  [CustomEditor(typeof(PlayerHudWire))]
  public class PlayerHudWireEditor : UnityEditor.Editor
  {
    public override void OnInspectorGUI()
    {
      DrawDefaultInspector();

      EditorGUILayout.Space(12f);

      var wire = (PlayerHudWire)target;

      EditorGUILayout.HelpBox(
        "HUD buduje sie w edytorze — NIE musisz odpalac gry.\n" +
        "1. Kliknij przycisk ponizej\n" +
        "2. W hierarchii pojawi sie EquipmentBar pod Canvas\n" +
        "3. Zmien wymiary w Equipment Bar Hud — od razu widac efekt",
        MessageType.Info);

      if (GUILayout.Button("ZBUDUJ / ODŚWIEŻ HUD TERAZ", GUILayout.Height(36f)))
      {
        Undo.RecordObject(wire, "Build Hexfire HUD");
        wire.BuildHudInEditor();
        EditorUtility.SetDirty(wire);
      }

      var bar = wire.GetComponent<EquipmentBarHud>();
      if (bar != null && GUILayout.Button("Tylko pasek ekwipunku"))
      {
        Undo.RecordObject(bar, "Build Equipment Bar");
        bar.BuildNow();
        EditorUtility.SetDirty(bar);
      }
    }
  }
}
#endif
