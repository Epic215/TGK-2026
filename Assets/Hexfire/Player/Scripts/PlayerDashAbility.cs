using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Hexfire.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Hexfire
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerDashAbility : MonoBehaviour
    {
        const string DefaultVfxAssetPath = "Assets/Hexfire/Player/Prefabs/VFX_Trail_Dark.prefab";

        [Header("Dash")]
        public float dashDistance = 5f;
        public float dashDuration = 0.15f;
        public float dashCooldown = 1f;

        [Header("VFX (optional)")]
        [Tooltip("Przeciągnij VFX_Trail_Dark z Assets/Hexfire/Player/Prefabs/")]
        public GameObject dashVfxPrefab;
        public Vector3 vfxLocalOffset = new Vector3(0f, 1f, 0f);
        [Tooltip("Skala całego efektu (transform + szerokość traila).")]
        public float vfxScale = 2.5f;
        [Tooltip("Jak długo trail zostaje widoczny po zakończeniu emisji.")]
        public float vfxTrailDuration = 3.5f;
        [Tooltip("Mnożnik szerokości TrailRenderer względem prefaba.")]
        public float vfxTrailWidth = 2f;
        [Tooltip("Po ilu sekundach od końca dasha usuń obiekt VFX.")]
        public float vfxLifetime = 4f;
        public bool parentVfxToPlayer = true;

        [Header("UI (optional)")]
        public CooldownRingView dashCooldownRing;

        public bool IsDashing { get; private set; }
        public float CooldownReady01 =>
          dashCooldown <= 0f ? 1f : Mathf.Clamp01(1f - (cooldownTimer / dashCooldown));

        CharacterController characterController;
        PlayerInput playerInput;
        InputAction dashAction;
        InputAction moveAction;
        PlayerIFrameBridge iFrameBridge;
        float cooldownTimer;
        bool warnedMissingVfx;

        void Awake()
        {
            characterController = GetComponent<CharacterController>();
            iFrameBridge = GetComponent<PlayerIFrameBridge>();
            EnsureDashVfxPrefab();
        }

        void Start()
        {
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                Debug.LogWarning("PlayerDashAbility: brak PlayerInput na obiekcie.", this);
                return;
            }

            dashAction = playerInput.actions["Dash"];
            moveAction = playerInput.actions["Move"];
            dashCooldownRing?.SetReadyAmount(CooldownReady01);
        }

        void Update()
        {
            cooldownTimer -= Time.deltaTime;
            dashCooldownRing?.SetReadyAmount(CooldownReady01);

            if (dashAction == null)
                return;

            if (!dashAction.WasPressedThisFrame() || cooldownTimer > 0f || IsDashing)
                return;

            Vector2 input = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            Vector3 direction = input != Vector2.zero
                ? new Vector3(input.x, 0f, input.y).normalized
                : transform.forward;

            StartCoroutine(DashRoutine(direction));
            cooldownTimer = dashCooldown;
        }

        IEnumerator DashRoutine(Vector3 direction)
        {
            IsDashing = true;
            iFrameBridge?.GrantIFrames(dashDuration);

            GameObject vfxInstance = SpawnDashVfx();

            float elapsed = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            while (elapsed < dashDuration)
            {
                float step = (dashDistance / dashDuration) * Time.deltaTime;
                characterController.Move(direction * step);

                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    720f * Time.deltaTime);

                elapsed += Time.deltaTime;
                yield return null;
            }

            IsDashing = false;

            if (vfxInstance != null)
            {
                StopTrailEmission(vfxInstance);
                float destroyDelay = Mathf.Max(vfxLifetime, vfxTrailDuration + 0.5f);
                if (destroyDelay > 0f)
                    Destroy(vfxInstance, destroyDelay);
            }
        }

        void EnsureDashVfxPrefab()
        {
            if (dashVfxPrefab != null)
                return;

#if UNITY_EDITOR
            dashVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultVfxAssetPath);
            if (dashVfxPrefab == null)
            {
                dashVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/Vefects/Trails VFX URP/VFX/Particles/VFX_Trail_Dark.prefab");
            }
#endif
        }

        GameObject SpawnDashVfx()
        {
            if (dashVfxPrefab == null)
            {
                if (!warnedMissingVfx)
                {
                    Debug.LogWarning(
                        "PlayerDashAbility: Dash VFX Prefab jest pusty. " +
                        "Przypisz VFX_Trail_Dark w Inspectorze (Player Dash Ability → Dash Vfx Prefab).",
                        this);
                    warnedMissingVfx = true;
                }
                return null;
            }

            Vector3 spawnPos = transform.position + transform.TransformDirection(vfxLocalOffset);
            GameObject instance = Instantiate(dashVfxPrefab, spawnPos, transform.rotation);

            if (parentVfxToPlayer)
                instance.transform.SetParent(transform, true);

            ApplyVfxSettings(instance);
            return instance;
        }

        void ApplyVfxSettings(GameObject root)
        {
            root.transform.localScale = Vector3.one * vfxScale;

            TrailRenderer[] trails = root.GetComponentsInChildren<TrailRenderer>(true);
            foreach (TrailRenderer trail in trails)
            {
                trail.time = vfxTrailDuration;
                trail.widthMultiplier *= vfxTrailWidth;
                trail.Clear();
                trail.emitting = true;
            }

            ParticleSystem[] systems = root.GetComponentsInChildren<ParticleSystem>(true);
            foreach (ParticleSystem ps in systems)
            {
                var main = ps.main;
                main.startSizeMultiplier *= vfxScale;
                ps.Clear(true);
                ps.Play(true);
            }

            AudioSource[] audioSources = root.GetComponentsInChildren<AudioSource>(true);
            foreach (AudioSource source in audioSources)
            {
                if (!source.isPlaying)
                    source.Play();
            }
        }

        static void StopTrailEmission(GameObject root)
        {
            TrailRenderer[] trails = root.GetComponentsInChildren<TrailRenderer>(true);
            foreach (TrailRenderer trail in trails)
                trail.emitting = false;
        }
    }
}
