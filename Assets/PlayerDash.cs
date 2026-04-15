using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    public float dashDistance = 5f;
    public float dashCooldown = 1f;

    private PlayerInput playerInput;
    private InputAction dashAction;
    private InputAction moveAction;
    private float cooldownTimer;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        dashAction = playerInput.actions["Dash"];
        moveAction = playerInput.actions["Move"];
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (dashAction.WasPressedThisFrame() && cooldownTimer <= 0f)
        {
            Dash();
            cooldownTimer = dashCooldown;
        }
    }

    void Dash()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 direction;

        // Jeśli gracz się porusza - dash w tym kierunku
        // Jeśli stoi - dash do przodu
        if (input != Vector2.zero)
            direction = new Vector3(input.x, 0f, input.y).normalized;
        else
            direction = transform.forward;

        Vector3 destination = transform.position + direction * dashDistance;

        // Sprawdź czy nie wchodzi w ścianę
        CharacterController cc = GetComponent<CharacterController>();
        cc.Move(destination - transform.position);
    }
}