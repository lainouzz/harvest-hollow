using UnityEngine;
using TMPro;

public class CurrencyHUD : MonoBehaviour
{
    [SerializeField] private Wallet wallet;
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
        UpdateDisplay(wallet.Coins);
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
