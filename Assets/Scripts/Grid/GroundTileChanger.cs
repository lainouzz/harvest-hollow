using UnityEngine;

[RequireComponent (typeof(FarmGrid))]
public class GroundTileChanger : MonoBehaviour
{
    [SerializeField] private GroundTile tilePrefab;
    [Tooltip("Lifts tiles slightly so they don't z-fight with the ground plane.")]
    [SerializeField] private float yOffset = 0f;

    private FarmGrid grid;
    private GroundTile[,] tiles;

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
