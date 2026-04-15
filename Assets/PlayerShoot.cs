using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 15f;
    public float shootCooldown = 0.3f;

    private PlayerInput playerInput;
    private InputAction shootAction;
    private float cooldownTimer;

    public bool IsShooting { get; private set; } // PlayerController to odczyta

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        shootAction = playerInput.actions["Shoot"];
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        // Czy przytrzymany przycisk strzelania
        IsShooting = shootAction.IsPressed();

        if (IsShooting)
            RotateTowardsCursor();

        if (IsShooting && cooldownTimer <= 0f)
        {
            Shoot();
            cooldownTimer = shootCooldown;
        }
    }

    private void RotateTowardsCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 target = ray.GetPoint(distance);
            Vector3 direction = target - transform.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.linearVelocity = firePoint.forward * bulletSpeed;

        // Ignoruj kolizję między pociskiem a graczem
        Collider bulletCollider = bullet.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();
        if (bulletCollider != null && playerCollider != null)
            Physics.IgnoreCollision(bulletCollider, playerCollider);

        Destroy(bullet, 3f);
    }

}