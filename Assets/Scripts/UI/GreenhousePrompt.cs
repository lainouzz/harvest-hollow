using TMPro;
using UnityEngine;

// Stand at the greenhouse door and press Interact (E) to restore it. Put this on a
// trigger collider in front of the door, with the prompt as a separate UI object.
public class GreenhousePrompt : MonoBehaviour
{
    [SerializeField] private Greenhouse greenhouse;

    [Tooltip("The whole prompt (key cap + text). Shown while you're in range.")]
    [SerializeField] private GameObject prompt;
    [SerializeField] private TMP_Text label;
    [SerializeField] private string buyFormat = "Restore greenhouse · {0}g";
    [SerializeField] private string poorFormat = "Need {0}g · you have {1}g";
    [SerializeField] private Color buyColor = new Color(1f, 0.945f, 0.816f);
    [SerializeField] private Color poorColor = new Color(0.95f, 0.64f, 0.54f);

    private GameInput input;
    private Wallet wallet;
    private PlayerLook playerLook;
    private bool playerInZone;
    private bool promptShown = true;   // so the first SetPrompt(false) actually hides it

    private void Awake()
    {
        input = new GameInput();
        if (greenhouse == null) greenhouse = FindFirstObjectByType<Greenhouse>(FindObjectsInactive.Include);
        wallet = FindAnyObjectByType<Wallet>();
        playerLook = FindAnyObjectByType<PlayerLook>();
        SetPrompt(false);
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void Update()
    {
        bool blocked = Time.timeScale == 0f || (playerLook != null && playerLook.isInShopUI);
        bool show = playerInZone && !blocked && greenhouse != null && wallet != null && !greenhouse.IsOwned;

        SetPrompt(show);
        if (!show) return;

        int price = greenhouse.Price;
        bool canAfford = wallet.CanAfford(price);
        if (label != null)
        {
            label.text = canAfford ? string.Format(buyFormat, price) : string.Format(poorFormat, price, wallet.Coins);
            label.color = canAfford ? buyColor : poorColor;
        }

        if (canAfford && input.Player.Interact.WasPressedThisFrame())
            greenhouse.TryPurchase(wallet.TrySpend);
    }

    private void SetPrompt(bool show)
    {
        if (show == promptShown) return;
        promptShown = show;
        if (prompt != null) prompt.SetActive(show);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInZone = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInZone = false;
    }
}
