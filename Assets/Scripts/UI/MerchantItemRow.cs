using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

// One row in a merchant list: icon, name, unit price, and a quantity stepper.
// Works for both Buy and Sell — the section decides the price and the max.
public class MerchantItemRow : MonoBehaviour
{
    [SerializeField] private Image icon;
    [FormerlySerializedAs("cropText")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;      // unit price
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Button incrementButton;
    [SerializeField] private Button decrementButton;

    public InventoryItem Item { get; private set; }
    public int Quantity { get; private set; }
    public int UnitPrice { get; private set; }
    public int LineTotal => UnitPrice * Quantity;

    private int maxQuantity;   // <= 0 means unlimited (buying)

    // Raised when +/- is pressed, so the section can re-sum the total.
    public event Action OnQuantityChanged;

    private void Awake()
    {
        if (incrementButton != null) incrementButton.onClick.AddListener(() => ChangeQuantity(1));
        if (decrementButton != null) decrementButton.onClick.AddListener(() => ChangeQuantity(-1));
    }

    public void Setup(InventoryItem item, int unitPrice, int maxQuantity)
    {
        Item = item;
        UnitPrice = unitPrice;
        this.maxQuantity = maxQuantity;
        Quantity = 0;

        if (icon != null) icon.sprite = item.icon;
        if (nameText != null) nameText.text = item.itemName;
        if (priceText != null) priceText.text = $"{unitPrice}G";

        Refresh();
    }

    public void ResetQuantity()
    {
        Quantity = 0;
        Refresh();
    }

    private void ChangeQuantity(int delta)
    {
        int q = Mathf.Max(0, Quantity + delta);
        if (maxQuantity > 0) q = Mathf.Min(q, maxQuantity);   // can't sell more than you own

        if (q == Quantity) return;   // hit a limit — nothing changed

        Quantity = q;
        Refresh();
        OnQuantityChanged?.Invoke();
    }

    private void Refresh()
    {
        if (quantityText != null) quantityText.text = Quantity.ToString();
    }
}
