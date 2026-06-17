#if UNITY_EDITOR
using BloodlinesUI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Hexfire.EditorTools
{
  public static class HexfireMenuSetup
  {
    const string MenuScenePath = "Assets/Scenes/Menu.unity";
    const string BloodlinesButtonPath = "Assets/Alebardium/Bloodlines UI/Prefabs/Button/Button 1 (Red).prefab";
    const string BannerTexturePath = "Assets/Hexfire/UI/Prefabs/Hellfire_banner.jpg";

    [MenuItem("Hexfire/Setup Main Menu (tlo + przyciski Start/Exit)")]
    public static void SetupMainMenu()
    {
      Scene menuScene = EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);

      Canvas canvas = Object.FindFirstObjectByType<Canvas>();
      if (canvas == null)
      {
        EditorUtility.DisplayDialog("Hexfire", "Brak Canvas w scenie Menu.", "OK");
        return;
      }

      Undo.RegisterFullObjectHierarchyUndo(canvas.gameObject, "Hexfire Main Menu Setup");

      ConfigureCanvas(canvas);
      EnsureEventSystem();

      MenuManager menuManager = canvas.GetComponent<MenuManager>();
      if (menuManager == null)
        menuManager = Undo.AddComponent<MenuManager>(canvas.gameObject);

      Transform canvasTransform = canvas.transform;
      CreateOrUpdateBackground(canvasTransform);

      DestroyIfExists(canvasTransform, "Start");
      DestroyIfExists(canvasTransform, "Exit");
      DestroyIfExists(canvasTransform, "Image");
      DestroyIfExists(canvasTransform, "Game");
      DestroyIfExists(canvasTransform, "Title");

      GameObject startButton = CreateMenuButton(canvasTransform, "Start", "START", new Vector2(0f, -40f));
      GameObject exitButton = CreateMenuButton(canvasTransform, "Exit", "EXIT", new Vector2(0f, -130f));

      WireButton(startButton, menuManager, nameof(MenuManager.StartGame));
      WireButton(exitButton, menuManager, nameof(MenuManager.QuitGame));

      EditorSceneManager.MarkSceneDirty(menuScene);
      EditorUtility.DisplayDialog(
        "Hexfire",
        "Menu gotowe:\n" +
        "- Tlo: Hellfire_banner\n" +
        "- Przyciski Bloodlines (START / EXIT)\n" +
        "- Jasniejsze kolory tekstu",
        "OK");
    }

    static void ConfigureCanvas(Canvas canvas)
    {
      CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
      if (scaler == null)
        scaler = Undo.AddComponent<CanvasScaler>(canvas.gameObject);

      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920f, 1080f);
      scaler.matchWidthOrHeight = 0.5f;

      if (canvas.GetComponent<GraphicRaycaster>() == null)
        Undo.AddComponent<GraphicRaycaster>(canvas.gameObject);
    }

    static void EnsureEventSystem()
    {
      if (Object.FindFirstObjectByType<EventSystem>() != null)
        return;

      var eventSystemObject = new GameObject("EventSystem");
      Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");
      eventSystemObject.AddComponent<EventSystem>();
      eventSystemObject.AddComponent<StandaloneInputModule>();
    }

    static void CreateOrUpdateBackground(Transform parent)
    {
      Transform existing = parent.Find("Background");
      GameObject backgroundObject = existing != null
        ? existing.gameObject
        : new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));

      if (existing == null)
      {
        Undo.RegisterCreatedObjectUndo(backgroundObject, "Create Background");
        backgroundObject.transform.SetParent(parent, false);
      }

      backgroundObject.transform.SetAsFirstSibling();

      RectTransform rect = backgroundObject.GetComponent<RectTransform>();
      StretchFullScreen(rect);

      RawImage rawImage = backgroundObject.GetComponent<RawImage>();
      if (rawImage == null)
        rawImage = Undo.AddComponent<RawImage>(backgroundObject);

      Texture2D banner = AssetDatabase.LoadAssetAtPath<Texture2D>(BannerTexturePath);
      rawImage.texture = banner;
      rawImage.color = Color.white;
      rawImage.raycastTarget = false;
    }

    static GameObject CreateMenuButton(Transform parent, string objectName, string label, Vector2 anchoredPosition)
    {
      GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BloodlinesButtonPath);
      GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
      Undo.RegisterCreatedObjectUndo(instance, "Create " + objectName);
      instance.name = objectName;

      RectTransform rect = instance.GetComponent<RectTransform>();
      rect.anchorMin = new Vector2(0.5f, 0.5f);
      rect.anchorMax = new Vector2(0.5f, 0.5f);
      rect.pivot = new Vector2(0.5f, 0.5f);
      rect.anchoredPosition = anchoredPosition;
      rect.sizeDelta = new Vector2(320f, 72f);

      TextMeshProUGUI buttonText = instance.GetComponentInChildren<TextMeshProUGUI>(true);
      if (buttonText != null)
      {
        buttonText.text = label;
        buttonText.fontSize = 30;
        buttonText.fontStyle = FontStyles.Bold;
      }

      ButtonTextColorChanger colorChanger = instance.GetComponent<ButtonTextColorChanger>();
      if (colorChanger != null)
      {
        colorChanger.defaultColor = new Color(1f, 0.95f, 0.75f, 1f);
        colorChanger.highlightedColor = new Color(1f, 1f, 1f, 1f);
        colorChanger.pressedColor = new Color(1f, 0.55f, 0.2f, 1f);
        colorChanger.disabledColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        if (colorChanger.buttonText != null)
          colorChanger.buttonText.color = colorChanger.defaultColor;
      }

      return instance;
    }

    static void WireButton(GameObject buttonObject, MenuManager menuManager, string methodName)
    {
      Button button = buttonObject.GetComponent<Button>();
      if (button == null || menuManager == null)
        return;

      button.onClick = new Button.ButtonClickedEvent();
      if (methodName == nameof(MenuManager.StartGame))
        button.onClick.AddListener(menuManager.StartGame);
      else if (methodName == nameof(MenuManager.QuitGame))
        button.onClick.AddListener(menuManager.QuitGame);
    }

    static void StretchFullScreen(RectTransform rect)
    {
      rect.anchorMin = Vector2.zero;
      rect.anchorMax = Vector2.one;
      rect.offsetMin = Vector2.zero;
      rect.offsetMax = Vector2.zero;
      rect.localScale = Vector3.one;
    }

    static void DestroyIfExists(Transform parent, string childName)
    {
      Transform child = parent.Find(childName);
      if (child != null)
        Undo.DestroyObjectImmediate(child.gameObject);
    }
  }
}
#endif
