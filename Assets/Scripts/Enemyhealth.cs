using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;
    public Slider hpSlider;

    void Start()
    {
        currentHealth = maxHealth;
        if (hpSlider != null) hpSlider.maxValue = maxHealth;
        if (hpSlider != null) hpSlider.value = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (hpSlider != null) hpSlider.value = currentHealth;
        if (currentHealth <= 0) Die();
    }
    public int GetHealth() => currentHealth;

    public int GetMaxHealth() => maxHealth;

    void Die()
    {
        Destroy(gameObject);
    }
}