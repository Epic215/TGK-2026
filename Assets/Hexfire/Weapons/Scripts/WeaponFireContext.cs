using UnityEngine;

namespace Hexfire.Weapons
{
  public readonly struct WeaponFireContext
  {
    public readonly Transform Shooter;
    public readonly Transform FirePoint;
    public readonly Vector3 Direction;
    public readonly string OwnerTag;
    public readonly System.Action BeginFireSequence;
    public readonly System.Action EndFireSequence;

    public WeaponFireContext(
      Transform shooter,
      Transform firePoint,
      Vector3 direction,
      string ownerTag,
      System.Action beginFireSequence = null,
      System.Action endFireSequence = null)
    {
      Shooter = shooter;
      FirePoint = firePoint;
      Direction = direction;
      OwnerTag = ownerTag;
      BeginFireSequence = beginFireSequence;
      EndFireSequence = endFireSequence;
    }
  }
}
