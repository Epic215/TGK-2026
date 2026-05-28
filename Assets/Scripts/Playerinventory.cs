using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public enum WeaponType { Single, Spread, Chaos }

public class PlayerInventory : MonoBehaviour
{
    [Header("Weapon Slots (max 3)")]
    public WeaponType[] slots = new WeaponType[3];
    private int activeSlot = 0;
    private bool[] hasWeapon = new bool[3];

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    public GameObject weaponDropPrefab;
    private InputAction dropAction;

    [Header("Fire Settings")]
    public float bulletSpeed = 18f;
    public float singleFireRate = 0.35f;
    public float spreadFireRate = 0.5f;
    public float chaosFireRate = 0.12f;
    public int chaosBurst = 5;

    [Header("UI — przeciągnij 3 obiekty TextMeshPro")]
    public TextMeshProUGUI slot1Text;
    public TextMeshProUGUI slot2Text;
    public TextMeshProUGUI slot3Text;

    private float fireTimer = 0f;
    private bool isBusy = false;

    private PlayerShoot playerShoot;
    private PlayerInput playerInput;
    private InputAction slot1Action;
    private InputAction slot2Action;
    private InputAction slot3Action;

    void Start()
{
    playerShoot = GetComponent<PlayerShoot>();
    playerInput = GetComponent<PlayerInput>();

    slot1Action = playerInput.actions["Slot1"];
    slot2Action = playerInput.actions["Slot2"];
    slot3Action = playerInput.actions["Slot3"];

    dropAction = playerInput.actions["Drop"];

    Debug.Log("Slot1: " + slot1Action);
    Debug.Log("Slot2: " + slot2Action);
    Debug.Log("Slot3: " + slot3Action);

    PickUpWeapon(WeaponType.Single);
    UpdateWeaponHUD();
}

    void Update()
    {
        fireTimer -= Time.deltaTime;

        if (slot1Action.WasPressedThisFrame()) { Debug.Log("Slot1 pressed"); SwitchSlot(0); }
        if (slot2Action.WasPressedThisFrame()) { Debug.Log("Slot2 pressed"); SwitchSlot(1); }
        if (slot3Action.WasPressedThisFrame()) { Debug.Log("Slot3 pressed"); SwitchSlot(2); }

        if (dropAction.WasPressedThisFrame()) DropCurrentWeapon();

        bool shooting = playerShoot != null ? playerShoot.IsShooting : false;

        if (shooting && !isBusy)
        {
            switch (slots[activeSlot])
            {
                case WeaponType.Single:
                    if (fireTimer <= 0f) { FireSingle(); fireTimer = singleFireRate; }
                    break;
                case WeaponType.Spread:
                    if (fireTimer <= 0f) { FireSpread(); fireTimer = spreadFireRate; }
                    break;
                case WeaponType.Chaos:
                    if (fireTimer <= 0f) { StartCoroutine(FireChaos()); fireTimer = chaosFireRate * chaosBurst + 0.4f; }
                    break;
            }
        }
    }

    void FireSingle() => SpawnBullet(firePoint.forward, 0f);

    void FireSpread()
    {
        foreach (float angle in new float[] { -15f, 0f, 15f })
        {
            SpawnBullet(firePoint.forward, angle);
        }
            
    }

    IEnumerator FireChaos()
    {
        isBusy = true;
        for (int i = 0; i < chaosBurst; i++)
        {
            SpawnBullet(firePoint.forward, Random.Range(-25f, 25f));
            yield return new WaitForSeconds(chaosFireRate);
        }
        isBusy = false;
    }

    void SpawnBullet(Vector3 baseDir, float angleOffset)
    {
        if (bulletPrefab == null || firePoint == null) return;

        Quaternion rotation = Quaternion.AngleAxis(angleOffset, Vector3.up)
                              * Quaternion.LookRotation(baseDir);

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, rotation);

        Bullet b = bullet.GetComponent<Bullet>();
        if (b != null) b.ownerTag = "Player";

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = rotation * Vector3.forward * bulletSpeed;

        Collider bulletCollider = bullet.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();
        if (bulletCollider != null && playerCollider != null)
            Physics.IgnoreCollision(bulletCollider, playerCollider);

        Destroy(bullet, 3f);
    }

    public void PickUpWeapon(WeaponType type)
    {
        int targetSlot = -1;
        for (int i = 0; i < slots.Length; i++)
            if (!hasWeapon[i]) { targetSlot = i; break; }
        if (targetSlot == -1) targetSlot = activeSlot;

        slots[targetSlot] = type;
        hasWeapon[targetSlot] = true;
        SwitchSlot(targetSlot);
        UpdateWeaponHUD();
    }   

    void DropCurrentWeapon()
    {
    if (!hasWeapon[activeSlot]) return;
    if (weaponDropPrefab == null) return;

    // Spawn prefabu broni przed graczem
    Vector3 dropPos = transform.position + transform.forward * 1.5f;
    GameObject dropped = Instantiate(weaponDropPrefab, dropPos, Quaternion.identity);

    // Ustaw typ broni na upuszczonym prefabie
    WeaponItem item = dropped.GetComponent<WeaponItem>();
    if (item != null) item.weaponType = slots[activeSlot];

    // Wyczyść slot
    hasWeapon[activeSlot] = false;

    // Przełącz na pierwszy dostępny slot
    for (int i = 0; i < slots.Length; i++)
    {
        if (hasWeapon[i]) { SwitchSlot(i); break; }
    }

    UpdateWeaponHUD();
    }

    void SwitchSlot(int index)
    {
        if (!hasWeapon[index]) return;
        activeSlot = index;
        UpdateWeaponHUD();
    }

    void UpdateWeaponHUD()
    {
        TextMeshProUGUI[] texts = { slot1Text, slot2Text, slot3Text };
        string[] labels = { "W1", "W2", "W3" };

        for (int i = 0; i < 3; i++)
        {
            if (texts[i] == null) continue;

            if (!hasWeapon[i])
            {
                texts[i].text = $"[{labels[i]}] ---";
                texts[i].color = Color.gray;
            }
            else
            {
                string weaponName = slots[i] switch
                {
                    WeaponType.Single => "SINGLE",
                    WeaponType.Spread => "SPREAD",
                    WeaponType.Chaos  => "CHAOS",
                    _ => "?"
                };
                texts[i].text = $"[{labels[i]}] {weaponName}";
                texts[i].color = (i == activeSlot) ? Color.yellow : Color.white;
            }
        }
    }
}