#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Hexfire.Weapons;

namespace Hexfire.EditorTools
{
  public static class BuildAllStarterWeapons
  {
    const string DataFolder = "Assets/Hexfire/Weapons/Data";
    const string PrefabFolder = "Assets/Hexfire/Weapons/Prefabs";
    const string IconFolder = "Assets/Hexfire/Weapons/Data";
    const string GreenOrbDataPath = DataFolder + "/Weapon_GreenFireOrb.asset";
    const string SwordPrefabPath = "Assets/Hovl Studio/Magic sword/Prefabs/MagicSword_Iron.prefab";
    static readonly string[] StaffPrefabPaths =
    {
      "Assets/WizardPBR/Prefabs/Staff01.prefab",
      "Assets/WizardPBR/Prefabs/Staff02.prefab",
      "Assets/WizardPBR/Prefabs/Staff03.prefab"
    };

    [MenuItem("Hexfire/Build All 5 Starter Weapons (pickupy + ikony)")]
    public static void Build()
    {
      EnsureFolders();

      Sprite[] icons = GenerateNumberedIcons(5);

      ConfigureGreenOrb(icons[0]);
      CreateStaffWeapon(1, icons[1], StaffPrefabPaths[0]);
      CreateStaffWeapon(2, icons[2], StaffPrefabPaths[1]);
      CreateStaffWeapon(3, icons[3], StaffPrefabPaths[2]);
      CreateSwordWeapon(icons[4]);

      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();

      EditorUtility.DisplayDialog(
        "Hexfire",
        "5 broni gotowych w:\n" + DataFolder + "\n" + PrefabFolder +
        "\n\nWrzuc pickupy na mape. Podnies: E.\nNie nadpisuje juz ustawien StaffWeapon (tylko ikony/pickupy).",
        "OK");
    }

    static void EnsureFolders()
    {
      if (!AssetDatabase.IsValidFolder("Assets/Hexfire/Weapons"))
        return;
      if (!AssetDatabase.IsValidFolder(DataFolder))
        AssetDatabase.CreateFolder("Assets/Hexfire/Weapons", "Data");
      if (!AssetDatabase.IsValidFolder(PrefabFolder))
        AssetDatabase.CreateFolder("Assets/Hexfire/Weapons", "Prefabs");
    }

    static Sprite[] GenerateNumberedIcons(int count)
    {
      var icons = new Sprite[count];
      for (int i = 0; i < count; i++)
      {
        int number = i + 1;
        string path = IconFolder + "/Icon_Weapon_" + number + ".png";
        icons[i] = GenerateNumberIcon(path, number);
      }

      return icons;
    }

    static Sprite GenerateNumberIcon(string path, int number)
    {
      const int size = 64;
      var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

      Color bg = new Color(0.12f, 0.12f, 0.16f, 0.95f);
      Color fg = Color.white;

      for (int y = 0; y < size; y++)
      {
        for (int x = 0; x < size; x++)
          texture.SetPixel(x, y, bg);
      }

      DrawDigit(texture, number, fg);
      texture.Apply();

      File.WriteAllBytes(path, texture.EncodeToPNG());
      Object.DestroyImmediate(texture);

      AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
      var importer = AssetImporter.GetAtPath(path) as TextureImporter;
      if (importer != null)
      {
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 64;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
      }

      return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void DrawDigit(Texture2D texture, int number, Color color)
    {
      bool[,] pattern = number switch
      {
        1 => Digit1(),
        2 => Digit2(),
        3 => Digit3(),
        4 => Digit4(),
        5 => Digit5(),
        _ => Digit1()
      };

      int offsetX = 20;
      int offsetY = 12;
      for (int y = 0; y < pattern.GetLength(0); y++)
      {
        for (int x = 0; x < pattern.GetLength(1); x++)
        {
          if (!pattern[y, x])
            continue;

          for (int py = 0; py < 4; py++)
          {
            for (int px = 0; px < 4; px++)
              texture.SetPixel(offsetX + x * 4 + px, offsetY + y * 4 + py, color);
          }
        }
      }
    }

    static bool[,] Digit1() => new bool[,]
    {
      { false, true, false },
      { true, true, false },
      { false, true, false },
      { false, true, false },
      { true, true, true }
    };

    static bool[,] Digit2() => new bool[,]
    {
      { true, true, true },
      { false, false, true },
      { true, true, true },
      { true, false, false },
      { true, true, true }
    };

    static bool[,] Digit3() => new bool[,]
    {
      { true, true, true },
      { false, false, true },
      { true, true, true },
      { false, false, true },
      { true, true, true }
    };

    static bool[,] Digit4() => new bool[,]
    {
      { true, false, true },
      { true, false, true },
      { true, true, true },
      { false, false, true },
      { false, false, true }
    };

    static bool[,] Digit5() => new bool[,]
    {
      { true, true, true },
      { true, false, false },
      { true, true, true },
      { false, false, true },
      { true, true, true }
    };

    static void ConfigureGreenOrb(Sprite icon)
    {
      var weapon = AssetDatabase.LoadAssetAtPath<GreenFireOrbWeaponData>(GreenOrbDataPath);
      if (weapon == null)
        return;

      weapon.weaponName = "1. Zielona Kula";
      weapon.showStaffIndex = 0;
      weapon.icon = icon;
      if (weapon.healCastVfxPrefab == null)
      {
        weapon.healCastVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
          "Assets/Hovl Studio/Magic effects pack/Prefabs/Character auras/Healing.prefab");
      }

      EditorUtility.SetDirty(weapon);
    }

    static void CreateStaffWeapon(int index, Sprite icon, string staffPrefabPath)
    {
      string dataPath = DataFolder + "/Weapon_Staff0" + index + ".asset";
      var weapon = AssetDatabase.LoadAssetAtPath<StaffWeaponData>(dataPath);
      if (weapon == null)
      {
        weapon = GetOrCreate<StaffWeaponData>(dataPath);
        weapon.showStaffIndex = index;
        weapon.heldVisualPrefab = null;
        ApplyDefaultStaffConfig(weapon, index);
      }

      if (weapon.icon == null)
        weapon.icon = icon;

      EditorUtility.SetDirty(weapon);

      string pickupPath = PrefabFolder + "/Staff0" + index + "Pickup.prefab";
      CreateStaffPickup(pickupPath, weapon, staffPrefabPath);
    }

    static void ApplyDefaultStaffConfig(StaffWeaponData weapon, int index)
    {
      GameObject projectile = AssetDatabase.LoadAssetAtPath<GameObject>(
        PrefabFolder + "/Projectile_Yellow.prefab");

      if (projectile != null)
      {
        weapon.projectilePrefab = projectile;
        weapon.visualEffectPrefab = null;
      }

      weapon.damage = 10f;
      weapon.projectileSpeed = 3.5f;
      weapon.projectileLifetime = 4f;
      weapon.fireRate = 0.5f;
      weapon.spreadAngles = new[] { -15f, 0f, 15f };
      weapon.chaosProjectileCount = 5;
      weapon.chaosAngleMin = -25f;
      weapon.chaosAngleMax = 25f;
      weapon.chaosShotDelay = 0.12f;
      weapon.chaosSeriesCooldown = 0.4f;

      switch (index)
      {
        case 1:
          weapon.weaponName = "2. Kostur Rubinowy";
          weapon.description = "LPM rozproszenie 3 kul. PPM odnowa many.";
          weapon.firePattern = WeaponFirePattern.Spread;
          weapon.usesMana = true;
          weapon.manaCost = 15f;
          weapon.projectileSpeed = 4f;
          weapon.abilityType = StaffAbilityType.ManaRestore;
        weapon.abilityManaRestore = 20f;
        weapon.abilityCooldown = 4f;
        weapon.abilityCastVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
          "Assets/Hovl Studio/Magic effects pack/Prefabs/Character auras/Buff.prefab");
        break;

        case 2:
          weapon.weaponName = "3. Kostur Szafirowy";
          weapon.description = "LPM chaos. PPM shotgun chaos.";
          weapon.firePattern = WeaponFirePattern.Chaos;
          weapon.usesMana = false;
          weapon.projectileSpeed = 2.5f;
          weapon.abilityType = StaffAbilityType.ShotgunChaos;
          weapon.abilityManaCost = 100f;
          weapon.abilityCooldown = 2.5f;
          weapon.abilityShotgunCount = 18;
          weapon.abilityProjectileSpeed = 7f;
          break;

        default:
          weapon.weaponName = (index + 1) + ". Kostur";
          weapon.firePattern = WeaponFirePattern.Single;
          break;
      }
    }

    static void CreateSwordWeapon(Sprite icon)
    {
      const string dataPath = DataFolder + "/Weapon_IceSword.asset";
      const string heldPath = PrefabFolder + "/HeldVisual_MagicSword.prefab";
      const string pickupPath = PrefabFolder + "/MagicSword_Ice.prefab";

      GameObject heldVisual = EnsureHeldSwordVisual(heldPath);

      var weapon = AssetDatabase.LoadAssetAtPath<MeleeSwordWeaponData>(dataPath);
      if (weapon == null)
      {
        weapon = GetOrCreate<MeleeSwordWeaponData>(dataPath);
        weapon.weaponName = "5. Magiczny Miecz";
        weapon.description = "LPM ciecie w zasiegu. PPM magic shield.";
        weapon.damage = 14f;
        weapon.usesMana = false;
        weapon.swingInterval = 0.5f;
        weapon.meleeRange = 2.4f;
        weapon.shieldManaCost = 50f;
        weapon.shieldDuration = 2.5f;
        weapon.shieldCooldown = 2.5f;
        weapon.meleeRange = 4.2f;
        weapon.meleeHalfAngle = 72f;
        weapon.showStaffIndex = 0;
        weapon.heldVisualPrefab = heldVisual;
        weapon.useStaffPose = true;
        weapon.heldLocalPosition = new Vector3(1.028f, 0f, -1.035f);
        weapon.heldLocalEulerAngles = new Vector3(90f, 90f, 90f);
        weapon.heldLocalScale = new Vector3(100f, 100f, 100f);
        weapon.slashVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
          "Assets/Hovl Studio/Magic effects pack/Prefabs/Slash effects/Charge slash blue.prefab");
        weapon.shieldVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
          "Assets/Hovl Studio/Magic effects pack/Prefabs/Magic shields/Magic shield blue.prefab");
      }

      if (weapon.icon == null)
        weapon.icon = icon;

      EditorUtility.SetDirty(weapon);

      string pickupSavePath = PrefabFolder + "/MagicSword_Ice.prefab";
      if (!System.IO.File.Exists(pickupSavePath))
        CreateSwordPickup(pickupSavePath, weapon);
    }

    static GameObject EnsureHeldSwordVisual(string savePath)
    {
      var existing = AssetDatabase.LoadAssetAtPath<GameObject>(savePath);
      if (existing != null)
        return existing;

      GameObject swordPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SwordPrefabPath);
      if (swordPrefab == null)
        return null;

      var root = new GameObject("HeldVisual_MagicSword");
      root.AddComponent<WeaponVisual>();

      GameObject sword = (GameObject)PrefabUtility.InstantiatePrefab(swordPrefab, root.transform);
      sword.transform.localPosition = Vector3.zero;
      sword.transform.localRotation = Quaternion.identity;
      sword.transform.localScale = Vector3.one * 0.12f;

      PrefabUtility.SaveAsPrefabAsset(root, savePath);
      Object.DestroyImmediate(root);
      return AssetDatabase.LoadAssetAtPath<GameObject>(savePath);
    }

    static void CreateStaffPickup(string savePath, WeaponData weapon, string staffPrefabPath)
    {
      GameObject staffPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(staffPrefabPath);
      if (staffPrefab == null)
        return;

      var root = new GameObject(Path.GetFileNameWithoutExtension(savePath));
      var rb = root.AddComponent<Rigidbody>();
      rb.useGravity = true;

      var box = root.AddComponent<BoxCollider>();
      box.size = new Vector3(0.6f, 0.3f, 0.6f);
      box.center = new Vector3(0f, 0.15f, 0f);

      GameObject staff = (GameObject)PrefabUtility.InstantiatePrefab(staffPrefab, root.transform);
      staff.transform.localPosition = new Vector3(0f, 0.15f, 0f);
      staff.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
      staff.transform.localScale = Vector3.one * 80f;

      var item = root.AddComponent<HexfireWeaponItem>();
      item.weaponData = weapon;
      item.pickupRadius = 2.5f;

      SavePickupPrefab(root, savePath);
    }

    static void CreateSwordPickup(string savePath, WeaponData weapon)
    {
      GameObject swordPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SwordPrefabPath);
      if (swordPrefab == null)
        return;

      var root = new GameObject("MagicSwordPickup");
      var rb = root.AddComponent<Rigidbody>();
      rb.useGravity = true;

      var box = root.AddComponent<BoxCollider>();
      box.size = new Vector3(0.5f, 0.15f, 1.2f);
      box.center = new Vector3(0f, 0.08f, 0f);

      GameObject sword = (GameObject)PrefabUtility.InstantiatePrefab(swordPrefab, root.transform);
      sword.transform.localPosition = new Vector3(0f, 0.1f, 0f);
      sword.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
      sword.transform.localScale = Vector3.one * 0.35f;

      var item = root.AddComponent<HexfireWeaponItem>();
      item.weaponData = weapon;
      item.pickupRadius = 2.5f;

      SavePickupPrefab(root, savePath);
    }

    static void SavePickupPrefab(GameObject root, string savePath)
    {
      var existing = AssetDatabase.LoadAssetAtPath<GameObject>(savePath);
      if (existing != null)
        PrefabUtility.SaveAsPrefabAssetAndConnect(root, savePath, InteractionMode.AutomatedAction);
      else
        PrefabUtility.SaveAsPrefabAsset(root, savePath);

      Object.DestroyImmediate(root);
    }

    static T GetOrCreate<T>(string path) where T : ScriptableObject
    {
      T asset = AssetDatabase.LoadAssetAtPath<T>(path);
      if (asset != null)
        return asset;

      asset = ScriptableObject.CreateInstance<T>();
      AssetDatabase.CreateAsset(asset, path);
      return asset;
    }
  }
}
#endif
