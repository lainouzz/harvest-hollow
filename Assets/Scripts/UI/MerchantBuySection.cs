using System.Collections.Generic;
using UnityEngine;

// The Buy list: a fixed set of items the merchant sells. Buying spends coins and
// deposits the items into the inventory. Put this on the BuySection object.
public class MerchantBuySection : MerchantSection
{
    [Header("Buy list")]
    [Tooltip("Items the merchant sells. Price comes from each item's `value`.")]
    [SerializeField] private List<InventoryItem> forSale = new();

    protected override IEnumerable<MerchantOffer> GetOffers()
    {
        foreach (InventoryItem item in forSale)
            if (item != null)
                yield return new MerchantOffer { item = item, unitPrice = item.value, maxQuantity = 0 };
    }

    public override void Confirm()
    {
        int total = GetTotal();
        if (total <= 0)
        {
            Debug.Log("Merchant: nothing selected to buy.");
            return;
        }

        if (inventory == null)
        {
            Debug.LogWarning("Merchant: no inventory found — can't deposit bought items.", this);
            return;
        }

        if (wallet == null || !wallet.TrySpend(total))
        {
            Debug.Log($"Merchant: not enough coins — need {total}, have {(wallet != null ? wallet.Coins : 0)}.");
            return;
        }

        foreach (MerchantItemRow row in rows)
        {
            if (row == null || row.Quantity <= 0) continue;

            inventory.Add(row.Item, row.Quantity);
            Debug.Log($"Bought {row.Quantity}x {row.Item.itemName} for {row.LineTotal}g.");
        }

        Debug.Log($"Merchant: purchase complete. Coins left: {wallet.Coins}.");
        ResetRows();
    }
}
