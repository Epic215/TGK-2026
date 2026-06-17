using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hexfire.UI
{
  [DisallowMultipleComponent]
  public class WeaponInfoPanel : MonoBehaviour
  {
    [Header("Reczne podpiecie (opcjonalne)")]
    public Button infoButton;
    public GameObject panelRoot;
    public TextMeshProUGUI detailText;

    [Header("Panel info")]
    public Vector2 infoPanelSize = new Vector2(520f, 320f);
    public float infoPanelGapAboveBar = 8f;
    public float infoButtonGapRightOfBar = 8f;

    PlayerEquipment equipment;
    EquipmentBarHud equipmentBar;
    bool panelVisible;

    void Reset()
    {
      equipmentBar = GetComponent<EquipmentBarHud>();
      HudEditorLayoutDefer.Schedule(this, BuildNow);
    }

    void OnValidate()
    {
      if (equipmentBar == null)
        equipmentBar = GetComponent<EquipmentBarHud>();

      HudEditorLayoutDefer.Schedule(this, TryApplyLayout);
    }

    void TryApplyLayout()
    {
      if (equipmentBar != null && equipmentBar.barRoot != null)
        ApplyLayout();
    }

    [ContextMenu("Zbuduj panel info")]
    public void BuildNow()
    {
      if (equipmentBar == null)
        equipmentBar = GetComponent<EquipmentBarHud>();

      EnsureUi();
      HudUiFactory.SetDirty(this);
    }

    public void Bind(PlayerEquipment playerEquipment, EquipmentBarHud barHud = null)
    {
      if (playerEquipment == null)
        return;

      if (equipment != null)
        equipment.OnEquipmentChanged -= Refresh;

      equipment = playerEquipment;
      equipmentBar = barHud != null ? barHud : GetComponent<EquipmentBarHud>();
      equipment.OnEquipmentChanged += Refresh;

      EnsureUi();
      Refresh();
    }

    void OnDestroy()
    {
      if (equipment != null)
        equipment.OnEquipmentChanged -= Refresh;
    }

    void EnsureUi()
    {
      if (equipmentBar == null)
        equipmentBar = GetComponent<EquipmentBarHud>();

      if (infoButton == null)
        infoButton = transform.Find("WeaponInfoButton")?.GetComponent<Button>();

      if (panelRoot == null)
        panelRoot = transform.Find("WeaponInfoPanel")?.gameObject;

      if (detailText == null)
        detailText = transform.Find("WeaponInfoPanel/WeaponInfoText")?.GetComponent<TextMeshProUGUI>();

      if (infoButton == null)
        infoButton = CreateInfoButton(transform);

      if (panelRoot == null || detailText == null)
        CreateInfoPanel(transform);

      ApplyLayout();

      if (panelRoot != null)
        panelRoot.SetActive(Application.isPlaying && panelVisible);

      if (infoButton != null && Application.isPlaying)
      {
        infoButton.onClick.RemoveListener(TogglePanel);
        infoButton.onClick.AddListener(TogglePanel);
      }
    }

    void TogglePanel()
    {
      panelVisible = !panelVisible;
      if (panelRoot != null)
        panelRoot.SetActive(panelVisible);

      if (panelVisible)
        Refresh();
    }

    void Refresh()
    {
      if (detailText == null || equipment == null)
        return;

      detailText.text = WeaponDetailTextBuilder.Build(equipment.ActiveWeapon);
    }

    static TextMeshProUGUI SceneFontSource =>
      GameObject.Find("MP_txt")?.GetComponent<TextMeshProUGUI>()
      ?? GameObject.Find("HP_text")?.GetComponent<TextMeshProUGUI>();

    static void ApplySceneFont(TextMeshProUGUI target)
    {
      TextMeshProUGUI source = SceneFontSource;
      if (source == null || target == null)
        return;

      target.font = source.font;
      target.fontSharedMaterial = source.fontSharedMaterial;
    }

    void ApplyLayout()
    {
      if (equipmentBar == null || equipmentBar.barRoot == null)
        return;

      RectTransform bar = equipmentBar.barRoot;

      if (infoButton != null)
      {
        RectTransform buttonRect = infoButton.GetComponent<RectTransform>();
        buttonRect.anchorMin = bar.anchorMin;
        buttonRect.anchorMax = bar.anchorMax;
        buttonRect.pivot = new Vector2(0f, 0.5f);
        buttonRect.sizeDelta = new Vector2(40f, 40f);
        buttonRect.anchoredPosition = bar.anchoredPosition
          + new Vector2(bar.sizeDelta.x + infoButtonGapRightOfBar, bar.sizeDelta.y * 0.5f);
      }

      if (panelRoot == null || detailText == null)
        return;

      RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
      panelRect.anchorMin = bar.anchorMin;
      panelRect.anchorMax = bar.anchorMax;
      panelRect.pivot = new Vector2(0f, 0f);
      panelRect.sizeDelta = infoPanelSize;
      panelRect.anchoredPosition = bar.anchoredPosition + new Vector2(0f, bar.sizeDelta.y + infoPanelGapAboveBar);

      detailText.fontSize = 22f;
      detailText.enableAutoSizing = true;
      detailText.fontSizeMin = 16f;
      detailText.fontSizeMax = 22f;
      detailText.textWrappingMode = TextWrappingModes.Normal;
      detailText.overflowMode = TextOverflowModes.Overflow;
    }

    Button CreateInfoButton(Transform parent)
    {
      var buttonObject = HudUiFactory.Create(
        "WeaponInfoButton",
        parent,
        typeof(RectTransform),
        typeof(CanvasRenderer),
        typeof(Image),
        typeof(Button));

      RectTransform rect = buttonObject.GetComponent<RectTransform>();
      rect.sizeDelta = new Vector2(40f, 40f);

      ManaBarVisuals.ApplyPanelColor(buttonObject.GetComponent<Image>(), new Color(0.12f, 0.12f, 0.12f, 0.9f));

      var labelObject = HudUiFactory.Create(
        "Label",
        buttonObject.transform,
        typeof(RectTransform),
        typeof(CanvasRenderer),
        typeof(TextMeshProUGUI));

      RectTransform labelRect = labelObject.GetComponent<RectTransform>();
      labelRect.anchorMin = Vector2.zero;
      labelRect.anchorMax = Vector2.one;
      labelRect.offsetMin = Vector2.zero;
      labelRect.offsetMax = Vector2.zero;

      TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
      label.text = "i";
      label.fontSize = 26f;
      label.alignment = TextAlignmentOptions.Center;
      label.color = Color.white;
      label.raycastTarget = false;
      ApplySceneFont(label);

      return buttonObject.GetComponent<Button>();
    }

    void CreateInfoPanel(Transform parent)
    {
      var panelObject = HudUiFactory.Create(
        "WeaponInfoPanel",
        parent,
        typeof(RectTransform),
        typeof(CanvasRenderer),
        typeof(Image));

      panelRoot = panelObject;
      panelObject.GetComponent<RectTransform>().sizeDelta = infoPanelSize;
      ManaBarVisuals.ApplyPanelColor(panelObject.GetComponent<Image>(), new Color(0.05f, 0.05f, 0.08f, 0.94f));

      var textObject = HudUiFactory.Create(
        "WeaponInfoText",
        panelObject.transform,
        typeof(RectTransform),
        typeof(CanvasRenderer),
        typeof(TextMeshProUGUI));

      RectTransform textRect = textObject.GetComponent<RectTransform>();
      textRect.anchorMin = Vector2.zero;
      textRect.anchorMax = Vector2.one;
      textRect.offsetMin = new Vector2(14f, 12f);
      textRect.offsetMax = new Vector2(-14f, -12f);

      detailText = textObject.GetComponent<TextMeshProUGUI>();
      detailText.text = "Opis broni pojawi sie w grze.";
      detailText.fontSize = 22f;
      detailText.alignment = TextAlignmentOptions.TopLeft;
      detailText.color = Color.white;
      detailText.richText = true;
      detailText.raycastTarget = false;
      ApplySceneFont(detailText);

      panelObject.SetActive(false);
    }
  }
}
