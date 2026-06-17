#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Hexfire.Weapons;

namespace Hexfire.EditorTools
{
  public static class HexfireWeaponPrefabBuilder
  {
    const string WeaponDataPath = "Assets/Hexfire/Weapons/Data/Weapon_GreenFireOrb.asset";
    const string VisualPrefabPath = "Assets/Hexfire/Weapons/Prefabs/WeaponVisual_GreenOrb.prefab";
    const string PickupPrefabPath = "Assets/Hexfire/Weapons/Prefabs/WeaponPickup_GreenOrb.prefab";
    const string LegacyVisualPath = "Assets/Hexfire/Weapons/Prefabs/HeldVisual_GreenOrb.prefab";
    const string FireVfxPath = "Assets/Hovl Studio/Procedural fire/Prefabs/Magic fire pro green.prefab";

    [MenuItem("Hexfire/Build Green Orb Weapon Prefabs (visual + pickup)")]
    public static void BuildGreenOrbPrefabs()
    {
      ProjectileWeaponData weaponData = AssetDatabase.LoadAssetAtPath<ProjectileWeaponData>(WeaponDataPath);
      if (weaponData == null)
      {
        EditorUtility.DisplayDialog("Hexfire", "Brak: " + WeaponDataPath, "OK");
        return;
      }

      GameObject visualRoot = CreateVisualRoot(weaponData);
      GameObject visualPrefab = SavePrefab(visualRoot, VisualPrefabPath);

      weaponData.heldVisualPrefab = visualPrefab;
      EditorUtility.SetDirty(weaponData);

      GameObject pickupRoot = CreatePickupRoot(weaponData, visualPrefab);
      SavePrefab(pickupRoot, PickupPrefabPath);

      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();

      EditorUtility.DisplayDialog(
        "Hexfire",
        "Gotowe:\n" + VisualPrefabPath + "\n" + PickupPrefabPath + "\n\nWeaponData tez zaktializowany.",
        "OK");
    }

    static GameObject CreateVisualRoot(WeaponData weaponData)
    {
      var existing = AssetDatabase.LoadAssetAtPath<GameObject>(LegacyVisualPath);
      if (existing != null)
      {
        var copy = Object.Instantiate(existing);
        copy.name = "WeaponVisual_GreenOrb";
        EnsureWeaponVisual(copy, weaponData);
        return copy;
      }

      var root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      root.name = "WeaponVisual_GreenOrb";
      root.transform.localScale = Vector3.one * 0.45f;
      Object.DestroyImmediate(root.GetComponent<Collider>());

      var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Hexfire/Weapons/Materials/Mat_HeldGreenOrb.mat");
      if (mat != null)
        root.GetComponent<Renderer>().sharedMaterial = mat;

      AddFireChild(root.transform);

      EnsureWeaponVisual(root, weaponData);
      return root;
    }

    static GameObject CreatePickupRoot(WeaponData weaponData, GameObject visualPrefab)
    {
      var root = new GameObject("WeaponPickup_GreenOrb");
      root.transform.localScale = Vector3.one * 0.6f;

      var visual = (GameObject)PrefabUtility.InstantiatePrefab(visualPrefab);
      visual.transform.SetParent(root.transform, false);
      visual.transform.localPosition = Vector3.zero;

      var pickupCollider = root.AddComponent<SphereCollider>();
      pickupCollider.isTrigger = true;
      pickupCollider.radius = 0.8f;

      var pickup = root.AddComponent<WeaponPickup>();
      pickup.weaponData = weaponData;

      return root;
    }

    static void EnsureWeaponVisual(GameObject root, WeaponData weaponData)
    {
      var visual = root.GetComponent<WeaponVisual>();
      if (visual == null)
        visual = root.AddComponent<WeaponVisual>();
      visual.weaponData = weaponData;
    }

    static void AddFireChild(Transform parent)
    {
      var firePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FireVfxPath);
      if (firePrefab == null)
        return;

      var fire = (GameObject)PrefabUtility.InstantiatePrefab(firePrefab, parent);
      fire.transform.localPosition = Vector3.zero;
      fire.transform.localRotation = Quaternion.identity;
      fire.transform.localScale = Vector3.one * 0.18f;
    }

    static GameObject SavePrefab(GameObject instance, string path)
    {
      EnsureFolderExists(path);
      var prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
      Object.DestroyImmediate(instance);
      return prefab;
    }

    static void EnsureFolderExists(string assetPath)
    {
      string dir = System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
      if (!string.IsNullOrEmpty(dir) && !AssetDatabase.IsValidFolder(dir))
      {
        string parent = System.IO.Path.GetDirectoryName(dir)?.Replace('\\', '/');
        string folderName = System.IO.Path.GetFileName(dir);
        if (!string.IsNullOrEmpty(parent))
          AssetDatabase.CreateFolder(parent, folderName);
      }
    }
  }
}
#endif
