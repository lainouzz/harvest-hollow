using UnityEngine;

[CreateAssetMenu(fileName = "Crop Inventory Item", menuName = "Item/Create New Item")]
public class InventoryItem : ScriptableObject
{
    public int id;
    public string itemName;
    public int value;
    public Sprite icon;

    [Tooltip("The crop this item plants. Empty for non-plantable items.")]
    public CropData crop;
}
