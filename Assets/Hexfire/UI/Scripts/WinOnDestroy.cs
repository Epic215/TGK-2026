using Hexfire.UI;
using UnityEngine;

namespace Hexfire
{
  /// <summary>
  /// Podlacz do obiektu (np. boss). Po zniszczeniu pokazuje ekran YOU WIN z RETRY / MENU.
  /// </summary>
  public class WinOnDestroy : MonoBehaviour
  {
    void OnDestroy()
    {
      if (!Application.isPlaying)
        return;

      if (GameOverMenu.ShouldIgnoreWinTrigger)
        return;

      if (GameOverMenu.Instance == null || GameOverMenu.Instance.IsOpen)
        return;

      GameOverMenu.Instance.ShowWin();
    }
  }
}
