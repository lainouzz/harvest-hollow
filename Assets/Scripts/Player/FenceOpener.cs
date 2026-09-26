using UnityEngine;
using UnityEngine.InputSystem;

// Aim at a fence and press the key to open a gap in it; aim at the ground right
// next to a gap and press it again to close it. Put this on the player.
public class FenceOpener : MonoBehaviour
{
    [SerializeField] private FarmGrid grid;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerLook playerLook;
    [SerializeField] private Key toggleKey = Key.X;
    [SerializeField] private float maxReach = 6f;

    [Tooltip("What the aim ray can hit. Exclude anything held in front of the camera.")]
    [SerializeField] private LayerMask aimMask = ~0;

    [Tooltip("How far from a cell's center (as a fraction of the cell size) you must aim " +
             "for it to count as aiming at that cell's edge rather than its middle.")]
    [SerializeField, Range(0.1f, 0.5f)] private float edgeZone = 0.3f;

    private void Awake()
    {
        if (grid == null) grid = FindAnyObjectByType<FarmGrid>();
        if (playerCamera == null) playerCamera = Camera.main;
        if (playerLook == null) playerLook = FindAnyObjectByType<PlayerLook>();
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current[toggleKey].wasPressedThisFrame) return;
        if (Time.timeScale == 0f) return;
        if (playerLook != null && playerLook.isInShopUI) return;

        if (!TryGetAimedEdge(out Vector2Int cell, out Vector2Int dir)) return;

        bool open = !grid.IsFenceOpen(cell, dir);
        grid.SetFenceOpen(cell, dir, open);
        Debug.Log(open ? $"Opened fence at {cell} {dir}." : $"Closed fence at {cell} {dir}.");
    }

    private bool TryGetAimedEdge(out Vector2Int cell, out Vector2Int dir)
    {
        cell = default;
        dir = default;
        if (grid == null || playerCamera == null) return false;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxReach, aimMask, QueryTriggerInteraction.Ignore))
            return false;

        // Nearest cell center (works outside the grid too, so you can aim from outside the fence).
        Vector3 local = hit.point - grid.Origin;
        float half = grid.CellSize * 0.5f;
        cell = new Vector2Int(
            Mathf.RoundToInt((local.x - half) / grid.StepX),
            Mathf.RoundToInt((local.z - half) / grid.StepZ));

        Vector3 center = grid.GridToWorld(cell);
        float dx = hit.point.x - center.x;
        float dz = hit.point.z - center.z;

        if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dz)) < grid.CellSize * edgeZone)
            return false;

        if (Mathf.Abs(dx) > Mathf.Abs(dz))
            dir = dx > 0f ? Vector2Int.right : Vector2Int.left;
        else
            dir = dz > 0f ? Vector2Int.up : Vector2Int.down;

        return grid.IsFencePerimeter(cell, dir);
    }
}
