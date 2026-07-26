using UnityEngine;
using UnityEngine.UI;

// Switches the merchant between its Buy and Sell sections and tints the tab
// buttons so the player can see which one they're in. Put this on the MerchantUI
// panel (or any object active while the shop is open).
public class MerchantTabs : MonoBehaviour
{
    [Header("Tab buttons")]
    [SerializeField] private Button buyTab;
    [SerializeField] private Button sellTab;

    [Header("Sections to show/hide")]
    [SerializeField] private GameObject buySection;
    [SerializeField] private GameObject sellSection;

    [Header("Tab colors")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    // Remembers the last tab across opens (defaults to Buy the first time).
    private bool showingBuy = true;

    private void Awake()
    {
        if (buyTab != null) buyTab.onClick.AddListener(ShowBuy);
        if (sellTab != null) sellTab.onClick.AddListener(ShowSell);
    }

    private void OnEnable()
    {
        //SetTab(showingBuy);   // reopen on whichever tab the player left on
    }

    public void ShowBuy() => SetTab(true);
    public void ShowSell() => SetTab(false);

    private void SetTab(bool buy)
    {
        showingBuy = buy;

        //if (buySection != null) buySection.SetActive(buy);
        //if (sellSection != null) sellSection.SetActive(!buy);

        Tint(buyTab, buy);
        Tint(sellTab, !buy);
    }

    // Sets the button's base colors so the active tab reads as highlighted.
    // Works with the default "Color Tint" button transition.
    private void Tint(Button button, bool active)
    {
        if (button == null) return;

        Color c = active ? activeColor : inactiveColor;
        ColorBlock colors = button.colors;
        colors.normalColor = c;
        colors.highlightedColor = c;
        colors.selectedColor = c;
        button.colors = colors;
    }
}
