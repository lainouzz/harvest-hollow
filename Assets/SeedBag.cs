using UnityEngine;

// Slides the seed mesh in the bag between "full" and "empty". "Full" is however
// many of the selected seed you had when you picked it, so the bag starts full on
// selection and drains as you plant.
public class SeedBag : MonoBehaviour
{
    [SerializeField] private Transform fullPos;
    [SerializeField] private Transform emptyPos;
    [SerializeField] private Transform seedPrefab;
    [SerializeField] private float slideSpeed = 2f;

    private InventoryManager inventory;
    private InventoryItem seedItem;
    private int seedFullCount;          // count captured when this seed was picked
    private Vector3 targetLocalPos;

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
        if (inventory == null) return;

        inventory.OnItemSelected -= SetSeed;
        inventory.OnChanged -= UpdateSeedAmount;
    }

    // LateUpdate so the slide wins over any bag animation on this object.
    private void LateUpdate()
    {
        seedPrefab.localPosition = Vector3.MoveTowards(
            seedPrefab.localPosition, targetLocalPos, slideSpeed * Time.deltaTime);
    }

    // A new seed is picked -> its current count becomes "full".
    private void SetSeed(InventoryItem item)
    {
        seedItem = item;
        seedFullCount = SeedCount();
        UpdateSeedAmount();
    }

    private void UpdateSeedAmount()
    {
        int count = SeedCount();

        // If you bought more, top the reference up so it still reads full.
        if (count > seedFullCount) seedFullCount = count;

        float percentage = seedFullCount > 0 ? Mathf.Clamp01((float)count / seedFullCount) : 0f;
        targetLocalPos = Vector3.Lerp(emptyPos.localPosition, fullPos.localPosition, percentage);
    }

    private int SeedCount() =>
        (inventory != null && seedItem != null) ? inventory.GetItemCount(seedItem) : 0;
}
