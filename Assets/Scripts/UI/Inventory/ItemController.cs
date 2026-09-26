using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemController : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button removeButton;
    [Tooltip("Clicking this selects the item. Empty = a Button on this object.")]
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
