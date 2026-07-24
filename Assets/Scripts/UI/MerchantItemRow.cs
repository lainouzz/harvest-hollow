using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// One buyable row in the merchant's Buy list: icon, crop name, unit price, and a
// quantity stepper. Goes on the "Item" template under Content. MerchantShop clones
// one of these per crop.
public class MerchantItemRow : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text cropText;
    [SerializeField] private TMP_Text priceText;      // unit price
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Button incrementButton;
    [SerializeField] private Button decrementButton;

    public CropData Crop { get; private set; }
    public int Quantity { get; private set; }
    public int UnitPrice => Crop != null ? Crop.seedCost : 0;
    public int LineTotal => UnitPrice * Quantity;

    // Raised when +/- is pressed, so the shop can re-sum the total.
    public event Action OnQuantityChanged;

    private void Awake()
    {
        if (incrementButton != null) incrementButton.onClick.AddListener(() => ChangeQuantity(1));
        if (decrementButton != null) decrementButton.onClick.AddListener(() => ChangeQuantity(-1));
    }

    public void Setup(CropData crop)
    {
        Crop = crop;
        Quantity = 0;

        if (icon != null) icon.sprite = crop.icon;
        if (cropText != null) cropText.text = crop.cropName;
        if (priceText != null) priceText.text = $"{UnitPrice}G";

        Refresh();
    }

    public void ResetQuantity()
    {
        Quantity = 0;
        Refresh();
    }

    private void ChangeQuantity(int delta)
    {
        // TODO: clamp to a max (stock limit, or what the player can afford) when
        //       the real economy exists. For now you can add unlimited.
        Quantity = Mathf.Max(0, Quantity + delta);
        Refresh();
        OnQuantityChanged?.Invoke();
    }

    private void Refresh()
    {
        if (quantityText != null) quantityText.text = Quantity.ToString();
    }
}
