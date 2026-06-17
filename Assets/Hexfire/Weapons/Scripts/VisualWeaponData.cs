using UnityEngine;

namespace Hexfire.Weapons
{
  [CreateAssetMenu(fileName = "VisualWeapon", menuName = "Hexfire/Weapons/Visual Weapon")]
  public class VisualWeaponData : WeaponData
  {
    [Min(0.1f)]
    public float fireRate = 1f;

    public override float FireInterval => fireRate;

    public override void Fire(WeaponFireContext context)
    {
    }
  }
}
