using UnityEngine;
using UnityEngine.Events;

public class Crop : MonoBehaviour
{
    [Tooltip("Stage models in order, last = ripe/harvest-ready. Leave empty to " +
             "fall back to a placeholder scale-up.")]
    [SerializeField] private GameObject[] stages;

    [Tooltip("Fired once when the crop becomes fully grown — hook a sparkle / sound / icon.")]
    [SerializeField] private UnityEvent onGrown = new UnityEvent();

    private CropData data;
    private float grownDays;
    private float growthPerDay = 1f;
    private int bonusYield;
    private int shownStage = -1;

    public bool IsGrown { get; private set; }
    public CropData Data => data;
    public UnityEvent  OnGrown => onGrown;

    public Vector2Int GridPosition { get; private set; }

    public float Progress { get; private set; }

    public int Yield => (data != null ? data.baseYield : 0) + bonusYield;

    public float GrownDays => grownDays;
    public float GrowthPerDay => growthPerDay;
    public int BonusYield => bonusYield;

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

    // Jumps straight to a saved amount of growth (call after Initialize). Doesn't fire
    // OnGrown — the grid refreshes the tile icon itself after loading.
    public void RestoreGrowth(float savedGrownDays)
    {
        if (data == null) return;

        grownDays = Mathf.Max(0f, savedGrownDays);
        float target = data.daysToGrow > 0 ? data.daysToGrow : 1;
        Progress = Mathf.Clamp01(grownDays / target);
        IsGrown = Progress >= 1f;
        ApplyGrowth(Progress);
    }

    private void OnEnable() => DayManager.OnDayChanged += OnDayChanged;
    private void OnDisable() => DayManager.OnDayChanged -= OnDayChanged;

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
            OnGrown?.Invoke();
        }
    }

    private void ApplyGrowth(float t)
    {
        if (stages == null || stages.Length == 0)
        {
            transform.localScale = Vector3.one * Mathf.Lerp(0.2f, 1f, t);
            return;
        }

        ShowStage(StageForProgress(t));
    }

    private int StageForProgress(float t)
    {
        int lastStage = stages.Length - 1;
        if (t >= 1f || IsGrown) return lastStage;
        if (stages.Length == 1) return 0;

        int growingStages = stages.Length - 1;
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
