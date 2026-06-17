using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hexfire.UI
{
  public class StatBarView : MonoBehaviour
  {
    public Slider slider;
    public TextMeshProUGUI valueText;
    public string textFormat = "{0} / {1}";

    public void SetValues(float current, float max)
    {
      if (slider != null)
      {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        slider.interactable = false;
      }

      if (valueText == null)
        return;

      valueText.text = string.Format(
        textFormat,
        Mathf.CeilToInt(current),
        Mathf.CeilToInt(max));
    }
  }
}
