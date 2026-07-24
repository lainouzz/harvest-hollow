using UnityEngine;

// Draws the grid lines at runtime so the board is visible in Play mode,
// not just via the Scene-view gizmo. Attach next to a FarmGrid.
[RequireComponent(typeof(FarmGrid))]
public class GridVisualizer : MonoBehaviour
{
    [SerializeField] private Color lineColor = new Color(1f, 1f, 1f, 0.6f);
    [Tooltip("Outline colour for locked cells you can buy (the expansion 'ghosts').")]
    [SerializeField] private Color purchasableColor = new Color(1f, 0.85f, 0.2f, 0.35f);
    [SerializeField] private float lineWidth = 0.03f;
    [SerializeField] private float yOffset = 0.01f; // lift lines off the ground to avoid z-fighting

    private FarmGrid grid;
    private Transform linesRoot;
    private Material lineMaterial;

    private void Awake()
    {
        grid = GetComponent<FarmGrid>();
    }

    private void OnEnable()
    {
        if (grid == null) grid = GetComponent<FarmGrid>();
        grid.OnGridChanged += Rebuild;   // redraw when a slot is bought
    }

    private void OnDisable()
    {
        grid.OnGridChanged -= Rebuild;
    }

    private void Start()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        if (grid == null) grid = GetComponent<FarmGrid>();

        if (linesRoot != null)
            Destroy(linesRoot.gameObject);

        linesRoot = new GameObject("GridLines").transform;
        linesRoot.SetParent(transform, worldPositionStays: false);
        linesRoot.localPosition = new Vector3(0f, yOffset, 0f);

        if (lineMaterial == null)
            lineMaterial = new Material(Shader.Find("Sprites/Default"));

        float cs = grid.CellSize;

        // Draw unlocked cells solid, and buyable cells as faint "ghost" outlines.
        for (int x = 0; x < grid.Width; ++x)
        {
            for (int z = 0; z < grid.Height; ++z)
            {
                Vector2Int cell = new Vector2Int(x, z);

                Color color;
                if (grid.IsUnlocked(cell)) color = lineColor;
                else if (grid.IsPurchasable(cell)) color = purchasableColor;
                else continue;   // fully locked & not adjacent — don't draw

                Vector3 corner = new Vector3(x * grid.StepX, 0f, z * grid.StepZ);
                CreateCell(corner, cs, color);
            }
        }
    }

    private void CreateCell(Vector3 corner, float cs, Color color)
    {
        var go = new GameObject("Cell");
        go.transform.SetParent(linesRoot, worldPositionStays: false);

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.material = lineMaterial;
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.numCapVertices = 2;
        lr.loop = true;
        lr.positionCount = 4;
        lr.SetPosition(0, corner);
        lr.SetPosition(1, corner + new Vector3(cs, 0f, 0f));
        lr.SetPosition(2, corner + new Vector3(cs, 0f, cs));
        lr.SetPosition(3, corner + new Vector3(0f, 0f, cs));
    }
}
