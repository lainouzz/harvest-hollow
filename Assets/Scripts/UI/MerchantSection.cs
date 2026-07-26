using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// One thing the merchant offers: an item, its unit price, and how many are
// available (<= 0 = unlimited, used for buying).
public struct MerchantOffer
{
    public InventoryItem item;
    public int unitPrice;
    public int maxQuantity;
}

// Shared logic for a merchant list section (Buy or Sell): spawns a row per offer,
// tracks the total, and confirms. Subclasses supply the offers and the action.
public abstract class MerchantSection : MonoBehaviour
{
    [Header("Scene references")]
    [Tooltip("The 'Content' object (with the Grid Layout Group). Rows spawn under it.")]
    [SerializeField] protected Transform contentParent;

    [Tooltip("The 'Item' template row. It gets cloned per offer and hidden itself.")]
    [SerializeField] protected MerchantItemRow itemTemplate;

    [Tooltip("The 'TotalPrice' text at the bottom of this section.")]
    [SerializeField] protected TMP_Text totalPriceText;

    [Tooltip("Button that confirms this section (BuyItemsButton / SellItemsButton).")]
    [SerializeField] protected Button confirmButton;

    [Tooltip("Empty = find the Wallet in the scene.")]
    [SerializeField] protected Wallet wallet;

    protected InventoryManager inventory;
    protected readonly List<MerchantItemRow> rows = new();

    protected virtual void Awake()
    {
        if (itemTemplate != null) itemTemplate.gameObject.SetActive(false);
        if (confirmButton != null) confirmButton.onClick.AddListener(Confirm);
        if (wallet == null) wallet = FindAnyObjectByType<Wallet>();

        // Found including inactive so it works even if the inventory panel is hidden.
        inventory = FindFirstObjectByType<InventoryManager>(FindObjectsInactive.Include);
    }

    // Rebuild whenever the section is shown, so it matches the current data.
    protected virtual void OnEnable()
    {
        BuildRows();
    }

    protected void BuildRows()
    {
        if (itemTemplate == null || contentParent == null) return;

        foreach (MerchantItemRow row in rows)
            if (row != null) Destroy(row.gameObject);
        rows.Clear();

        foreach (MerchantOffer offer in GetOffers())
        {
            if (offer.item == null) continue;

            MerchantItemRow row = Instantiate(itemTemplate, contentParent);
            row.gameObject.SetActive(true);
            row.Setup(offer.item, offer.unitPrice, offer.maxQuantity);
            row.OnQuantityChanged += RecalculateTotal;
            rows.Add(row);
        }

        RecalculateTotal();
    }

    protected int GetTotal()
    {
        int total = 0;
        foreach (MerchantItemRow row in rows)
            if (row != null) total += row.LineTotal;
        return total;
    }

    protected virtual void RecalculateTotal()
    {
        if (totalPriceText != null)
            totalPriceText.text = $"Total price: {GetTotal()}g";
    }

    protected void ResetRows()
    {
        foreach (MerchantItemRow row in rows)
            if (row != null) row.ResetQuantity();
        RecalculateTotal();
    }

    // What this section lists.
    protected abstract IEnumerable<MerchantOffer> GetOffers();

    // What the confirm button does. Public so it can also be wired in the inspector.
    public abstract void Confirm();
}
