using TMPro;
using UnityEngine;

// Shows "[B] Buy plot · 15g" under the crosshair while you're aiming at a plot you can
// buy, or "Need 15g · you have 9g" when you can't afford it.
public class BuyPlotPrompt : MonoBehaviour
{
    [SerializeField] private FarmExpansion expansion;
    [SerializeField] private FarmGrid grid;
    [SerializeField] private Wallet wallet;

    [Tooltip("The whole prompt (key cap + text). Shown and hidden as a unit.")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text label;
    [SerializeField] private string buyFormat = "Buy plot · {0}g";
    [SerializeField] private string poorFormat = "Need {0}g · you have {1}g";
    [SerializeField] private Color buyColor = new Color(1f, 0.945f, 0.816f);
    [SerializeField] private Color poorColor = new Color(0.95f, 0.64f, 0.54f);

    private bool shown = true;   // so the first SetShown(false) in Awake actually hides it
    private int shownCost = -1;
    private int shownCoins = -1;

    private void Awake()
    {
        if (expansion == null) expansion = FindAnyObjectByType<FarmExpansion>();
        if (grid == null) grid = FindAnyObjectByType<FarmGrid>();
        if (wallet == null) wallet = FindAnyObjectByType<Wallet>();
        SetShown(false);
    }

    private void Update()
    {
        bool aimingAtPlot = expansion != null && grid != null && wallet != null
                            && expansion.CanBuyNow
                            && expansion.TryGetLookedAtCell(out Vector2Int cell)
                            && grid.IsPurchasable(cell);

        SetShown(aimingAtPlot);
        if (!aimingAtPlot || label == null) return;

        int cost = expansion.NextSlotCost;
        int coins = wallet.Coins;
        if (cost == shownCost && coins == shownCoins) return;   // only rebuild the text when it changes

        shownCost = cost;
        shownCoins = coins;

        bool canAfford = wallet.CanAfford(cost);
        label.text = canAfford ? string.Format(buyFormat, cost) : string.Format(poorFormat, cost, coins);
        label.color = canAfford ? buyColor : poorColor;
    }

    private void SetShown(bool show)
    {
        if (show == shown) return;
        shown = show;
        if (root != null) root.SetActive(show);
    }
}
