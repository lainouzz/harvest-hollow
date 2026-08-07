using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// Holds the player's items as stacks (item + count) and rebuilds the UI list on change.
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Serializable]
    public class Stack
    {
        public InventoryItem item;
        public int count;
    }

    [FormerlySerializedAs("itemConent")]
    [SerializeField] private Transform itemContent;
    [FormerlySerializedAs("inventoryItem")]
    [SerializeField] private GameObject itemPrefab;

    [Tooltip("Items the player starts with (e.g. Potato x10, Wheat x5).")]
    [SerializeField] private List<Stack> startingItems = new();

    private readonly List<Stack> stacks = new();
    public IReadOnlyList<Stack> Stacks => stacks;

    public event Action OnChanged;
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
        foreach (Stack s in startingItems)
            if (s != null) Add(s.item, s.count);
    }

    public void Add(InventoryItem item, int count = 1)
    {
        if (item == null || count <= 0) return;

        Stack stack = GetStack(item);
        if (stack != null) stack.count += count;
        else stacks.Add(new Stack { item = item, count = count });

        Changed();
    }

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

    public void Refresh()
    {
        if (itemContent == null || itemPrefab == null) return;

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
