using UnityEngine;

public class SeedBag : MonoBehaviour
{
    [SerializeField] private Transform fullPos;
    [SerializeField] private Transform emptyPos;
    [SerializeField] private Transform seedPrefab;
    [SerializeField] private float slideSpeed = 2f;

    private InventoryManager inventory;
    private InventoryItem seedItem;
    private int seedFullCount;
    private Vector3 targetLocalPos;
    private bool wasVisible;

    private void Awake()
    {
        inventory = FindFirstObjectByType<InventoryManager>(FindObjectsInactive.Include);
        targetLocalPos = seedPrefab.localPosition;
    }

    private void OnEnable()
    {
        if (inventory == null) return;

        inventory.OnItemSelected += SetSeed;
        inventory.OnChanged += UpdateSeedAmount;

        SetSeed(inventory.SelectedItem);
    }

    private void OnDisable()
    {
        wasVisible = false;
        if (inventory == null) return;

        inventory.OnItemSelected -= SetSeed;
        inventory.OnChanged -= UpdateSeedAmount;
    }

    // LateUpdate so the slide wins over any bag animation on this object.
    // When the bag is taken out, snap straight to the right fill level: switching tools
    // resets the bag to its rest pose, and sliding from "full" every time would look odd.
    private void LateUpdate()
    {
        bool visible = seedPrefab.gameObject.activeInHierarchy;

        if (visible && !wasVisible)
            seedPrefab.localPosition = targetLocalPos;
        else
            seedPrefab.localPosition = Vector3.MoveTowards(
                seedPrefab.localPosition, targetLocalPos, slideSpeed * Time.deltaTime);

        wasVisible = visible;
    }

    private void SetSeed(InventoryItem item)
    {
        seedItem = item;
        seedFullCount = SeedCount();
        UpdateSeedAmount();
    }

    private void UpdateSeedAmount()
    {
        int count = SeedCount();

        if (count > seedFullCount) seedFullCount = count;

        float percentage = seedFullCount > 0 ? Mathf.Clamp01((float)count / seedFullCount) : 0f;
        targetLocalPos = Vector3.Lerp(emptyPos.localPosition, fullPos.localPosition, percentage);
    }

    private int SeedCount() =>
        (inventory != null && seedItem != null) ? inventory.GetItemCount(seedItem) : 0;
}
