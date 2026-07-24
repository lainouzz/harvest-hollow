using UnityEngine;
using UnityEngine.InputSystem;

// Plays the "use" animation of whatever tool is currently held.
//
// Each tool GameObject (the children ItemSwitcher toggles) should have its own
// Animator with a trigger named to match `useTrigger` (default "Use"). This
// tracks which tool is active and fires that trigger on its Animator.
//
// Two ways to drive it:
//   - Leave `playOnAttack` on and it plays whenever you left-click (self-contained).
//   - Turn it off and call PlayUse() yourself (e.g. from PlantingController, so the
//     swing only plays when an action actually succeeds).
public class ToolAnimator : MonoBehaviour
{
    [Tooltip("Which ItemSwitcher's held tool to animate. Empty = find one in the scene.")]
    [SerializeField] private ItemSwitcher itemSwitcher;

    [Tooltip("Trigger parameter on each tool's Animator that plays its use animation.")]
    [SerializeField] private string useTrigger = "Use";

    [Tooltip("Play the use animation automatically on left-click.")]
    [SerializeField] private bool playOnAttack = true;

    [Tooltip("Seconds to block tool-switching while the use animation plays. " +
             "Set this to (roughly) your longest use clip's length.")]
    [SerializeField] private float useDuration = 0.5f;

    private GameInput input;
    private Animator currentAnimator;

    private void Awake()
    {
        input = new GameInput();
        if (itemSwitcher == null) itemSwitcher = FindAnyObjectByType<ItemSwitcher>();
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
        if (playOnAttack && input.Player.Attack.WasPressedThisFrame())
            PlayUse();
    }

    // Plays the current tool's use animation. Safe to call when nothing (or a
    // tool without an Animator, like "None") is held — it just does nothing.
    public void PlayUse()
    {
        if (currentAnimator == null) return;

        currentAnimator.SetTrigger(useTrigger);

        // Block switching until the swing finishes, so it can't be cut off.
        if (itemSwitcher != null)
            itemSwitcher.LockSwitching(useDuration);
    }

    private void OnItemChanged(ItemsInHand item)
    {
        RefreshCurrentAnimator();
    }

    // Grabs the Animator off whichever tool is now in hand.
    private void RefreshCurrentAnimator()
    {
        GameObject tool = itemSwitcher != null ? itemSwitcher.CurrentItemObject : null;
        currentAnimator = tool != null ? tool.GetComponentInChildren<Animator>() : null;
    }
}
