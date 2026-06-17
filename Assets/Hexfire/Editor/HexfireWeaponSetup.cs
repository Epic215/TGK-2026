#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Hexfire.EditorTools
{
  public static class HexfireWeaponSetup
  {
    const string IconPath = "Assets/Hexfire/Weapons/Data/Icon_GreenFireOrb.png";

    [MenuItem("Hexfire/Generate Green Fire Weapon Icon")]
    public static void GenerateIcon()
    {
      const int size = 64;
      var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
      var center = new Vector2(size * 0.5f, size * 0.5f);
      float radius = size * 0.42f;

      for (int y = 0; y < size; y++)
      {
        for (int x = 0; x < size; x++)
        {
          float dist = Vector2.Distance(new Vector2(x, y), center);
          if (dist <= radius)
          {
            float edge = Mathf.Clamp01(1f - (dist - radius + 2f) / 2f);
            texture.SetPixel(x, y, new Color(0.2f, 0.95f, 0.35f, edge));
          }
          else
          {
            texture.SetPixel(x, y, Color.clear);
          }
        }
      }

      texture.Apply();

      byte[] png = texture.EncodeToPNG();
      Object.DestroyImmediate(texture);

      File.WriteAllBytes(IconPath, png);
      AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceUpdate);

      var importer = AssetImporter.GetAtPath(IconPath) as TextureImporter;
      if (importer != null)
      {
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 64;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
      }

      var weapon = AssetDatabase.LoadAssetAtPath<Hexfire.Weapons.ProjectileWeaponData>(
        "Assets/Hexfire/Weapons/Data/Weapon_GreenFireOrb.asset");
      if (weapon != null)
      {
        weapon.icon = AssetDatabase.LoadAssetAtPath<Sprite>(IconPath);
        EditorUtility.SetDirty(weapon);
        AssetDatabase.SaveAssets();
      }

      EditorUtility.DisplayDialog("Hexfire", "Ikona zapisana:\n" + IconPath, "OK");
    }
  }
}
#endif
