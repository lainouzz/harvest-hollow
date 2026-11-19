using TMPro;
using UnityEngine;

// Stand by the coop and press Interact (E): collects waiting eggs first, otherwise
// buys a chicken. Put this on a trigger collider next to the coop's nest box.
public class CoopPrompt : MonoBehaviour
{
    [SerializeField] private ChickenCoop coop;

    [Tooltip("The whole prompt (key cap + text). Shown while you're in range.")]
    [SerializeField] private GameObject prompt;
    [SerializeField] private TMP_Text label;
    [SerializeField] private string collectFormat = "Collect {0} egg{1}";
    [SerializeField] private string buyFormat = "Buy chicken · {0}g ({1}/{2})";
    [SerializeField] private string poorFormat = "Need {0}g · you have {1}g";
    [SerializeField] private string fullText = "No eggs yet · come back tomorrow";
    [SerializeField] private Color normalColor = new Color(1f, 0.945f, 0.816f);
    [SerializeField] private Color poorColor = new Color(0.95f, 0.64f, 0.54f);

    private GameInput input;
    private Wallet wallet;
    private PlayerLook playerLook;
    private bool playerInZone;
    private bool promptShown = true;   // so the first SetPrompt(false) actually hides it

    private void Awake()
    {
        input = new GameInput();
        if (coop == null) coop = GetComponentInParent<ChickenCoop>();
        wallet = FindAnyObjectByType<Wallet>();
        playerLook = FindAnyObjectByType<PlayerLook>();
        SetPrompt(false);
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void Update()
    {
        bool blocked = Time.timeScale == 0f || (playerLook != null && playerLook.isInShopUI);
        bool show = playerInZone && !blocked && coop != null && wallet != null;

        SetPrompt(show);
        if (!show) return;

        bool pressed = input.Player.Interact.WasPressedThisFrame();

        if (coop.EggsWaiting > 0)
        {
            SetLabel(string.Format(collectFormat, coop.EggsWaiting, coop.EggsWaiting == 1 ? "" : "s"), normalColor);
            if (pressed) coop.CollectEggs();
        }
        else if (!coop.IsFull)
        {
            bool canAfford = wallet.CanAfford(coop.Price);
            SetLabel(canAfford
                    ? string.Format(buyFormat, coop.Price, coop.ChickenCount, coop.MaxChickens)
                    : string.Format(poorFormat, coop.Price, wallet.Coins),
                canAfford ? normalColor : poorColor);
            if (pressed && canAfford) coop.TryBuyChicken(wallet);
        }
        else
        {
            SetLabel(fullText, normalColor);
        }
    }

    private void SetLabel(string text, Color color)
    {
        if (label == null) return;
        label.text = text;
        label.color = color;
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
