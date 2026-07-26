using UnityEngine;

// One asset per crop type (Carrot, Wheat, ...). Create via
// Assets > Create > Farm > Crop Data, then drop it on the PlantingController.
[CreateAssetMenu(fileName = "Crop", menuName = "Farm/Crop Data")]
public class CropData : ScriptableObject
{
    public string cropName = "Crop";

    [Tooltip("Prefab that gets spawned when planted. Add a Crop component to it for growth.")]
    public GameObject prefab;

    [Tooltip("Icon shown in UI (shop rows, hotbar, inventory).")]
    public Sprite icon;

    [Tooltip("Footprint on the grid, in cells.")]
    public Vector2Int size = Vector2Int.one;

    // TODO: neither of these is read by anything yet — no money system exists.
    //       seedCost should be charged on plant, sellValue paid on sell (which,
    //       per the plan, happens when you sell to a person, not on the grid).
    [Header("Economy")]
    public int seedCost = 1;
    public int sellValue = 3;

    [Header("Growth")]
    [Tooltip("How many days from planting to fully grown (on unwatered soil).")]
    public int daysToGrow = 3;

    [Tooltip("How many items you get when harvesting this crop.")]
    public int baseYield = 1;

    [Tooltip("The inventory item this crop drops when harvested (baseYield-many of it).")]
    public InventoryItem produce;

    [Header("Watered soil bonus")]
    [Tooltip("Growth-per-day multiplier when planted on watered soil. 1 = normal, " +
             "1.5 = each day counts as 1.5 days of growth.")]
    public float wateredGrowthMultiplier = 1.5f;

    [Tooltip("Extra yield when planted on watered soil.")]
    public int wateredYieldBonus = 1;

    // Growth stage visuals live on the crop prefab's Crop component (`stages`),
    // since they're child models, not data.
}
