using UnityEngine;

namespace Hexfire.Weapons
{
  public abstract class ProjectileWeaponData : WeaponData
  {
    [Header("Pattern (jak stary PlayerInventory)")]
    public WeaponFirePattern firePattern = WeaponFirePattern.Single;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    [Tooltip("Opcjonalny efekt wizualny (np. Procedural Fire green) — dziecko pocisku.")]
    public GameObject visualEffectPrefab;
    public float visualEffectScale = 0.35f;

    [Header("Timing")]
    [Tooltip("Cooldown miedzy strzalami (Single / Spread).")]
    public float fireRate = 0.5f;
    [Tooltip("Predkosc pocisku LPM — osobno dla kazdej broni.")]
    public float projectileSpeed = 6f;
    public float projectileLifetime = 4f;
    public float spawnForwardOffset = 0.6f;

    [Header("Spread")]
    [Tooltip("Katy w stopniach wzgledem celu (np. -15, 0, 15) — wszystkie naraz.")]
    public float[] spreadAngles = { -15f, 0f, 15f };

    [Header("Chaos")]
    [Tooltip("Ile pociskow w jednej serii chaos.")]
    public int chaosProjectileCount = 5;
    public float chaosAngleMin = -25f;
    public float chaosAngleMax = 25f;
    [Tooltip("Opoznienie miedzy pociskami w serii chaos.")]
    public float chaosShotDelay = 0.12f;
    [Tooltip("Dodatkowy cooldown po calej serii chaos.")]
    public float chaosSeriesCooldown = 0.4f;

    [Header("Serpentine (ROTMG — dwa węże na przemian)")]
    [Tooltip("Ile pociskow na jeden strzal (2 = klasyczny serpentine).")]
    public int serpentineProjectileCount = 2;
    [Tooltip("Szerokosc wijania — predkosc boczna fali.")]
    public float serpentineAmplitude = 3.5f;
    [Tooltip("Czestotliwosc wijania (im wyzsza, tym czesciej skreca).")]
    public float serpentineFrequency = 4.5f;

    public override float FireInterval
    {
      get
      {
        if (firePattern == WeaponFirePattern.Chaos)
          return chaosShotDelay * Mathf.Max(1, chaosProjectileCount) + chaosSeriesCooldown;

        return fireRate;
      }
    }

    public override void Fire(WeaponFireContext context)
    {
      if (projectilePrefab == null || context.FirePoint == null)
        return;

      Vector3 baseDirection = context.Direction.sqrMagnitude > 0.0001f
        ? context.Direction.normalized
        : context.FirePoint.forward;

      switch (firePattern)
      {
        case WeaponFirePattern.Spread:
          FireSpread(context, baseDirection);
          break;

        case WeaponFirePattern.Chaos:
          FireChaos(context, baseDirection);
          break;

        case WeaponFirePattern.Serpentine:
          FireSerpentine(context, baseDirection);
          break;

        default:
          SpawnProjectileWithStats(
            context,
            baseDirection,
            0f,
            GetDamage(),
            GetProjectileSpeed(),
            GetProjectileLifetime());
          break;
      }
    }

    protected virtual float GetDamage() => damage;
    protected virtual float GetProjectileSpeed() => projectileSpeed;
    protected virtual float GetProjectileLifetime() => projectileLifetime;
    protected virtual float GetVisualEffectScale() => visualEffectScale;

    void FireSpread(WeaponFireContext context, Vector3 baseDirection)
    {
      float shotDamage = GetDamage();
      float shotSpeed = GetProjectileSpeed();
      float shotLifetime = GetProjectileLifetime();

      if (spreadAngles == null || spreadAngles.Length == 0)
      {
        SpawnProjectileWithStats(context, baseDirection, 0f, shotDamage, shotSpeed, shotLifetime);
        return;
      }

      for (int i = 0; i < spreadAngles.Length; i++)
        SpawnProjectileWithStats(context, baseDirection, spreadAngles[i], shotDamage, shotSpeed, shotLifetime);
    }

    void FireSerpentine(WeaponFireContext context, Vector3 baseDirection)
    {
      float shotDamage = GetDamage();
      float shotSpeed = GetProjectileSpeed();
      float shotLifetime = GetProjectileLifetime();
      int count = Mathf.Max(2, serpentineProjectileCount);

      for (int i = 0; i < count; i++)
      {
        float phase = (i & 1) == 0 ? 0f : Mathf.PI;
        SpawnProjectileWithStats(
          context,
          baseDirection,
          0f,
          shotDamage,
          shotSpeed,
          shotLifetime,
          serpentineAmplitude,
          serpentineFrequency,
          phase);
      }
    }

    void FireChaos(WeaponFireContext context, Vector3 baseDirection)
    {
      int count = Mathf.Max(1, chaosProjectileCount);
      var host = context.Shooter != null
        ? context.Shooter.GetComponent<MonoBehaviour>()
        : null;

      if (host == null)
      {
        for (int i = 0; i < count; i++)
        {
          float angle = Random.Range(chaosAngleMin, chaosAngleMax);
          SpawnProjectileWithStats(
            context,
            baseDirection,
            angle,
            GetDamage(),
            GetProjectileSpeed(),
            GetProjectileLifetime());
        }

        return;
      }

      host.StartCoroutine(FireChaosRoutine(context, baseDirection, count));
    }

    System.Collections.IEnumerator FireChaosRoutine(
      WeaponFireContext context,
      Vector3 baseDirection,
      int count)
    {
      context.BeginFireSequence?.Invoke();

      try
      {
        float shotDamage = GetDamage();
        float shotSpeed = GetProjectileSpeed();
        float shotLifetime = GetProjectileLifetime();

        for (int i = 0; i < count; i++)
        {
          float angle = Random.Range(chaosAngleMin, chaosAngleMax);
          SpawnProjectileWithStats(context, baseDirection, angle, shotDamage, shotSpeed, shotLifetime);

          if (chaosShotDelay > 0f && i < count - 1)
            yield return new WaitForSeconds(chaosShotDelay);
        }
      }
      finally
      {
        context.EndFireSequence?.Invoke();
      }
    }

    protected void SpawnProjectileWithStats(
      WeaponFireContext context,
      Vector3 baseDirection,
      float angleOffset,
      float shotDamage,
      float shotSpeed,
      float shotLifetime,
      float weaveAmplitude = 0f,
      float weaveFrequency = 0f,
      float weavePhase = 0f)
    {
      Vector3 direction = Quaternion.AngleAxis(angleOffset, Vector3.up) * baseDirection;
      if (direction.sqrMagnitude < 0.0001f)
        return;

      direction.Normalize();
      Vector3 spawnPosition = context.FirePoint.position + direction * spawnForwardOffset;
      SpawnProjectileAt(
        context,
        spawnPosition,
        direction,
        shotDamage,
        shotSpeed,
        shotLifetime,
        weaveAmplitude,
        weaveFrequency,
        weavePhase);
    }

    protected void SpawnProjectileAt(
      WeaponFireContext context,
      Vector3 spawnPosition,
      Vector3 direction,
      float shotDamage,
      float shotSpeed,
      float shotLifetime,
      float weaveAmplitude = 0f,
      float weaveFrequency = 0f,
      float weavePhase = 0f)
    {
      if (projectilePrefab == null || direction.sqrMagnitude < 0.0001f)
        return;

      direction.Normalize();
      Quaternion rotation = Quaternion.LookRotation(direction);

      GameObject projectileObject = Instantiate(projectilePrefab, spawnPosition, rotation);

      if (visualEffectPrefab != null)
      {
        GameObject vfx = Instantiate(visualEffectPrefab, projectileObject.transform);
        vfx.transform.localPosition = Vector3.zero;
        vfx.transform.localRotation = Quaternion.identity;
        vfx.transform.localScale = Vector3.one * GetVisualEffectScale();
      }

      Projectile projectile = projectileObject.GetComponent<Projectile>();
      if (projectile != null)
      {
        projectile.Initialize(
          shotDamage,
          shotSpeed,
          direction,
          shotLifetime,
          context.OwnerTag,
          context.Shooter,
          weaveAmplitude,
          weaveFrequency,
          weavePhase);
      }
    }
  }
}
