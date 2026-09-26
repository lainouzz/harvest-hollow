using System.Collections.Generic;
using UnityEngine;

// Lists every CropData / InventoryItem the save file can refer to, so a saved name
// can be turned back into the real asset. Asset names must be unique.
[CreateAssetMenu(fileName = "SaveDatabase", menuName = "Farm/Save Database")]
public class SaveDatabase : ScriptableObject
{
    [SerializeField] private List<CropData> crops = new();
    [SerializeField] private List<InventoryItem> items = new();

    public CropData FindCrop(string cropName) => Find(crops, cropName);
    public InventoryItem FindItem(string itemName) => Find(items, itemName);

    private static T Find<T>(List<T> list, string assetName) where T : Object
    {
        if (string.IsNullOrEmpty(assetName)) return null;

        foreach (T asset in list)
            if (asset != null && asset.name == assetName) return asset;

        Debug.LogWarning($"SaveDatabase: no {typeof(T).Name} named '{assetName}'. " +
                         "Was it renamed, or is it missing from the database?");
        return null;
    }

#if UNITY_EDITOR
    [ContextMenu("Auto-fill from project")]
    private void AutoFill()
    {
        crops = LoadAll<CropData>();
        items = LoadAll<InventoryItem>();
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"SaveDatabase: found {crops.Count} crops and {items.Count} items.");
    }

    private static List<T> LoadAll<T>() where T : Object
    {
        List<T> result = new();
        foreach (string guid in UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            result.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path));
        }
        return result;
    }
#endif
}
