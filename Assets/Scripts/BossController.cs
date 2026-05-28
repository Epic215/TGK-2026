using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ──────────────────────────────────────────────────────────────────────────────
//  BOSS CONTROLLER  –  inspirowany Cannonballfrogiem z Enter the Gungeon
//
//  FAZY:
//    Phase 1 (HP > 60%)  – skoki + pojedyncze strzały + krzyżowe volley
//    Phase 2 (HP 30-60%) – burst + SPIRALA (z SpiralShooter) + bomby
//    Phase 3 (HP < 30%)  – szał: MANDALA (z MandalaBullet) + teleport + spirala podwójna
//
//  WYMAGANE TAGI / KOMPONENTY:
//    • GameObject gracza z tagiem "Player" i komponentem PlayerHealth
//    • Prefaby: bulletPrefab, mandalaBulletPrefab, bombPrefab, warnCirclePrefab
//    • firePoint (Transform podrzędny bossa)
//    • Komponenty: CharacterController, EnemyHealth (z polem currentHealth/maxHealth)
// ──────────────────────────────────────────────────────────────────────────────

public enum BossPhase { Phase1, Phase2, Phase3 }

[RequireComponent(typeof(CharacterController))]
public class BossController : MonoBehaviour
{
    // ── Referencje ────────────────────────────────────────────────────────────
    [Header("References")]
    public GameObject bulletPrefab;
    public GameObject mandalaBulletPrefab;  // MandalaBullet – zakrzywiony, wraca do punktu startu
    // public GameObject bombPrefab;
    // public GameObject warnCirclePrefab;
    public Transform  firePoint;

    // ── Ruch ──────────────────────────────────────────────────────────────────
    [Header("Movement")]
    public float moveSpeed        = 3.5f;
    public float chaseStopRange   = 5f;
    public float jumpForce        = 12f;
    public float gravity          = 28f;
    public float jumpCooldownMin  = 2f;
    public float jumpCooldownMax  = 4f;
    public float teleportCooldown = 8f;

    // ── Pociski ───────────────────────────────────────────────────────────────
    [Header("Bullets")]
    public float bulletSpeed      = 12f;
    public float bombSpeed        = 6f;

    [Header("Mandala Settings")]
    public int   mandalaArmCount       = 8;
    public float mandalaRotationSpeed  = 1f;
    public float mandalaFireRate       = 0.1f;
    public float mandalaSpeed          = 15f;
    public float mandalaCooldown       = 5f;

    // ── Ataki – timery ────────────────────────────────────────────────────────
    [Header("Attack Timings")]
    public float singleFireRate   = 1.5f;
    public float crossVolleyRate  = 3f;
    public float burstCooldown    = 2.5f;
    public int   burstBullets     = 10;
    public float bombCooldown     = 5f;
    public int   bombCount        = 5;
    public float bombWarnDuration = 1.2f;
    public float teleportCooldownTimer = 8f;

    // ── SpiralShooter – Spiral ────────────────────────────────────────────────
    [Header("Spiral Pattern (faza 2)")]
    public float spiralFireRate   = 0.06f;   // odpowiednik SpiralShooter.fireRate
    public float spiralDuration   = 3f;
    public float spiralPause      = 2.5f;
    public float spiralRotSpeed   = 25f;     // stopni na tick – odpowiednik rotationSpeed
    public int   spiralArms       = 2;       // odpowiednik armsCount
    public float spiralCooldown   = 6f;

    // ── SpiralShooter – Mandala ───────────────────────────────────────────────
    [Header("Mandala Pattern (faza 3)")]

    private bool isDoingPattern = false;

    // ── HP progi faz ──────────────────────────────────────────────────────────
    [Header("Phase Thresholds (0-1)")]
    [Range(0f,1f)] public float phase2Threshold = 0.6f;
    [Range(0f,1f)] public float phase3Threshold = 0.3f;

    public float aggroRange = 20f;

    // ── Prywatne ─────────────────────────────────────────────────────────────
    private Transform          player;
    private CharacterController cc;
    private EnemyHealth        health;
    private BossPhase          currentPhase = BossPhase.Phase1;
    

    private float  verticalVelocity = 0f;
    private float  jumpTimer        = 0f;

    private float  singleTimer      = 0f;
    private float  crossTimer       = 0f;
    private float  burstTimer       = 0f;
    private float  bombTimer        = 0f;
    private float  teleportTimer    = 0f;
    private float  spiralTimer      = 0f;
    private float  mandalaTimer     = 0f;

    // Spirala – kąt trzymany między wywołaniami (jak w SpiralShooter)
    private float  spiralAngle      = 0f;
    // Mandala – kąt obrotu (jak w SpiralShooter.mandalaAngle)
    private float  mandalaAngle     = 0f;

    private bool   isBusy           = false;

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        cc     = GetComponent<CharacterController>();
        health = GetComponent<EnemyHealth>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        jumpTimer    = Random.Range(jumpCooldownMin, jumpCooldownMax);
        teleportTimer = teleportCooldown;
    }

    void Update()
    {
        if (player == null) return;

        UpdatePhase();
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= aggroRange)
        {
            HandleMovement();
            HandleAttacks();
        }

        singleTimer   -= Time.deltaTime;
        crossTimer    -= Time.deltaTime;
        burstTimer    -= Time.deltaTime;
        bombTimer     -= Time.deltaTime;
        teleportTimer -= Time.deltaTime;
        spiralTimer   -= Time.deltaTime;
        mandalaTimer  -= Time.deltaTime;
        jumpTimer     -= Time.deltaTime;
    }

    // ── Faza ──────────────────────────────────────────────────────────────────
    void UpdatePhase()
    {
        if (health == null) return;
        float hpRatio = (float)health.GetHealth() / health.GetMaxHealth();

        BossPhase prev = currentPhase;

        if      (hpRatio <= phase3Threshold) currentPhase = BossPhase.Phase3;
        else if (hpRatio <= phase2Threshold) currentPhase = BossPhase.Phase2;
        else                                 currentPhase = BossPhase.Phase1;

        if (prev != currentPhase) OnPhaseChange(currentPhase);
    }

    void OnPhaseChange(BossPhase phase)
    {
        switch (phase)
        {
            case BossPhase.Phase2:
                burstTimer  = 0.3f;
                spiralTimer = 0.5f;
                jumpTimer   = 0.2f;
                break;

            case BossPhase.Phase3:
                bombTimer    = 0.1f;
                mandalaTimer = 0.1f;
                StartCoroutine(PhaseThreeEntrance());
                break;
        }
    }

    // ── Ruch ──────────────────────────────────────────────────────────────────
    void HandleMovement()
    {
        if (!cc.isGrounded)
            verticalVelocity -= gravity * Time.deltaTime;
        else if (verticalVelocity < 0f)
            verticalVelocity = -2f;

        float dist = Vector3.Distance(transform.position, player.position);
        Vector3 move = Vector3.zero;

        if (dist > chaseStopRange && !isDoingPattern)
        {
            Vector3 toPlayer = (player.position - transform.position);
            toPlayer.y = 0f;
            toPlayer.Normalize();
            float speed = currentPhase == BossPhase.Phase3 ? moveSpeed * 1.5f :
                        currentPhase == BossPhase.Phase2 ? moveSpeed * 1.2f : moveSpeed;
            move = toPlayer * speed;
        }

        if (cc.isGrounded && jumpTimer <= 0f && !isDoingPattern)
        {
            StartCoroutine(JumpTowardPlayer());
            jumpTimer = Random.Range(jumpCooldownMin, jumpCooldownMax);
        }

        move.y = verticalVelocity;
        cc.Move(move * Time.deltaTime);
        FacePlayer();
    }

    IEnumerator JumpTowardPlayer()
    {
        isBusy = true;
        verticalVelocity = jumpForce;
        yield return new WaitForSeconds(0.2f);

        // Strzelaj w powietrzu
        if (currentPhase >= BossPhase.Phase2) ShootCrossVolley();
        else                                  ShootSingle();

        yield return new WaitForSeconds(0.1f);
        isBusy = false;
    }

    // ── Ataki ─────────────────────────────────────────────────────────────────
    void HandleAttacks()
    {
        if (isBusy) return;

        switch (currentPhase)
        {
            // ── FAZA 1: Single + Cross Volley ──────────────────────────────
            case BossPhase.Phase1:
                if (singleTimer <= 0f) { ShootSingle();      singleTimer = singleFireRate; }
                if (crossTimer  <= 0f) { ShootCrossVolley(); crossTimer  = crossVolleyRate; }
                break;

            // ── FAZA 2: + Burst + SPIRALA (SpiralShooter.FireSpiral) ───────
            case BossPhase.Phase2:
                if (singleTimer <= 0f) { ShootSingle();                        singleTimer = singleFireRate * 0.8f; }
                if (crossTimer  <= 0f) { ShootCrossVolley();                   crossTimer  = crossVolleyRate * 0.8f; }
                if (burstTimer  <= 0f) { StartCoroutine(ShootBurst());         burstTimer  = burstCooldown; }
                if (spiralTimer <= 0f) { StartCoroutine(ShootSpiral());        spiralTimer = spiralCooldown; }
                // if (bombTimer   <= 0f) { StartCoroutine(SpawnBombs(bombCount));bombTimer   = bombCooldown; }
                break;

            // ── FAZA 3: + MANDALA (SpiralShooter.FireMandala) + Teleport ──
            case BossPhase.Phase3:
                if (singleTimer  <= 0f) { ShootSingle();                              singleTimer  = singleFireRate * 0.5f; }
                if (crossTimer   <= 0f) { ShootCrossVolley();                         crossTimer   = crossVolleyRate * 0.6f; }
                if (burstTimer   <= 0f) { StartCoroutine(ShootBurst());               burstTimer   = burstCooldown * 0.7f; }
                if (spiralTimer  <= 0f) { StartCoroutine(ShootSpiral());              spiralTimer  = spiralCooldown * 0.8f; }
                if (mandalaTimer <= 0f) { StartCoroutine(ShootMandalaPattern());      mandalaTimer = mandalaCooldown; }
                // if (bombTimer    <= 0f) { StartCoroutine(SpawnBombs(bombCount + 3));  bombTimer    = bombCooldown * 0.6f; }
                if (teleportTimer<= 0f) { StartCoroutine(Teleport());                 teleportTimer= teleportCooldown; }
                break;
        }
    }

    // ─── Single ───────────────────────────────────────────────────────────────
    void ShootSingle()
    {
        if (player == null || firePoint == null) return;
        Vector3 dir = (player.position - firePoint.position);
        dir.y = 0f;
        dir.Normalize();
        SpawnBullet(bulletPrefab, firePoint.position, dir * bulletSpeed);
    }

    // ─── Cross Volley ─────────────────────────────────────────────────────────
    void ShootCrossVolley()
    {
        int rays = currentPhase == BossPhase.Phase3 ? 8 : 4;
        for (int i = 0; i < rays; i++)
        {
            float  angle = (360f / rays) * i;
            Vector3 dir  = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
            SpawnBullet(bulletPrefab, firePoint.position, dir * bulletSpeed);
        }
    }

    // ─── Burst ────────────────────────────────────────────────────────────────
    IEnumerator ShootBurst()
    {
        isBusy = true;
        Vector3 toPlayer = (player.position - firePoint.position);
        toPlayer.y = 0f;
        toPlayer.Normalize();

        for (int i = 0; i < burstBullets; i++)
        {
            float   spread = Mathf.Lerp(-40f, 40f, (float)i / (burstBullets - 1));
            Vector3 vel    = Quaternion.AngleAxis(spread, Vector3.up) * toPlayer * bulletSpeed * 1.1f;
            SpawnBullet(bulletPrefab, firePoint.position, vel);
            yield return new WaitForSeconds(0.06f);
        }
        isBusy = false;
    }

    // ─── SPIRALA – odwzorowanie SpiralShooter.FireSpiral() ───────────────────
    //   spiralAngle += spiralRotSpeed  (jak rotationAngle += rotationSpeed)
    //   N ramion rozłożonych co 360/arms stopni
    IEnumerator ShootSpiral()
    {
        isBusy = true;
        float elapsed = 0f;
        int   arms    = currentPhase == BossPhase.Phase3 ? spiralArms + 2 : spiralArms;

        while (elapsed < spiralDuration)
        {
            for (int arm = 0; arm < arms; arm++)
            {
                float   armOffset = (360f / arms) * arm;
                float   angle     = spiralAngle + armOffset;
                float   radians   = angle * Mathf.Deg2Rad;

                // Dokładnie jak w SpiralShooter.FireSpiral – wektor z Cos/Sin
                Vector3 direction = new Vector3(
                    Mathf.Cos(radians),
                    0f,
                    Mathf.Sin(radians)
                ).normalized;

                SpawnBullet(bulletPrefab, firePoint.position, direction * bulletSpeed * 0.9f);
            }

            // spiralAngle += rotationSpeed (jak w SpiralShooter)
            spiralAngle += spiralRotSpeed;
            if (spiralAngle >= 360f) spiralAngle -= 360f;

            elapsed += spiralFireRate;
            yield return new WaitForSeconds(spiralFireRate);
        }

        yield return new WaitForSeconds(spiralPause);
        isBusy = false;
    }

    // ─── MANDALA – odwzorowanie SpiralShooter.FireMandala() ──────────────────
    //   mandalaAngle += mandalaRotationSpeed  (jak w SpiralShooter)
    //   pociski MandalaBullet – zakrzywiają się i wracają do punktu startu
    IEnumerator ShootMandalaPattern()
    {
        isDoingPattern = true;
        isBusy = true;
        float duration = 8f;   // jak długo trwa jeden atak mandali
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            // Obrót – dokładnie jak SpiralShooter.FireMandala
            mandalaAngle += mandalaRotationSpeed;
            if (mandalaAngle >= 360f) mandalaAngle -= 360f;

            for (int a = 0; a < mandalaArmCount; a++)
            {
                float   angle = mandalaAngle + (360f / mandalaArmCount) * a;
                float   rad   = angle * Mathf.Deg2Rad;
                Vector3 dir   = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)).normalized;

                Vector3 spawnPos = firePoint.position + dir * 1.5f;
                SpawnBullet(mandalaBulletPrefab, spawnPos, dir * mandalaSpeed, isMandala: true);
            }

            elapsed += mandalaFireRate;
            yield return new WaitForSeconds(mandalaFireRate);
        }

        isBusy = false;
        isDoingPattern = false;
    }

    // ─── Wejście fazy 3 – pełna mandala na powitanie ─────────────────────────
    IEnumerator PhaseThreeEntrance()
    {
        isBusy = true;

        // 6-ramienna spirala przez 2 obroty – sygnalizuje przejście
        float angle = 0f;
        for (int i = 0; i < 60; i++)
        {
            for (int a = 0; a < 6; a++)
            {
                float  off  = (360f / 6) * a;
                float  rad  = (angle + off) * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)).normalized;
                SpawnBullet(bulletPrefab, firePoint.position, dir * bulletSpeed);
            }
            angle = (angle + 6f) % 360f;
            yield return new WaitForSeconds(0.05f);
        }

        isBusy = false;
    }

    // ─── Bomby z ostrzeżeniem ─────────────────────────────────────────────────
    // IEnumerator SpawnBombs(int count)
    // {
    //     isBusy = true;
    //     var targets = new List<Vector3>();

    //     for (int i = 0; i < count; i++)
    //     {
    //         Vector3 target;
    //         if (i < count / 2)
    //         {
    //             Vector2 rnd = Random.insideUnitCircle * 3.5f;
    //             target = player.position + new Vector3(rnd.x, 0f, rnd.y);
    //         }
    //         else
    //         {
    //             target = transform.position + new Vector3(
    //                 Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
    //         }

    //         targets.Add(target);

    //         if (warnCirclePrefab != null)
    //             Destroy(Instantiate(warnCirclePrefab, target, Quaternion.identity),
    //                     bombWarnDuration + 0.1f);
    //     }

    //     yield return new WaitForSeconds(bombWarnDuration);

    //     foreach (Vector3 t in targets)
    //     {
    //         if (bombPrefab == null) break;
    //         Vector3 dir = (t - firePoint.position).normalized;
    //         dir.y = 0f;
    //         SpawnBullet(bombPrefab, firePoint.position, dir * bombSpeed);
    //     }

    //     isBusy = false;
    // }

    // ─── Teleport (faza 3) ────────────────────────────────────────────────────
    IEnumerator Teleport()
    {
        isBusy = true;

        Renderer[] rends = GetComponentsInChildren<Renderer>();
        foreach (var r in rends) r.enabled = false;

        yield return new WaitForSeconds(0.4f);

        Vector3 behind = player.position - player.forward * 2.5f;
        behind.y = transform.position.y;

        cc.enabled = false;
        transform.position = behind;
        cc.enabled = true;

        yield return new WaitForSeconds(0.1f);
        foreach (var r in rends) r.enabled = true;

        yield return StartCoroutine(ShootBurst());
        isBusy = false;
    }

    // ── SpawnBullet ───────────────────────────────────────────────────────────
    void SpawnBullet(GameObject prefab, Vector3 spawnPos, Vector3 velocity,
                     bool isMandala = false)
    {
        if (prefab == null) return;
        velocity.y = 0f;
        if (velocity == Vector3.zero) return;

        GameObject bullet = Instantiate(prefab, spawnPos,
                                        Quaternion.LookRotation(velocity));
        bullet.transform.SetParent(null);

        // Ustaw MandalaBullet layer – tak jak robi SpiralShooter.FireMandala
        if (isMandala)
            bullet.layer = LayerMask.NameToLayer("MandalaBullet");

        // ownerTag
        Bullet    b  = bullet.GetComponent<Bullet>();
        if (b  != null) b.ownerTag  = "Enemy";

        MandalaBullet mb = bullet.GetComponent<MandalaBullet>();
        if (mb != null) mb.ownerTag = "Enemy";

        // Ignoruj własne collidery
        Collider bulletCol = bullet.GetComponent<Collider>();
        foreach (var col in GetComponents<Collider>())
            if (bulletCol != null) Physics.IgnoreCollision(bulletCol, col);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic    = false;
            rb.linearVelocity = velocity;
        }

        Destroy(bullet, 5f);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, Quaternion.LookRotation(dir),
                360f * Time.deltaTime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 30f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseStopRange);
    }
}