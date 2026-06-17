using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hexfire.UI
{
  [DefaultExecutionOrder(-100)]
  [DisallowMultipleComponent]
  public class PlayerHudWire : MonoBehaviour
  {
    const string HpTextName = "HP_text";
    const string MpTextName = "MP_txt";

    static readonly string[] LegacySlotUiNames =
    {
      "Slot1Text", "Slot2Text", "Slot3Text",
      "Slot1Icon", "Slot2Icon", "Slot3Icon",
      "W1Icon", "W2Icon", "W3Icon"
    };

    [Header("Komponenty na tym Canvas")]
    public EquipmentBarHud equipmentBarHud;
    public WeaponInfoPanel weaponInfoPanel;

    [Header("Canvas Scaler")]
    public bool applyCanvasScalerSettings = true;
    public Vector2 referenceResolution = new Vector2(1920f, 1080f);

    [Range(0f, 1f)]
    public float matchWidthOrHeight = 0.5f;

    void Reset()
    {
      HudEditorLayoutDefer.Schedule(this, () =>
      {
        EnsureComponents();
        BuildHudInEditor();
      });
    }

    void OnValidate()
    {
      if (equipmentBarHud == null)
        equipmentBarHud = GetComponent<EquipmentBarHud>();
      if (weaponInfoPanel == null)
        weaponInfoPanel = GetComponent<WeaponInfoPanel>();
    }

    [ContextMenu("Zbuduj caly HUD (edytor)")]
    public void BuildHudInEditor()
    {
      EnsureComponents();
      equipmentBarHud.BuildNow();
      weaponInfoPanel.BuildNow();
    }

    void EnsureComponents()
    {
      equipmentBarHud = GetComponent<EquipmentBarHud>();
      if (equipmentBarHud == null)
        equipmentBarHud = gameObject.AddComponent<EquipmentBarHud>();

      weaponInfoPanel = GetComponent<WeaponInfoPanel>();
      if (weaponInfoPanel == null)
        weaponInfoPanel = gameObject.AddComponent<WeaponInfoPanel>();
    }

    void Awake()
    {
      EnsureComponents();

      if (applyCanvasScalerSettings)
        StabilizeCanvasScaler();

      if (Application.isPlaying)
        NormalizeLegacyHudScale();

      TextMeshProUGUI hpText = FindText(HpTextName);
      TextMeshProUGUI mpText = FindText(MpTextName);

      StatBarView healthBar = WireBar(
        FindBarObject("HealthBar", "Slider 1 (Horizontal)"),
        hpText);

      StatBarView manaBar = WireBar(
        FindBarObject("ManaBar", "Slider 2 (Horizontal)"),
        mpText);

      if (manaBar != null)
        ManaBarVisuals.ApplyBlueFill(manaBar.transform);

      CooldownRingView dashRing = WireDash(
        FindBarObject("DashCooldown", "Slider 1 (Round)"));

      if (!Application.isPlaying)
        return;

      GameObject player = GameObject.FindGameObjectWithTag("Player");
      if (player == null)
        player = GameObject.Find("Player_Mage");

      if (player == null)
        return;

      PlayerHealth health = player.GetComponent<PlayerHealth>();
      if (health != null)
      {
        health.healthBar = healthBar;
        health.healthText = hpText;
      }

      PlayerMana mana = player.GetComponent<PlayerMana>();
      if (mana != null)
      {
        mana.manaBar = manaBar;
        mana.manaText = mpText;
      }

      PlayerDashAbility dash = player.GetComponent<PlayerDashAbility>();
      if (dash != null)
        dash.dashCooldownRing = dashRing;

      PlayerEquipment equipment = player.GetComponent<PlayerEquipment>();
      if (equipment != null)
        WireEquipment(equipment);
    }

    void WireEquipment(PlayerEquipment equipment)
    {
      equipment.weaponNameText ??= FindText("WeaponNameText");
      equipment.weaponDescriptionText ??= FindText("WeaponDescriptionText");
      equipment.pickupHintText ??= FindText("PickupHintText");

      HideLegacySlotUi();

      equipmentBarHud.EnsureBuilt(transform);
      equipmentBarHud.Bind(equipment);
      weaponInfoPanel.Bind(equipment, equipmentBarHud);
    }

    void StabilizeCanvasScaler()
    {
      CanvasScaler scaler = GetComponent<CanvasScaler>();
      if (scaler == null)
        return;

      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = referenceResolution;
      scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
      scaler.matchWidthOrHeight = matchWidthOrHeight;
    }

    static void NormalizeLegacyHudScale()
    {
      foreach (string objectName in new[] { HpTextName, MpTextName })
      {
        GameObject hudObject = GameObject.Find(objectName);
        if (hudObject == null)
          continue;

        if (hudObject.transform.localScale != Vector3.one)
          hudObject.transform.localScale = Vector3.one;
      }
    }

    static void HideLegacySlotUi()
    {
      foreach (string objectName in LegacySlotUiNames)
      {
        GameObject legacyObject = GameObject.Find(objectName);
        if (legacyObject != null)
          legacyObject.SetActive(false);
      }
    }

    static TextMeshProUGUI FindText(string objectName)
    {
      GameObject textObject = GameObject.Find(objectName);
      return textObject != null ? textObject.GetComponent<TextMeshProUGUI>() : null;
    }

    static GameObject FindBarObject(string primaryName, string fallbackName)
    {
      GameObject bar = GameObject.Find(primaryName);
      if (bar != null)
        return bar;

      return GameObject.Find(fallbackName);
    }

    static StatBarView WireBar(GameObject barObject, TextMeshProUGUI valueText)
    {
      if (barObject == null)
        return null;

      Slider slider = barObject.GetComponent<Slider>();
      if (slider == null)
        return null;

      slider.interactable = false;

      if (barObject.transform.localScale != Vector3.one)
        barObject.transform.localScale = Vector3.one;

      Animator animator = barObject.GetComponent<Animator>();
      if (animator != null)
        animator.enabled = false;

      StatBarView view = barObject.GetComponent<StatBarView>();
      if (view == null)
        view = barObject.AddComponent<StatBarView>();

      view.slider = slider;
      view.valueText = valueText;
      view.textFormat = "{0} / {1}";

      return view;
    }

    static CooldownRingView WireDash(GameObject ringObject)
    {
      if (ringObject == null)
        return null;

      Slider slider = ringObject.GetComponent<Slider>();
      if (slider == null)
        return null;

      slider.interactable = false;
      slider.minValue = 0f;
      slider.maxValue = 1f;
      slider.value = 1f;

      if (ringObject.transform.localScale != Vector3.one)
        ringObject.transform.localScale = Vector3.one;

      CooldownRingView ring = ringObject.GetComponent<CooldownRingView>();
      if (ring == null)
        ring = ringObject.AddComponent<CooldownRingView>();

      ring.slider = slider;
      ring.backgroundImage = FindDashBackground(ringObject.transform);
      ring.backgroundAlpha = 0.45f;
      ring.percentText = ringObject.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
      ring.SetReadyAmount(1f);

      return ring;
    }

    static Image FindDashBackground(Transform root)
    {
      Transform nested = root.Find("Round Image/Background");
      if (nested != null)
        return nested.GetComponent<Image>();

      Transform direct = root.Find("Background");
      if (direct != null)
        return direct.GetComponent<Image>();

      Image[] images = root.GetComponentsInChildren<Image>(true);
      foreach (Image image in images)
      {
        if (image.gameObject.name == "Background")
          return image;
      }

      return null;
    }
  }
}
