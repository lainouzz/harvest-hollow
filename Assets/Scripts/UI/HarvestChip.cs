using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One "icon  Potato ×12" chip on the end-of-day summary.
public class HarvestChip : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text label;

    public void Set(InventoryItem item, int amount)
    {
        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.enabled = item.icon != null;
        }

        if (label != null) label.text = $"{item.itemName} <b>×{amount}</b>";
    }
}
