using UnityEngine;

namespace Hexfire
{
  public class PlayerIFrameBridge : MonoBehaviour
  {
    public void GrantIFrames(float duration)
    {
      GetComponent<PlayerHealth>()?.GrantIFrames(duration);
    }
  }
}
