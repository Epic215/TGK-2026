#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hexfire.Weapons;

namespace Hexfire.EditorTools
{
  public static class HexfirePlayerSetup
  {
    const string GreenFireWeaponPath = "Assets/Hexfire/Weapons/Data/Weapon_GreenFireOrb.asset";
    const string PlayerMagePrefabPath = "Assets/Hexfire/Player/Prefabs/Player_Mage.prefab";

    [MenuItem("Hexfire/Setup Player Mage (scena — aktywny gracz)")]
    public static void SetupPlayerMageInScene()
    {
      GameObject player = GameObject.FindGameObjectWithTag("Player");
      if (player == null)
        player = GameObject.Find("Player_Mage");

      if (player == null)
      {
        EditorUtility.DisplayDialog(
          "Hexfire",
          "Nie znaleziono gracza w scenie (tag Player lub nazwa Player_Mage).",
          "OK");
        return;
      }

      Undo.RegisterFullObjectHierarchyUndo(player, "Hexfire Player Mage Setup");
      ApplySetup(player);
      EditorUtility.SetDirty(player);

      if (!Application.isPlaying)
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

      ShowDoneDialog();
    }

    [MenuItem("Hexfire/Setup Player Mage Prefab")]
    public static void SetupPlayerMagePrefab()
    {
      GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PlayerMagePrefabPath);
      if (prefabRoot == null)
      {
        EditorUtility.DisplayDialog("Hexfire", "Nie znaleziono Player_Mage.prefab.", "OK");
        return;
      }

      try
      {
        ApplySetup(prefabRoot);
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerMagePrefabPath);
        AssetDatabase.SaveAssets();
      }
      finally
      {
        PrefabUtility.UnloadPrefabContents(prefabRoot);
      }

      ShowDoneDialog();
    }

    static void ApplySetup(GameObject player)
    {
      if (!player.CompareTag("Player"))
        player.tag = "Player";

      Transform firePoint = player.transform.Find("FirePoint");
      if (firePoint == null)
      {
        var firePointObject = new GameObject("FirePoint");
        firePoint = firePointObject.transform;
        firePoint.SetParent(player.transform, false);
        firePoint.localPosition = new Vector3(0f, 0.55f, 0.45f);
        firePoint.localRotation = Quaternion.identity;
      }

      GetOrAdd<HeldWeaponVisual>(player);
      PlayerEquipment equipment = GetOrAdd<PlayerEquipment>(player);
      GetOrAdd<PlayerShoot>(player);
      GetOrAdd<PlayerHealth>(player);
      GetOrAdd<PlayerMana>(player);
      GetOrAdd<PlayerIFrameBridge>(player);

      WeaponData startingWeapon = AssetDatabase.LoadAssetAtPath<WeaponData>(GreenFireWeaponPath);
      equipment.firePoint = firePoint;
      equipment.startingWeapon = startingWeapon;

      HeldWeaponVisual heldVisual = player.GetComponent<HeldWeaponVisual>();
      if (heldVisual != null && startingWeapon != null)
        heldVisual.ShowWeapon(startingWeapon);
    }

    static void ShowDoneDialog()
    {
      EditorUtility.DisplayDialog(
        "Hexfire",
        "Player_Mage skonfigurowany:\n" +
        "- PlayerHealth, PlayerMana, PlayerIFrameBridge\n" +
        "- PlayerEquipment + HeldWeaponVisual + PlayerShoot\n" +
        "- FirePoint + tag Player\n" +
        "- Zielona kula jako bron startowa\n\n" +
        "HUD: na Canvas dodaj PlayerHudWire + paski z Hexfire/UI/Prefabs/HUD.",
        "OK");
    }

    static T GetOrAdd<T>(GameObject target) where T : Component
    {
      T component = target.GetComponent<T>();
      if (component != null)
        return component;

      return target.AddComponent<T>();
    }
  }
}
#endif
