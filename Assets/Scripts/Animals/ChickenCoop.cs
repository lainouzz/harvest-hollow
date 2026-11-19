using System.Collections.Generic;
using UnityEngine;

// Owns the chickens and their eggs. Each chicken lays an egg every morning into the
// nest box; the player collects them (and buys new chickens) through CoopPrompt.
public class ChickenCoop : MonoBehaviour
{
    [Header("Chickens")]
    [SerializeField] private Chicken chickenPrefab;
    [SerializeField] private int price = 150;
    [SerializeField] private int maxChickens = 4;
    [Tooltip("Where a newly bought chicken appears, e.g. the bottom of the ramp.")]
    [SerializeField] private Transform spawnPoint;

    [Header("Pen (the area chickens wander in)")]
    [Tooltip("Centre of the pen. Its rotation turns the area. Empty = this object.")]
    [SerializeField] private Transform penCenter;
    [SerializeField] private Vector2 penSize = new Vector2(6f, 6f);

    [Header("Eggs")]
    [SerializeField] private InventoryItem egg;
    [Tooltip("The nest stops filling at this many eggs per chicken, so eggs can't pile up forever.")]
    [SerializeField] private int maxEggsPerChicken = 3;
    [Tooltip("Egg models in the nest box, shown one per waiting egg.")]
    [SerializeField] private GameObject[] eggVisuals;

    [Header("Sound (optional)")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip buyClip;
    [SerializeField] private AudioClip collectClip;

    private readonly List<Chicken> chickens = new();
    private DayStats dayStats;

    public int ChickenCount => chickens.Count;
    public int MaxChickens => maxChickens;
    public int Price => price;
    public bool IsFull => chickens.Count >= maxChickens;
    public int EggsWaiting { get; private set; }

    private void Awake()
    {
        if (penCenter == null) penCenter = transform;
        dayStats = FindAnyObjectByType<DayStats>();
        RefreshEggs();
    }

    private void OnEnable() => DayManager.OnDayChanged += LayEggs;
    private void OnDisable() => DayManager.OnDayChanged -= LayEggs;

    private void LayEggs(int day)
    {
        EggsWaiting = Mathf.Min(EggsWaiting + chickens.Count, chickens.Count * maxEggsPerChicken);
        RefreshEggs();
    }

    public bool TryBuyChicken(Wallet wallet)
    {
        if (IsFull || wallet == null || !wallet.TrySpend(price)) return false;

        Spawn(spawnPoint != null ? spawnPoint.position : RandomPointInPen());
        PlaySound(buyClip);
        return true;
    }

    public int CollectEggs()
    {
        int n = EggsWaiting;
        if (n <= 0 || egg == null || InventoryManager.Instance == null) return 0;

        InventoryManager.Instance.Add(egg, n);
        if (dayStats != null) dayStats.RecordHarvest(egg, n);

        EggsWaiting = 0;
        RefreshEggs();
        PlaySound(collectClip);
        return n;
    }

    // For save/load: replaces the flock instantly.
    public void Restore(int count, int eggs)
    {
        foreach (Chicken c in chickens)
            if (c != null) Destroy(c.gameObject);
        chickens.Clear();

        count = Mathf.Clamp(count, 0, maxChickens);
        for (int i = 0; i < count; i++) Spawn(RandomPointInPen());

        EggsWaiting = Mathf.Max(0, eggs);
        RefreshEggs();
    }

    private void Spawn(Vector3 position)
    {
        if (chickenPrefab == null) return;

        Chicken c = Instantiate(chickenPrefab, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        c.Init(this);
        chickens.Add(c);
    }

    private void RefreshEggs()
    {
        for (int i = 0; i < eggVisuals.Length; i++)
            if (eggVisuals[i] != null) eggVisuals[i].SetActive(i < EggsWaiting);
    }

    private void PlaySound(AudioClip clip)
    {
        if (source != null && clip != null) source.PlayOneShot(clip);
    }

    public Vector3 RandomPointInPen()
    {
        Vector3 c = penCenter.position;
        return c + penCenter.right * Random.Range(-penSize.x, penSize.x) * 0.5f
                 + penCenter.forward * Random.Range(-penSize.y, penSize.y) * 0.5f;
    }

    public Vector3 ClampToPen(Vector3 p)
    {
        Vector3 c = penCenter.position;
        Vector3 d = p - c;
        float x = Mathf.Clamp(Vector3.Dot(d, penCenter.right), -penSize.x * 0.5f, penSize.x * 0.5f);
        float z = Mathf.Clamp(Vector3.Dot(d, penCenter.forward), -penSize.y * 0.5f, penSize.y * 0.5f);
        return c + penCenter.right * x + penCenter.forward * z;
    }

    private void OnDrawGizmosSelected()
    {
        Transform t = penCenter != null ? penCenter : transform;
        Gizmos.color = new Color(1f, 0.85f, 0.2f);
        Gizmos.matrix = Matrix4x4.TRS(t.position, t.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(penSize.x, 0.05f, penSize.y));
    }
}
