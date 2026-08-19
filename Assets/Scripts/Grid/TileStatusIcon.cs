using UnityEngine;

// Shows a status icon above one ground tile. Put this on the tile prefab (as a
// child that faces the camera) and assign the icon objects. FarmGrid tells it
// which status to show.
public class TileStatusIcon : MonoBehaviour
{
    [SerializeField] private GameObject empty;
    [SerializeField] private GameObject needsWater;
    [SerializeField] private GameObject readyToHarvest;

    public void Show(CellStatus status)
    {
        if (empty != null) empty.SetActive(status == CellStatus.Empty);
        if (needsWater != null) needsWater.SetActive(status == CellStatus.NeedsWater);
        if (readyToHarvest != null) readyToHarvest.SetActive(status == CellStatus.ReadyToHarvest);
    }
}
