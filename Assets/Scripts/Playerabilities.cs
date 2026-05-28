using UnityEngine;
using System.Collections;
using TMPro;
 
// ============================================================
// Dołącz do gracza razem z PlayerController2 i PlayerHealth.
// Dash używa CharacterController, Shield to placeholder z UI.
// ============================================================
 
public class PlayerAbilities : MonoBehaviour
{
    [Header("Dash")]
    public float dashSpeed = 28f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.8f;
    private float dashCooldownTimer = 0f;
    private bool isDashing = false;
 
    [Header("Shield")]
    public float shieldDuration = 2f;
    public float shieldCooldown = 8f;
    private float shieldCooldownTimer = 0f;
 
    [Header("UI")]
    public TextMeshProUGUI dashCooldownText;    
    public TextMeshProUGUI shieldCooldownText; 
 
    // Placeholder wizualny tarczy — podepnij tutaj animator lub obiekt tarczy
    public GameObject shieldVisualPlaceholder;
 
    private CharacterController cc;
    private PlayerHealth playerHealth;
    private Vector3 dashDirection;
 
    // Dostęp z zewnątrz (np. PlayerController2 może sprawdzić czy trwa dash)
    public bool IsDashing => isDashing;
 
    void Start()
    {
        cc = GetComponent<CharacterController>();
        playerHealth = GetComponent<PlayerHealth>();
        if (shieldVisualPlaceholder != null) shieldVisualPlaceholder.SetActive(false);
    }
 
    void Update()
    {
        dashCooldownTimer -= Time.deltaTime;
        shieldCooldownTimer -= Time.deltaTime;
 
        // Dash — prawy klik myszy lub Shift
        if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.LeftShift))
            && dashCooldownTimer <= 0f && !isDashing)
        {
            TryDash();
        }
 
        // Shield — klawisz Q
        if (Input.GetKeyDown(KeyCode.Q) && shieldCooldownTimer <= 0f)
        {
            StartCoroutine(ActivateShield());
        }
 
        UpdateCooldownHUD();
    }
 
    // ─── Dash ─────────────────────────────────────────────────
 
    void TryDash()
    {
        Vector2 rawInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
 
        if (rawInput.sqrMagnitude < 0.01f) return;
 
        dashDirection = new Vector3(rawInput.x, 0f, rawInput.y).normalized;
        dashCooldownTimer = dashCooldown;
        StartCoroutine(DashCoroutine());
    }
 
    IEnumerator DashCoroutine()
    {
        isDashing = true;
        float elapsed = 0f;
 
        while (elapsed < dashDuration)
        {
            float step = dashSpeed * Time.deltaTime;
            cc.Move(dashDirection * step);
            elapsed += Time.deltaTime;
            yield return null;
        }
 
        isDashing = false;
    }
 
    // ─── Shield ───────────────────────────────────────────────
 
    IEnumerator ActivateShield()
    {
        shieldCooldownTimer = shieldCooldown;
        playerHealth?.SetShield(true);
 
        if (shieldVisualPlaceholder != null)
            shieldVisualPlaceholder.SetActive(true);
 
        yield return new WaitForSeconds(shieldDuration);
 
        playerHealth?.SetShield(false);
 
        if (shieldVisualPlaceholder != null)
            shieldVisualPlaceholder.SetActive(false);
    }
 
    // ─── HUD ──────────────────────────────────────────────────
 
    void UpdateCooldownHUD()
    {
        if (dashCooldownText != null)
        {
            dashCooldownText.text = dashCooldownTimer > 0f
                ? $"DASH: {dashCooldownTimer:F1}s"
                : "DASH: READY";
        }
 
        if (shieldCooldownText != null)
        {
            shieldCooldownText.text = shieldCooldownTimer > 0f
                ? $"SHIELD: {shieldCooldownTimer:F1}s"
                : "SHIELD: READY";
        }
    }
}