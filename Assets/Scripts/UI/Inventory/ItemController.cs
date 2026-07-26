using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The view for one inventory row (on the "Item" prefab). References are assigned
// in the prefab, so there's no transform.Find at runtime.
public class ItemController : MonoBehaviour
{
    [Tooltip("The 'ItemName' text.")]
    [SerializeField] private TMP_Text nameText;
    [Tooltip("The 'ItemIcon' image.")]
    [SerializeField] private Image iconImage;
    [Tooltip("The 'RemoveButton'.")]
    [SerializeField] private Button removeButton;

    [Tooltip("The row's own button (clicking it selects the item). " +
             "Empty = a Button on this object.")]
    [SerializeField] private Button rowButton;

    public InventoryItem Item { get; private set; }

    public void Setup(InventoryItem item, int count, Action onSelect, Action onRemove)
    {
        Item = item;

        if (nameText != null)
            nameText.text = count > 1 ? $"{item.itemName} x{count}" : item.itemName;

        if (iconImage != null)
            iconImage.sprite = item.icon;

        // Clear listeners first — rows are rebuilt, don't stack them.
        if (rowButton == null) rowButton = GetComponent<Button>();
        if (rowButton != null)
        {
            rowButton.onClick.RemoveAllListeners();
            rowButton.onClick.AddListener(() => onSelect?.Invoke());
        }

        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(() => onRemove?.Invoke());
        }
    }
}
