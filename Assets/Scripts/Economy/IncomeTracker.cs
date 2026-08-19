using UnityEngine;
using TMPro;

// Debug/tuning readout: gold earned and spent per day, plus a rough gold-per-crop-
// per-day figure ("p") to size expansion pricing. Remove once the economy is tuned.
public class IncomeTracker : MonoBehaviour
{
    [Tooltip("Optional on-screen text. Empty = log to the Console.")]
    [SerializeField] private TMP_Text readout;

    private Wallet wallet;
    private FarmGrid grid;

    private int prevCoins;
    private int earnedToday;
    private int spentToday;
    private int totalEarned;
    private int totalSpent;
    private int daysMeasured;

    private void Awake()
    {
        wallet = FindFirstObjectByType<Wallet>(FindObjectsInactive.Include);
        grid = FindFirstObjectByType<FarmGrid>(FindObjectsInactive.Include);
    }

    private void OnEnable()
    {
        if (wallet != null)
        {
            prevCoins = wallet.Coins;
            wallet.OnCoinsChanged += OnCoinsChanged;
        }

        DayManager.OnDayChanged += OnDayChanged;
    }

    private void OnDisable()
    {
        if (wallet != null) wallet.OnCoinsChanged -= OnCoinsChanged;
        DayManager.OnDayChanged -= OnDayChanged;
    }

    private void OnCoinsChanged(int total)
    {
        int delta = total - prevCoins;
        prevCoins = total;

        if (delta > 0) earnedToday += delta;
        else spentToday += -delta;
    }

    private void OnDayChanged(int day)
    {
        /*TODO: 
         Create a end of day/next time UI if all profit, total spent and display day and time*/

        daysMeasured++;
        totalEarned += earnedToday;
        totalSpent += spentToday;

        int crops = grid != null ? grid.PlantedCrops.Count : 0;
        float avgEarnedPerDay = (float)totalEarned / daysMeasured;
        float avgSpentPerDay = (float)totalSpent / daysMeasured;
        float perCrop = crops > 0 ? avgEarnedPerDay / crops : 0f;

        string text =
            $"Day {day - 1} ended\n" +
            $"Earned: {earnedToday}g   Spent: {spentToday}g\n" +
            $"Avg/day: +{avgEarnedPerDay:0} earned, -{avgSpentPerDay:0} spent\n" +
            $"Farming {crops} crops  ->  ~{perCrop:0.0}g per crop/day (avg)";

        Debug.Log(text);
        if (readout != null) readout.text = text;

        earnedToday = 0;
        spentToday = 0;
    }
}
