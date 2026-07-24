using UnityEngine;

// TODO: this whole component currently does NOTHING. `tiles` is never filled (the
//       old Spawn() is gone), so HandleCellStateChanged always early-outs. Tilling
//       works because FarmGrid.TryTill talks to the placed tile's GroundTile directly.
//       Decide: delete this, or bring back per-cell tile spawning (spawn a tile for
//       every cell on Start) and drop the "left-click places a tile" step.
[RequireComponent (typeof(FarmGrid))]
public class GroundTileChanger : MonoBehaviour
{
    [SerializeField] private GroundTile tilePrefab;
    [Tooltip("Lifts tiles slightly so they don't z-fight with the ground plane.")]
    [SerializeField] private float yOffset = 0f;

    private FarmGrid grid;
    private GroundTile[,] tiles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        grid = GetComponent<FarmGrid>();
    }

    private void OnEnable()
    {
        if (grid == null) grid = GetComponent<FarmGrid>();
        grid.OnCellStateChanged += HandleCellStateChanged;
    }

    private void HandleCellStateChanged(Vector2Int cell, TileState state)
    {
        if (tiles == null || !grid.IsInsideGrid(cell)) return;

        GroundTile tile = tiles[cell.x, cell.y];
        if (tile != null) tile.SetState(state);
    }

    private void OnDisable()
    {
        grid.OnCellStateChanged -= HandleCellStateChanged;
    }
}
