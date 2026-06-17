using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Hexfire.Weapons;

namespace Hexfire
{
  [DefaultExecutionOrder(50)]
  public class PlayerEquipment : MonoBehaviour
  {
    public const int SlotCount = 3;

    [Header("Equipment")]
    public Transform firePoint;
    [Tooltip("Bron startowa w slocie 1.")]
    public WeaponData startingWeapon;
    [Tooltip("Prefab upuszczonej broni (jak stary WeaponTest).")]
    public GameObject weaponDropPrefab;

    [Header("HUD (opcjonalne — jak stary PlayerInventory)")]
    public TextMeshProUGUI slot1Text;
    public TextMeshProUGUI slot2Text;
    public TextMeshProUGUI slot3Text;
    public Image[] slotIconImages = new Image[SlotCount];
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI weaponDescriptionText;
    [Tooltip("Np. [E] Podnies: Zielona Kula")]
    public TextMeshProUGUI pickupHintText;

    readonly WeaponData[] slots = new WeaponData[SlotCount];
    int activeSlot;
    float fireCooldown;

    PlayerShoot playerShoot;
    HeldWeaponVisual heldWeaponVisual;
    PlayerMana playerMana;
    PlayerHealth playerHealth;
    PlayerIFrameBridge playerIFrameBridge;
    PlayerInput playerInput;
    InputAction slot1Action;
    InputAction slot2Action;
    InputAction slot3Action;
    InputAction interactAction;
    InputAction dropAction;
    InputAction healAction;

    float healCooldown;
    bool firePatternBusy;

    HexfireWeaponItem nearestPickup;
    static HexfireWeaponItem[] pickupBuffer = Array.Empty<HexfireWeaponItem>();

    public WeaponData ActiveWeapon => HasWeaponInSlot(activeSlot) ? slots[activeSlot] : null;
    public int ActiveSlotIndex => activeSlot;

    public event Action OnEquipmentChanged;

    void Awake()
    {
      playerShoot = GetComponent<PlayerShoot>();
      heldWeaponVisual = GetComponent<HeldWeaponVisual>();
      playerMana = GetComponent<PlayerMana>();
      playerHealth = GetComponent<PlayerHealth>();
      playerIFrameBridge = GetComponent<PlayerIFrameBridge>();
    }

    void Start()
    {
      if (startingWeapon != null)
        TryAddWeapon(startingWeapon, preferSlot: 0);
      else
        RefreshHeldVisual();

      RefreshHud();
      RefreshPickupHint();
    }

    void Update()
    {
      EnsureInputBound();

      if (slot1Action != null && slot1Action.WasPressedThisFrame())
        SelectSlot(0);
      if (slot2Action != null && slot2Action.WasPressedThisFrame())
        SelectSlot(1);
      if (slot3Action != null && slot3Action.WasPressedThisFrame())
        SelectSlot(2);

      if (dropAction != null && dropAction.WasPressedThisFrame())
        DropActiveWeapon();

      if (interactAction != null && interactAction.WasPressedThisFrame())
        TryPickupNearest();

      UpdateNearestPickup();
      fireCooldown -= Time.deltaTime;
      healCooldown -= Time.deltaTime;

      TryFireActiveWeapon();
      TryUseWeaponAbility();
    }

    void TryUseWeaponAbility()
    {
      if (healAction == null || !healAction.WasPressedThisFrame())
        return;

      if (healCooldown > 0f)
        return;

      if (ActiveWeapon is GreenFireOrbWeaponData healWeapon)
      {
        TryHealWithGreenOrb(healWeapon);
        return;
      }

      if (ActiveWeapon is StaffWeaponData staffWeapon)
      {
        if (staffWeapon.abilityType == StaffAbilityType.None)
          return;

        if (playerMana != null && !playerMana.HasEnough(staffWeapon.abilityManaCost))
          return;

        if (!staffWeapon.TryUseAbility(transform, firePoint, playerMana))
          return;

        healCooldown = staffWeapon.abilityCooldown;
        RequestShootAnimation();
        return;
      }

      if (ActiveWeapon is MeleeSwordWeaponData swordWeapon)
      {
        if (playerMana != null && !playerMana.HasEnough(swordWeapon.shieldManaCost))
          return;

        if (!swordWeapon.TryActivateShield(transform, playerMana, playerIFrameBridge, this))
          return;

        healCooldown = swordWeapon.shieldCooldown;
        RequestShieldAnimation();
      }
    }

    void TryHealWithGreenOrb(GreenFireOrbWeaponData healWeapon)
    {
      if (playerHealth != null && playerHealth.IsAtFullHealth)
        return;

      if (playerMana != null && !playerMana.HasEnough(healWeapon.HealManaCost))
        return;

      if (playerMana != null && !playerMana.TrySpend(healWeapon.HealManaCost))
        return;

      if (firePoint == null)
        return;

      healWeapon.ApplyHeal(transform, firePoint);
      healCooldown = healWeapon.healRate;
    }

    void TryFireActiveWeapon()
    {
      if (playerShoot == null || !playerShoot.IsShooting || ActiveWeapon == null)
        return;

      if (firePatternBusy)
        return;

      if (!CanFireWeapon(ActiveWeapon))
        return;

      if (!HasEnoughManaForFire(ActiveWeapon))
        return;

      if (fireCooldown > 0f)
        return;

      if (!TrySpendManaForFire(ActiveWeapon))
        return;

      FireActiveWeapon();
      fireCooldown = ActiveWeapon.FireInterval;
      RequestShootAnimation();
    }

    void RequestShootAnimation()
    {
      shootAnimationRequested = true;
    }

    public bool ConsumeShootAnimationRequest()
    {
      if (!shootAnimationRequested)
        return false;

      shootAnimationRequested = false;
      return true;
    }

    bool shootAnimationRequested;
    bool shieldAnimationRequested;

    public bool ConsumeShieldAnimationRequest()
    {
      if (!shieldAnimationRequested)
        return false;

      shieldAnimationRequested = false;
      return true;
    }

    void RequestShieldAnimation()
    {
      shieldAnimationRequested = true;
    }

    void EnsureInputBound()
    {
      if (dropAction != null)
        return;

      if (playerInput == null)
        playerInput = GetComponent<PlayerInput>();

      if (playerInput == null)
        return;

      var actions = playerInput.actions;
      slot1Action = actions.FindAction("Slot1", true);
      slot2Action = actions.FindAction("Slot2", true);
      slot3Action = actions.FindAction("Slot3", true);
      interactAction = actions.FindAction("Interact", true);
      dropAction = actions.FindAction("Drop", true);
      healAction = actions.FindAction("Heal", false);
    }


    public static bool CanFireWeapon(WeaponData weapon)
    {
      if (weapon is MeleeSwordWeaponData)
        return true;

      if (weapon is not ProjectileWeaponData projectileWeapon)
        return false;

      return projectileWeapon.projectilePrefab != null;
    }

    bool HasEnoughManaForFire(WeaponData weapon)
    {
      if (weapon == null || !weapon.usesMana)
        return true;

      float cost = GetFireManaCost(weapon);
      if (cost <= 0f)
        return true;

      if (playerMana == null)
        return true;

      return playerMana.HasEnough(cost);
    }

    bool TrySpendManaForFire(WeaponData weapon) => TrySpendMana(weapon);

    static float GetFireManaCost(WeaponData weapon)
    {
      if (weapon == null || !weapon.usesMana)
        return 0f;

      return weapon.manaCost;
    }

    bool HasEnoughMana(WeaponData weapon)
    {
      if (weapon == null || !weapon.usesMana || weapon.manaCost <= 0f)
        return true;

      if (playerMana == null)
        return true;

      return playerMana.HasEnough(weapon.manaCost);
    }

    bool TrySpendMana(WeaponData weapon)
    {
      if (weapon == null || !weapon.usesMana || weapon.manaCost <= 0f)
        return true;

      if (playerMana == null)
        return true;

      return playerMana.TrySpend(weapon.manaCost);
    }

    void FireActiveWeapon()
    {
      if (firePoint == null)
      {
        Debug.LogWarning("Hexfire.PlayerEquipment: brak firePoint.", this);
        return;
      }

      Vector3 direction = firePoint.forward;
      var context = new WeaponFireContext(
        transform,
        firePoint,
        direction,
        tag,
        () => firePatternBusy = true,
        () => firePatternBusy = false);
      ActiveWeapon.Fire(context);
    }

    public bool TryAddWeapon(WeaponData weapon, int preferSlot = -1)
    {
      if (weapon == null)
        return false;

      int targetSlot = preferSlot >= 0 && preferSlot < SlotCount && !HasWeaponInSlot(preferSlot)
        ? preferSlot
        : FindFirstEmptySlot();

      if (targetSlot < 0)
        return false;

      slots[targetSlot] = weapon;
      SelectSlot(targetSlot);
      RefreshHud();
      OnEquipmentChanged?.Invoke();
      return true;
    }

    public bool TryPickupNearest()
    {
      HexfireWeaponItem item = FindNearestPickup();
      if (item == null || item.weaponData == null)
        return false;

      int targetSlot = FindFirstEmptySlot();
      if (targetSlot < 0)
        DropActiveWeapon();

      targetSlot = FindFirstEmptySlot();
      if (targetSlot < 0)
        return false;

      slots[targetSlot] = item.weaponData;
      Destroy(item.gameObject);
      SelectSlot(targetSlot);
      RefreshHud();
      OnEquipmentChanged?.Invoke();
      return true;
    }

    public void DropActiveWeapon()
    {
      DropWeapon(activeSlot);
    }

    public void DropWeapon(int slot)
    {
      if (slot < 0 || slot >= SlotCount || !HasWeaponInSlot(slot))
        return;

      WeaponData weapon = slots[slot];
      slots[slot] = null;
      SpawnDroppedWeapon(weapon);

      if (slot == activeSlot)
      {
        int nextSlot = FindFirstOccupiedSlot();
        activeSlot = nextSlot >= 0 ? nextSlot : slot;
      }

      RefreshHeldVisual();
      RefreshHud();
      OnEquipmentChanged?.Invoke();
    }

    void SpawnDroppedWeapon(WeaponData weapon)
    {
      if (weapon == null)
        return;

      GameObject prefab = weapon.pickupPrefab != null ? weapon.pickupPrefab : weaponDropPrefab;
      if (prefab == null)
      {
        Debug.LogWarning($"Hexfire.PlayerEquipment: brak prefabu drop dla {weapon.name}.", this);
        return;
      }

      Vector3 dropPosition = transform.position + transform.forward * 1.5f;
      dropPosition.y = transform.position.y + 0.25f;

      GameObject dropped = Instantiate(prefab, dropPosition, Quaternion.identity);

      HexfireWeaponItem item = dropped.GetComponent<HexfireWeaponItem>();
      if (item == null)
        item = dropped.AddComponent<HexfireWeaponItem>();

      item.weaponData = weapon;
      if (item.pickupRadius < 0.1f)
        item.pickupRadius = 2.5f;
    }

    void UpdateNearestPickup()
    {
      HexfireWeaponItem closest = FindNearestPickup();

      if (closest == nearestPickup)
      {
        RefreshPickupHint();
        return;
      }

      if (nearestPickup != null)
        nearestPickup.SetHighlighted(false);

      nearestPickup = closest;

      if (nearestPickup != null)
        nearestPickup.SetHighlighted(true);

      RefreshPickupHint();
    }

    HexfireWeaponItem FindNearestPickup()
    {
      int count = HexfireWeaponItem.ActiveCount;
      if (count == 0)
        return null;

      if (pickupBuffer.Length < count)
        pickupBuffer = new HexfireWeaponItem[count];

      HexfireWeaponItem.GetAllActive(pickupBuffer, out count);

      HexfireWeaponItem best = null;
      float bestDistance = float.MaxValue;
      Vector3 playerPosition = transform.position;

      for (int i = 0; i < count; i++)
      {
        HexfireWeaponItem item = pickupBuffer[i];
        if (item == null || item.weaponData == null)
          continue;

        float distance = item.DistanceTo(playerPosition);
        if (distance > item.pickupRadius || distance >= bestDistance)
          continue;

        bestDistance = distance;
        best = item;
      }

      return best;
    }

    void RefreshPickupHint()
    {
      if (pickupHintText == null)
        return;

      if (nearestPickup == null || nearestPickup.weaponData == null)
      {
        pickupHintText.enabled = false;
        return;
      }

      pickupHintText.enabled = true;
      pickupHintText.text = $"[E] Podnies: {nearestPickup.weaponData.weaponName}";
    }

    public void SelectSlot(int index)
    {
      if (index < 0 || index >= SlotCount || !HasWeaponInSlot(index))
        return;

      activeSlot = index;
      RefreshHeldVisual();
      RefreshHud();
      OnEquipmentChanged?.Invoke();
    }

    public WeaponData GetWeaponInSlot(int index)
    {
      if (index < 0 || index >= SlotCount)
        return null;
      return slots[index];
    }

    public bool HasWeaponInSlot(int index)
    {
      return index >= 0 && index < SlotCount && slots[index] != null;
    }

    int FindFirstEmptySlot()
    {
      for (int i = 0; i < SlotCount; i++)
      {
        if (!HasWeaponInSlot(i))
          return i;
      }

      return -1;
    }

    int FindFirstOccupiedSlot()
    {
      for (int i = 0; i < SlotCount; i++)
      {
        if (HasWeaponInSlot(i))
          return i;
      }

      return -1;
    }

    void RefreshHeldVisual()
    {
      if (heldWeaponVisual != null)
        heldWeaponVisual.ShowWeapon(ActiveWeapon);
    }

    public void RefreshHudDisplay() => RefreshHud();

    void RefreshHud()
    {
      TextMeshProUGUI[] slotTexts = { slot1Text, slot2Text, slot3Text };

      for (int i = 0; i < SlotCount; i++)
      {
        if (slotTexts[i] != null)
        {
          if (!HasWeaponInSlot(i))
          {
            slotTexts[i].text = "---";
            slotTexts[i].color = Color.gray;
          }
          else
          {
            slotTexts[i].text = slots[i].weaponName;
            slotTexts[i].color = i == activeSlot
              ? new Color(0.92f, 0.95f, 1f, 1f)
              : new Color(0.75f, 0.75f, 0.78f, 1f);
          }
        }
      }

      WeaponData active = ActiveWeapon;
      if (weaponNameText != null)
        weaponNameText.text = active != null ? active.weaponName : string.Empty;

      if (weaponDescriptionText != null)
        weaponDescriptionText.text = active != null ? active.description : string.Empty;
    }
  }
}
