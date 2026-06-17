using UnityEngine;
using TMPro;
using Hexfire.UI;

namespace Hexfire
{
  public class PlayerMana : MonoBehaviour
  {
    [Header("Mana")]
    public float maxMana = 100f;
    public float currentMana;
    public float manaRegenPerSecond = 5f;

    [Header("UI")]
    public StatBarView manaBar;
    public TextMeshProUGUI manaText;

    public bool HasEnough(float amount) => amount <= 0f || currentMana >= amount;

    void Start()
    {
      currentMana = maxMana;
      UpdateHud();
    }

    void Update()
    {
      if (currentMana >= maxMana)
        return;

      currentMana = Mathf.Min(maxMana, currentMana + manaRegenPerSecond * Time.deltaTime);
      UpdateHud();
    }

    public bool TrySpend(float amount)
    {
      if (amount <= 0f)
        return true;

      if (currentMana < amount)
        return false;

      currentMana -= amount;
      UpdateHud();
      return true;
    }

    public void Restore(float amount)
    {
      if (amount <= 0f)
        return;

      currentMana = Mathf.Min(maxMana, currentMana + amount);
      UpdateHud();
    }

    void UpdateHud()
    {
      if (manaBar != null)
        manaBar.SetValues(currentMana, maxMana);
      else if (manaText != null)
        manaText.text = $"MANA: {Mathf.CeilToInt(currentMana)} / {Mathf.CeilToInt(maxMana)}";
    }
  }
}
