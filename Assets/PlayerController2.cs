using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController2 : MonoBehaviour
{
    public float speed = 8f;
    public float gravity = -20f;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private CharacterController cc;
    private float verticalVelocity;
    private PlayerShoot playerShoot;
    private PlayerAbilities abilities;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        cc = GetComponent<CharacterController>();
        playerShoot = GetComponent<PlayerShoot>();
        abilities = GetComponent<PlayerAbilities>();
    }

    void Update()
    {
        // Jeśli trwa dash — ruch obsługuje PlayerAbilities
        if (abilities != null && abilities.IsDashing) return;

        // Grawitacja
        if (cc.isGrounded)
            verticalVelocity = -2f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        // Ruch
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 movement = new Vector3(input.x, 0f, input.y).normalized;

        // Obracaj w kierunku ruchu TYLKO gdy nie strzelasz
        bool isShooting = playerShoot != null && playerShoot.IsShooting;
        if (movement != Vector3.zero && !isShooting)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(movement),
                0.15f
            );
        }

        Vector3 velocity = movement * speed;
        velocity.y = verticalVelocity;
        cc.Move(velocity * Time.deltaTime);
    }
}