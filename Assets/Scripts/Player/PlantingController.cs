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
    [SerializeField] private CropData selectedSeed;

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
    }

    private void OnEnable()
    {
        input.Enable();

        if (itemSwitcher != null)
            itemSwitcher.OnItemChanged += OnItemChanged;
    }

    private void OnDisable()
    {
        input.Disable();

        if (itemSwitcher != null)
            itemSwitcher.OnItemChanged -= OnItemChanged;
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
            

        // TODO: E currently just deletes. It should HARVEST instead: only work if
        //       the Crop on that cell IsGrown, add crop.sellValue/produce to the
        //       inventory, then clear the crop but leave the tile tilled.
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
                grid.TryPlantSeed(aimedCell, selectedSeed);
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

        // TODO: check the inventory has seeds for selectedCrop and deduct one on
        //       success (TryPlant returns false if it didn't plant - don't deduct then).
        grid.TryPlant(aimedCell, selectedCrop);
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
                return selectedSeed != null && grid.CanPlantSeed(aimedCell);

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
