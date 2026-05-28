using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Invincibility Frames")]
    public float iFrameDuration = 0.7f;
    private float iFrameTimer = 0f;

    [Header("UI")]
    public TextMeshProUGUI healthText;   // np. "HP: 85"
    public TextMeshProUGUI shieldText;   // np. "SHIELD: READY"

    [Header("VFX")]
    public ParticleSystem onDamageParticles;

    [Header("Camera")]
    public CameraFollow cameraObject;

    // Shield placeholder — podpinasz tu później animator/logikę tarczy
    private bool shieldActive = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHUD();
    }

    void Update()
    {
        if (iFrameTimer > 0f)
            iFrameTimer -= Time.deltaTime;
    }

    public void TakeDamage(int amount)
    {
        if (iFrameTimer > 0f) return;
        if (shieldActive) return; // tarcza blokuje obrażenia

        currentHealth -= amount;
        iFrameTimer = iFrameDuration;

        if (cameraObject != null) cameraObject.shake();
        if (onDamageParticles != null)
            Instantiate(onDamageParticles, transform.position, Quaternion.identity);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        UpdateHUD();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHUD();
    }

    // Wywołaj z PlayerAbilities gdy tarcza jest aktywna
    public void SetShield(bool active)
    {
        shieldActive = active;
        UpdateHUD();
    }

    public bool IsShieldActive() => shieldActive;

    void Die()
    {
        Debug.Log("Player died!");
        Destroy(gameObject);
    }

    void UpdateHUD()
    {
        if (healthText != null)
            healthText.text = $"HP: {currentHealth} / {maxHealth}";

        if (shieldText != null)
            shieldText.text = shieldActive ? "[ SHIELD ACTIVE ]" : "SHIELD: READY";
    }
}