using UnityEngine;

namespace Hexfire.Weapons
{
  public abstract class WeaponData : ScriptableObject
  {
    [Header("Info")]
    public string weaponName = "Weapon";
    [TextArea(2, 5)]
    public string description = "Opis broni.";
    public Sprite icon;

    [Header("Combat")]
    [Min(0f)]
    public float damage = 10f;

    [Header("Mana")]
    public bool usesMana = true;
    [Min(0f)]
    public float manaCost = 10f;

    [Header("Wyglad w rece")]
    [Tooltip("1-3 = Staff01-03. 0 = prefab przy rece.")]
    [Range(0, 3)]
    public int showStaffIndex;

    [Tooltip("Prefab przy rece (kula, miecz).")]
    public GameObject heldVisualPrefab;

    [Tooltip("Prefab na ziemi po upuszczeniu (E podnies).")]
    public GameObject pickupPrefab;

    public bool useStaffPose = true;
    public Vector3 heldLocalPosition;
    public Vector3 heldLocalEulerAngles;
    public Vector3 heldLocalScale = Vector3.one;

    public abstract float FireInterval { get; }

    public abstract void Fire(WeaponFireContext context);
  }
}
