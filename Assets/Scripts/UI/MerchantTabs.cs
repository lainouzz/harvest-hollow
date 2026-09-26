using UnityEngine;
using UnityEngine.UI;

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

    private bool showingBuy = true;

    private void Awake()
    {
        if (buyTab != null) buyTab.onClick.AddListener(ShowBuy);
        if (sellTab != null) sellTab.onClick.AddListener(ShowSell);
    }

    public void ShowBuy() => SetTab(true);
    public void ShowSell() => SetTab(false);

    private void SetTab(bool buy)
    {
        showingBuy = buy;

        Tint(buyTab, buy);
        Tint(sellTab, !buy);
    }

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
