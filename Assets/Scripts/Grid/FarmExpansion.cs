using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(FarmGrid))]
public class FarmExpansion : MonoBehaviour
{
    [SerializeField] private FarmGrid grid;

    [Tooltip("Where the money comes from. Empty = find the Wallet in the scene.")]
    [SerializeField] private Wallet wallet;

    [Header("Plot pricing")]
    [Tooltip("Base price of the first slot.")]
    [SerializeField] private int baseSlotCost = 10;
    [Tooltip("Added to the price for each slot already bought (0 = flat price).")]
    [SerializeField] private int costIncreasePerSlot = 5;

    [Header("Look-at buying")]
    [Tooltip("Camera the player looks through. Empty = Camera.main.")]
    [SerializeField] private Camera playerCamera;
    [Tooltip("Layer(s) the grid ground sits on — same as PlantingController.")]
    [SerializeField] private LayerMask groundMask;
    [Tooltip("How close you must be to buy a plot you're looking at.")]
    [SerializeField] private float maxReach = 6f;

    private int slotsBought;
    private PlayerLook playerLook;

    public int NextSlotCost => baseSlotCost + slotsBought * costIncreasePerSlot;

    public int SlotsBought
    {
        get => slotsBought;
        set => slotsBought = Mathf.Max(0, value);
    }

    private void Awake()
    {
        if (grid == null) grid = GetComponent<FarmGrid>();
        if (playerCamera == null) playerCamera = Camera.main;
        if (wallet == null) wallet = FindAnyObjectByType<Wallet>();
        playerLook = FindAnyObjectByType<PlayerLook>();
    }

    // Same rule as the other tools: no buying while paused or in a menu.
    public bool CanBuyNow => Time.timeScale > 0f && (playerLook == null || !playerLook.isInShopUI);

    private void Update()
    {
        if (!CanBuyNow) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.bKey.wasPressedThisFrame)
            TryBuyLookedAtSlot();
    }

    public bool TryBuySlot(Vector2Int cell)
    {
        if (!grid.IsPurchasable(cell))
        {
            Debug.Log($"Cell {cell} can't be bought (locked & not next to your farm, or already owned).");
            return false;
        }

        int cost = NextSlotCost;
        if (wallet == null || !wallet.TrySpend(cost))
        {
            Debug.Log($"Not enough coins: need {cost}, have {(wallet != null ? wallet.Coins : 0)}.");
            return false;
        }

        slotsBought++;   // before Unlock, so the price icons redraw with the new price
        grid.Unlock(cell);
        Debug.Log($"Bought plot {cell} for {cost}. Coins left: {wallet.Coins}.");
        return true;
    }

    public bool TryBuyNextSlot()
    {
        if (!grid.TryGetNextPurchasableCell(out Vector2Int cell))
        {
            Debug.Log("Farm is fully expanded — nothing left to buy.");
            return false;
        }

        return TryBuySlot(cell);
    }

    public bool TryBuyLookedAtSlot()
    {
        if (!TryGetLookedAtCell(out Vector2Int cell))
            return false;

        return TryBuySlot(cell);
    }

    public bool TryGetLookedAtCell(out Vector2Int cell)
    {
        cell = default;
        if (playerCamera == null) return false;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxReach, groundMask))
            return false;

        cell = grid.WorldToGrid(hit.point);
        return grid.IsInsideGrid(cell);
    }
}
