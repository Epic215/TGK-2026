#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Hexfire.Weapons;

namespace Hexfire.EditorTools
{
  public static class CreateGreenOrbPickup
  {
    const string WeaponDataPath = "Assets/Hexfire/Weapons/Data/Weapon_GreenFireOrb.asset";
    const string SavePath = "Assets/Hexfire/Weapons/Prefabs/GreenOrbPickup.prefab";
    const string FirePath = "Assets/Hovl Studio/Procedural fire/Prefabs/Magic fire pro green.prefab";
    const string MatPath = "Assets/Hexfire/Weapons/Materials/Mat_HeldGreenOrb.mat";

    [MenuItem("Hexfire/Create Green Orb Pickup (jak WeaponTest)")]
    public static void Create()
    {
      WeaponData data = AssetDatabase.LoadAssetAtPath<WeaponData>(WeaponDataPath);
      if (data == null)
      {
        EditorUtility.DisplayDialog("Hexfire", "Brak Weapon_GreenFireOrb w Data.", "OK");
        return;
      }

      var root = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      root.name = "GreenOrbPickup";
      root.transform.localScale = Vector3.one * 0.8f;

      var rb = root.AddComponent<Rigidbody>();
      rb.useGravity = true;

      var col = root.GetComponent<SphereCollider>();
      col.isTrigger = false;

      var mat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
      if (mat != null)
        root.GetComponent<Renderer>().sharedMaterial = mat;

      var firePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FirePath);
      if (firePrefab != null)
      {
        var fire = (GameObject)PrefabUtility.InstantiatePrefab(firePrefab, root.transform);
        fire.transform.localPosition = Vector3.zero;
        fire.transform.localScale = Vector3.one * 0.25f;
      }

      var item = root.AddComponent<HexfireWeaponItem>();
      item.weaponData = data;
      item.pickupRadius = 2.5f;

      LinkHandVisualIfMissing(data);

      PrefabUtility.SaveAsPrefabAsset(root, SavePath);
      Object.DestroyImmediate(root);

      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();

      var created = AssetDatabase.LoadAssetAtPath<GameObject>(SavePath);
      EditorGUIUtility.PingObject(created);

      EditorUtility.DisplayDialog(
        "Hexfire",
        "Prefab gotowy:\n" + SavePath + "\n\nWrzuc na mape. Podnies: E w zasiegu.",
        "OK");
    }

    static void LinkHandVisualIfMissing(WeaponData data)
    {
      if (data.heldVisualPrefab != null)
        return;

      var existing = AssetDatabase.LoadAssetAtPath<GameObject>(
        "Assets/Hexfire/Weapons/Prefabs/HeldVisual_GreenOrb.prefab");
      if (existing == null)
        return;

      data.heldVisualPrefab = existing;
      EditorUtility.SetDirty(data);
    }
  }
}
#endif
