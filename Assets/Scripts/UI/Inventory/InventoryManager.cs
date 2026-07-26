using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Holds the player's items as stacks (item + count) and rebuilds the UI list
// whenever it changes.
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Serializable]
    public class Stack
    {
        public InventoryItem item;
        public int count;
    }

    [Tooltip("The 'Content' object under the viewport (has the grid layout).")]
    [FormerlySerializedAs("itemConent")]
    [SerializeField] private Transform itemContent;

    [Tooltip("The 'Item' row prefab — must have an ItemController.")]
    [FormerlySerializedAs("inventoryItem")]
    [SerializeField] private GameObject itemPrefab;

    [Tooltip("Items the player starts the game with (e.g. Potato x10, Wheat x5).")]
    [SerializeField] private List<Stack> startingItems = new();

    private readonly List<Stack> stacks = new();
    public IReadOnlyList<Stack> Stacks => stacks;

    // Fired whenever the contents change (so a HUD count, etc. can react too).
    public event Action OnChanged;

    // Fired when the player clicks a row (e.g. to pick a seed to plant).
    public event Action<InventoryItem> OnItemSelected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        // Give the player their starting items.
        foreach (Stack s in startingItems)
            if (s != null) Add(s.item, s.count);
    }

    // Adds `count` of an item, stacking onto an existing entry if present.
    public void Add(InventoryItem item, int count = 1)
    {
        if (item == null || count <= 0) return;

        Stack stack = GetStack(item);
        if (stack != null) stack.count += count;
        else stacks.Add(new Stack { item = item, count = count });

        Changed();
    }

    // Removes `count` of an item; drops the stack entirely when it hits zero.
    public void Remove(InventoryItem item, int count = 1)
    {
        if (item == null || count <= 0) return;

        Stack stack = GetStack(item);
        if (stack == null) return;

        stack.count -= count;
        if (stack.count <= 0) stacks.Remove(stack);

        Changed();
    }

    public int CountOf(InventoryItem item)
    {
        Stack stack = GetStack(item);
        return stack != null ? stack.count : 0;
    }

    private Stack GetStack(InventoryItem item)
    {
        foreach (Stack s in stacks)
            if (s.item == item) return s;
        return null;
    }

    private void Changed()
    {
        OnChanged?.Invoke();
        Refresh();
    }

    // Rebuilds the whole list UI from the current stacks.
    public void Refresh()
    {
        if (itemContent == null || itemPrefab == null) return;

        // Clear old rows first (the original bug: rows kept piling up).
        for (int i = itemContent.childCount - 1; i >= 0; --i)
            Destroy(itemContent.GetChild(i).gameObject);

        foreach (Stack stack in stacks)
        {
            GameObject obj = Instantiate(itemPrefab, itemContent);

            if (obj.TryGetComponent(out ItemController row))
            {
                Stack captured = stack;   // capture for the button callbacks
                row.Setup(
                    captured.item,
                    captured.count,
                    onSelect: () => OnItemSelected?.Invoke(captured.item),
                    onRemove: () => Remove(captured.item, captured.count));
            }
        }
    }
}
