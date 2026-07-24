using UnityEngine;
using UnityEngine.UI;

// Highlights the hotbar slot for the currently held item. Put this on the "Slots"
// object (the one with the Grid Layout Group) — it uses each slot's background
// Image. Slot order must match the ItemsInHand enum / ItemSwitcher item order.
public class HotbarHUD : MonoBehaviour
{
    private enum HighlightStyle { Color, Sprite }

    [SerializeField] private ItemSwitcher itemSwitcher;

    [Tooltip("Slot background Images, in item order. Empty = auto-fill from the " +
             "direct children of Slots Parent (their Image, not the Icon child).")]
    [SerializeField] private Image[] slotBackgrounds;

    [Tooltip("Where to auto-fill slots from. Empty = this object.")]
    [SerializeField] private Transform slotsParent;

    [SerializeField] private HighlightStyle style = HighlightStyle.Color;

    [Header("Color style")]
    [Tooltip("Selected slot = its normal colour multiplied by this (1 = same, 0.8 = a bit darker).")]
    [Range(0f, 1f)]
    [SerializeField] private float highlightDarken = 0.8f;

    [Header("Sprite style")]
    [Tooltip("Sprite shown on the selected slot. Unselected slots keep their normal sprite.")]
    [SerializeField] private Sprite highlightedSprite;

    // Captured at startup so unselected slots can be restored exactly.
    private Color[] normalColors;
    private Sprite[] normalSprites;

    private void Awake()
    {
        if (itemSwitcher == null) itemSwitcher = FindAnyObjectByType<ItemSwitcher>();
        if (slotsParent == null) slotsParent = transform;

        AutoFillSlots();
        CaptureNormals();
    }

    private void OnEnable()
    {
        if (itemSwitcher != null)
            itemSwitcher.OnItemChanged += OnItemChanged;
    }

    private void OnDisable()
    {
        if (itemSwitcher != null)
            itemSwitcher.OnItemChanged -= OnItemChanged;
    }

    private void Start()
    {
        // Reflect whatever's already equipped.
        if (itemSwitcher != null)
            Highlight((int)itemSwitcher.Current);
    }

    private void OnItemChanged(ItemsInHand item)
    {
        Highlight((int)item);
    }

    private void Highlight(int selectedIndex)
    {
        if (slotBackgrounds == null) return;

        for (int i = 0; i < slotBackgrounds.Length; ++i)
        {
            Image slot = slotBackgrounds[i];
            if (slot == null) continue;

            bool selected = i == selectedIndex;

            if (style == HighlightStyle.Color)
            {
                slot.color = selected ? normalColors[i] * new Color(highlightDarken, highlightDarken, highlightDarken, 1f)
                                      : normalColors[i];
            }
            else // Sprite
            {
                slot.sprite = selected && highlightedSprite != null ? highlightedSprite : normalSprites[i];
            }
        }
    }

    private void AutoFillSlots()
    {
        if (slotBackgrounds != null && slotBackgrounds.Length > 0) return;

        slotBackgrounds = new Image[slotsParent.childCount];
        for (int i = 0; i < slotsParent.childCount; ++i)
            slotBackgrounds[i] = slotsParent.GetChild(i).GetComponent<Image>();
    }

    private void CaptureNormals()
    {
        normalColors = new Color[slotBackgrounds.Length];
        normalSprites = new Sprite[slotBackgrounds.Length];

        for (int i = 0; i < slotBackgrounds.Length; ++i)
        {
            if (slotBackgrounds[i] == null) continue;
            normalColors[i] = slotBackgrounds[i].color;
            normalSprites[i] = slotBackgrounds[i].sprite;
        }
    }
}
