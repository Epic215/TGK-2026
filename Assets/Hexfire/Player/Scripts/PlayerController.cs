using UnityEngine;
using UnityEngine.InputSystem;

namespace Hexfire
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerDashAbility))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float speed = 8f;
        public float gravity = -25f;
        [Range(0.05f, 1f)]
        public float rotationSmoothing = 0.15f;

        [Header("Ground")]
        public LayerMask groundLayers = ~0;
        [Tooltip("Siła docisku do podłogi gdy gracz stoi / lekko unosi się nad nią.")]
        public float groundStick = 10f;
        [Tooltip("Maks. odległość nad podłogą, z której postać zostanie przyciągnięta w dół.")]
        public float maxGroundSnap = 0.4f;
        public bool snapToGroundOnStart = true;

        CharacterController characterController;
        PlayerInput playerInput;
        InputAction moveAction;
        PlayerDashAbility dashAbility;
        PlayerShoot playerShoot;
        float verticalVelocity;

        void Awake()
        {
            characterController = GetComponent<CharacterController>();
            dashAbility = GetComponent<PlayerDashAbility>();
            playerShoot = GetComponent<PlayerShoot>();
        }

        void Start()
        {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogWarning("Hexfire.PlayerController: brak PlayerInput.", this);
            }
            else
            {
                moveAction = playerInput.actions["Move"];
            }

            if (snapToGroundOnStart)
                SnapToGroundImmediate();
        }

        void Update()
        {
            if (dashAbility != null && dashAbility.IsDashing)
                return;

            ApplyGravity();

            Vector2 input = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            Vector3 movement = new Vector3(input.x, 0f, input.y);
            if (movement.sqrMagnitude > 1f)
                movement.Normalize();

            bool isShooting = playerShoot != null && playerShoot.IsShooting;
            if (movement.sqrMagnitude > 0.01f && !isShooting)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(movement),
                    rotationSmoothing);
            }

            Vector3 velocity = movement * speed;
            velocity.y = verticalVelocity;
            characterController.Move(velocity * Time.deltaTime);

            if (IsGrounded())
                TrySnapToGround();
        }

        void ApplyGravity()
        {
            if (IsGrounded())
                verticalVelocity = -groundStick;
            else
                verticalVelocity += gravity * Time.deltaTime;
        }

        bool IsGrounded()
        {
            if (characterController.isGrounded)
                return true;

            return TryGetGroundHit(out _);
        }

        bool TryGetGroundHit(out RaycastHit hit)
        {
            Vector3 worldCenter = transform.position + characterController.center;
            float castDistance = characterController.height * 0.5f + maxGroundSnap + 0.15f;

            if (Physics.SphereCast(
                    worldCenter + Vector3.up * 0.05f,
                    characterController.radius * 0.85f,
                    Vector3.down,
                    out hit,
                    castDistance,
                    groundLayers,
                    QueryTriggerInteraction.Ignore))
            {
                return true;
            }

            return Physics.Raycast(
                worldCenter + Vector3.up * 0.05f,
                Vector3.down,
                out hit,
                castDistance,
                groundLayers,
                QueryTriggerInteraction.Ignore);
        }

        void TrySnapToGround()
        {
            if (!TryGetGroundHit(out RaycastHit hit))
                return;

            float bottomY = transform.position.y + characterController.center.y - characterController.height * 0.5f;
            float gap = hit.point.y - bottomY;

            if (gap > 0f && gap <= maxGroundSnap)
                characterController.Move(Vector3.up * gap);
        }

        void SnapToGroundImmediate()
        {
            for (int i = 0; i < 8; i++)
            {
                if (!TryGetGroundHit(out RaycastHit hit))
                    break;

                float bottomY = transform.position.y + characterController.center.y - characterController.height * 0.5f;
                float gap = hit.point.y - bottomY;

                if (gap <= 0.01f)
                    break;

                characterController.Move(Vector3.up * gap);
            }

            verticalVelocity = -groundStick;
        }
    }
}
