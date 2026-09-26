using UnityEngine;

public class GroundTile : MonoBehaviour
{
    [Header("Visuals — one child object per state")]
    [SerializeField] private GameObject untilledVisual;
    [SerializeField] private GameObject tilledVisual;
    [SerializeField] private GameObject wateredTilledVisual;

    private void Awake()
    {
        WarnIfRoot(untilledVisual, nameof(untilledVisual));
        WarnIfRoot(tilledVisual, nameof(tilledVisual));
        WarnIfRoot(wateredTilledVisual, nameof(wateredTilledVisual));
    }

    public void SetState(TileState state)
    {
        SetVisual(untilledVisual, state == TileState.Untilled);
        SetVisual(tilledVisual, state == TileState.Tilled);
        SetVisual(wateredTilledVisual, state == TileState.Watered);
    }

    private void SetVisual(GameObject visual, bool active)
    {
        if (visual == null || visual == gameObject) return;
        visual.SetActive(active);
    }

    private void WarnIfRoot(GameObject visual, string fieldName)
    {
        if (visual == gameObject)
            Debug.LogError($"GroundTile: '{fieldName}' is set to the tile's own root. " +
                           "Point it at a child object instead — it will be ignored.", this);
    }
}
