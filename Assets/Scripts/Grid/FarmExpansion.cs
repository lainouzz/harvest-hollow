using UnityEngine;
using UnityEngine.InputSystem;

// Placeholder farm-slot shop. Holds fake currency and buys plots from the grid.
// The grid only knows locked/unlocked — cost/currency lives here.
//
// Two ways to buy:
//   - TryBuySlot(cell)      : buy a specific plot. Call this from UI.
//   - TryBuyLookedAtSlot()  : buy whatever buyable plot you're aiming at.
//
// TODO: the B-key is a debug binding for the look-at buy; replace with your UI.
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

    public int NextSlotCost => baseSlotCost + slotsBought * costIncreasePerSlot;

    private void Awake()
    {
        if (grid == null) grid = GetComponent<FarmGrid>();
        if (playerCamera == null) playerCamera = Camera.main;
        if (wallet == null) wallet = FindAnyObjectByType<Wallet>();
    }

    private void Update()
    {
        // Placeholder: press B to buy the buyable plot you're looking at.
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.bKey.wasPressedThisFrame)
            TryBuyLookedAtSlot();
    }

    // ---- UI path -------------------------------------------------------------

    // Buy a specific plot. This is what buttons / menus should call.
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

        grid.Unlock(cell);
        slotsBought++;
        Debug.Log($"Bought plot {cell} for {cost}. Coins left: {wallet.Coins}.");
        return true;
    }

    // Convenience for a simple "expand" button — buys the first buyable plot.
    public bool TryBuyNextSlot()
    {
        if (!grid.TryGetNextPurchasableCell(out Vector2Int cell))
        {
            Debug.Log("Farm is fully expanded — nothing left to buy.");
            return false;
        }

        return TryBuySlot(cell);
    }

    // ---- Look-at path --------------------------------------------------------

    // Buy the buyable plot the player is looking at. Returns false if not aiming
    // at one (or it can't be afforded — TryBuySlot handles the messaging).
    public bool TryBuyLookedAtSlot()
    {
        if (!TryGetLookedAtCell(out Vector2Int cell))
            return false;

        return TryBuySlot(cell);
    }

    // TODO: this raycast duplicates PlantingController.UpdateAimedCell — if a third
    //       thing ever needs aiming, pull it into a shared "GridAimer" component.
    private bool TryGetLookedAtCell(out Vector2Int cell)
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
