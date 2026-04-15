using UnityEngine;

public class SpiralShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 15f;
    public float fireRate = 0.1f;
    
    private float nextFireTime = 0f;
    private float rotationAngle = 0f;
    private int bulletCount = 12;

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            FireSpiral();
            nextFireTime = Time.time + fireRate;
        }
    }

    void FireSpiral()
    {
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = (360f / bulletCount) * i + rotationAngle;
            
            float radians = angle * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(
                Mathf.Cos(radians),
                0,
                Mathf.Sin(radians)
            ).normalized;
            
            GameObject projectile = Instantiate(
                projectilePrefab,
                transform.position + direction * 1.5f,
                Quaternion.identity
            );
            
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = direction * projectileSpeed;
            }
        }
        
        rotationAngle += 360f / bulletCount;
    }
}