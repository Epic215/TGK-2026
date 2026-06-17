using UnityEngine;

namespace Hexfire.UI
{
  public class ManaBarFillTint : MonoBehaviour
  {
    void Awake()
    {
      ManaBarVisuals.ApplyBlueFill(transform);
    }
  }
}
