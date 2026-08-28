using System;
using UnityEngine;


[RequireComponent(typeof(FarmGrid))]
public class FenceBuilder : MonoBehaviour
{
    [SerializeField] private GameObject fenceSegmentPrefab;
    [SerializeField] private int entranceStart = 0;

    private FarmGrid grid;
    private Transform root;

    private void Awake() => grid = GetComponent<FarmGrid>();
    private void OnEnable() => grid.OnGridChanged += Rebuild;
    private void OnDisable() => grid.OnGridChanged -= Rebuild;
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

                bool isEntrance = (z == entranceStart);

                if (!grid.IsUnlocked(cell + Vector2Int.right)) { SpawnFence(cell, Vector2Int.right); }
                if (!grid.IsUnlocked(cell + Vector2Int.left) && !isEntrance) { SpawnFence(cell, Vector2Int.left); }
                if (!grid.IsUnlocked(cell + Vector2Int.up)) { SpawnFence(cell, Vector2Int.up); }
                if (!grid.IsUnlocked(cell + Vector2Int.down)) { SpawnFence(cell, Vector2Int.down); }
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
