using UnityEngine;

namespace Hexfire.Weapons
{
  /// <summary>
  /// Tylko wyglad broni w scenie / w rece — jak Staff01 (mesh + VFX).
  /// Statystyki strzelania sa w WeaponData.
  /// </summary>
  public class WeaponVisual : MonoBehaviour
  {
    [Tooltip("Dane broni (obrazenia, fire rate, pocisk).")]
    public WeaponData weaponData;
  }
}
