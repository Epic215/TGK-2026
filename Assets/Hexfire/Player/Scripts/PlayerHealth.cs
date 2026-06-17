using UnityEngine;
using TMPro;
using Hexfire.UI;

namespace Hexfire
{
  public class PlayerHealth : MonoBehaviour
  {
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Invincibility Frames")]
    public float iFrameDuration = 0.7f;
    float iFrameTimer;

    [Header("UI")]
    public StatBarView healthBar;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI shieldText;

    [Header("VFX")]
    public ParticleSystem onDamageParticles;

    [Header("Camera")]
    public CameraFollow cameraObject;

    bool shieldActive;

    public bool IsAtFullHealth => currentHealth >= maxHealth;

    void Start()
    {
      currentHealth = maxHealth;
      UpdateHud();
    }

    void Update()
    {
      if (iFrameTimer > 0f)
        iFrameTimer -= Time.deltaTime;
    }

    public void GrantIFrames(float duration)
    {
      if (duration > 0f)
        iFrameTimer = Mathf.Max(iFrameTimer, duration);
    }

    public void TakeDamage(int amount)
    {
      if (iFrameTimer > 0f)
        return;

      if (shieldActive)
        return;

      currentHealth -= amount;
      iFrameTimer = iFrameDuration;

      if (cameraObject != null)
        cameraObject.shake();

      if (onDamageParticles != null)
        Instantiate(onDamageParticles, transform.position, Quaternion.identity);

      if (currentHealth <= 0)
      {
        currentHealth = 0;
        Die();
      }

      UpdateHud();
    }

    public void Heal(int amount)
    {
      if (amount <= 0)
        return;

      currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
      UpdateHud();
    }

    public void SetShield(bool active)
    {
      shieldActive = active;
      UpdateHud();
    }

    public bool IsShieldActive() => shieldActive;

    void Die()
    {
      Debug.Log("Player died!");
      Destroy(gameObject);
    }

    void UpdateHud()
    {
      if (healthBar != null)
        healthBar.SetValues(currentHealth, maxHealth);
      else if (healthText != null)
        healthText.text = $"HP: {currentHealth} / {maxHealth}";

      if (shieldText != null)
        shieldText.text = shieldActive ? "[ SHIELD ACTIVE ]" : "SHIELD: READY";
    }
  }
}
