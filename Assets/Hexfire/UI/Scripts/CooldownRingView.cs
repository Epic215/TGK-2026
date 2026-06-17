using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hexfire.UI
{
  public class CooldownRingView : MonoBehaviour
  {
    [Range(0f, 1f)]
    public float backgroundAlpha = 0.45f;

    public Slider slider;
    public Image backgroundImage;
    public TextMeshProUGUI percentText;

    public void SetReadyAmount(float ready01)
    {
      if (slider == null)
        return;

      float clamped = Mathf.Clamp01(ready01);
      slider.minValue = 0f;
      slider.maxValue = 1f;
      slider.value = clamped;
      slider.interactable = false;

      if (percentText != null)
        percentText.text = $"{Mathf.RoundToInt(clamped * 100f)}%";
    }

    void Awake()
    {
      if (slider == null)
        slider = GetComponent<Slider>();

      if (backgroundImage == null)
        backgroundImage = FindBackgroundImage(transform);

      if (percentText == null)
      {
        Transform textTransform = transform.Find("Text");
        if (textTransform != null)
          percentText = textTransform.GetComponent<TextMeshProUGUI>();
      }

      ApplyBackgroundAlpha();
      SetReadyAmount(slider != null ? slider.value : 1f);
    }

    void OnValidate()
    {
      ApplyBackgroundAlpha();
    }

    static Image FindBackgroundImage(Transform root)
    {
      Transform nested = root.Find("Round Image/Background");
      if (nested != null)
        return nested.GetComponent<Image>();

      Transform direct = root.Find("Background");
      if (direct != null)
        return direct.GetComponent<Image>();

      Image[] images = root.GetComponentsInChildren<Image>(true);
      foreach (Image image in images)
      {
        if (image.gameObject.name == "Background")
          return image;
      }

      return null;
    }

    void ApplyBackgroundAlpha()
    {
      if (backgroundImage == null)
        return;

      Color color = backgroundImage.color;
      color.a = backgroundAlpha;
      backgroundImage.color = color;
    }
  }
}
