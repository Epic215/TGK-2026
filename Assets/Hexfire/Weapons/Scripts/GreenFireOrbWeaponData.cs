using UnityEngine;

namespace Hexfire.Weapons
{
  [CreateAssetMenu(fileName = "Weapon_GreenFireOrb", menuName = "Hexfire/Weapons/Green Fire Orb")]
  public class GreenFireOrbWeaponData : ProjectileWeaponData
  {
    [Header("Zielona Kula — leczenie (PPM)")]
    [Tooltip("Ile many kosztuje jedno leczenie (PPM).")]
    public float healManaCost = 40f;

    [Tooltip("Ile HP leczy PPM. 0 = domyslnie 25 HP.")]
    public int healAmount;

    [Tooltip("Cooldown leczenia (sekundy).")]
    public float healRate = 1.25f;

    const int DefaultHealAmount = 25;

    [Tooltip("Aura/efekt na graczu przy leczeniu.")]
    public GameObject healCastVfxPrefab;
    public float healVfxScale = 1f;
    public float healVfxDuration = 2f;
    public Vector3 healVfxLocalOffset = new Vector3(0f, 0.9f, 0f);

    [Header("Zielona Kula — pocisk (LPM, 0 many)")]
    public float extraLifetime;
    public float vfxScaleMultiplier = 1f;

    public float HealManaCost => healManaCost > 0f ? healManaCost : manaCost;

    public int HealPerCast => healAmount > 0 ? healAmount : DefaultHealAmount;

    protected override float GetProjectileLifetime() => projectileLifetime + extraLifetime;

    protected override float GetVisualEffectScale() => visualEffectScale * vfxScaleMultiplier;

    public void ApplyHeal(Transform shooter, Transform castPoint)
    {
      if (shooter == null)
        return;

      var health = shooter.GetComponent<Hexfire.PlayerHealth>();
      if (health == null)
        return;

      health.Heal(HealPerCast);
      WeaponCastVfx.SpawnOnCharacter(
        shooter,
        healCastVfxPrefab,
        healVfxLocalOffset,
        healVfxScale,
        healVfxDuration);
    }

    void Reset()
    {
      weaponName = "Zielona Kula";
      description = "LPM: pocisk (0 many). PPM: leczenie (+25 HP, 40 many).";
      damage = 12f;
      usesMana = false;
      manaCost = 0f;
      healManaCost = 40f;
      healAmount = 25;
      healRate = 1.25f;
      firePattern = WeaponFirePattern.Single;
      fireRate = 1.25f;
      projectileSpeed = 5f;
      projectileLifetime = 4f;
      visualEffectScale = 0.3f;
      heldLocalEulerAngles = new Vector3(90f, 90f, 90f);
      heldLocalScale = new Vector3(30f, 30f, 30f);
      extraLifetime = 0f;
      vfxScaleMultiplier = 1f;
    }
  }
}
