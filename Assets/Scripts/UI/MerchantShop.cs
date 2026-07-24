using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Fills the merchant's Buy list from a set of crops, tracks quantities/total, and
// does a (fake) purchase. Put this on MerchantUI (or the BuySection object).
//
// Selling isn't here yet — you can't store crops until the inventory exists, so
// this is buy-only and just spends fake money.
public class MerchantShop : MonoBehaviour
{
    [Header("What's for sale")]
    [SerializeField] private List<CropData> buyableCrops = new();

    [Header("Scene references")]
    [Tooltip("The 'Content' object (with the Grid Layout Group). Rows spawn under it.")]
    [SerializeField] private Transform contentParent;

    [Tooltip("The 'Item' template row. It gets cloned per crop and hidden itself.")]
    [SerializeField] private MerchantItemRow itemTemplate;

    [Tooltip("The 'TotalPrice' text at the bottom.")]
    [SerializeField] private TMP_Text totalPriceText;

    [Tooltip("Button that confirms the purchase (e.g. BuyItemsButton).")]
    [SerializeField] private Button buyButton;

    [Tooltip("Where the money comes from. Empty = find the Wallet in the scene.")]
    [SerializeField] private Wallet wallet;

    // TODO: bought seeds should go into the inventory in BuyAll() once it exists.

    private readonly List<MerchantItemRow> rows = new();

    private void Awake()
    {
        if (itemTemplate != null) itemTemplate.gameObject.SetActive(false);
        if (buyButton != null) buyButton.onClick.AddListener(BuyAll);
        if (wallet == null) wallet = FindAnyObjectByType<Wallet>();
    }

    // Rebuild each time the shop opens, so the list always matches buyableCrops.
    private void OnEnable()
    {
        BuildRows();
    }

    private void BuildRows()
    {
        if (itemTemplate == null || contentParent == null) return;

        foreach (MerchantItemRow row in rows)
            if (row != null) Destroy(row.gameObject);
        rows.Clear();

        foreach (CropData crop in buyableCrops)
        {
            if (crop == null) continue;

            MerchantItemRow row = Instantiate(itemTemplate, contentParent);
            row.gameObject.SetActive(true);
            row.Setup(crop);
            row.OnQuantityChanged += RecalculateTotal;
            rows.Add(row);
        }

        RecalculateTotal();
    }

    private int GetTotal()
    {
        int total = 0;
        foreach (MerchantItemRow row in rows)
            if (row != null) total += row.LineTotal;
        return total;
    }

    private void RecalculateTotal()
    {
        if (totalPriceText != null)
            totalPriceText.text = $"Total price: {GetTotal()}g";
    }

    // Fake buy: spends the money and logs what was bought. No inventory yet.
    public void BuyAll()
    {
        int total = GetTotal();
        if (total <= 0)
        {
            Debug.Log("Merchant: nothing selected to buy.");
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

            Debug.Log($"Bought {row.Quantity}x {row.Crop.cropName} for {row.LineTotal}g.");
            // TODO: add row.Quantity of row.Crop (as seeds) to the inventory here.
        }

        Debug.Log($"Merchant: purchase complete. Coins left: {wallet.Coins}.");

        foreach (MerchantItemRow row in rows)
            if (row != null) row.ResetQuantity();

        RecalculateTotal();
    }
}
