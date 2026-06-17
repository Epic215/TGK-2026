using System.Collections;
using UnityEngine;

namespace Hexfire.Enemies
{
  public enum HexfireEnemyMoveMode
  {
    Chase,
    Stationary,
    Strafe,
    Retreat,
    Orbit,
    Kite,
    Charge
  }

  public enum HexfireEnemyAttackPattern
  {
    Single,
    Burst,
    Spiral,
    Ring,
    Fan,
    Scatter,
    Cross,
    PulseRing,
    Alternating,
    Shotgun,
    Wave
  }

  [RequireComponent(typeof(CharacterController))]
  public class HexfireEnemyController : MonoBehaviour
  {
    [Header("Behaviour")]
    public HexfireEnemyMoveMode moveMode = HexfireEnemyMoveMode.Chase;
    public HexfireEnemyAttackPattern attackPattern = HexfireEnemyAttackPattern.Single;
    public bool freezeMovementWhileAttacking = true;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float chaseStopRange = 4f;
    public float chaseStartRange = 28f;
    public float strafeSpeed = 3f;
    public float retreatRange = 2.5f;
    public float preferredRange = 7f;
    public float rangeTolerance = 1.2f;
    public float zigzagAmplitude = 0.55f;
    public float zigzagFrequency = 2.2f;

    [Header("Orbit / Kite")]
    public float orbitSpeed = 3.2f;
    public int orbitDirection = 1;

    [Header("Charge")]
    public float chargeSpeed = 9f;
    public float chargeDistance = 4f;
    public float chargeCooldown = 3.5f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 14f;
    public float aimLeadStrength = 0.35f;

    [Header("Single")]
    public float singleFireRate = 1.1f;

    [Header("Burst")]
    public int burstBullets = 7;
    public float burstCooldown = 2.2f;
    public float burstSpread = 28f;

    [Header("Spiral")]
    public float spiralFireRate = 0.05f;
    public float spiralDuration = 2.2f;
    public float spiralPause = 1.8f;
    public float spiralAngleStep = 22f;
    public int spiralArms = 2;

    [Header("Ring")]
    public int ringBulletCount = 8;
    public float ringCooldown = 2.8f;

    [Header("Fan")]
    public int fanBulletCount = 5;
    public float fanAngle = 50f;
    public float fanCooldown = 1.8f;

    [Header("Scatter")]
    public int scatterBulletCount = 10;
    public float scatterCooldown = 3f;

    [Header("Cross")]
    public int crossRays = 4;
    public float crossCooldown = 2.4f;

    [Header("Pulse Ring")]
    public int pulseRingWaves = 3;
    public float pulseRingDelay = 0.18f;
    public float pulseRingRotationStep = 18f;
    public float pulseRingCooldown = 3.2f;

    [Header("Alternating")]
    public float alternatingAngle = 28f;
    public float alternatingCooldown = 1.3f;

    [Header("Shotgun")]
    public int shotgunPellets = 6;
    public float shotgunSpread = 22f;
    public float shotgunCooldown = 2f;

    [Header("Wave")]
    public int waveShots = 5;
    public float waveSweepAngle = 40f;
    public float waveShotDelay = 0.1f;
    public float waveCooldown = 2.6f;

    Transform player;
    CharacterController controller;
    HexfireEnemyAnimator enemyAnimator;
    float fireTimer;
    float zigzagPhase;
    float strafeTimer;
    int strafeDir = 1;
    int alternatingSide = 1;
    float spiralAngle;
    float pulseRingBaseAngle;
    float verticalVelocity;
    float chargeTimer;
    bool isBusy;

    void Start()
    {
      controller = GetComponent<CharacterController>();
      enemyAnimator = GetComponent<HexfireEnemyAnimator>();
      orbitDirection = Random.value > 0.5f ? 1 : -1;
      alternatingSide = Random.value > 0.5f ? 1 : -1;
      chargeTimer = chargeCooldown * 0.5f;
      TryFindPlayer();
    }

    void Update()
    {
      if (player == null)
        TryFindPlayer();

      if (player == null)
        return;

      fireTimer -= Time.deltaTime;
      chargeTimer -= Time.deltaTime;
      zigzagPhase += Time.deltaTime * zigzagFrequency;

      float dist = HorizontalDistanceToPlayer();
      if (dist > chaseStartRange)
        return;

      if (!isBusy || !freezeMovementWhileAttacking)
        HandleMovement(dist);

      FacePlayer();

      if (!isBusy)
        HandleAttack();
    }

    void TryFindPlayer()
    {
      GameObject found = GameObject.FindGameObjectWithTag("Player");
      if (found != null)
        player = found.transform;
    }

    float HorizontalDistanceToPlayer()
    {
      Vector3 delta = player.position - transform.position;
      delta.y = 0f;
      return delta.magnitude;
    }

    void HandleMovement(float dist)
    {
      switch (moveMode)
      {
        case HexfireEnemyMoveMode.Chase:
          MoveChase(dist);
          break;
        case HexfireEnemyMoveMode.Stationary:
          break;
        case HexfireEnemyMoveMode.Strafe:
          MoveStrafe(dist);
          break;
        case HexfireEnemyMoveMode.Retreat:
          MoveRetreat(dist);
          break;
        case HexfireEnemyMoveMode.Orbit:
          MoveOrbit(dist);
          break;
        case HexfireEnemyMoveMode.Kite:
          MoveKite(dist);
          break;
        case HexfireEnemyMoveMode.Charge:
          MoveCharge(dist);
          break;
      }
    }

    void MoveChase(float dist)
    {
      Vector3 toPlayer = FlatToPlayer();
      Vector3 right = Vector3.Cross(Vector3.up, toPlayer);
      Vector3 zigzag = right * (Mathf.Sin(zigzagPhase) * zigzagAmplitude);

      Vector3 velocity = Vector3.zero;
      if (dist > chaseStopRange)
      {
        float speedScale = Mathf.Clamp01((dist - chaseStopRange) / Mathf.Max(0.5f, preferredRange - chaseStopRange));
        velocity = (toPlayer + zigzag).normalized * (moveSpeed * Mathf.Lerp(0.45f, 1f, speedScale));
      }

      ApplyMove(velocity);
    }

    void MoveStrafe(float dist)
    {
      Vector3 toPlayer = FlatToPlayer();
      Vector3 right = Vector3.Cross(Vector3.up, toPlayer);

      strafeTimer -= Time.deltaTime;
      if (strafeTimer <= 0f)
      {
        strafeDir = Random.value > 0.5f ? 1 : -1;
        strafeTimer = Random.Range(0.45f, 0.9f);
      }

      Vector3 move = right * strafeDir * strafeSpeed;
      move += GetRangeCorrection(toPlayer, dist) * 0.6f;
      ApplyMove(move);
    }

    void MoveRetreat(float dist)
    {
      Vector3 toPlayer = FlatToPlayer();
      Vector3 move = Vector3.zero;

      if (dist < retreatRange)
        move = -toPlayer * moveSpeed;
      else if (dist > preferredRange)
        move = toPlayer * (moveSpeed * 0.7f);

      ApplyMove(move);
    }

    void MoveOrbit(float dist)
    {
      Vector3 toPlayer = FlatToPlayer();
      Vector3 right = Vector3.Cross(Vector3.up, toPlayer);
      Vector3 move = right * orbitDirection * orbitSpeed;
      move += GetRangeCorrection(toPlayer, dist);
      ApplyMove(move);
    }

    void MoveKite(float dist)
    {
      Vector3 toPlayer = FlatToPlayer();
      Vector3 right = Vector3.Cross(Vector3.up, toPlayer);

      Vector3 move = right * orbitDirection * (orbitSpeed * 0.85f);
      if (dist < preferredRange - rangeTolerance)
        move -= toPlayer * moveSpeed;
      else if (dist > preferredRange + rangeTolerance)
        move += toPlayer * (moveSpeed * 0.55f);

      ApplyMove(move);
    }

    void MoveCharge(float dist)
    {
      Vector3 toPlayer = FlatToPlayer();

      if (chargeTimer <= 0f && dist > chaseStopRange && dist < preferredRange + 4f)
      {
        StartCoroutine(DoCharge(toPlayer));
        chargeTimer = chargeCooldown;
        return;
      }

      MoveKite(dist);
    }

    IEnumerator DoCharge(Vector3 direction)
    {
      isBusy = true;
      float traveled = 0f;

      while (traveled < chargeDistance)
      {
        ApplyMove(direction * chargeSpeed);
        traveled += chargeSpeed * Time.deltaTime;
        yield return null;
      }

      isBusy = false;
    }

    Vector3 GetRangeCorrection(Vector3 toPlayer, float dist)
    {
      if (dist > preferredRange + rangeTolerance)
        return toPlayer * moveSpeed * 0.5f;
      if (dist < preferredRange - rangeTolerance)
        return -toPlayer * moveSpeed * 0.5f;
      return Vector3.zero;
    }

    void ApplyMove(Vector3 horizontal)
    {
      if (controller.isGrounded)
        verticalVelocity = -2f;
      else
        verticalVelocity += -20f * Time.deltaTime;

      horizontal.y = verticalVelocity;
      controller.Move(horizontal * Time.deltaTime);
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
        320f * Time.deltaTime);
    }

    void HandleAttack()
    {
      switch (attackPattern)
      {
        case HexfireEnemyAttackPattern.Single:
          if (fireTimer <= 0f) { ShootSingle(); fireTimer = singleFireRate; }
          break;
        case HexfireEnemyAttackPattern.Burst:
          if (fireTimer <= 0f) { StartCoroutine(ShootBurst()); fireTimer = burstCooldown; }
          break;
        case HexfireEnemyAttackPattern.Spiral:
          if (fireTimer <= 0f) { StartCoroutine(ShootSpiral()); fireTimer = spiralDuration + spiralPause; }
          break;
        case HexfireEnemyAttackPattern.Ring:
          if (fireTimer <= 0f) { ShootRing(); fireTimer = ringCooldown; }
          break;
        case HexfireEnemyAttackPattern.Fan:
          if (fireTimer <= 0f) { ShootFan(); fireTimer = fanCooldown; }
          break;
        case HexfireEnemyAttackPattern.Scatter:
          if (fireTimer <= 0f) { ShootScatter(); fireTimer = scatterCooldown; }
          break;
        case HexfireEnemyAttackPattern.Cross:
          if (fireTimer <= 0f) { ShootCross(); fireTimer = crossCooldown; }
          break;
        case HexfireEnemyAttackPattern.PulseRing:
          if (fireTimer <= 0f) { StartCoroutine(ShootPulseRing()); fireTimer = pulseRingCooldown; }
          break;
        case HexfireEnemyAttackPattern.Alternating:
          if (fireTimer <= 0f) { ShootAlternating(); fireTimer = alternatingCooldown; }
          break;
        case HexfireEnemyAttackPattern.Shotgun:
          if (fireTimer <= 0f) { ShootShotgun(); fireTimer = shotgunCooldown; }
          break;
        case HexfireEnemyAttackPattern.Wave:
          if (fireTimer <= 0f) { StartCoroutine(ShootWave()); fireTimer = waveCooldown; }
          break;
      }
    }

    Vector3 GetAimedDirection()
    {
      if (firePoint == null || player == null)
        return Vector3.forward;

      Vector3 target = player.position;
      CharacterController playerController = player.GetComponent<CharacterController>();
      if (playerController != null && aimLeadStrength > 0f)
      {
        Vector3 playerVel = playerController.velocity;
        playerVel.y = 0f;
        float timeToReach = HorizontalDistanceToPlayer() / Mathf.Max(bulletSpeed, 0.1f);
        target += playerVel * (timeToReach * aimLeadStrength);
      }

      return FlatDirection(firePoint.position, target);
    }

    void ShootSingle()
    {
      if (firePoint == null)
        return;

      Spawn(GetAimedDirection());
      enemyAnimator?.PlayAttack();
    }

    IEnumerator ShootBurst()
    {
      isBusy = true;
      Vector3 baseDir = GetAimedDirection();

      for (int i = 0; i < burstBullets; i++)
      {
        float t = burstBullets <= 1 ? 0.5f : (float)i / (burstBullets - 1);
        float spread = Mathf.Lerp(-burstSpread, burstSpread, t);
        Spawn(Quaternion.AngleAxis(spread, Vector3.up) * baseDir);
        yield return new WaitForSeconds(0.07f);
      }

      enemyAnimator?.PlayAttack();
      isBusy = false;
    }

    IEnumerator ShootSpiral()
    {
      isBusy = true;
      float elapsed = 0f;

      while (elapsed < spiralDuration)
      {
        for (int arm = 0; arm < spiralArms; arm++)
        {
          float angle = spiralAngle + (360f / spiralArms) * arm;
          Spawn(Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward, 0.92f);
        }

        spiralAngle = (spiralAngle + spiralAngleStep) % 360f;
        elapsed += spiralFireRate;
        yield return new WaitForSeconds(spiralFireRate);
      }

      enemyAnimator?.PlayAttack();
      yield return new WaitForSeconds(spiralPause);
      isBusy = false;
    }

    void ShootRing()
    {
      for (int i = 0; i < ringBulletCount; i++)
      {
        float angle = (360f / ringBulletCount) * i;
        Spawn(Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward);
      }

      enemyAnimator?.PlayAttack();
    }

    void ShootCross()
    {
      int rays = Mathf.Max(4, crossRays);
      for (int i = 0; i < rays; i++)
      {
        float angle = (360f / rays) * i;
        Spawn(Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward);
      }

      enemyAnimator?.PlayAttack();
    }

    IEnumerator ShootPulseRing()
    {
      isBusy = true;

      for (int wave = 0; wave < pulseRingWaves; wave++)
      {
        float waveOffset = pulseRingBaseAngle + pulseRingRotationStep * wave;
        for (int i = 0; i < ringBulletCount; i++)
        {
          float angle = waveOffset + (360f / ringBulletCount) * i;
          Spawn(Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward, 0.95f);
        }

        yield return new WaitForSeconds(pulseRingDelay);
      }

      pulseRingBaseAngle = (pulseRingBaseAngle + pulseRingRotationStep) % 360f;
      enemyAnimator?.PlayAttack();
      isBusy = false;
    }

    void ShootAlternating()
    {
      Vector3 baseDir = GetAimedDirection();
      float side = alternatingAngle * alternatingSide;
      Spawn(Quaternion.AngleAxis(side, Vector3.up) * baseDir);
      alternatingSide *= -1;
      enemyAnimator?.PlayAttack();
    }

    void ShootShotgun()
    {
      Vector3 baseDir = GetAimedDirection();
      float half = shotgunSpread * 0.5f;

      for (int i = 0; i < shotgunPellets; i++)
      {
        float t = shotgunPellets <= 1 ? 0.5f : (float)i / (shotgunPellets - 1);
        float spread = Mathf.Lerp(-half, half, t);
        Spawn(Quaternion.AngleAxis(spread, Vector3.up) * baseDir, 1.05f);
      }

      enemyAnimator?.PlayAttack();
    }

    IEnumerator ShootWave()
    {
      isBusy = true;
      Vector3 baseDir = GetAimedDirection();
      float half = waveSweepAngle * 0.5f;

      for (int i = 0; i < waveShots; i++)
      {
        float t = waveShots <= 1 ? 0.5f : (float)i / (waveShots - 1);
        float spread = Mathf.Lerp(-half, half, t);
        Spawn(Quaternion.AngleAxis(spread, Vector3.up) * baseDir);
        yield return new WaitForSeconds(waveShotDelay);
      }

      enemyAnimator?.PlayAttack();
      isBusy = false;
    }

    void ShootFan()
    {
      Vector3 baseDir = GetAimedDirection();
      float half = fanAngle * 0.5f;

      for (int i = 0; i < fanBulletCount; i++)
      {
        float t = fanBulletCount <= 1 ? 0.5f : (float)i / (fanBulletCount - 1);
        float spread = Mathf.Lerp(-half, half, t);
        Spawn(Quaternion.AngleAxis(spread, Vector3.up) * baseDir);
      }

      enemyAnimator?.PlayAttack();
    }

    void ShootScatter()
    {
      for (int i = 0; i < scatterBulletCount; i++)
      {
        float angle = Random.Range(0f, 360f);
        Spawn(Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward, Random.Range(0.85f, 1.1f));
      }

      enemyAnimator?.PlayAttack();
    }

    void Spawn(Vector3 direction, float speedMultiplier = 1f)
    {
      if (bulletPrefab == null || firePoint == null)
        return;

      HexfireEnemyBulletSpawner.Spawn(
        bulletPrefab,
        firePoint.position,
        direction.normalized * bulletSpeed * speedMultiplier,
        this);
    }

    static Vector3 FlatDirection(Vector3 from, Vector3 to)
    {
      Vector3 dir = to - from;
      dir.y = 0f;
      return dir.sqrMagnitude < 0.001f ? Vector3.forward : dir.normalized;
    }

    Vector3 FlatToPlayer()
    {
      return FlatDirection(transform.position, player.position);
    }

    void OnDrawGizmosSelected()
    {
      Gizmos.color = Color.red;
      Gizmos.DrawWireSphere(transform.position, chaseStartRange);
      Gizmos.color = Color.yellow;
      Gizmos.DrawWireSphere(transform.position, chaseStopRange);
      Gizmos.color = Color.cyan;
      Gizmos.DrawWireSphere(transform.position, preferredRange);
    }
  }
}
