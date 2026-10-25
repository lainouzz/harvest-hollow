using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [SerializeField] private SaveDatabase database;

    [Header("Found automatically if left empty")]
    [SerializeField] private FarmGrid grid;
    [SerializeField] private FarmExpansion expansion;
    [SerializeField] private Wallet wallet;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (grid == null) grid = FindAnyObjectByType<FarmGrid>();
        if (expansion == null) expansion = FindAnyObjectByType<FarmExpansion>();
        if (wallet == null) wallet = FindAnyObjectByType<Wallet>();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Wait one frame so every other Start() has run first (the inventory hands out
    // starting items and the wallet announces its default coins there) — loading
    // after that overwrites them cleanly instead of racing them.
    private IEnumerator Start()
    {
        yield return null;
        Load();
    }

    public void Save()
    {
        if (database == null)
        {
            Debug.LogError("SaveManager: no SaveDatabase assigned, can't save.", this);
            return;
        }

        SaveData save = new SaveData();

        if (DayManager.Instance != null) save.day = DayManager.Instance.CurrentDay;
        if (wallet != null) save.coins = wallet.Coins;
        if (expansion != null) save.slotsBought = expansion.SlotsBought;

        if (InventoryManager.Instance != null)
        {
            foreach (InventoryManager.Stack s in InventoryManager.Instance.Stacks)
                if (s.item != null) save.items.Add(new ItemSave { item = s.item.name, count = s.count });
        }

        if (grid != null) grid.CaptureState(save);

        SaveSystem.Write(save);
        Debug.Log($"Game saved (day {save.day}).");
    }

    public void Load()
    {
        if (database == null || !SaveSystem.TryRead(out SaveData save)) return;

        if (wallet != null) wallet.SetCoins(save.coins);
        if (expansion != null) expansion.SlotsBought = save.slotsBought;

        if (InventoryManager.Instance != null)
        {
            List<InventoryManager.Stack> stacks = new();
            foreach (ItemSave i in save.items)
            {
                InventoryItem item = database.FindItem(i.item);
                if (item != null) stacks.Add(new InventoryManager.Stack { item = item, count = i.count });
            }
            InventoryManager.Instance.LoadStacks(stacks);
        }

        if (grid != null) grid.RestoreState(save, database);

        // Last, so anything that resets on "day loaded" (like DayStats) sees the restored coins.
        if (DayManager.Instance != null) DayManager.Instance.SetDay(save.day);

        Debug.Log($"Game loaded (day {save.day}).");
    }
}
