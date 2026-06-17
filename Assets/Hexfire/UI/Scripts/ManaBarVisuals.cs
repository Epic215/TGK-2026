using UnityEngine;
using UnityEngine.UI;

namespace Hexfire.UI
{
  public static class ManaBarVisuals
  {
    static Sprite whiteFillSprite;

    public static readonly Color FillBlue = new Color(0.15f, 0.55f, 1f, 1f);

    public static Sprite GetUiWhiteSprite() => GetWhiteFillSprite();

    public static void ApplyBlueFill(Transform barRoot)
    {
      if (barRoot == null)
        return;

      Image fill = barRoot.Find("Fill Area/Fill")?.GetComponent<Image>();
      if (fill == null)
        return;

      fill.sprite = GetWhiteFillSprite();
      fill.type = Image.Type.Simple;
      fill.color = FillBlue;
    }

    public static void ApplyPanelColor(Image image, Color color)
    {
      if (image == null)
        return;

      image.sprite = GetWhiteFillSprite();
      image.type = Image.Type.Simple;
      image.color = color;
    }

    static Sprite GetWhiteFillSprite()
    {
      if (whiteFillSprite != null)
        return whiteFillSprite;

      var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
      texture.SetPixel(0, 0, Color.white);
      texture.Apply();

      whiteFillSprite = Sprite.Create(
        texture,
        new Rect(0f, 0f, 1f, 1f),
        new Vector2(0.5f, 0.5f),
        100f);

      return whiteFillSprite;
    }
  }
}
