using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Hexfire.Weapons
{
  /// <summary>
  /// Bron na mapie — podnoszenie przez gracza (E, najblizsza w zasiegu).
  /// </summary>
  public class HexfireWeaponItem : MonoBehaviour
  {
    static readonly List<HexfireWeaponItem> ActiveItems = new List<HexfireWeaponItem>(32);

    [Header("Bron")]
    public WeaponData weaponData;

    [Header("Podnoszenie")]
    public float pickupRadius = 2.5f;

    [Header("Podswietlenie (opcjonalne)")]
    public TextMeshProUGUI pickupHintText;
    public Renderer[] highlightRenderers;

    Color[] defaultColors;

    public static int ActiveCount => ActiveItems.Count;

    public static void GetAllActive(HexfireWeaponItem[] buffer, out int count)
    {
      count = ActiveItems.Count;
      for (int i = 0; i < count; i++)
        buffer[i] = ActiveItems[i];
    }

    void OnEnable()
    {
      if (!ActiveItems.Contains(this))
        ActiveItems.Add(this);

      CacheDefaultColors();
      SetHighlighted(false);
    }

    void OnDisable()
    {
      ActiveItems.Remove(this);
      SetHighlighted(false);
    }

    public float DistanceTo(Vector3 position)
    {
      return Vector3.Distance(transform.position, position);
    }

    public bool IsInRange(Vector3 position)
    {
      return DistanceTo(position) <= pickupRadius;
    }

    public void SetHighlighted(bool on)
    {
      if (pickupHintText != null)
      {
        pickupHintText.enabled = on && weaponData != null;
        if (on && weaponData != null)
          pickupHintText.text = $"[E] {weaponData.weaponName}";
      }

      if (highlightRenderers == null || defaultColors == null)
        return;

      for (int i = 0; i < highlightRenderers.Length; i++)
      {
        Renderer renderer = highlightRenderers[i];
        if (renderer == null)
          continue;

        renderer.material.color = on
          ? Color.Lerp(defaultColors[i], Color.yellow, 0.45f)
          : defaultColors[i];
      }
    }

    void CacheDefaultColors()
    {
      if (highlightRenderers == null || highlightRenderers.Length == 0)
        return;

      defaultColors = new Color[highlightRenderers.Length];
      for (int i = 0; i < highlightRenderers.Length; i++)
      {
        Renderer renderer = highlightRenderers[i];
        defaultColors[i] = renderer != null ? renderer.material.color : Color.white;
      }
    }

    void OnDrawGizmosSelected()
    {
      Gizmos.color = Color.green;
      Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
  }
}
