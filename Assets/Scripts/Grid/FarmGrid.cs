using System;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Fence")]
    [Tooltip("Fence sides that start open, e.g. the farm entrance.")]
    [SerializeField] private List<FenceOpening> startingOpenings = new()
    {
        new FenceOpening { cell = Vector2Int.zero, side = FenceSide.Left }
    };

    private GridCellData[,] cells;

    // Each open fence side is stored once, as the right or top edge of a cell, so
    // the same gap is found whichever side of the fence you look at it from.
    private readonly HashSet<Vector3Int> fenceOpenings = new();

    private readonly Dictionary<string, PlacedObjectData> placed = new();

    private readonly List<Crop> plantedCrops = new();
    public IReadOnlyList<Crop> PlantedCrops => plantedCrops;

    private Wallet wallet;

    public Vector3 Origin => transform.position;
    public int Width => gridWidth;
    public int Height => gridHeight;
    public float CellSize => cellSize;
    
    public float StepX => cellSize + cellSpacing.x;
    public float StepZ => cellSize + cellSpacing.z;

    public event Action<Vector2Int, TileState> OnCellStateChanged;

    public event Action OnGridChanged;

    public event Action OnFenceChanged;

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

        fenceOpenings.Clear();
        foreach (FenceOpening o in startingOpenings)
            fenceOpenings.Add(EdgeKey(o.cell, SideToDir(o.side)));
    }

    public bool TryPlant(Vector2Int cell, CropData crop)
    {
        if (crop == null || crop.prefab == null) return false;
        if (!CanPlace(cell, crop.size)) return false;

        Vector3 worldPos = GridToWorld(cell, crop.size);
        GameObject instance = Instantiate(crop.prefab, worldPos, Quaternion.Euler(-90, 0, 0), transform);
        
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

    public bool TryPlantSeed(Vector2Int cell, CropData crop)
    {
        if (crop == null || crop.prefab == null) return false;
        if (!CanPlantSeed(cell)) return false;

        bool watered = cells[cell.x, cell.y].state == TileState.Watered;
        float growthMultiplier = watered ? crop.wateredGrowthMultiplier : 1f;
        int bonusYield = watered ? crop.wateredYieldBonus : 0;

        SpawnCrop(cell, crop, growthMultiplier, bonusYield);
        return true;
    }

    private Crop SpawnCrop(Vector2Int cell, CropData crop, float growthMultiplier, int bonusYield)
    {
        Vector3 worldPos = GridToWorld(cell, crop.size) + Vector3.up * 0.1f;
        GameObject instance = Instantiate(crop.prefab, worldPos, Quaternion.Euler(-90, 0, 0), transform);

        if (!instance.TryGetComponent(out Crop cropComponent))
            cropComponent = instance.AddComponent<Crop>();

        cropComponent.Initialize(crop, cell, growthMultiplier, bonusYield);

        cells[cell.x, cell.y].crop = cropComponent;
        plantedCrops.Add(cropComponent);
        cropComponent.OnGrown.AddListener(() => RefreshStatus(cell));
        RefreshStatus(cell);
        return cropComponent;
    }

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

    public bool HarvestAt(Vector2Int cell)
    {
        if (!CanHarvest(cell)) return false;

        GridCellData data = cells[cell.x, cell.y];
        Crop crop = data.crop;

        Debug.Log($"Harvested {crop.Data.cropName} x{crop.Yield}!");

        if (InventoryManager.Instance != null && crop.Data.produce != null)
            InventoryManager.Instance.Add(crop.Data.produce, crop.Yield);

        plantedCrops.Remove(crop);
        Destroy(crop.gameObject);
        data.crop = null;
        RefreshStatus(cell);

        if (wallet == null) wallet = FindAnyObjectByType<Wallet>();
        if (wallet != null) wallet.Add(10);

        return true;
    }

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

    public bool TryTill(Vector2Int cell)
    {
        if (!IsInsideGrid(cell)) return false;

        GridCellData data = cells[cell.x, cell.y];
        if (!data.isOccupied) return false;
        if (data.state != TileState.Untilled) return false;

        SetCellState(cell, data, TileState.Tilled);
        return true;
    }

    public bool TryWater(Vector2Int cell)
    {
        if (!IsInsideGrid(cell)) return false;

        GridCellData data = cells[cell.x, cell.y];
        if (!data.isOccupied) return false;
        if (data.state != TileState.Tilled) return false;

        SetCellState(cell, data, TileState.Watered);
        return true;
    }

    private void SetCellState(Vector2Int cell, GridCellData data, TileState state)
    {
        data.state = state;

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

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return GridToWorld(gridPos, Vector2Int.one);
    }

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
                    !cells[cell.x, cell.y].isUnlocked ||
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

        // Any opening this plot swallowed is now inside the farm, so forget it.
        foreach (Vector2Int dir in FenceSides)
            if (!IsFencePerimeter(cell, dir)) fenceOpenings.Remove(EdgeKey(cell, dir));

        OnGridChanged?.Invoke();
        return true;
    }

    public static readonly Vector2Int[] FenceSides =
        { Vector2Int.right, Vector2Int.up, Vector2Int.left, Vector2Int.down };

    // A fence side is one where the farm meets the outside: exactly one of the two
    // cells it separates is unlocked.
    public bool IsFencePerimeter(Vector2Int cell, Vector2Int dir) =>
        IsUnlocked(cell) != IsUnlocked(cell + dir);

    public bool IsFenceOpen(Vector2Int cell, Vector2Int dir) =>
        fenceOpenings.Contains(EdgeKey(cell, dir));

    public bool SetFenceOpen(Vector2Int cell, Vector2Int dir, bool open)
    {
        if (!IsFencePerimeter(cell, dir)) return false;

        Vector3Int key = EdgeKey(cell, dir);
        bool changed = open ? fenceOpenings.Add(key) : fenceOpenings.Remove(key);
        if (changed) OnFenceChanged?.Invoke();
        return changed;
    }

    private static Vector3Int EdgeKey(Vector2Int cell, Vector2Int dir)
    {
        if (dir == Vector2Int.left) return new Vector3Int(cell.x - 1, cell.y, 0);
        if (dir == Vector2Int.down) return new Vector3Int(cell.x, cell.y - 1, 1);
        return new Vector3Int(cell.x, cell.y, dir == Vector2Int.right ? 0 : 1);
    }

    private static Vector2Int SideToDir(FenceSide side) => side switch
    {
        FenceSide.Right => Vector2Int.right,
        FenceSide.Up => Vector2Int.up,
        FenceSide.Left => Vector2Int.left,
        _ => Vector2Int.down
    };

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
    
    public void CaptureState(SaveData save)
    {
        save.unlockedCells.Clear();
        save.tiles.Clear();
        save.tileStates.Clear();
        save.crops.Clear();
        save.fenceOpenings.Clear();

        foreach (Vector3Int key in fenceOpenings)
            save.fenceOpenings.Add(new EdgeSave { x = key.x, z = key.y, axis = key.z });

        for (int x = 0; x < gridWidth; ++x)
        {
            for (int z = 0; z < gridHeight; ++z)
            {
                GridCellData c = cells[x, z];

                if (c.isUnlocked)
                    save.unlockedCells.Add(new CellSave { x = x, z = z });

                if (c.isOccupied && c.state != TileState.Untilled)
                    save.tileStates.Add(new StateSave { x = x, z = z, state = (int)c.state });

                if (c.crop != null && c.crop.Data != null)
                {
                    save.crops.Add(new CropSave
                    {
                        x = x,
                        z = z,
                        crop = c.crop.Data.name,
                        grownDays = c.crop.GrownDays,
                        growthPerDay = c.crop.GrowthPerDay,
                        bonusYield = c.crop.BonusYield
                    });
                }
            }
        }

        foreach (PlacedObjectData po in placed.Values)
        {
            if (po.crop != null)
                save.tiles.Add(new TileSave { x = po.gridPosition.x, z = po.gridPosition.y, tile = po.crop.name });
        }
    }

    // Rebuilds the farm from a save: unlocked plots first, then tiles, then their
    // tilled/watered state, then the crops growing on them (order matters — each
    // step needs the previous one in place).
    public void RestoreState(SaveData save, SaveDatabase db)
    {
        ClearAll();

        if (save.unlockedCells.Count > 0)
        {
            foreach (GridCellData c in cells) c.isUnlocked = false;

            foreach (CellSave c in save.unlockedCells)
                if (IsInsideGrid(new Vector2Int(c.x, c.z))) cells[c.x, c.z].isUnlocked = true;
        }

        if (save.fenceOpenings != null)
        {
            fenceOpenings.Clear();
            foreach (EdgeSave e in save.fenceOpenings)
                fenceOpenings.Add(new Vector3Int(e.x, e.z, e.axis));
        }

        foreach (TileSave t in save.tiles)
            TryPlant(new Vector2Int(t.x, t.z), db.FindCrop(t.tile));

        foreach (StateSave s in save.tileStates)
        {
            Vector2Int cell = new Vector2Int(s.x, s.z);
            if (!IsInsideGrid(cell) || !cells[s.x, s.z].isOccupied) continue;

            SetCellState(cell, cells[s.x, s.z], (TileState)s.state);
        }

        foreach (CropSave c in save.crops)
        {
            Vector2Int cell = new Vector2Int(c.x, c.z);
            CropData data = db.FindCrop(c.crop);
            if (data == null || data.prefab == null || !CanPlantSeed(cell)) continue;

            Crop crop = SpawnCrop(cell, data, c.growthPerDay, c.bonusYield);
            crop.RestoreGrowth(c.grownDays);
            RefreshStatus(cell);
        }

        OnGridChanged?.Invoke();
    }

    private void ClearAll()
    {
        foreach (Crop crop in plantedCrops)
            if (crop != null) Destroy(crop.gameObject);
        plantedCrops.Clear();

        foreach (PlacedObjectData po in placed.Values)
            if (po.instance != null) Destroy(po.instance);
        placed.Clear();

        foreach (GridCellData c in cells)
        {
            c.isOccupied = false;
            c.id = null;
            c.state = TileState.Untilled;
            c.crop = null;
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

public enum FenceSide { Right, Up, Left, Down }

[Serializable]
public struct FenceOpening
{
    public Vector2Int cell;
    public FenceSide side;
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
