#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenScript))]
public class MapGenEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    var myScript = (MapGenScript)target;
    if (GUILayout.Button("Build Object"))
      myScript.PressButon();
  }
}
#endif
