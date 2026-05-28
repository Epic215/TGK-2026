using UnityEngine;

public class SpiralShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject mandalaPrefab;
    public float projectileSpeed = 3f;
    public float fireRate = 0.1f;

    [Header("Fire Mode")]
    public FireMode fireMode = FireMode.Spiral;

    public enum FireMode
    {
        Spiral,
        Mandala,
    }

    [Header("Spiral Settings")]
    public float rotationSpeed = 5f;
    public int armsCount = 3;

    [Header("Mandala Settings")]
    public int mandalaArmCount = 4;       
    public float mandalaRotationSpeed = 60f;
    public float mandalaFireRate = 0.15f;

    private float nextFireTime = 0f;
    private float nextMandalaFireTime = 0f;
    private float rotationAngle = 0f;
    private float mandalaAngle = 0f;

    void Update()
    {
        if (fireMode == FireMode.Mandala)
            transform.Rotate(0f, mandalaRotationSpeed * Time.deltaTime * 10f, 0f);
        if (fireMode == FireMode.Spiral && Time.time >= nextFireTime)
        {
            FireSpiral();
            nextFireTime = Time.time + fireRate;
        }

        if (fireMode == FireMode.Mandala && Time.time >= nextMandalaFireTime)
        {
            FireMandala();
            nextMandalaFireTime = Time.time + mandalaFireRate;
        }
    }

    void FireSpiral()
    {
        for (int arm = 0; arm < armsCount; arm++)
        {
            float armOffset = (360f / armsCount) * arm;
            float angle = rotationAngle + armOffset;

            float radians = angle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(
                Mathf.Cos(radians),
                0,
                Mathf.Sin(radians)
            ).normalized;

            GameObject projectile = Instantiate(
                projectilePrefab,
                transform.position + direction * 1.5f,
                Quaternion.LookRotation(direction)
            );

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = direction * projectileSpeed;
            }
        }

        rotationAngle += rotationSpeed;
        if (rotationAngle >= 360f) rotationAngle -= 360f;
    }

    void FireMandala()
    {
        mandalaAngle += mandalaRotationSpeed;
        // Debug.Log("mandalaAngle: " + mandalaAngle);
        if (mandalaAngle >= 360f) mandalaAngle -= 360f;

        for (int a = 0; a < mandalaArmCount; a++)
        {
            float angle = mandalaAngle + (360f / mandalaArmCount) * a;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)).normalized;

            GameObject projectile = Instantiate(
                mandalaPrefab,
                transform.position + dir * 1.5f,
                Quaternion.LookRotation(dir)
            );

            projectile.layer = LayerMask.NameToLayer("MandalaBullet"); // <-- tu

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = dir * projectileSpeed;
            }
        }
    }
}