using System.Collections.Generic;
using UnityEngine;

// Plays the "you got something" feedback when a crop is harvested: a floating
// pickup popup and a pluck/pop sound. Driven by FarmGrid.OnHarvested, so it only
// fires on a real harvest, never on a missed swing.
public class HarvestFeedback : MonoBehaviour
{
    [SerializeField] private FarmGrid grid;

    [Header("Popup")]
    [SerializeField] private PickupPopup popupPrefab;
    [SerializeField] private float popupHeight = 0.8f;
    [Tooltip("{0} = amount, {1} = item name.")]
    [SerializeField] private string format = "+{0} {1}";

    [Header("Sound")]
    [SerializeField] private AudioSource source;
    [SerializeField] private List<AudioClip> popClips = new();
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.1f);

    private void Awake()
    {
        if (grid == null) grid = FindAnyObjectByType<FarmGrid>();
    }

    private void OnEnable()
    {
        if (grid != null) grid.OnHarvested += OnHarvested;
    }

    private void OnDisable()
    {
        if (grid != null) grid.OnHarvested -= OnHarvested;
    }

    private void OnHarvested(Vector2Int cell, InventoryItem item, int amount)
    {
        Vector3 pos = grid.GridToWorld(cell) + Vector3.up * popupHeight;

        if (popupPrefab != null && item != null)
        {
            PickupPopup popup = Instantiate(popupPrefab, pos, Quaternion.identity);
            popup.Show(string.Format(format, amount, item.itemName), item.icon);
        }

        if (source != null && popClips.Count > 0)
        {
            source.pitch = Random.Range(pitchRange.x, pitchRange.y);
            source.PlayOneShot(popClips[Random.Range(0, popClips.Count)]);
        }
    }
}
