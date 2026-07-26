using UnityEngine;

[CreateAssetMenu(fileName = "Crop Inventory Item", menuName = "Item/Create New Item")]
public class InventoryItem : ScriptableObject
{
    public int id;
    public string itemName;
    public int value;
    public Sprite icon;

    [Tooltip("The crop this item plants when selected. Leave empty for items that " +
             "aren't seeds/plantable.")]
    public CropData crop;
}
