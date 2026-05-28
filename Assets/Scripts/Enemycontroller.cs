using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum EnemyMoveMode { Chase, Stationary, Strafe }
public enum EnemyShootMode { Single, Burst, Spiral }

public class EnemyController : MonoBehaviour
{
    [Header("Behaviour")]
    public EnemyMoveMode  moveMode  = EnemyMoveMode.Chase;
    public EnemyShootMode shootMode = EnemyShootMode.Single;

    [Header("Movement")]
    public float moveSpeed       = 4f;
    public float chaseStopRange  = 4f;
    public float chaseStartRange = 30f;
    public float strafeSpeed     = 3f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform  firePoint;
    public float      bulletSpeed = 14f;

    [Header("Single Settings")]
    public float singleFireRate = 1.2f;

    [Header("Burst Settings")]
    public int   burstBullets  = 7;
    public float burstCooldown = 2f;

    [Header("Spiral Settings")]
    public float spiralFireRate  = 0.05f;
    public float spiralDuration  = 2.5f;
    public float spiralPause     = 2f;
    public float spiralAngleStep = 25f;
    

    private Transform         player;
    private CharacterController cc;
    private float             fireTimer   = 0f;
    private bool              isBusy      = false;
    private float             strafeTimer = 0f;
    private int               strafeDir   = 1;
    private float             spiralAngle = 0f;
    private float             verticalVelocity = 0f;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (player == null) return;

        fireTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= chaseStartRange)
            HandleMovement();

        if (dist <= chaseStartRange && !isBusy)
            HandleShooting();
    }

    // ─── RUCH ────────────────────────────────────────────────

    void HandleMovement()
    {
        switch (moveMode)
        {
            case EnemyMoveMode.Chase:      MoveChase();  break;
            case EnemyMoveMode.Stationary: FacePlayer(); break;
            case EnemyMoveMode.Strafe:     MoveStrafe(); break;
        }
    }

    void MoveChase()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        Vector3 toPlayer = (player.position - transform.position);
        toPlayer.y = 0f;
        toPlayer.Normalize();

        Vector3 velocity = Vector3.zero;
        if (dist > chaseStopRange)
            velocity = toPlayer * moveSpeed;

        velocity.y = -2f;
        cc.Move(velocity * Time.deltaTime);

        FacePlayer();
    }

    void MoveStrafe()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        Vector3 toPlayer = (player.position - transform.position);
        toPlayer.y = 0f;
        toPlayer.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, toPlayer);

        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0f)
        {
            strafeDir   = (Random.value > 0.5f) ? 1 : -1;
            strafeTimer = Random.Range(0.4f, 0.8f);
        }

        Vector3 move = right * strafeDir * strafeSpeed;

        if (dist > chaseStopRange + 1f)
            move += toPlayer * moveSpeed * 0.5f;
        else if (dist < chaseStopRange - 1f)
            move -= toPlayer * moveSpeed * 0.5f;

        if (cc.isGrounded)
            verticalVelocity = -2f;
        else
            verticalVelocity += -20f * Time.deltaTime;

        move.y = verticalVelocity;
        cc.Move(move * Time.deltaTime);

        FacePlayer();
    }

    void FacePlayer()
    {
        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(dir),
                300f * Time.deltaTime);
    }

    // ─── STRZELANIE ──────────────────────────────────────────

    void HandleShooting()
    {
        switch (shootMode)
        {
            case EnemyShootMode.Single:
                if (fireTimer <= 0f) { ShootSingle(); fireTimer = singleFireRate; }
                break;
            case EnemyShootMode.Burst:
                if (fireTimer <= 0f) { StartCoroutine(ShootBurst()); fireTimer = burstCooldown; }
                break;
            case EnemyShootMode.Spiral:
                if (fireTimer <= 0f) { StartCoroutine(ShootSpiral()); fireTimer = spiralDuration + spiralPause; }
                break;
        }
    }

    void ShootSingle()
    {
        if (player == null || firePoint == null) return;
        Vector3 dir = (player.position - firePoint.position).normalized;
        dir.y = 0f;
        SpawnBullet(dir, 0f);
    }

    IEnumerator ShootBurst()
    {
        isBusy = true;

        Vector3 toPlayer = (player.position - firePoint.position).normalized;
        toPlayer.y = 0f;

        int focused = burstBullets / 2;
        int random  = burstBullets - focused;

        // połowa skupiona na graczu
        for (int i = 0; i < focused; i++)
        {
            float angle = Random.Range(-20f, 20f);
            SpawnBullet(toPlayer, angle);
        }

        for (int i = 0; i < random; i++)
        {
            float angle = Random.Range(0f, 360f);
            Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
            SpawnBullet(dir, 0f);
        }

        yield return new WaitForSeconds(0.1f);
        isBusy = false;
    }

    IEnumerator ShootSpiral()
    {
        isBusy = true;
        float elapsed = 0f;

        while (elapsed < spiralDuration)
        {
            Vector3 dir = Quaternion.AngleAxis(spiralAngle, Vector3.up) * Vector3.forward;
            SpawnBullet(dir, 0f);
            spiralAngle += spiralAngleStep;
            elapsed     += spiralFireRate;
            yield return new WaitForSeconds(spiralFireRate);
        }

        yield return new WaitForSeconds(spiralPause);
        isBusy = false;
    }

    void SpawnBullet(Vector3 direction, float angleOffset)
    {
        if (bulletPrefab == null || firePoint == null) return;

        direction.y = 0f;
        if (direction == Vector3.zero) return;

        Vector3 vel = Quaternion.AngleAxis(angleOffset, Vector3.up) * direction * bulletSpeed;

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(vel));
        bullet.transform.SetParent(null);

        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null) b.ownerTag = "Enemy";

        Collider bulletCol = bullet.GetComponent<Collider>();
        Collider[] enemyCols = GetComponents<Collider>();
        foreach (var col in enemyCols)
            if (bulletCol != null) Physics.IgnoreCollision(bulletCol, col);

        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            vel.y = 0f;
            bulletRb.linearVelocity = vel;
        }

        Destroy(bullet, 3f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseStartRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseStopRange);
    }
}