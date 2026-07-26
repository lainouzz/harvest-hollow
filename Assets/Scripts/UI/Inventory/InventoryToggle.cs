using UnityEngine;
using UnityEngine.InputSystem;

// Opens/closes the inventory panel on Tab. Put this on an ALWAYS-ACTIVE object
// (e.g. the Player) — not on the panel itself, or it would stop running the
// moment the panel is hidden and you could never reopen it.
public class InventoryToggle : MonoBehaviour
{
    [Tooltip("The inventory panel to show/hide (e.g. playerInventory).")]
    [SerializeField] private GameObject inventoryPanel;

    [Tooltip("Optional — freezes camera look and frees the cursor while open.")]
    [SerializeField] private PlayerLook playerLook;

    [SerializeField] private Key toggleKey = Key.Tab;

    private void Start()
    {
        SetOpen(false);   // start closed and in a known state
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

    // Public so the Exit button can hook Close() directly.
    public void Open() => SetOpen(true);
    public void Close() => SetOpen(false);

    private void SetOpen(bool open)
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(open);

        // Make sure the list reflects the latest items when it opens.
        if (open && InventoryManager.Instance != null)
            InventoryManager.Instance.Refresh();

        if (playerLook != null)
            playerLook.isInShopUI = open;   // reuse the "a UI is open, stop looking" flag

        Cursor.lockState = open ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = open;
    }
}
