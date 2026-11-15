using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ToolSoundPlayer : MonoBehaviour
{
    [System.Serializable]
    public class ToolSound
    {
        public ItemsInHand tool;
        public List<AudioClip> clips = new();
    }

    [SerializeField] private ItemSwitcher itemSwitcher;
    [SerializeField] private PlayerLook player;
    [SerializeField] private AudioSource source;
    [SerializeField] private bool playOnAttack = true;
    [SerializeField] private List<ToolSound> toolSounds = new();

    private GameInput input;
    private WaterCan can;
    private int lastClipIndex = -1;

    private void Awake()
    {
        input = new GameInput();
        if (itemSwitcher == null) itemSwitcher = FindAnyObjectByType<ItemSwitcher>();
        if (player == null) player = FindAnyObjectByType<PlayerLook>();
        if (source == null) source = GetComponent<AudioSource>();
        can = FindFirstObjectByType<WaterCan>(FindObjectsInactive.Include);
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void Update()
    {
        if (!playOnAttack) return;
        if (Time.timeScale == 0f) return;

        bool inUI = player != null && player.isInShopUI;
        if (input.Player.Attack.WasPressedThisFrame() && !inUI)
            PlayCurrentToolSound();
    }

    public void PlayCurrentToolSound()
    {
        if (itemSwitcher == null || source == null) return;

        // An empty can plays its own "empty" sound instead of a splash.
        if (itemSwitcher.Current == ItemsInHand.WaterCan && can != null && can.IsEmpty) return;

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
