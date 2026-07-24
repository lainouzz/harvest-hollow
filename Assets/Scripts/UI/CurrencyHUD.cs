using UnityEngine;
using TMPro;

// Shows the player's current money in the always-on HUD. Put it on the currency
// text object (top-left). It updates whenever the Wallet changes.
public class CurrencyHUD : MonoBehaviour
{
    [Tooltip("Where the money lives. Empty = find the Wallet in the scene.")]
    [SerializeField] private Wallet wallet;

    [Tooltip("The text to write into. Empty = a TMP_Text on this object.")]
    [SerializeField] private TMP_Text currencyText;

    [Tooltip("{0} = the coin amount.")]
    [SerializeField] private string format = "{0}g";

    private void Awake()
    {
        if (wallet == null) wallet = FindAnyObjectByType<Wallet>();
        if (currencyText == null) currencyText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (wallet == null) return;

        wallet.OnCoinsChanged += UpdateDisplay;
        UpdateDisplay(wallet.Coins);   // show the current amount right away
    }

    private void OnDisable()
    {
        if (wallet != null)
            wallet.OnCoinsChanged -= UpdateDisplay;
    }

    private void UpdateDisplay(int coins)
    {
        if (currencyText != null)
            currencyText.text = string.Format(format, coins);
    }
}
