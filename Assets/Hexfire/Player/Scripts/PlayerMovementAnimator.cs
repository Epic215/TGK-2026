using UnityEngine;
using UnityEngine.InputSystem;

namespace Hexfire
{
    [RequireComponent(typeof(Animator))]
    [DefaultExecutionOrder(100)]
    public class PlayerMovementAnimator : MonoBehaviour
    {
        static readonly int SpeedHash = Animator.StringToHash("Speed");
        static readonly int ShootHash = Animator.StringToHash("Shoot");
        static readonly int ShieldHash = Animator.StringToHash("Shield");

        Animator animator;
        PlayerInput playerInput;
        InputAction moveAction;
        PlayerDashAbility dashAbility;
        PlayerEquipment playerEquipment;
        bool hasShieldParam;

        void Awake()
        {
            animator = GetComponent<Animator>();
            dashAbility = GetComponent<PlayerDashAbility>();
            playerEquipment = GetComponent<PlayerEquipment>();
        }

        void Start()
        {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
                moveAction = playerInput.actions["Move"];

            if (animator == null || animator.runtimeAnimatorController == null)
            {
                Debug.LogWarning("PlayerMovementAnimator: brak Animator Controller.", this);
                return;
            }

            bool hasSpeedParam = false;
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.nameHash == SpeedHash)
                    hasSpeedParam = true;
                if (param.nameHash == ShieldHash)
                    hasShieldParam = true;
            }

            if (!hasSpeedParam)
            {
                Debug.LogWarning(
                    "PlayerMovementAnimator: controller nie ma parametru 'Speed'. " +
                    "Przypisz MageLocomotion zamiast WizardAnimControl.",
                    this);
            }
        }

        void Update()
        {
            if (animator == null)
                return;

            float speed = 0f;

            if (dashAbility != null && dashAbility.IsDashing)
            {
                speed = 1f;
            }
            else if (moveAction != null)
            {
                Vector2 input = moveAction.ReadValue<Vector2>();
                speed = Mathf.Clamp01(input.magnitude);
            }

            animator.SetFloat(SpeedHash, speed);

            if (playerEquipment != null && playerEquipment.ConsumeShootAnimationRequest())
                animator.SetTrigger(ShootHash);

            if (hasShieldParam && playerEquipment != null && playerEquipment.ConsumeShieldAnimationRequest())
                animator.SetTrigger(ShieldHash);
        }
    }
}
