using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private LayerMask groundMask;

    private Camera mainCamera;
    private PlayerInput playerInput;
    private InputAction shootAction;

    public bool IsShooting { get; private set; }

    private void Start()
    {
        mainCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();
        shootAction = playerInput.actions["Shoot"];
    }

    private void Update()
    {
        IsShooting = shootAction.IsPressed();

        if (IsShooting)
            RotateTowardsCursor();
    }

    private void RotateTowardsCursor()
    {
        var (success, position) = GetMousePosition();
        if (success)
        {
            var direction = position - transform.position;
            direction.y = 0;
            if (direction != Vector3.zero)
                transform.forward = direction;
        }
    }

    private (bool success, Vector3 position) GetMousePosition()
    {
        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundMask))
            return (success: true, position: hitInfo.point);
        else
            return (success: false, position: Vector3.zero);
    }
}

//     private void Shoot()
//     {
//         GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
//         Rigidbody rb = bullet.GetComponent<Rigidbody>();
//         rb.linearVelocity = firePoint.forward * bulletSpeed;

//         Collider bulletCollider = bullet.GetComponent<Collider>();
//         Collider playerCollider = GetComponent<Collider>();
//         if (bulletCollider != null && playerCollider != null)
//             Physics.IgnoreCollision(bulletCollider, playerCollider);

//         Destroy(bullet, 3f);
//     }
// }