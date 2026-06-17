using UnityEngine;
using Hexfire;

namespace Hexfire.Weapons
{
  public enum StaffAbilityType
  {
    None,
    ManaRestore,
    ShotgunChaos
  }

  /// <summary>
  /// Kostur — LPM dziedziczy pattern z ProjectileWeaponData (Single/Spread/Chaos).
  /// Mana za LPM: tylko WeaponData.manaCost (raz na akcje strzalu, nie per kula).
  /// PPM: osobna umiejetnosc z wlasnymi parametrami.
  /// </summary>
  [CreateAssetMenu(fileName = "StaffWeapon", menuName = "Hexfire/Weapons/Staff Weapon")]
  public class StaffWeaponData : ProjectileWeaponData
  {
    [Header("Umiejetnosc PPM")]
    public StaffAbilityType abilityType = StaffAbilityType.None;
    [Min(0f)]
    public float abilityManaCost;
    [Min(0f)]
    public float abilityCooldown = 4f;
    [Tooltip("Ile many przywraca (ManaRestore).")]
    public float abilityManaRestore = 20f;

    [Header("ManaRestore — VFX na graczu")]
    public GameObject abilityCastVfxPrefab;
    public float abilityVfxScale = 1f;
    public float abilityVfxDuration = 2f;
    public Vector3 abilityVfxLocalOffset = new Vector3(0f, 0.9f, 0f);

    [Header("Shotgun (PPM)")]
    public int abilityShotgunCount = 16;
    public float abilityShotgunAngleMin = -75f;
    public float abilityShotgunAngleMax = 75f;
    [Range(0f, 1f)]
    public float abilityForwardConeRatio = 0.55f;
    public float abilityForwardConeHalfAngle = 22f;
    [Tooltip("Obrazenia pociskow umiejetnosci. 0 = damage broni.")]
    public float abilityDamage;
    [Tooltip("Predkosc pociskow umiejetnosci. 0 = projectileSpeed broni.")]
    public float abilityProjectileSpeed;
    [Tooltip("Czas zycia pociskow umiejetnosci. 0 = projectileLifetime broni.")]
    public float abilityProjectileLifetime;

    public bool TryUseAbility(Transform shooter, Transform castPoint, PlayerMana mana)
    {
      if (abilityType == StaffAbilityType.None || castPoint == null)
        return false;

      switch (abilityType)
      {
        case StaffAbilityType.ManaRestore:
          if (abilityManaCost > 0f && (mana == null || !mana.TrySpend(abilityManaCost)))
            return false;

          mana?.Restore(abilityManaRestore);
          WeaponCastVfx.SpawnOnCharacter(
            shooter,
            abilityCastVfxPrefab,
            abilityVfxLocalOffset,
            abilityVfxScale,
            abilityVfxDuration);
          return true;

        case StaffAbilityType.ShotgunChaos:
          if (abilityManaCost > 0f && (mana == null || !mana.TrySpend(abilityManaCost)))
            return false;

          Vector3 direction = castPoint.forward;
          var context = new WeaponFireContext(shooter, castPoint, direction, shooter != null ? shooter.tag : "Player");
          FireShotgunAbility(context);
          return true;

        default:
          return false;
      }
    }

    void FireShotgunAbility(WeaponFireContext context)
    {
      if (projectilePrefab == null || context.FirePoint == null)
        return;

      Vector3 baseDirection = context.Direction.sqrMagnitude > 0.0001f
        ? context.Direction.normalized
        : context.FirePoint.forward;

      float shotDamage = abilityDamage > 0f ? abilityDamage : damage;
      float shotSpeed = abilityProjectileSpeed > 0f ? abilityProjectileSpeed : projectileSpeed;
      float shotLifetime = abilityProjectileLifetime > 0f ? abilityProjectileLifetime : projectileLifetime;

      int count = Mathf.Max(1, abilityShotgunCount);
      for (int i = 0; i < count; i++)
      {
        float angle = PickShotgunAngle();
        SpawnProjectileWithStats(context, baseDirection, angle, shotDamage, shotSpeed, shotLifetime);
      }
    }

    float PickShotgunAngle()
    {
      if (Random.value < abilityForwardConeRatio)
        return Random.Range(-abilityForwardConeHalfAngle, abilityForwardConeHalfAngle);

      return Random.Range(abilityShotgunAngleMin, abilityShotgunAngleMax);
    }
  }
}
