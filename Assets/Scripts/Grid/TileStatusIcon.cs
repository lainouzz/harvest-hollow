using UnityEngine;

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
