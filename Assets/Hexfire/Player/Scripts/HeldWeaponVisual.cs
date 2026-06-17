using UnityEngine;
using Hexfire.Weapons;

namespace Hexfire
{
  public class HeldWeaponVisual : MonoBehaviour
  {
    static readonly string[] StaffNames = { "Staff01", "Staff02", "Staff03" };

    Transform attachPoint;
    Vector3 handLocalPosition;

    GameObject spawnedVisual;

    void Awake()
    {
      attachPoint = FindDeepChild(transform, "Weapon");
      if (attachPoint == null)
        return;

      Transform pose = attachPoint.Find("Staff01");
      if (pose == null)
        return;

      handLocalPosition = pose.localPosition;
    }

    public void ShowWeapon(WeaponData weapon)
    {
      if (attachPoint == null)
        return;

      HideSpawned();
      SetAllStaffs(false);

      if (weapon == null)
        return;

      if (weapon.showStaffIndex > 0 && weapon.showStaffIndex <= StaffNames.Length)
      {
        Transform staff = attachPoint.Find(StaffNames[weapon.showStaffIndex - 1]);
        if (staff != null)
          staff.gameObject.SetActive(true);

        return;
      }

      if (weapon.heldVisualPrefab == null)
        return;

      spawnedVisual = Instantiate(weapon.heldVisualPrefab, attachPoint);
      spawnedVisual.transform.localPosition = weapon.heldLocalPosition.sqrMagnitude > 0.0001f
        ? weapon.heldLocalPosition
        : handLocalPosition;
      spawnedVisual.transform.localRotation = Quaternion.Euler(weapon.heldLocalEulerAngles);
      spawnedVisual.transform.localScale = weapon.heldLocalScale.sqrMagnitude > 0.0001f
        ? weapon.heldLocalScale
        : Vector3.one;
      spawnedVisual.SetActive(true);

      foreach (var rb in spawnedVisual.GetComponentsInChildren<Rigidbody>(true))
        Destroy(rb);

      foreach (var col in spawnedVisual.GetComponentsInChildren<Collider>(true))
        Destroy(col);

      foreach (var pickup in spawnedVisual.GetComponentsInChildren<HexfireWeaponItem>(true))
        Destroy(pickup);
    }

    void SetAllStaffs(bool on)
    {
      if (attachPoint == null)
        return;

      for (int i = 0; i < StaffNames.Length; i++)
      {
        Transform staff = attachPoint.Find(StaffNames[i]);
        if (staff != null)
          staff.gameObject.SetActive(on);
      }
    }

    void HideSpawned()
    {
      if (spawnedVisual == null)
        return;

      Destroy(spawnedVisual);
      spawnedVisual = null;
    }

    static Transform FindDeepChild(Transform parent, string name)
    {
      if (parent.name == name)
        return parent;

      for (int i = 0; i < parent.childCount; i++)
      {
        Transform found = FindDeepChild(parent.GetChild(i), name);
        if (found != null)
          return found;
      }

      return null;
    }
  }
}
