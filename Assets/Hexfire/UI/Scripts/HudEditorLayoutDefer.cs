using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hexfire.UI
{
  public static class HudEditorLayoutDefer
  {
    static readonly HashSet<int> PendingIds = new();
    static readonly List<(int id, MonoBehaviour behaviour, Action action)> Queue = new();
    static bool flushHooked;

    public static void Schedule(MonoBehaviour behaviour, Action action)
    {
      if (behaviour == null || action == null)
        return;

      int id = behaviour.GetInstanceID();

#if UNITY_EDITOR
      if (!Application.isPlaying)
      {
        if (!PendingIds.Add(id))
          return;

        Queue.Add((id, behaviour, action));

        if (!flushHooked)
        {
          flushHooked = true;
          EditorApplication.delayCall += Flush;
        }

        return;
      }
#endif

      action();
    }

#if UNITY_EDITOR
    static void Flush()
    {
      flushHooked = false;

      for (int i = 0; i < Queue.Count; i++)
      {
        (int id, MonoBehaviour behaviour, Action action) item = Queue[i];
        PendingIds.Remove(item.id);

        if (item.behaviour != null)
          item.action();
      }

      Queue.Clear();
    }
#endif
  }
}
