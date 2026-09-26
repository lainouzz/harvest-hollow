using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public struct MerchantOffer
{
    public InventoryItem item;
    public int unitPrice;
    public int maxQuantity;
}

public abstract class MerchantSection : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] protected Transform contentParent;
    [SerializeField] protected MerchantItemRow itemTemplate;
    [SerializeField] protected TMP_Text totalPriceText;
    [Tooltip("BuyItemsButton / SellItemsButton.")]
    [SerializeField] protected Button confirmButton;
    [SerializeField] protected Wallet wallet;

    protected InventoryManager inventory;
    protected readonly List<MerchantItemRow> rows = new();

    protected virtual void Awake()
    {
        if (itemTemplate != null) itemTemplate.gameObject.SetActive(false);
        if (confirmButton != null) confirmButton.onClick.AddListener(Confirm);
        if (wallet == null) wallet = FindAnyObjectByType<Wallet>();

        // Include inactive so it works even if the inventory panel is hidden.
        inventory = FindFirstObjectByType<InventoryManager>(FindObjectsInactive.Include);
    }

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

    protected abstract IEnumerable<MerchantOffer> GetOffers();
    public abstract void Confirm();
}
