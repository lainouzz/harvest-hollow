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

    public GameObject CurrentItemObject =>
        (items != null && currentIndex >= 0 && currentIndex < items.Length)
            ? items[currentIndex]
            : null;

    public event Action<ItemsInHand> OnItemChanged;

    private GameInput inputActions;

    private float switchUnlockTime;

    // Every transform under each item, with its local pose from before any animation played.
    private Transform[][] restTransforms;
    private (Vector3 pos, Quaternion rot, Vector3 scale)[][] restPoses;

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

        CaptureRestPoses();
    }

    private void CaptureRestPoses()
    {
        restTransforms = new Transform[items.Length][];
        restPoses = new (Vector3, Quaternion, Vector3)[items.Length][];

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null) continue;

            restTransforms[i] = items[i].GetComponentsInChildren<Transform>(true);
            restPoses[i] = new (Vector3, Quaternion, Vector3)[restTransforms[i].Length];
            for (int t = 0; t < restTransforms[i].Length; t++)
            {
                Transform tr = restTransforms[i][t];
                restPoses[i][t] = (tr.localPosition, tr.localRotation, tr.localScale);
            }
        }
    }

    // An Animator takes whatever pose its object is in when it's switched on as its
    // "default" — and the empty Idle state keeps writing that pose. A tool switched off
    // mid-swing would come back frozen mid-swing, so put it back at rest first.
    private void RestorePose(int i)
    {
        if (restTransforms[i] == null) return;

        for (int t = 0; t < restTransforms[i].Length; t++)
        {
            Transform tr = restTransforms[i][t];
            if (tr == null) continue;
            (Vector3 pos, Quaternion rot, Vector3 scale) = restPoses[i][t];
            tr.SetLocalPositionAndRotation(pos, rot);
            tr.localScale = scale;
        }
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Start()
    {
        Equip(currentIndex);
    }

    private void Update()
    {
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

        Equip(index);
    }

    public void Equip(int index)
    {
        if (items.Length == 0) return;

        currentIndex = Mathf.Clamp(index, 0, items.Length - 1);

        for (int i = 0; i < items.Length; ++i)
        {
            if (items[i] == null) continue;

            bool show = i == currentIndex;
            if (show && !items[i].activeSelf) RestorePose(i);
            items[i].SetActive(show);
        }

        OnItemChanged?.Invoke(Current);
    }
}
