using UnityEngine;

// The visual for one ground cell. It reflects the cell's TileState — it does not
// own the state (the FarmGrid does).
//
// Assign the three visuals as CHILD objects of the tile, never the tile's own
// root: switching the root off would take the collider and this script with it.
public class GroundTile : MonoBehaviour
{
    [Header("Visuals — one child object per state")]
    [SerializeField] private GameObject untilledVisual;
    [SerializeField] private GameObject tilledVisual;
    [SerializeField] private GameObject wateredTilledVisual;

    private void Awake()
    {
        // Catches the "tile goes invisible" trap: toggling the root off kills
        // the whole tile, so refuse to treat it as a state visual.
        WarnIfRoot(untilledVisual, nameof(untilledVisual));
        WarnIfRoot(tilledVisual, nameof(tilledVisual));
        WarnIfRoot(wateredTilledVisual, nameof(wateredTilledVisual));
    }

    // TODO: Watered is a separate state here, so a watered tile is no longer
    //       "Tilled". It really wants to be tilled + a watered flag (a watered
    //       tile is still tilled). Revisit when the real watering logic lands.
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
