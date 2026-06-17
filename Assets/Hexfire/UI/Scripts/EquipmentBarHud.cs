using System.Collections;
using Hexfire.Weapons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hexfire.UI
{
  [DisallowMultipleComponent]
  public class EquipmentBarHud : MonoBehaviour
  {
    const string BarObjectName = "EquipmentBar";

#if UNITY_EDITOR
    const string SlotSelectedSpritePath =
      "Assets/Pixel_HUD_UI_FreeKit/Sprites/UI Elements/UI_Slot_Selected.png";
#endif

    static readonly Color BarBackground = new Color(0.04f, 0.04f, 0.06f, 0.88f);
    static readonly Color FrameIdle = new Color(0.14f, 0.14f, 0.17f, 1f);
    static readonly Color FrameHighlight = new Color(1f, 1f, 1f, 0.95f);

    [Header("Sloty")]
    [Tooltip("Ramka aktywnego slotu (UI_Slot_Selected z Pixel HUD).")]
    public Sprite slotSelectedSprite;

    [Header("Pozycja — widac od razu w edytorze")]
    public HudAnchorCorner anchorCorner = HudAnchorCorner.BottomLeft;

    [Tooltip("Odsuniecie od rogu (przy 1920x1080).")]
    public Vector2 cornerOffset = new Vector2(24f, 24f);

    [Header("Sloty")]
    public float slotSize = 72f;
    public float slotSpacing = 12f;
    public float barPaddingLeft = 12f;

    [Header("Pasek (auto-liczony z slotow)")]
    public bool autoBarSize = true;
    public Vector2 barSize = new Vector2(276f, 108f);

    [Header("Obiekty (po Zbuduj HUD)")]
    public RectTransform barRoot;
    public Image[] slotFrames = new Image[3];
    public Image[] slotIcons = new Image[3];
    public TextMeshProUGUI[] slotLabels = new TextMeshProUGUI[3];

    PlayerEquipment equipment;

    void Reset()
    {
      HudEditorLayoutDefer.Schedule(this, BuildNow);
    }

    void OnValidate()
    {
      if (autoBarSize)
        barSize = CalculateBarSize();

      EnsureDefaultSlotSprites();
      HudEditorLayoutDefer.Schedule(this, RefreshLayout);
    }

    void Awake()
    {
      EnsureDefaultSlotSprites();
    }

#if UNITY_EDITOR
    void EnsureDefaultSlotSprites()
    {
      if (slotSelectedSprite == null)
      {
        slotSelectedSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(
          SlotSelectedSpritePath);
      }
    }
#else
    void EnsureDefaultSlotSprites() { }
#endif

    void RefreshLayout()
    {
      CacheReferences();
      if (barRoot == null)
        return;

      ApplyAnchoring();
      UpdateSlotLayout();
      ConfigureIconImages();
      ApplySlotFrameStyles();
    }

    void ApplySlotFrameStyles()
    {
      int active = equipment != null ? equipment.ActiveSlotIndex : 0;
      for (int i = 0; i < 3; i++)
        ApplyFrameHighlight(i, i == active);
    }

    void ConfigureIconImages()
    {
      for (int i = 0; i < 3; i++)
      {
        Image icon = GetSlotIcon(i);
        if (icon != null)
          SetupIconImage(icon);
      }
    }

    [ContextMenu("Zbuduj pasek ekwipunku")]
    public void BuildNow()
    {
      EnsureDefaultSlotSprites();

      if (autoBarSize)
        barSize = CalculateBarSize();

      EnsureBarRoot();
      ApplyAnchoring();
      CacheReferences();

      if (slotIcons[0] == null)
        CreateAllSlots();
      else
      {
        CreateAllSlots();
        UpdateSlotLayout();
      }

      CacheReferences();
      ConfigureIconImages();

      Image background = barRoot.GetComponent<Image>();
      if (background == null)
        background = barRoot.gameObject.AddComponent<Image>();
      ManaBarVisuals.ApplyPanelColor(background, BarBackground);

      HudUiFactory.SetDirty(this);
      HudUiFactory.SetDirty(gameObject);
    }

    public void EnsureBuilt(Transform canvas)
    {
      CacheReferences();

      if (barRoot != null && slotIcons[0] != null)
      {
        ApplyAnchoring();
        ConfigureIconImages();
        return;
      }

      BuildNow();
    }

    public void Bind(PlayerEquipment playerEquipment)
    {
      equipment = playerEquipment;
      if (equipment == null)
        return;

      CacheReferences();
      if (slotIcons[0] == null)
        BuildNow();

      ApplyAnchoring();
      ConfigureIconImages();

      equipment.slot1Text = slotLabels[0];
      equipment.slot2Text = slotLabels[1];
      equipment.slot3Text = slotLabels[2];
      equipment.slotIconImages = GetSlotIconArray();

      equipment.OnEquipmentChanged -= OnEquipmentChanged;
      equipment.OnEquipmentChanged += OnEquipmentChanged;
      OnEquipmentChanged();

      if (Application.isPlaying)
        StartCoroutine(RefreshIconsNextFrame());
    }

    IEnumerator RefreshIconsNextFrame()
    {
      yield return null;
      RefreshFrames();
    }

    void OnEquipmentChanged()
    {
      RefreshFrames();
      equipment?.RefreshHudDisplay();
    }

    void OnDestroy()
    {
      if (equipment != null)
        equipment.OnEquipmentChanged -= OnEquipmentChanged;
    }

    Vector2 CalculateBarSize()
    {
      float width = barPaddingLeft * 2f + slotSize * 3f + slotSpacing * 2f;
      float height = slotSize + 36f;
      return new Vector2(width, height);
    }

    void EnsureBarRoot()
    {
      if (barRoot != null)
        return;

      Transform existing = transform.Find(BarObjectName);
      if (existing != null)
      {
        barRoot = existing as RectTransform;
        return;
      }

      var barObject = HudUiFactory.Create(BarObjectName, transform, typeof(RectTransform));
      barRoot = barObject.GetComponent<RectTransform>();
    }

    void CacheReferences()
    {
      if (barRoot == null)
        barRoot = transform.Find(BarObjectName) as RectTransform;

      if (barRoot == null)
        return;

      for (int i = 0; i < 3; i++)
      {
        Transform column = barRoot.Find($"Slot{i + 1}Column");
        if (column == null)
          continue;

        slotFrames[i] = column.Find($"Slot{i + 1}Frame")?.GetComponent<Image>();
        RemoveLegacyColumnIcon(i, column);
        slotIcons[i] = FindSlotIcon(i, column);
        slotLabels[i] = column.Find($"Slot{i + 1}Label")?.GetComponent<TextMeshProUGUI>();
      }
    }

    public void ApplyAnchoring()
    {
      if (barRoot == null)
        return;

      GetAnchor(anchorCorner, out Vector2 anchor, out Vector2 pivot);
      barRoot.anchorMin = anchor;
      barRoot.anchorMax = anchor;
      barRoot.pivot = pivot;
      barRoot.sizeDelta = barSize;
      barRoot.anchoredPosition = GetAnchoredPosition(anchorCorner, cornerOffset);
    }

    void UpdateSlotLayout()
    {
      if (barRoot == null)
        return;

      for (int i = 0; i < 3; i++)
      {
        float x = barPaddingLeft + slotSize * 0.5f + i * (slotSize + slotSpacing);
        Transform column = barRoot.Find($"Slot{i + 1}Column");
        if (column == null)
          continue;

        RectTransform columnRect = column.GetComponent<RectTransform>();
        columnRect.sizeDelta = new Vector2(slotSize, barSize.y - 12f);
        columnRect.anchoredPosition = new Vector2(x, 6f);

        Transform frame = column.Find($"Slot{i + 1}Frame");
        if (frame != null)
          frame.GetComponent<RectTransform>().sizeDelta = new Vector2(slotSize, slotSize);

        Transform label = column.Find($"Slot{i + 1}Label");
        if (label != null)
          label.GetComponent<RectTransform>().sizeDelta = new Vector2(slotSize + 8f, 24f);

        EnsureIconDrawOrder(i, column);
      }
    }

    static void GetAnchor(HudAnchorCorner corner, out Vector2 anchor, out Vector2 pivot)
    {
      switch (corner)
      {
        case HudAnchorCorner.BottomRight:
          anchor = new Vector2(1f, 0f);
          pivot = new Vector2(1f, 0f);
          break;
        case HudAnchorCorner.TopLeft:
          anchor = new Vector2(0f, 1f);
          pivot = new Vector2(0f, 1f);
          break;
        case HudAnchorCorner.TopRight:
          anchor = new Vector2(1f, 1f);
          pivot = new Vector2(1f, 1f);
          break;
        case HudAnchorCorner.BottomCenter:
          anchor = new Vector2(0.5f, 0f);
          pivot = new Vector2(0.5f, 0f);
          break;
        default:
          anchor = Vector2.zero;
          pivot = Vector2.zero;
          break;
      }
    }

    static Vector2 GetAnchoredPosition(HudAnchorCorner corner, Vector2 offset)
    {
      return corner switch
      {
        HudAnchorCorner.BottomRight => new Vector2(-offset.x, offset.y),
        HudAnchorCorner.TopLeft => new Vector2(offset.x, -offset.y),
        HudAnchorCorner.TopRight => new Vector2(-offset.x, -offset.y),
        HudAnchorCorner.BottomCenter => new Vector2(0f, offset.y),
        _ => offset
      };
    }

    void RefreshFrames()
    {
      if (equipment == null)
        return;

      int active = equipment.ActiveSlotIndex;
      for (int i = 0; i < 3; i++)
      {
        ApplyFrameHighlight(i, i == active);
        ApplyWeaponIcon(i, equipment.GetWeaponInSlot(i));
      }
    }

    void ApplyFrameHighlight(int index, bool isActive)
    {
      if (slotFrames[index] == null)
        return;

      Image frame = slotFrames[index];

      Outline outline = frame.GetComponent<Outline>();
      if (outline != null)
        outline.effectColor = Color.clear;

      if (isActive && slotSelectedSprite != null)
      {
        frame.sprite = slotSelectedSprite;
        frame.type = Image.Type.Simple;
        frame.preserveAspect = false;
        frame.color = Color.white;
        return;
      }

      ApplyHexfireIdleFrame(frame);

      if (slotSelectedSprite == null)
      {
        if (outline == null)
          outline = frame.gameObject.AddComponent<Outline>();

        outline.effectColor = isActive ? FrameHighlight : Color.clear;
        outline.effectDistance = new Vector2(2f, -2f);
        outline.useGraphicAlpha = true;
      }
    }

    static void ApplyHexfireIdleFrame(Image frame)
    {
      ManaBarVisuals.ApplyPanelColor(frame, FrameIdle);
    }

    void ApplyWeaponIcon(int index, WeaponData weapon)
    {
      Image icon = GetSlotIcon(index);
      if (icon == null)
        return;

      icon.transform.SetAsLastSibling();

      if (weapon != null && weapon.icon != null)
      {
        icon.gameObject.SetActive(true);
        icon.sprite = weapon.icon;
        icon.type = Image.Type.Simple;
        icon.preserveAspect = true;
        icon.color = Color.white;
        icon.enabled = true;
        icon.canvasRenderer.cullTransparentMesh = false;
      }
      else
      {
        icon.sprite = null;
        icon.enabled = false;
      }
    }

    void CreateAllSlots()
    {
      TextMeshProUGUI fontSource = FindSceneFont();

      for (int i = 0; i < 3; i++)
      {
        float x = barPaddingLeft + slotSize * 0.5f + i * (slotSize + slotSpacing);
        Transform column = barRoot.Find($"Slot{i + 1}Column");

        if (column == null)
        {
          CreateSlot(i, x, fontSource);
          continue;
        }

        EnsureSlotParts(i, column, fontSource);
      }

      CacheReferences();
    }

    void EnsureSlotParts(int index, Transform column, TextMeshProUGUI fontSource)
    {
      Transform frame = column.Find($"Slot{index + 1}Frame");
      if (frame == null)
      {
        float x = barPaddingLeft + slotSize * 0.5f + index * (slotSize + slotSpacing);
        DestroyImmediateSafe(column.gameObject);
        CreateSlot(index, x, fontSource);
        return;
      }

      RemoveLegacyColumnIcon(index, column);

      Image frameImage = frame.GetComponent<Image>();
      if (frameImage != null)
      {
        slotFrames[index] = frameImage;
        SetupSlotFrameImage(frameImage, false);
      }

      ReparentKeyToFrame(index, column, frame);

      Image icon = FindSlotIcon(index, column);
      if (icon == null)
        icon = CreateSlotIcon(index, frame);

      slotIcons[index] = icon;
      icon.transform.SetAsLastSibling();

      Transform label = column.Find($"Slot{index + 1}Label");
      if (label != null)
        slotLabels[index] = label.GetComponent<TextMeshProUGUI>();
    }

    static void RemoveLegacyColumnIcon(int index, Transform column)
    {
      Transform legacy = column.Find($"Slot{index + 1}Icon");
      if (legacy != null && legacy.parent == column)
        DestroyImmediateSafe(legacy.gameObject);
    }

    static void ReparentKeyToFrame(int index, Transform column, Transform frame)
    {
      Transform key = column.Find($"Slot{index + 1}Key");
      if (key == null)
        return;

      if (key.parent == frame)
        return;

      key.SetParent(frame, false);
      RectTransform keyRect = key.GetComponent<RectTransform>();
      keyRect.anchorMin = new Vector2(0f, 1f);
      keyRect.anchorMax = new Vector2(0f, 1f);
      keyRect.pivot = new Vector2(0f, 1f);
      keyRect.anchoredPosition = new Vector2(4f, -2f);
      keyRect.sizeDelta = new Vector2(20f, 20f);
    }

    static Image FindSlotIcon(int index, Transform column)
    {
      Transform frame = column.Find($"Slot{index + 1}Frame");
      if (frame == null)
        return null;

      Transform icon = frame.Find($"Slot{index + 1}Icon");
      return icon != null ? icon.GetComponent<Image>() : null;
    }

    Image CreateSlotIcon(int index, Transform frame)
    {
      Transform existing = frame.Find($"Slot{index + 1}Icon");
      if (existing != null)
        DestroyImmediateSafe(existing.gameObject);

      var iconObject = HudUiFactory.Create(
        $"Slot{index + 1}Icon",
        frame,
        typeof(RectTransform),
        typeof(CanvasRenderer),
        typeof(Image));

      RectTransform iconRect = iconObject.GetComponent<RectTransform>();
      iconRect.anchorMin = Vector2.zero;
      iconRect.anchorMax = Vector2.one;
      iconRect.offsetMin = new Vector2(4f, 4f);
      iconRect.offsetMax = new Vector2(-4f, -4f);

      Image icon = iconObject.GetComponent<Image>();
      SetupIconImage(icon);
      iconObject.transform.SetAsLastSibling();
      return icon;
    }

    static void SetupIconImage(Image icon)
    {
      icon.type = Image.Type.Simple;
      icon.color = Color.white;
      icon.preserveAspect = true;
      icon.raycastTarget = false;
      icon.enabled = true;
    }

    void EnsureIconDrawOrder(int index, Transform column)
    {
      Transform frame = column.Find($"Slot{index + 1}Frame");
      if (frame == null)
        return;

      Image icon = FindSlotIcon(index, column);
      if (icon != null)
        icon.transform.SetAsLastSibling();
    }

    public Image GetSlotIcon(int index)
    {
      if (barRoot == null)
        barRoot = transform.Find(BarObjectName) as RectTransform;

      if (barRoot == null)
        return null;

      Transform column = barRoot.Find($"Slot{index + 1}Column");
      if (column == null)
        return null;

      Transform frame = column.Find($"Slot{index + 1}Frame");
      if (frame == null)
        return null;

      RemoveLegacyColumnIcon(index, column);

      Image icon = FindSlotIcon(index, column);
      if (icon == null)
        icon = CreateSlotIcon(index, frame);

      slotIcons[index] = icon;
      return icon;
    }

    public Image[] GetSlotIconArray()
    {
      var icons = new Image[3];
      for (int i = 0; i < 3; i++)
        icons[i] = GetSlotIcon(i);
      return icons;
    }

    static TextMeshProUGUI FindSceneFont()
    {
      return GameObject.Find("MP_txt")?.GetComponent<TextMeshProUGUI>()
        ?? GameObject.Find("HP_text")?.GetComponent<TextMeshProUGUI>();
    }

    void CreateSlot(int index, float localX, TextMeshProUGUI fontSource)
    {
      var column = HudUiFactory.Create($"Slot{index + 1}Column", barRoot, typeof(RectTransform));
      RectTransform columnRect = column.GetComponent<RectTransform>();
      columnRect.anchorMin = new Vector2(0f, 0.5f);
      columnRect.anchorMax = new Vector2(0f, 0.5f);
      columnRect.pivot = new Vector2(0.5f, 0.5f);
      columnRect.sizeDelta = new Vector2(slotSize, barSize.y - 12f);
      columnRect.anchoredPosition = new Vector2(localX, 6f);

      var frameObject = HudUiFactory.Create(
        $"Slot{index + 1}Frame",
        column.transform,
        typeof(RectTransform),
        typeof(CanvasRenderer),
        typeof(Image));

      RectTransform frameRect = frameObject.GetComponent<RectTransform>();
      frameRect.anchorMin = new Vector2(0.5f, 1f);
      frameRect.anchorMax = new Vector2(0.5f, 1f);
      frameRect.pivot = new Vector2(0.5f, 1f);
      frameRect.sizeDelta = new Vector2(slotSize, slotSize);
      frameRect.anchoredPosition = Vector2.zero;

      Image frame = frameObject.GetComponent<Image>();
      SetupSlotFrameImage(frame, false);
      slotFrames[index] = frame;

      var keyObject = HudUiFactory.Create(
        $"Slot{index + 1}Key",
        frameObject.transform,
        typeof(RectTransform),
        typeof(CanvasRenderer),
        typeof(TextMeshProUGUI));

      RectTransform keyRect = keyObject.GetComponent<RectTransform>();
      keyRect.anchorMin = new Vector2(0f, 1f);
      keyRect.anchorMax = new Vector2(0f, 1f);
      keyRect.pivot = new Vector2(0f, 1f);
      keyRect.anchoredPosition = new Vector2(4f, -2f);
      keyRect.sizeDelta = new Vector2(20f, 20f);

      TextMeshProUGUI keyText = keyObject.GetComponent<TextMeshProUGUI>();
      keyText.text = (index + 1).ToString();
      keyText.fontSize = 18f;
      keyText.fontStyle = FontStyles.Bold;
      keyText.alignment = TextAlignmentOptions.TopLeft;
      keyText.color = new Color(1f, 1f, 1f, 0.85f);
      keyText.raycastTarget = false;
      CopyFont(fontSource, keyText);

      var labelObject = HudUiFactory.Create(
        $"Slot{index + 1}Label",
        column.transform,
        typeof(RectTransform),
        typeof(CanvasRenderer),
        typeof(TextMeshProUGUI));

      RectTransform labelRect = labelObject.GetComponent<RectTransform>();
      labelRect.anchorMin = new Vector2(0.5f, 0f);
      labelRect.anchorMax = new Vector2(0.5f, 0f);
      labelRect.pivot = new Vector2(0.5f, 0f);
      labelRect.sizeDelta = new Vector2(slotSize + 8f, 24f);
      labelRect.anchoredPosition = Vector2.zero;

      TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
      label.text = index == 0 ? "Slot 1" : "---";
      label.fontSize = 14f;
      label.alignment = TextAlignmentOptions.Top;
      label.overflowMode = TextOverflowModes.Ellipsis;
      label.textWrappingMode = TextWrappingModes.NoWrap;
      label.color = Color.white;
      label.raycastTarget = false;
      CopyFont(fontSource, label);
      slotLabels[index] = label;

      Image icon = CreateSlotIcon(index, frameObject.transform);
      slotIcons[index] = icon;
    }

    static void DestroyImmediateSafe(Object target)
    {
      if (target == null)
        return;

#if UNITY_EDITOR
      if (!Application.isPlaying)
        Object.DestroyImmediate(target);
      else
#endif
        Object.Destroy(target);
    }

    static void CopyFont(TextMeshProUGUI source, TextMeshProUGUI target)
    {
      if (source == null || target == null)
        return;

      target.font = source.font;
      target.fontSharedMaterial = source.fontSharedMaterial;
    }

    void SetupSlotFrameImage(Image frame, bool isActive)
    {
      if (frame == null)
        return;

      frame.raycastTarget = false;

      if (isActive && slotSelectedSprite != null)
      {
        frame.sprite = slotSelectedSprite;
        frame.type = Image.Type.Simple;
        frame.preserveAspect = false;
        frame.color = Color.white;
        return;
      }

      ApplyHexfireIdleFrame(frame);
    }
  }
}
