using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class WeaponItem : MonoBehaviour
{
    [Header("Weapon Settings")]
    public WeaponType weaponType = WeaponType.Single;
    public float pickupRadius = 2.5f;

    [Header("UI Hint")]
    public TextMeshProUGUI pickupHintText;

    private Transform player;
    private PlayerInventory inventory;
    private PlayerInput playerInput;
    private InputAction interactAction;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            inventory = playerObj.GetComponent<PlayerInventory>();
            playerInput = playerObj.GetComponent<PlayerInput>();
            interactAction = playerInput.actions["Interact"];
        }
        if (pickupHintText != null) pickupHintText.enabled = false;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool playerInRange = dist <= pickupRadius;

        if (pickupHintText != null)
            pickupHintText.enabled = playerInRange;

        if (playerInRange && interactAction.WasPressedThisFrame())
        {
            inventory?.PickUpWeapon(weaponType);
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}