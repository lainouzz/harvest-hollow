using UnityEngine;
using UnityEngine.InputSystem;

// Opens/closes the inventory panel on Tab. Put on an always-active object (e.g. the
// Player), NOT the panel — a hidden object stops updating and couldn't reopen it.
public class InventoryToggle : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [Tooltip("Optional — freezes look and frees the cursor while open.")]
    [SerializeField] private PlayerLook playerLook;
    [SerializeField] private Key toggleKey = Key.Tab;

    private void Start()
    {
        SetOpen(false);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard[toggleKey].wasPressedThisFrame)
            Toggle();
    }

    public void Toggle()
    {
        if (inventoryPanel != null)
            SetOpen(!inventoryPanel.activeSelf);
    }

    public void Open() => SetOpen(true);
    public void Close() => SetOpen(false);

    private void SetOpen(bool open)
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(open);

        if (open && InventoryManager.Instance != null)
            InventoryManager.Instance.Refresh();

        if (playerLook != null)
            playerLook.isInShopUI = open;

        Cursor.lockState = open ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = open;
    }
}
