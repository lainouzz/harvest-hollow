using UnityEngine;
using UnityEngine.InputSystem;

public class ToolAnimator : MonoBehaviour
{
    [SerializeField] private ItemSwitcher itemSwitcher;
    [SerializeField] private PlayerLook player;
    [Tooltip("Trigger on each tool's Animator that plays its use animation.")]
    [SerializeField] private string useTrigger = "Use";
    [SerializeField] private bool playOnAttack = true;
    [Tooltip("Seconds to block switching while it plays (~your longest use clip).")]
    [SerializeField] private float useDuration = 0.5f;

    private GameInput input;
    private Animator currentAnimator;

    private void Awake()
    {
        input = new GameInput();
        if (itemSwitcher == null) itemSwitcher = FindAnyObjectByType<ItemSwitcher>();
        if (player == null) player = FindAnyObjectByType<PlayerLook>();
    }

    private void OnEnable()
    {
        input.Enable();

        if (itemSwitcher != null)
            itemSwitcher.OnItemChanged += OnItemChanged;

        RefreshCurrentAnimator();
    }

    private void OnDisable()
    {
        input.Disable();

        if (itemSwitcher != null)
            itemSwitcher.OnItemChanged -= OnItemChanged;
    }

    private void Update()
    {
        if (playOnAttack && input.Player.Attack.WasPressedThisFrame() && !player.isInShopUI)
            PlayUse();
    }

    public void PlayUse()
    {
        if (currentAnimator == null) return;

        currentAnimator.SetTrigger(useTrigger);

        if (itemSwitcher != null)
            itemSwitcher.LockSwitching(useDuration);
    }

    private void OnItemChanged(ItemsInHand item)
    {
        RefreshCurrentAnimator();
    }

    private void RefreshCurrentAnimator()
    {
        GameObject tool = itemSwitcher != null ? itemSwitcher.CurrentItemObject : null;
        currentAnimator = tool != null ? tool.GetComponentInChildren<Animator>() : null;
    }
}
