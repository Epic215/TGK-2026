using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hexfire;

namespace Hexfire.Weapons
{
  [CreateAssetMenu(fileName = "MeleeSword", menuName = "Hexfire/Weapons/Melee Sword")]
  public class MeleeSwordWeaponData : WeaponData
  {
    [Header("Melee LPM")]
    [Min(0.1f)]
    public float swingInterval = 0.5f;
    [Min(0.5f)]
    public float meleeRange = 4.2f;
    [Range(10f, 89f)]
    public float meleeHalfAngle = 72f;
    public GameObject slashVfxPrefab;
    public float slashVfxScale = 1f;
    public Vector3 slashVfxLocalOffset = new Vector3(0f, 0.5f, 0.8f);

    [Header("Magic Shield PPM")]
    public float shieldManaCost = 50f;
    public float shieldDuration = 2.5f;
    public float shieldCooldown = 2.5f;
    public GameObject shieldVfxPrefab;
    public float shieldVfxScale = 1f;
    public Vector3 shieldVfxLocalOffset = Vector3.zero;

    public override float FireInterval => swingInterval;

    public override void Fire(WeaponFireContext context)
    {
      if (context.Shooter == null)
        return;

      Vector3 forward = context.Direction.sqrMagnitude > 0.0001f
        ? context.Direction.normalized
        : context.Shooter.forward;

      Transform castPoint = context.FirePoint != null ? context.FirePoint : context.Shooter;
      SpawnSlashVfx(castPoint, forward);
      DealMeleeDamage(context.Shooter, castPoint.position, forward);
    }

    public bool TryActivateShield(Transform shooter, PlayerMana mana, PlayerIFrameBridge iFrameBridge, MonoBehaviour host)
    {
      if (shooter == null)
        return false;

      if (shieldManaCost > 0f && (mana == null || !mana.TrySpend(shieldManaCost)))
        return false;

      iFrameBridge?.GrantIFrames(shieldDuration);

      if (shieldVfxPrefab == null)
        return true;

      if (host != null)
      {
        host.StartCoroutine(ShieldVfxRoutine(shooter));
        return true;
      }

      SpawnShieldVfx(shooter);
      return true;
    }

    IEnumerator ShieldVfxRoutine(Transform shooter)
    {
      GameObject vfx = SpawnShieldVfx(shooter);
      yield return new WaitForSeconds(shieldDuration);

      if (vfx != null)
        Destroy(vfx);
    }

    GameObject SpawnShieldVfx(Transform shooter)
    {
      Vector3 position = shooter.position + shieldVfxLocalOffset;
      GameObject vfx = Instantiate(shieldVfxPrefab, position, shooter.rotation, shooter);
      vfx.transform.localScale = Vector3.one * shieldVfxScale;
      return vfx;
    }

    void SpawnSlashVfx(Transform castPoint, Vector3 forward)
    {
      if (slashVfxPrefab == null)
        return;

      Vector3 position = castPoint.position + castPoint.rotation * slashVfxLocalOffset;
      Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
      GameObject vfx = Instantiate(slashVfxPrefab, position, rotation);

      if (slashVfxScale != 1f)
        vfx.transform.localScale = Vector3.one * slashVfxScale;

      Destroy(vfx, 1.5f);
    }

    void DealMeleeDamage(Transform shooter, Vector3 origin, Vector3 forward)
    {
      Vector3 flatForward = forward;
      flatForward.y = 0f;
      if (flatForward.sqrMagnitude < 0.0001f)
        flatForward = shooter.forward;

      flatForward.Normalize();
      Vector3 center = origin + flatForward * (meleeRange * 0.5f) + Vector3.up * 0.55f;
      Vector3 halfExtents = new Vector3(meleeRange * 0.55f, 1.25f, meleeRange * 0.7f);

      Collider[] hits = Physics.OverlapBox(
        center,
        halfExtents,
        Quaternion.LookRotation(flatForward),
        ~0,
        QueryTriggerInteraction.Collide);

      var damaged = new HashSet<int>();
      int damageAmount = Mathf.RoundToInt(damage);

      for (int i = 0; i < hits.Length; i++)
      {
        Collider hit = hits[i];
        if (hit == null)
          continue;

        if (hit.transform.IsChildOf(shooter))
          continue;

        if (!IsValidMeleeTarget(hit.gameObject))
          continue;

        Vector3 toTarget = hit.transform.position - origin;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.0001f)
          continue;

        if (Vector3.Angle(flatForward, toTarget.normalized) > meleeHalfAngle)
          continue;

        GameObject root = hit.attachedRigidbody != null
          ? hit.attachedRigidbody.gameObject
          : hit.transform.root.gameObject;

        int id = root.GetInstanceID();
        if (!damaged.Add(id))
          continue;

        ApplyDamage(root, damageAmount, shooter.gameObject);
      }
    }

    static bool IsValidMeleeTarget(GameObject target)
    {
      if (target.CompareTag("Enemy"))
        return true;

      return target.GetComponentInParent<IDamageable>() != null;
    }

    static void ApplyDamage(GameObject target, int amount, GameObject source)
    {
      IDamageable damageable = target.GetComponentInParent<IDamageable>();
      if (damageable != null)
      {
        damageable.TakeDamage(amount, source);
        return;
      }

      if (target.CompareTag("Enemy"))
        target.GetComponent<EnemyHealth>()?.TakeDamage(amount);
    }
  }
}
