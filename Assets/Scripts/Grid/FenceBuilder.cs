using UnityEngine;

[RequireComponent(typeof(FarmGrid))]
public class FenceBuilder : MonoBehaviour
{
    [SerializeField] private GameObject fenceSegmentPrefab;

    private FarmGrid grid;
    private Transform root;

    private void Awake() => grid = GetComponent<FarmGrid>();

    private void OnEnable()
    {
        grid.OnGridChanged += Rebuild;
        grid.OnFenceChanged += Rebuild;
    }

    private void OnDisable()
    {
        grid.OnGridChanged -= Rebuild;
        grid.OnFenceChanged -= Rebuild;
    }

    private void Start() => Rebuild();

    private void Rebuild()
    {
        if (grid == null) grid = GetComponent<FarmGrid>();

        if (root != null) Destroy(root.gameObject);

        root = new GameObject("FenceRoot").transform;
        root.SetParent(transform, worldPositionStays: false);

        for (int x = 0; x < grid.Width; ++x)
        {
            for (int z = 0; z < grid.Height; ++z)
            {
                Vector2Int cell = new Vector2Int(x, z);
                if (!grid.IsUnlocked(cell)) continue;

                foreach (Vector2Int dir in FarmGrid.FenceSides)
                {
                    if (!grid.IsUnlocked(cell + dir) && !grid.IsFenceOpen(cell, dir))
                        SpawnFence(cell, dir);
                }
            }
        }
    }

    private void SpawnFence(Vector2Int cell, Vector2Int dir)
    {
        Vector3 center = grid.GridToWorld(cell);
        Vector3 worldDir = new Vector3(dir.x, 0, dir.y);

        Vector3 pos = center + worldDir * (grid.CellSize * 0.5f);
        Quaternion rot = Quaternion.LookRotation(worldDir);

        Instantiate(fenceSegmentPrefab, pos, rot, root);
    }
}
