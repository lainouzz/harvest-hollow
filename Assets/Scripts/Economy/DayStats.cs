using System.Collections.Generic;
using UnityEngine;

// Tracks what happened today (harvests, coins earned and spent) for the end-of-day
// summary. Resets every new day, and when a save is loaded.
public class DayStats : MonoBehaviour
{
    [SerializeField] private Wallet wallet;

    private FarmGrid[] grids;
    private readonly Dictionary<InventoryItem, int> harvested = new();
    private int prevCoins;

    public IReadOnlyDictionary<InventoryItem, int> Harvested => harvested;
    public int Earned { get; private set; }
    public int Spent { get; private set; }
    public int Profit => Earned - Spent;

    // Crops that are ripe already or will finish growing tonight.
    public int CropsReadyTomorrow
    {
        get
        {
            int count = 0;
            foreach (FarmGrid grid in grids)
                foreach (Crop crop in grid.PlantedCrops)
                    if (crop != null && (crop.IsGrown || crop.WillRipenTonight(grid.IsWatered(crop.GridPosition))))
                        count++;
            return count;
        }
    }

    private void Awake()
    {
        grids = FindObjectsByType<FarmGrid>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (wallet == null) wallet = FindAnyObjectByType<Wallet>();
    }

    private void OnEnable()
    {
        FarmGrid.AnyHarvested += OnHarvested;
        if (wallet != null) wallet.OnCoinsChanged += OnCoinsChanged;
        DayManager.OnDayChanged += OnNewDay;
        DayManager.OnDayLoaded += OnNewDay;
    }

    private void OnDisable()
    {
        FarmGrid.AnyHarvested -= OnHarvested;
        if (wallet != null) wallet.OnCoinsChanged -= OnCoinsChanged;
        DayManager.OnDayChanged -= OnNewDay;
        DayManager.OnDayLoaded -= OnNewDay;
    }

    private void Start() => ResetDay();

    private void OnHarvested(FarmGrid grid, Vector2Int cell, InventoryItem item, int amount)
    {
        if (item == null) return;

        harvested.TryGetValue(item, out int current);
        harvested[item] = current + amount;
    }

    private void OnCoinsChanged(int total)
    {
        int delta = total - prevCoins;
        prevCoins = total;

        if (delta > 0) Earned += delta;
        else Spent -= delta;
    }

    private void OnNewDay(int day) => ResetDay();

    private void ResetDay()
    {
        harvested.Clear();
        Earned = 0;
        Spent = 0;
        prevCoins = wallet != null ? wallet.Coins : 0;
    }
}
