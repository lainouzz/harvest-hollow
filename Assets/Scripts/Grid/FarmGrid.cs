using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using Color = UnityEngine.Color;


public class FarmGrid : MonoBehaviour
{
    [Tooltip("MAX grid size — the farm can expand up to this. Raise it above the " +
             "start size below to leave room for buyable slots.")]
    [SerializeField] private int gridWidth = 8;
    [SerializeField] private int gridHeight = 8;
    [SerializeField] private float cellSize = 1f;

    [SerializeField] private Vector3 cellSpacing;

    [Header("Expansion")]
    [Tooltip("Cells unlocked at the start. The rest (up to the max size) are locked " +
             "and can be bought — see FarmExpansion.")]
    [SerializeField] private int startWidth = 3;
    [SerializeField] private int startHeight = 3;

    private GridCellData[,] cells;

    // id -> what's planted there. id is also stored on each occupied cell so we
    // can look up a planted object from any cell it covers.
    private readonly Dictionary<string, PlacedObjectData> placed = new();

    // Every crop currently growing. Read this for UI (progress bars, counts, etc).
    private readonly List<Crop> plantedCrops = new();
    public IReadOnlyList<Crop> PlantedCrops => plantedCrops;

    private Wallet wallet;   // found lazily; used for the placeholder harvest reward

    public Vector3 Origin => transform.position;
    public int Width => gridWidth;
    public int Height => gridHeight;
    public float CellSize => cellSize;
    
    // Distance from one cell's start to the next: the cell itself, plus the gap.
    // Gap of 0 = cells touch (so it works fine before you ever set a spacing).
    public float StepX => cellSize + cellSpacing.x;
    public float StepZ => cellSize + cellSpacing.z;

    public event Action<Vector2Int, TileState> OnCellStateChanged;

    // Raised when the playable area changes (a slot was bought). The visualizer
    // and any buy-UI listen so they can redraw.
    public event Action OnGridChanged;

    private void Awake()
    {
        InitGrid();
    }

    private void InitGrid()
    {
        int unlockedW = Mathf.Clamp(startWidth, 0, gridWidth);
        int unlockedH = Mathf.Clamp(startHeight, 0, gridHeight);

        cells = new GridCellData[gridWidth, gridHeight];
        for (int x = 0; x < gridWidth; ++x)
        {
            for (int z = 0; z < gridHeight; ++z)
            {
                cells[x, z] = new GridCellData
                {
                    isUnlocked = x < unlockedW && z < unlockedH
                };
            }
        }
    }

    // ---------------------------------------------------------------
    // Planting / removal API — called by PlantingController.
    // ---------------------------------------------------------------

    public bool TryPlant(Vector2Int cell, CropData crop)
    {
        if (crop == null || crop.prefab == null) return false;
        if (!CanPlace(cell, crop.size)) return false;

        Vector3 worldPos = GridToWorld(cell, crop.size);
        GameObject instance = Instantiate(crop.prefab, worldPos, Quaternion.Euler(-90, 0, 0), transform);
        
        // Freshly placed ground starts untilled.
        if (instance.TryGetComponent(out GroundTile groundTile))
            groundTile.SetState(TileState.Untilled);
           
        string id = Guid.NewGuid().ToString();
        OccupyCells(cell, crop.size, id);

        placed[id] = new PlacedObjectData
        {
            id = id,
            gridPosition = cell,
            gridSize = crop.size,
            crop = crop,
            instance = instance
        };
        
        RefreshStatus(cell);
        return true;
    }

    // Plants a crop ON an existing tile (the crop is a separate layer from the
    // tile — the tile stays put underneath). Watered soil gives a growth + yield
    // boost. Returns false if the cell isn't ready or already has a crop.
    public bool TryPlantSeed(Vector2Int cell, CropData crop)
    {
        if (crop == null || crop.prefab == null) return false;
        if (!CanPlantSeed(cell)) return false;

        GridCellData data = cells[cell.x, cell.y];

        Vector3 worldPos = GridToWorld(cell, crop.size) + Vector3.up * 0.1f;
        GameObject instance = Instantiate(crop.prefab, worldPos, Quaternion.Euler(-90, 0, 0), transform);

        if (!instance.TryGetComponent(out Crop cropComponent))
            cropComponent = instance.AddComponent<Crop>();

        // Watered soil -> faster growth and extra yield.
        bool watered = data.state == TileState.Watered;
        float growthMultiplier = watered ? crop.wateredGrowthMultiplier : 1f;
        int bonusYield = watered ? crop.wateredYieldBonus : 0;

        cropComponent.Initialize(crop, cell, growthMultiplier, bonusYield);

        data.crop = cropComponent;
        plantedCrops.Add(cropComponent);
        cropComponent.OnGrown.AddListener(() => RefreshStatus(cell));
        RefreshStatus(cell);   // clear the "needs water" icon now something's planted
        return true;
    }

    // Can a seed go here? Needs a tilled/watered tile with nothing growing yet.
    public bool CanPlantSeed(Vector2Int cell)
    {
        if (!IsInsideGrid(cell)) return false;

        GridCellData data = cells[cell.x, cell.y];
        return data.isOccupied && data.state != TileState.Untilled && data.crop == null;
    }

    public bool RemoveAt(Vector2Int cell)
    {
        if (!IsInsideGrid(cell)) return false;

        GridCellData cellData = cells[cell.x, cell.y];

        if (cellData.crop != null)
        {
            plantedCrops.Remove(cellData.crop);
            Destroy(cellData.crop.gameObject);
            cellData.crop = null;     
        }

        string id = cellData.id;
        if (string.IsNullOrEmpty(id)) return false;
        if (!placed.TryGetValue(id, out PlacedObjectData data)) return false;

        if (data.instance != null) Destroy(data.instance);
        FreeCells(data.gridPosition, data.gridSize);
        placed.Remove(id);
        return true;
    }

    // Harvests a fully-grown crop: drops its produce into the inventory, removes
    // the plant, and leaves the tile behind so you can replant.
    public bool HarvestAt(Vector2Int cell)
    {
        if (!CanHarvest(cell)) return false;

        GridCellData data = cells[cell.x, cell.y];
        Crop crop = data.crop;

        Debug.Log($"Harvested {crop.Data.cropName} x{crop.Yield}!");

        // Drop this crop's own produce into the inventory, yield-many of it.
        if (InventoryManager.Instance != null && crop.Data.produce != null)
            InventoryManager.Instance.Add(crop.Data.produce, crop.Yield);

        plantedCrops.Remove(crop);
        Destroy(crop.gameObject);
        data.crop = null;
        RefreshStatus(cell);   // clear the "ready to harvest" icon

        if (wallet == null) wallet = FindAnyObjectByType<Wallet>();
        if (wallet != null) wallet.Add(10);

        return true;
    }

    // Can the harvest tool act here? Only if a grown crop is present.
    public bool CanHarvest(Vector2Int cell)
    {
        if (!IsInsideGrid(cell)) return false;

        Crop crop = cells[cell.x, cell.y].crop;
        return crop != null && crop.IsGrown;
    }

    public bool TryGetPlacedAt(Vector2Int cell, out PlacedObjectData data)
    {
        data = null;
        if (!IsInsideGrid(cell)) return false;

        string id = cells[cell.x, cell.y].id;
        if (string.IsNullOrEmpty(id)) return false;
        return placed.TryGetValue(id, out data);
    }

    // Tills a placed, untilled tile: Untilled -> Tilled. Needs a tile there
    // first (you place it, then hoe it). Updates the tile's visual.
    public bool TryTill(Vector2Int cell)
    {
        if (!IsInsideGrid(cell)) return false;

        GridCellData data = cells[cell.x, cell.y];
        if (!data.isOccupied) return false;                // nothing to till yet
        if (data.state != TileState.Untilled) return false; // only raw ground

        SetCellState(cell, data, TileState.Tilled);
        return true;
    }

    // Waters tilled soil: Tilled -> Watered.
    // TODO: visual only for now — nothing consumes the water yet. Real logic
    //       (drying out each day, gating Crop growth) comes next.
    public bool TryWater(Vector2Int cell)
    {
        if (!IsInsideGrid(cell)) return false;

        GridCellData data = cells[cell.x, cell.y];
        if (!data.isOccupied) return false;
        if (data.state != TileState.Tilled) return false; // must be tilled first

        SetCellState(cell, data, TileState.Watered);
        return true;
    }

    private void SetCellState(Vector2Int cell, GridCellData data, TileState state)
    {
        data.state = state;

        // Tell the placed tile to switch its look.
        if (placed.TryGetValue(data.id, out PlacedObjectData po) &&
            po.instance != null &&
            po.instance.TryGetComponent(out GroundTile tile))
        {
            tile.SetState(state);
        }

        OnCellStateChanged?.Invoke(cell, state);
        RefreshStatus(cell);
    }

    public TileState GetState(Vector2Int cell)
    {
        if (!IsInsideGrid(cell)) return TileState.Untilled;
        return cells[cell.x, cell.y].state;
    }

    public bool IsTilled(Vector2Int cell)
    {
        if (!IsInsideGrid(cell)) return false;
        return cells[cell.x, cell.y].state == TileState.Tilled;
    }

    public bool IsWatered(Vector2Int cell)
    {
        if (!IsInsideGrid(cell)) return false;
        return cells[cell.x, cell.y].state == TileState.Watered;
    }

    // ---------------------------------------------------------------
    // Grid math
    // ---------------------------------------------------------------

    // Center of a single cell.
    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return GridToWorld(gridPos, Vector2Int.one);
    }

    // Center of a multi-cell object whose bottom-left cell is gridPos.
    public Vector3 GridToWorld(Vector2Int gridPos, Vector2Int size)
    {
        return new Vector3(
           Origin.x + gridPos.x * StepX + (size.x * cellSize) * 0.5f,
           Origin.y,
           Origin.z + gridPos.y * StepZ + (size.y * cellSize) * 0.5f
            );
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float localX = worldPos.x - Origin.x;
        float localZ = worldPos.z - Origin.z;

        int x = Mathf.FloorToInt(localX / StepX);
        int z = Mathf.FloorToInt(localZ / StepZ);

        // If the point fell in the gap between cells (past a cell's footprint),
        // report "no cell" so you can't plant in the spacing.
        if (localX - x * StepX > cellSize || localZ - z * StepZ > cellSize)
            return new Vector2Int(-1, -1);

        return new Vector2Int(x, z);
    }

    public bool IsInsideGrid(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < gridWidth &&
               gridPos.y >= 0 && gridPos.y < gridHeight;
    }

    public bool IsCellOccupied(Vector2Int gridPos)
    {
        if (!IsInsideGrid(gridPos)) return true;
        return cells[gridPos.x, gridPos.y].isOccupied;
    }

    public bool CanPlace(Vector2Int gridPos, Vector2Int size)
    {
        for (int x = 0; x < size.x; ++x)
        {
            for (int z = 0; z < size.y; ++z)
            {
                Vector2Int cell = new Vector2Int(gridPos.x + x, gridPos.y + z);
                if (!IsInsideGrid(cell) ||
                    !cells[cell.x, cell.y].isUnlocked ||   // can't build on locked ground
                    cells[cell.x, cell.y].isOccupied)
                    return false;
            }
        }
        return true;
    }

    public bool IsUnlocked(Vector2Int cell)
    {
        if (!IsInsideGrid(cell)) return false;
        return cells[cell.x, cell.y].isUnlocked;
    }

    // A locked cell that touches an unlocked one — i.e. the next ring you can buy.
    public bool IsPurchasable(Vector2Int cell)
    {
        if (!IsInsideGrid(cell)) return false;
        if (cells[cell.x, cell.y].isUnlocked) return false;

        return IsUnlocked(cell + Vector2Int.right) ||
               IsUnlocked(cell + Vector2Int.left) ||
               IsUnlocked(cell + Vector2Int.up) ||
               IsUnlocked(cell + Vector2Int.down);
    }

    public bool TryGetNextPurchasableCell(out Vector2Int cell)
    {
        for (int x = 0; x < gridWidth; ++x)
        {
            for (int z = 0; z < gridHeight; ++z)
            {
                Vector2Int c = new Vector2Int(x, z);
                if (IsPurchasable(c))
                {
                    cell = c;
                    return true;
                }
            }
        }

        cell = default;
        return false;
    }

    public bool Unlock(Vector2Int cell)
    {
        if (!IsPurchasable(cell)) return false;

        cells[cell.x, cell.y].isUnlocked = true;
        OnGridChanged?.Invoke();
        return true;
    }

    private void OccupyCells(Vector2Int gridPos, Vector2Int size, string id)
    {
        for (int x = 0; x < size.x; ++x)
        {
            for (int z = 0; z < size.y; ++z)
            {
                Vector2Int cell = new Vector2Int(gridPos.x + x, gridPos.y + z);
                if (IsInsideGrid(cell))
                {
                    cells[cell.x, cell.y].isOccupied = true;
                    cells[cell.x, cell.y].id = id;
                }
            }
        }
    }

    private void FreeCells(Vector2Int gridPos, Vector2Int size)
    {
        for (int x = 0; x < size.x; ++x)
        {
            for (int z = 0; z < size.y; ++z)
            {
                Vector2Int cell = new Vector2Int(gridPos.x + x, gridPos.y + z);
                if (IsInsideGrid(cell))
                {
                    cells[cell.x, cell.y].isOccupied = false;
                    cells[cell.x, cell.y].id = null;
                    cells[cell.x, cell.y].state = TileState.Untilled;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        for (int x = 0; x < gridWidth; ++x)
        {
            for (int z = 0; z < gridHeight; ++z)
            {
                Vector3 corner = Origin + new Vector3(x * StepX, 0f, z * StepZ);
                Vector3 a = corner;
                Vector3 b = corner + new Vector3(cellSize, 0f, 0f);
                Vector3 c = corner + new Vector3(cellSize, 0f, cellSize);
                Vector3 d = corner + new Vector3(0f, 0f, cellSize);
                Gizmos.DrawLine(a, b);
                Gizmos.DrawLine(b, c);
                Gizmos.DrawLine(c, d);
                Gizmos.DrawLine(d, a);
            }
        }
    }
    
    public CellStatus GetStatus(Vector2Int cell)
    {
        GridCellData data = cells[cell.x, cell.y];
        if (!data.isOccupied) return CellStatus.Empty;
        if (data.crop != null) return data.crop.IsGrown ? CellStatus.ReadyToHarvest : CellStatus.None;
        if (data.state == TileState.Tilled) return CellStatus.NeedsWater;
        return CellStatus.None;
    }

    // Updates the icon on the tile at this cell (does nothing if no tile is there).
    private void RefreshStatus(Vector2Int cell)
    {
        if (!IsInsideGrid(cell)) return;

        string id = cells[cell.x, cell.y].id;
        if (string.IsNullOrEmpty(id)) return;
        if (!placed.TryGetValue(id, out PlacedObjectData po) || po.instance == null) return;

        TileStatusIcon icon = po.instance.GetComponentInChildren<TileStatusIcon>(true);
        if (icon != null) icon.Show(GetStatus(cell));
    }
}

public enum TileState
{
    Untilled = 0,
    Tilled = 1,
    Watered = 2
}

public enum CellStatus
{
    None,
    Empty,
    NeedsWater,
    ReadyToHarvest,
}

public class GridCellData
{
    public bool isUnlocked;
    public bool isOccupied;
    public string id;
    public TileState state = TileState.Untilled;
    public Crop crop;
}

public class PlacedObjectData
{
    public string id;
    public Vector2Int gridPosition;
    public Vector2Int gridSize;
    public CropData crop;
    public GameObject instance;
}
