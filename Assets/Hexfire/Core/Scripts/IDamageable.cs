using UnityEngine;

namespace Hexfire
{
  public interface IDamageable
  {
    void TakeDamage(float amount, GameObject source);
  }
}
