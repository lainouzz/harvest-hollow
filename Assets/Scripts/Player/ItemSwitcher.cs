using System;
using UnityEngine;

public enum ItemsInHand
{
    Hoe = 0,
    WaterCan = 1,
    Seeds = 2,
    HarvestTool = 3,
    None = 4,
}

// Cycles which item is held. Put this on the "Items" object — its children are
// the item objects, in the same order as the ItemsInHand enum.
public class ItemSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject[] items;

    [SerializeField] private int currentIndex;

    public ItemsInHand Current => (ItemsInHand)currentIndex;

    // The active tool GameObject (the one currently shown in-hand), or null.
    public GameObject CurrentItemObject =>
        (items != null && currentIndex >= 0 && currentIndex < items.Length)
            ? items[currentIndex]
            : null;

    public event Action<ItemsInHand> OnItemChanged;

    private GameInput inputActions;

    // While Time.time is before this, switching is blocked (e.g. mid tool-swing).
    private float switchUnlockTime;

    public bool CanSwitch => Time.time >= switchUnlockTime;

    private void Awake()
    {
        inputActions = new GameInput();

        if (items == null || items.Length == 0)
        {
            items = new GameObject[transform.childCount];
            for (int i = 0; i < transform.childCount; ++i)
                items[i] = transform.GetChild(i).gameObject;
        }
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Start()
    {
        Equip(currentIndex);
    }

    // TODO: add number-key hotbar bindings (1/2/3/4 -> Equip(0..3)) and/or scroll
    //       wheel. Cycling is fine for 4 items, worse once there are more.
    private void Update()
    {
        // Can't switch mid-animation, so the tool anim never gets cut off / stuck.
        if (!CanSwitch) return;

        if (inputActions.Player.Hotbar1.WasPressedThisFrame())
            SelectSlot(0);
        if (inputActions.Player.Hotbar2.WasPressedThisFrame())
            SelectSlot(1);
        if (inputActions.Player.Hotbar3.WasPressedThisFrame())
            SelectSlot(2);
        if (inputActions.Player.Hotbar4.WasPressedThisFrame())
            SelectSlot(3);
        if (inputActions.Player.Hotbar5.WasPressedThisFrame())
            SelectSlot(4);
    }

    // Blocks switching for `seconds`. Overlapping locks extend, never shorten,
    // so a second use mid-swing pushes the unlock time out rather than back.
    public void LockSwitching(float seconds)
    {
        switchUnlockTime = Mathf.Max(switchUnlockTime, Time.time + seconds);
    }

    private void SelectSlot(int index)
    {
        if (index >= items.Length)
            return;

        Equip(index);//
    }

    public void Equip(int index)
    {
        if (items.Length == 0) return;

        currentIndex = Mathf.Clamp(index, 0, items.Length - 1);

        for (int i = 0; i < items.Length; ++i)
        {
            if (items[i] != null)
                items[i].SetActive(i == currentIndex);
        }

        OnItemChanged?.Invoke(Current);
    }
}
