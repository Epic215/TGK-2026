using UnityEngine;

namespace Hexfire.Weapons
{
  static class WeaponCastVfx
  {
    public static GameObject SpawnOnCharacter(
      Transform character,
      GameObject prefab,
      Vector3 localOffset,
      float scale,
      float duration)
    {
      if (character == null || prefab == null)
        return null;

      GameObject vfx = Object.Instantiate(prefab, character);
      vfx.transform.localPosition = localOffset;
      vfx.transform.localRotation = Quaternion.identity;
      vfx.transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);
      Object.Destroy(vfx, Mathf.Max(0.1f, duration));
      return vfx;
    }
  }
}
