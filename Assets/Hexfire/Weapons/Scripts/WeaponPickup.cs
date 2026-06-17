using UnityEngine;

namespace Hexfire.Weapons
{
  [RequireComponent(typeof(Collider))]
  public class WeaponPickup : MonoBehaviour
  {
    public WeaponData weaponData;

    void Reset()
    {
      Collider col = GetComponent<Collider>();
      if (col != null)
        col.isTrigger = true;

      WeaponVisual visual = GetComponentInChildren<WeaponVisual>();
      if (visual != null && visual.weaponData != null)
        weaponData = visual.weaponData;
    }

    void OnTriggerEnter(Collider other)
    {
      if (weaponData == null)
        return;

      PlayerEquipment equipment = other.GetComponentInParent<PlayerEquipment>();
      if (equipment == null)
        return;

      if (equipment.TryAddWeapon(weaponData))
        Destroy(gameObject);
    }
  }
}
