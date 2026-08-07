using System.Collections.Generic;
using UnityEngine;

// The Sell list: built from what the player owns. Selling removes items and pays out.
public class MerchantSellSection : MerchantSection
{
    [Header("Sell pricing")]
    [Tooltip("Payout as a fraction of value (0.5 = sell for half).")]
    [Range(0f, 1f)]
    [SerializeField] private float sellMultiplier = 0.5f;

    protected override void OnEnable()
    {
        base.OnEnable();

        // Keep the list in sync if the inventory changes while Sell is open.
        if (inventory != null) inventory.OnChanged += BuildRows;
    }

    private void OnDisable()
    {
        if (inventory != null) inventory.OnChanged -= BuildRows;
    }

    protected override IEnumerable<MerchantOffer> GetOffers()
    {
        if (inventory == null) yield break;

        foreach (InventoryManager.Stack stack in inventory.Stacks)
            if (stack != null && stack.item != null)
                yield return new MerchantOffer
                {
                    item = stack.item,
                    unitPrice = Mathf.RoundToInt(stack.item.value * sellMultiplier),
                    maxQuantity = stack.count,
                };
    }

    protected override void RecalculateTotal()
    {
        if (totalPriceText != null)
            totalPriceText.text = $"Total: {GetTotal()}g";
    }

    public override void Confirm()
    {
        int total = GetTotal();
        if (total <= 0)
        {
            Debug.Log("Merchant: nothing selected to sell.");
            return;
        }
        if (inventory == null) return;

        // Snapshot first — removing items fires OnChanged, which rebuilds the rows.
        var sales = new List<(InventoryItem item, int qty, int lineTotal)>();
        foreach (MerchantItemRow row in rows)
            if (row != null && row.Quantity > 0)
                sales.Add((row.Item, row.Quantity, row.LineTotal));

        inventory.OnChanged -= BuildRows;   // don't rebuild once per removal
        foreach (var sale in sales)
        {
            inventory.Remove(sale.item, sale.qty);
            Debug.Log($"Sold {sale.qty}x {sale.item.itemName} for {sale.lineTotal}g.");
        }
        inventory.OnChanged += BuildRows;

        if (wallet != null) wallet.Add(total);
        Debug.Log($"Merchant: sold for {total}g. Coins: {(wallet != null ? wallet.Coins : 0)}.");

        BuildRows();
    }
}
