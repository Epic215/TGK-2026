using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    public float dashDistance = 5f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;

    public bool IsDashing { get; private set; }

    private PlayerInput playerInput;
    private InputAction dashAction;
    private InputAction moveAction;
    private float cooldownTimer;
    private CharacterController cc;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        dashAction = playerInput.actions["Dash"];
        moveAction = playerInput.actions["Move"];
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (dashAction.WasPressedThisFrame() && cooldownTimer <= 0f && !IsDashing)
        {
            Vector2 input = moveAction.ReadValue<Vector2>();
            Vector3 direction = input != Vector2.zero
                ? new Vector3(input.x, 0f, input.y).normalized
                : transform.forward;

            StartCoroutine(DashCoroutine(direction));
            cooldownTimer = dashCooldown;
        }
    }

    IEnumerator DashCoroutine(Vector3 direction)
{
    IsDashing = true;
    float elapsed = 0f;
    Quaternion targetRot = Quaternion.LookRotation(direction);

    while (elapsed < dashDuration)
    {
        float step = (dashDistance / dashDuration) * Time.deltaTime;
        cc.Move(direction * step);

        
         transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            720f * Time.deltaTime
        );

        elapsed += Time.deltaTime;
        yield return null;
    }

    IsDashing = false;
}
}