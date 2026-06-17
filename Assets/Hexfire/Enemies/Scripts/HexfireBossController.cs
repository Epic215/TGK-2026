using System.Collections;
using UnityEngine;

namespace Hexfire.Enemies
{
  public enum HexfireBossPhase
  {
    Phase1,
    Phase2,
    Phase3
  }

  [RequireComponent(typeof(CharacterController))]
  public class HexfireBossController : MonoBehaviour
  {
    [Header("References")]
    public GameObject bulletPrefab;
    public GameObject mandalaBulletPrefab;
    public Transform firePoint;

    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float chaseStopRange = 5f;
    public float jumpForce = 12f;
    public float gravity = 28f;
    public float jumpCooldownMin = 2f;
    public float jumpCooldownMax = 4f;
    public float teleportCooldown = 8f;
    public float aggroRange = 22f;

    [Header("Bullets")]
    public float bulletSpeed = 12f;

    [Header("Mandala")]
    public int mandalaArmCount = 6;
    public float mandalaRotationSpeed = 120f;
    public float mandalaFireRate = 0.12f;
    public float mandalaSpeed = 14f;
    public float mandalaDuration = 6f;
    public float mandalaCooldown = 10f;

    [Header("Attack Timings")]
    public float singleFireRate = 1.4f;
    public float crossVolleyRate = 3f;
    public float burstCooldown = 2.4f;
    public int burstBullets = 10;
    public float ringCooldown = 4f;
    public int ringBulletCount = 12;
    public float fanCooldown = 2.5f;
    public int fanBulletCount = 7;
    public float fanAngle = 70f;

    [Header("Spiral")]
    public float spiralFireRate = 0.06f;
    public float spiralDuration = 2.8f;
    public float spiralPause = 2f;
    public float spiralRotSpeed = 24f;
    public int spiralArms = 2;
    public float spiralCooldown = 6f;

    [Header("Phase Thresholds (0-1)")]
    [Range(0f, 1f)] public float phase2Threshold = 0.6f;
    [Range(0f, 1f)] public float phase3Threshold = 0.3f;

    Transform player;
    CharacterController controller;
    EnemyHealth health;
    HexfireEnemyAnimator enemyAnimator;

    HexfireBossPhase currentPhase = HexfireBossPhase.Phase1;
    float verticalVelocity;
    float jumpTimer;
    float singleTimer;
    float crossTimer;
    float burstTimer;
    float ringTimer;
    float fanTimer;
    float spiralTimer;
    float mandalaTimer;
    float teleportTimer;
    float spiralAngle;
    float mandalaAngle;
    bool isBusy;
    bool isDoingPattern;

    void Start()
    {
      controller = GetComponent<CharacterController>();
      health = GetComponent<EnemyHealth>();
      enemyAnimator = GetComponent<HexfireEnemyAnimator>();

      GameObject found = GameObject.FindGameObjectWithTag("Player");
      if (found != null)
        player = found.transform;

      jumpTimer = Random.Range(jumpCooldownMin, jumpCooldownMax);
      teleportTimer = teleportCooldown;
    }

    void Update()
    {
      if (player == null)
        return;

      UpdatePhase();
      TickTimers();

      float dist = HorizontalDistance();
      if (dist > aggroRange)
        return;

      HandleMovement(dist);
      HandleAttacks();
    }

    void TickTimers()
    {
      singleTimer -= Time.deltaTime;
      crossTimer -= Time.deltaTime;
      burstTimer -= Time.deltaTime;
      ringTimer -= Time.deltaTime;
      fanTimer -= Time.deltaTime;
      spiralTimer -= Time.deltaTime;
      mandalaTimer -= Time.deltaTime;
      teleportTimer -= Time.deltaTime;
      jumpTimer -= Time.deltaTime;
    }

    void UpdatePhase()
    {
      if (health == null)
        return;

      float hpRatio = (float)health.GetHealth() / Mathf.Max(1, health.GetMaxHealth());
      HexfireBossPhase previous = currentPhase;

      if (hpRatio <= phase3Threshold)
        currentPhase = HexfireBossPhase.Phase3;
      else if (hpRatio <= phase2Threshold)
        currentPhase = HexfireBossPhase.Phase2;
      else
        currentPhase = HexfireBossPhase.Phase1;

      if (previous != currentPhase)
        OnPhaseChanged(currentPhase);
    }

    void OnPhaseChanged(HexfireBossPhase phase)
    {
      if (phase == HexfireBossPhase.Phase2)
      {
        burstTimer = 0.2f;
        spiralTimer = 0.4f;
      }
      else if (phase == HexfireBossPhase.Phase3)
      {
        mandalaTimer = 0.1f;
        teleportTimer = 0.2f;
        StartCoroutine(PhaseThreeEntrance());
      }
    }

    float HorizontalDistance()
    {
      Vector3 delta = player.position - transform.position;
      delta.y = 0f;
      return delta.magnitude;
    }

    void HandleMovement(float dist)
    {
      if (!controller.isGrounded)
        verticalVelocity -= gravity * Time.deltaTime;
      else if (verticalVelocity < 0f)
        verticalVelocity = -2f;

      Vector3 move = Vector3.zero;
      if (dist > chaseStopRange && !isDoingPattern)
      {
        Vector3 toPlayer = FlatToPlayer();
        float speed = currentPhase switch
        {
          HexfireBossPhase.Phase3 => moveSpeed * 1.45f,
          HexfireBossPhase.Phase2 => moveSpeed * 1.2f,
          _ => moveSpeed
        };
        move = toPlayer * speed;
      }

      if (controller.isGrounded && jumpTimer <= 0f && !isDoingPattern && currentPhase != HexfireBossPhase.Phase1)
      {
        StartCoroutine(JumpAttack());
        jumpTimer = Random.Range(jumpCooldownMin, jumpCooldownMax);
      }

      move.y = verticalVelocity;
      controller.Move(move * Time.deltaTime);
      FacePlayer();
    }

    IEnumerator JumpAttack()
    {
      isBusy = true;
      verticalVelocity = jumpForce;
      yield return new WaitForSeconds(0.18f);

      if (currentPhase >= HexfireBossPhase.Phase2)
        ShootCrossVolley();
      else
        ShootSingle();

      isBusy = false;
    }

    void HandleAttacks()
    {
      if (isBusy)
        return;

      switch (currentPhase)
      {
        case HexfireBossPhase.Phase1:
          if (singleTimer <= 0f) { ShootSingle(); singleTimer = singleFireRate; }
          if (crossTimer <= 0f) { ShootCrossVolley(); crossTimer = crossVolleyRate; }
          break;

        case HexfireBossPhase.Phase2:
          if (singleTimer <= 0f) { ShootSingle(); singleTimer = singleFireRate * 0.85f; }
          if (crossTimer <= 0f) { ShootCrossVolley(); crossTimer = crossVolleyRate * 0.85f; }
          if (burstTimer <= 0f) { StartCoroutine(ShootBurst()); burstTimer = burstCooldown; }
          if (fanTimer <= 0f) { ShootFan(); fanTimer = fanCooldown; }
          if (spiralTimer <= 0f) { StartCoroutine(ShootSpiral()); spiralTimer = spiralCooldown; }
          break;

        case HexfireBossPhase.Phase3:
          if (singleTimer <= 0f) { ShootSingle(); singleTimer = singleFireRate * 0.55f; }
          if (crossTimer <= 0f) { ShootCrossVolley(10); crossTimer = crossVolleyRate * 0.65f; }
          if (burstTimer <= 0f) { StartCoroutine(ShootBurst()); burstTimer = burstCooldown * 0.75f; }
          if (ringTimer <= 0f) { ShootRing(); ringTimer = ringCooldown * 0.8f; }
          if (spiralTimer <= 0f) { StartCoroutine(ShootSpiral(currentPhase == HexfireBossPhase.Phase3)); spiralTimer = spiralCooldown * 0.75f; }
          if (mandalaTimer <= 0f) { StartCoroutine(ShootMandala()); mandalaTimer = mandalaCooldown; }
          if (teleportTimer <= 0f) { StartCoroutine(TeleportBehindPlayer()); teleportTimer = teleportCooldown; }
          break;
      }
    }

    void ShootSingle()
    {
      if (firePoint == null)
        return;

      Vector3 dir = FlatDirection(firePoint.position, player.position);
      SpawnBullet(bulletPrefab, firePoint.position, dir * bulletSpeed);
      enemyAnimator?.PlayAttack();
    }

    void ShootCrossVolley(int rays = 4)
    {
      if (currentPhase == HexfireBossPhase.Phase3)
        rays = 8;

      for (int i = 0; i < rays; i++)
      {
        float angle = (360f / rays) * i;
        Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
        SpawnBullet(bulletPrefab, firePoint.position, dir * bulletSpeed);
      }

      enemyAnimator?.PlayAttack();
    }

    void ShootFan()
    {
      Vector3 baseDir = FlatDirection(firePoint.position, player.position);
      float half = fanAngle * 0.5f;

      for (int i = 0; i < fanBulletCount; i++)
      {
        float t = fanBulletCount <= 1 ? 0.5f : (float)i / (fanBulletCount - 1);
        float spread = Mathf.Lerp(-half, half, t);
        Vector3 dir = Quaternion.AngleAxis(spread, Vector3.up) * baseDir;
        SpawnBullet(bulletPrefab, firePoint.position, dir * bulletSpeed);
      }

      enemyAnimator?.PlayAttack();
    }

    void ShootRing()
    {
      for (int i = 0; i < ringBulletCount; i++)
      {
        float angle = (360f / ringBulletCount) * i + Random.Range(-6f, 6f);
        Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
        SpawnBullet(bulletPrefab, firePoint.position, dir * bulletSpeed * 0.95f);
      }

      enemyAnimator?.PlayAttack();
    }

    IEnumerator ShootBurst()
    {
      isBusy = true;
      Vector3 baseDir = FlatDirection(firePoint.position, player.position);

      for (int i = 0; i < burstBullets; i++)
      {
        float spread = Mathf.Lerp(-38f, 38f, burstBullets <= 1 ? 0.5f : (float)i / (burstBullets - 1));
        Vector3 vel = Quaternion.AngleAxis(spread, Vector3.up) * baseDir * bulletSpeed * 1.08f;
        SpawnBullet(bulletPrefab, firePoint.position, vel);
        yield return new WaitForSeconds(0.055f);
      }

      enemyAnimator?.PlayAttack();
      isBusy = false;
    }

    IEnumerator ShootSpiral(bool extraArms = false)
    {
      isBusy = true;
      float elapsed = 0f;
      int arms = extraArms ? spiralArms + 2 : spiralArms;

      while (elapsed < spiralDuration)
      {
        for (int arm = 0; arm < arms; arm++)
        {
          float angle = spiralAngle + (360f / arms) * arm;
          float rad = angle * Mathf.Deg2Rad;
          Vector3 dir = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
          SpawnBullet(bulletPrefab, firePoint.position, dir * bulletSpeed * 0.9f);
        }

        spiralAngle = (spiralAngle + spiralRotSpeed) % 360f;
        elapsed += spiralFireRate;
        yield return new WaitForSeconds(spiralFireRate);
      }

      enemyAnimator?.PlayAttack();
      yield return new WaitForSeconds(spiralPause);
      isBusy = false;
    }

    IEnumerator ShootMandala()
    {
      if (mandalaBulletPrefab == null)
        yield break;

      isDoingPattern = true;
      isBusy = true;
      float elapsed = 0f;

      while (elapsed < mandalaDuration)
      {
        mandalaAngle = (mandalaAngle + mandalaRotationSpeed * mandalaFireRate) % 360f;

        for (int arm = 0; arm < mandalaArmCount; arm++)
        {
          float angle = mandalaAngle + (360f / mandalaArmCount) * arm;
          float rad = angle * Mathf.Deg2Rad;
          Vector3 dir = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
          Vector3 spawnPos = firePoint.position + dir * 1.4f;
          SpawnBullet(mandalaBulletPrefab, spawnPos, dir * mandalaSpeed, mandala: true);
        }

        elapsed += mandalaFireRate;
        yield return new WaitForSeconds(mandalaFireRate);
      }

      enemyAnimator?.PlayAttack();
      isBusy = false;
      isDoingPattern = false;
    }

    IEnumerator PhaseThreeEntrance()
    {
      isBusy = true;
      float angle = 0f;

      for (int i = 0; i < 48; i++)
      {
        for (int arm = 0; arm < 6; arm++)
        {
          float off = (360f / 6f) * arm;
          float rad = (angle + off) * Mathf.Deg2Rad;
          Vector3 dir = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
          SpawnBullet(bulletPrefab, firePoint.position, dir * bulletSpeed);
        }

        angle = (angle + 7f) % 360f;
        yield return new WaitForSeconds(0.05f);
      }

      isBusy = false;
    }

    IEnumerator TeleportBehindPlayer()
    {
      isBusy = true;

      Renderer[] renderers = GetComponentsInChildren<Renderer>();
      foreach (Renderer renderer in renderers)
        renderer.enabled = false;

      yield return new WaitForSeconds(0.35f);

      Vector3 behind = player.position - player.forward * 2.8f;
      behind.y = transform.position.y;

      controller.enabled = false;
      transform.position = behind;
      controller.enabled = true;

      yield return new WaitForSeconds(0.08f);
      foreach (Renderer renderer in renderers)
        renderer.enabled = true;

      yield return StartCoroutine(ShootBurst());
      isBusy = false;
    }

    void SpawnBullet(GameObject prefab, Vector3 spawnPos, Vector3 velocity, bool mandala = false)
    {
      HexfireEnemyBulletSpawner.Spawn(prefab, spawnPos, velocity, this, mandala);
    }

    void FacePlayer()
    {
      Vector3 dir = player.position - transform.position;
      dir.y = 0f;
      if (dir.sqrMagnitude < 0.001f)
        return;

      transform.rotation = Quaternion.RotateTowards(
        transform.rotation,
        Quaternion.LookRotation(dir),
        360f * Time.deltaTime);
    }

    Vector3 FlatToPlayer()
    {
      return FlatDirection(transform.position, player.position);
    }

    static Vector3 FlatDirection(Vector3 from, Vector3 to)
    {
      Vector3 dir = to - from;
      dir.y = 0f;
      return dir.sqrMagnitude < 0.001f ? Vector3.forward : dir.normalized;
    }

    void OnDrawGizmosSelected()
    {
      Gizmos.color = Color.red;
      Gizmos.DrawWireSphere(transform.position, aggroRange);
      Gizmos.color = Color.yellow;
      Gizmos.DrawWireSphere(transform.position, chaseStopRange);
    }
  }
}
