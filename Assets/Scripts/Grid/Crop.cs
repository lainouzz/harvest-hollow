using System.Collections;
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
    private int nightsGrowing;
    private int wetNights;
    private int lockedBonusYield;   // bonus earned at planting under the 1.1 rules (old saves)
    private int shownStage = -1;

    public bool IsGrown { get; private set; }
    public CropData Data => data;
    public UnityEvent  OnGrown => onGrown;

    public Vector2Int GridPosition { get; private set; }

    public float Progress { get; private set; }

    // Watered on at least half of the nights it spent growing.
    public bool EarnedWaterBonus => nightsGrowing > 0 && wetNights * 2 >= nightsGrowing;

    public int Yield => data == null ? 0
        : data.baseYield + Mathf.Max(lockedBonusYield, EarnedWaterBonus ? data.wateredYieldBonus : 0);

    public float GrownDays => grownDays;
    public int NightsGrowing => nightsGrowing;
    public int WetNights => wetNights;
    public int LockedBonusYield => lockedBonusYield;

    private float TargetDays => data != null && data.daysToGrow > 0 ? data.daysToGrow : 1;

    // True if one more night (wet or dry) will finish this crop.
    public bool WillRipenTonight(bool wet) =>
        !IsGrown && data != null && grownDays + GrowthFor(wet) >= TargetDays;

    public void Initialize(CropData cropData, Vector2Int cell)
    {
        data = cropData;
        GridPosition = cell;

        grownDays = 0f;
        nightsGrowing = 0;
        wetNights = 0;
        lockedBonusYield = 0;
        Progress = 0f;
        IsGrown = false;
        shownStage = -1;
        ApplyGrowth(0f);
    }

    // Jumps straight to saved growth (call after Initialize). Doesn't fire OnGrown,
    // the grid refreshes the tile icon itself after loading.
    public void RestoreGrowth(float savedGrownDays, int savedNights, int savedWetNights, int savedLockedBonus)
    {
        if (data == null) return;

        grownDays = Mathf.Max(0f, savedGrownDays);
        nightsGrowing = Mathf.Max(0, savedNights);
        wetNights = Mathf.Clamp(savedWetNights, 0, nightsGrowing);
        lockedBonusYield = Mathf.Max(0, savedLockedBonus);

        Progress = Mathf.Clamp01(grownDays / TargetDays);
        IsGrown = Progress >= 1f;
        ApplyGrowth(Progress);
    }

    // Called by FarmGrid once per night, before the soil dries. Wet soil grows faster;
    // dry soil still grows, just slower.
    public void GrowOneNight(bool wet)
    {
        if (data == null || IsGrown) return;

        nightsGrowing++;
        if (wet) wetNights++;
        grownDays += GrowthFor(wet);

        Progress = Mathf.Clamp01(grownDays / TargetDays);
        ApplyGrowth(Progress);

        if (Progress >= 1f)
        {
            IsGrown = true;
            OnGrown?.Invoke();
        }
    }

    private float GrowthFor(bool wet) => wet ? Mathf.Max(1f, data.wateredGrowthMultiplier) : 1f;

    // Quick bounce up, then shrink to nothing, then destroy.
    public void PlayHarvested(float duration = 0.2f)
    {
        StartCoroutine(ShrinkAndDestroy(duration));
    }

    private IEnumerator ShrinkAndDestroy(float duration)
    {
        Vector3 startScale = transform.localScale;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float scale = k < 0.3f
                ? Mathf.Lerp(1f, 1.15f, k / 0.3f)
                : Mathf.Lerp(1.15f, 0f, (k - 0.3f) / 0.7f);

            transform.localScale = startScale * scale;
            yield return null;
        }

        Destroy(gameObject);
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
