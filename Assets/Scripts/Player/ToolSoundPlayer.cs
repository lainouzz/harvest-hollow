using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Plays a "use" sound for the currently held tool on left-click. Assign clips per
// tool below; a random one plays with slight pitch/volume variation.
public class ToolSoundPlayer : MonoBehaviour
{
    [System.Serializable]
    public class ToolSound
    {
        public ItemsInHand tool;
        public List<AudioClip> clips = new();
    }

    [SerializeField] private ItemSwitcher itemSwitcher;
    [SerializeField] private PlayerLook player;     // to skip playing while in a UI
    [SerializeField] private AudioSource source;
    [SerializeField] private bool playOnAttack = true;
    [SerializeField] private List<ToolSound> toolSounds = new();

    private GameInput input;
    private int lastClipIndex = -1;

    private void Awake()
    {
        input = new GameInput();
        if (itemSwitcher == null) itemSwitcher = FindAnyObjectByType<ItemSwitcher>();
        if (player == null) player = FindAnyObjectByType<PlayerLook>();
        if (source == null) source = GetComponent<AudioSource>();
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void Update()
    {
        if (!playOnAttack) return;
        if (Time.timeScale == 0f) return;   // game is paused

        bool inUI = player != null && player.isInShopUI;
        if (input.Player.Attack.WasPressedThisFrame() && !inUI)
            PlayCurrentToolSound();
    }

    // Public so it can also be called on a successful action instead of every click.
    public void PlayCurrentToolSound()
    {
        if (itemSwitcher == null || source == null) return;

        List<AudioClip> clips = ClipsFor(itemSwitcher.Current);
        if (clips == null || clips.Count == 0) return;

        int index = Random.Range(0, clips.Count);
        if (clips.Count > 1 && index == lastClipIndex)
            index = (index + 1) % clips.Count;
        lastClipIndex = index;

        source.pitch = Random.Range(0.95f, 1.05f);
        source.PlayOneShot(clips[index], Random.Range(0.9f, 1f));
    }

    private List<AudioClip> ClipsFor(ItemsInHand tool)
    {
        foreach (ToolSound ts in toolSounds)
            if (ts.tool == tool) return ts.clips;
        return null;
    }
}
