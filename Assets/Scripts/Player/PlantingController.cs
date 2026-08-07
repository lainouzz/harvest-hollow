using UnityEngine;
using UnityEngine.InputSystem;

// Put this on the player. It aims a ray out of the camera (the crosshair, since
// the cursor is locked), finds the grid cell being looked at, and acts on it
// based on the item currently held (see ItemSwitcher).
public class PlantingController : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private FarmGrid grid;
    [SerializeField] private Animator anim;

    [Tooltip("Decides what a left-click does. Leave empty to find it in the scene.")]
    [SerializeField] private ItemSwitcher itemSwitcher;

    [Tooltip("Layer(s) the grid ground sits on. The ray only hits these.")]
    [SerializeField] private LayerMask groundMask;

    [Tooltip("How far the player can reach to plant, in world units.")]
    [SerializeField] private float maxReach = 6f;

    [SerializeField] private CropData selectedCrop;
    [Tooltip("Set by clicking an inventory item at runtime.")]
    [SerializeField] private CropData selectedSeed;
    private InventoryItem selectedSeedItem;   // the inventory stack the seed comes from
    private InventoryManager inventory;
    private PlayerLook playerLook;

    [Header("Preview (optional)")]
    [Tooltip("A flat quad/decal that highlights the aimed cell. Leave empty to skip.")]
    [SerializeField] private GameObject cellHighlight;
    [SerializeField] private Color validColor = new Color(0.3f, 1f, 0.3f, 0.5f);
    [SerializeField] private Color invalidColor = new Color(1f, 0.3f, 0.3f, 0.5f);

    private GameInput input;
    private ItemsInHand currentItem = ItemsInHand.None;

    private bool hasAimedCell;
    private Vector2Int aimedCell;

    private void Awake()
    {
        input = new GameInput();
        if (playerCamera == null) playerCamera = Camera.main;
        if (itemSwitcher == null) itemSwitcher = FindAnyObjectByType<ItemSwitcher>();
        if (playerLook == null) playerLook = FindAnyObjectByType<PlayerLook>();

        inventory = FindFirstObjectByType<InventoryManager>(FindObjectsInactive.Include);
    }

    private void OnEnable()
    {
        input.Enable();

        if (itemSwitcher != null)
            itemSwitcher.OnItemChanged += OnItemChanged;

        if (inventory != null)
            inventory.OnItemSelected += OnInventoryItemSelected;
    }

    private void OnDisable()
    {
        input.Disable();

        if (itemSwitcher != null)
            itemSwitcher.OnItemChanged -= OnItemChanged;

        if (inventory != null)
            inventory.OnItemSelected -= OnInventoryItemSelected;
    }

    // Clicking an inventory item picks the crop it plants as the current seed.
    private void OnInventoryItemSelected(InventoryItem item)
    {
        if (item == null || item.crop == null) return;

        selectedSeedItem = item;
        selectedSeed = item.crop;
        Debug.Log($"Selected {item.crop.cropName} to plant.");
    }

    private void Start()
    {
        if (itemSwitcher != null)
            currentItem = itemSwitcher.Current;
    }

    private void OnItemChanged(ItemsInHand item)
    {
        currentItem = item;
    }

    private void Update()
    {
        UpdateAimedCell();
        UpdateHighlight();

        if (!hasAimedCell) return;

        if (input.Player.Attack.WasPressedThisFrame())
            HandleLeftClick(currentItem);
    }

    // What a left-click does depends on what you're holding.
    private void HandleLeftClick(ItemsInHand item)
    {
        switch (item)
        {
            case ItemsInHand.Hoe:
                if (!grid.IsCellOccupied(aimedCell))
                    TryPlantAimed();
                else
                    grid.TryTill(aimedCell);
                break;

            case ItemsInHand.WaterCan:
                grid.TryWater(aimedCell);
                break;

            case ItemsInHand.Seeds:
                TryPlantSelectedSeed();
                break;

            case ItemsInHand.HarvestTool:
                grid.HarvestAt(aimedCell);
                break;

            case ItemsInHand.None:
                grid.RemoveAt(aimedCell);
                break;
            default:
                break;
        }
    }

    private void UpdateAimedCell()
    {
        hasAimedCell = false;
        if (grid == null || playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxReach, groundMask))
            return;

        Vector2Int cell = grid.WorldToGrid(hit.point);
        if (!grid.IsInsideGrid(cell)) return;

        aimedCell = cell;
        hasAimedCell = true;
    }

    private void TryPlantAimed()
    {
        if (selectedCrop == null)
        {
            Debug.LogWarning("PlantingController: no crop selected.", this);
            return;
        }

        grid.TryPlant(aimedCell, selectedCrop);
    }

    // Plants the selected seed and consumes one from the inventory — but only if
    // the plant actually happened. No seed in stock = can't plant.
    private void TryPlantSelectedSeed()
    {
        if (!HasSeedInStock())
        {
            Debug.Log(selectedSeed == null
                ? "No seed selected — pick one from the inventory."
                : $"Out of {selectedSeed.cropName} seeds.");
            return;
        }

        if (grid.TryPlantSeed(aimedCell, selectedSeed))
            inventory.Remove(selectedSeedItem, 1);
    }

    private bool HasSeedInStock()
    {
        return selectedSeedItem != null &&
               selectedSeed != null &&
               inventory != null &&
               inventory.CountOf(selectedSeedItem) > 0;
    }

    private void UpdateHighlight()
    {
        if (cellHighlight == null) return;

        cellHighlight.SetActive(hasAimedCell);
        if (!hasAimedCell) return;

        Vector2Int size = selectedCrop != null ? selectedCrop.size : Vector2Int.one;
        cellHighlight.transform.position = grid.GridToWorld(aimedCell, size) + Vector3.up * 0.02f;

        if (cellHighlight.TryGetComponent(out Renderer r))
            r.material.color = CanActHere(size) ? validColor : invalidColor;
    }

    // Green only when the item you're holding can actually act on this cell.
    private bool CanActHere(Vector2Int size)
    {
        switch (currentItem)
        {
            case ItemsInHand.Hoe:
                if (!grid.IsCellOccupied(aimedCell))
                    return selectedCrop != null && grid.CanPlace(aimedCell, size);
                return grid.GetState(aimedCell) == TileState.Untilled;

            case ItemsInHand.WaterCan:
                return grid.GetState(aimedCell) == TileState.Tilled;

            case ItemsInHand.Seeds:
                return HasSeedInStock() && grid.CanPlantSeed(aimedCell);

            case ItemsInHand.HarvestTool:
                return grid.CanHarvest(aimedCell);

            default:
                return false;
        }

        // TODO: this only checks the anchor cell, so a multi-cell crop (size > 1x1)
        //       can show green when part of its footprint isn't valid.
    }

    public void SetSelectedCrop(CropData crop) => selectedCrop = crop;
}
