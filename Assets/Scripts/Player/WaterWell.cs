using TMPro;
using UnityEngine;

// Stand by the well and press Interact (E) to fill the watering can. Put this on a
// trigger collider around the well, with the prompt as a separate UI object.
public class WaterWell : MonoBehaviour
{
    [Tooltip("The whole prompt (key cap + text). Shown while you're in range.")]
    [SerializeField] private GameObject prompt;
    [SerializeField] private TMP_Text promptLabel;
    [SerializeField] private string refillText = "Refill watering can";
    [SerializeField] private string fullText = "Watering can is full";

    private GameInput input;
    private WaterCan can;
    private PlayerLook playerLook;
    private bool playerInZone;
    private bool promptShown = true;   // so the first SetPrompt(false) actually hides it

    private void Awake()
    {
        input = new GameInput();
        can = FindFirstObjectByType<WaterCan>(FindObjectsInactive.Include);
        playerLook = FindAnyObjectByType<PlayerLook>();
        SetPrompt(false);
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void Update()
    {
        bool blocked = Time.timeScale == 0f || (playerLook != null && playerLook.isInShopUI);
        bool show = playerInZone && !blocked && can != null;

        SetPrompt(show);
        if (!show) return;

        if (promptLabel != null) promptLabel.text = can.IsFull ? fullText : refillText;

        if (!can.IsFull && input.Player.Interact.WasPressedThisFrame())
            can.Refill();
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
