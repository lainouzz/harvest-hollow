using System;
using System.Collections.Generic;
using UnityEngine;

public enum CropTier { Starter, Common, Premium }

[Serializable]
public class BuyableCrop
{
    public InventoryItem item;
    public CropTier tier = CropTier.Common;
}

public class MerchantBuySection : MerchantSection
{
    [Header("Catalog")]
    [Tooltip("Everything the merchant *can* sell, each tagged with a tier.")]
    [SerializeField] private List<BuyableCrop> catalog = new();

    [Header("Stock chance per tier (0-1)")]
    [Range(0f, 1f)] [SerializeField] private float starterChance = 1f;
    [Range(0f, 1f)] [SerializeField] private float commonChance = 1f;
    [Range(0f, 1f)] [SerializeField] private float premiumChance = 0.4f;

    private readonly List<InventoryItem> currentStock = new();
    private int lastRestockDay = int.MinValue;

    protected override void OnEnable()
    {
        // "The car arrived" = a new day. Reopening the same day keeps the same
        // stock (no reroll-until-premium exploit).
        int day = DayManager.Instance != null ? DayManager.Instance.CurrentDay : 0;
        if (day != lastRestockDay)
        {
            lastRestockDay = day;
            RollStock();
        }

        base.OnEnable();
    }

    public void Restock()
    {
        RollStock();
        if (isActiveAndEnabled) BuildRows();
    }

    private void RollStock()
    {
        currentStock.Clear();
        foreach (BuyableCrop entry in catalog)
        {
            if (entry.item == null) continue;
            if (UnityEngine.Random.value <= ChanceFor(entry.tier))
                currentStock.Add(entry.item);
        }
    }

    private float ChanceFor(CropTier tier) => tier switch
    {
        CropTier.Starter => starterChance,
        CropTier.Common => commonChance,
        CropTier.Premium => premiumChance,
        _ => 1f,
    };

    protected override IEnumerable<MerchantOffer> GetOffers()
    {
        foreach (InventoryItem item in currentStock)
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
