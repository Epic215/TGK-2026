using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hexfire.UI
{
  public static class HudUiFactory
  {
    public static GameObject Create(string name, Transform parent, params System.Type[] components)
    {
      var gameObject = new GameObject(name, components);
      gameObject.transform.SetParent(parent, false);

#if UNITY_EDITOR
      if (!Application.isPlaying)
        Undo.RegisterCreatedObjectUndo(gameObject, "Hexfire HUD");
#endif

      return gameObject;
    }

    public static void SetDirty(Object target)
    {
#if UNITY_EDITOR
      if (!Application.isPlaying && target != null)
        EditorUtility.SetDirty(target);
#endif
    }
  }
}
