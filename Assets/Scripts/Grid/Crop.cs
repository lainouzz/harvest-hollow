using UnityEngine;
using UnityEngine.Events;

// Attach to a crop prefab. Grows in whole days (advanced by the DayManager) and
// swaps between stage models as it grows — no per-frame work.
//
// Put each stage as a CHILD of the prefab and drag them into `stages` in order:
//   sprout -> ... -> ripe (the LAST stage is the harvest-ready look).
public class Crop : MonoBehaviour
{
    [Tooltip("Stage models in order, last = ripe/harvest-ready. Leave empty to " +
             "fall back to a placeholder scale-up.")]
    [SerializeField] private GameObject[] stages;

    [Tooltip("Fired once when the crop becomes fully grown — hook a sparkle / sound / icon.")]
    [SerializeField] private UnityEvent onGrown;

    private CropData data;
    private float grownDays;             // growth accumulated so far, in days
    private float growthPerDay = 1f;     // > 1 grows faster (watered soil)
    private int bonusYield;
    private int shownStage = -1;

    public bool IsGrown { get; private set; }
    public CropData Data => data;

    // Which cell this crop is planted on (handy for UI / lookups).
    public Vector2Int GridPosition { get; private set; }

    // 0 = just planted, 1 = fully grown. Read this for a progress bar.
    public float Progress { get; private set; }

    // What you get on harvest (base yield + any watered bonus).
    public int Yield => (data != null ? data.baseYield : 0) + bonusYield;

    // growthMultiplier > 1 grows faster; bonusYield is added to the harvest.
    public void Initialize(CropData cropData, Vector2Int cell, float growthMultiplier = 1f, int bonusYield = 0)
    {
        data = cropData;
        GridPosition = cell;
        growthPerDay = Mathf.Max(0.01f, growthMultiplier);
        this.bonusYield = bonusYield;

        grownDays = 0f;
        Progress = 0f;
        IsGrown = false;
        shownStage = -1;
        ApplyGrowth(0f);
    }

    private void OnEnable() => DayManager.OnDayChanged += OnDayChanged;
    private void OnDisable() => DayManager.OnDayChanged -= OnDayChanged;

    // Each day advances growth by growthPerDay (watered soil counts as more).
    // TODO: real watering — only apply the watered bonus on days it was watered.
    private void OnDayChanged(int day)
    {
        if (data == null || IsGrown) return;

        grownDays += growthPerDay;

        float target = data.daysToGrow > 0 ? data.daysToGrow : 1;
        Progress = Mathf.Clamp01(grownDays / target);
        ApplyGrowth(Progress);

        if (Progress >= 1f)
        {
            IsGrown = true;
            onGrown?.Invoke();
        }
    }

    // t: 0 = just planted, 1 = fully grown.
    private void ApplyGrowth(float t)
    {
        if (stages == null || stages.Length == 0)
        {
            // Placeholder while a crop has no stage models assigned.
            transform.localScale = Vector3.one * Mathf.Lerp(0.2f, 1f, t);
            return;
        }

        ShowStage(StageForProgress(t));
    }

    // Ripe (last) stage at full growth; the earlier stages spread across [0,1).
    private int StageForProgress(float t)
    {
        int lastStage = stages.Length - 1;
        if (t >= 1f || IsGrown) return lastStage;
        if (stages.Length == 1) return 0;

        int growingStages = stages.Length - 1;   // all but the ripe one
        return Mathf.Clamp(Mathf.FloorToInt(t * growingStages), 0, growingStages - 1);
    }

    private void ShowStage(int index)
    {
        if (index == shownStage) return;
        shownStage = index;

        for (int i = 0; i < stages.Length; ++i)
            if (stages[i] != null) stages[i].SetActive(i == index);
    }
}
