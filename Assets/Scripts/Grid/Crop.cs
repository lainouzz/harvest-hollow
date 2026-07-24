using UnityEngine;

// Attach to a crop prefab. Handles growth over time.
// Scaffold: for now it just scales up. Swap this for stage-model swapping,
// material changes, etc. when you have art.
public class Crop : MonoBehaviour
{
    private CropData data;
    private float growTimer;
    private float growthMultiplier = 1f;
    private int bonusYield;

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
        this.growthMultiplier = Mathf.Max(0.01f, growthMultiplier);
        this.bonusYield = bonusYield;

        growTimer = 0f;
        Progress = 0f;
        IsGrown = false;
        ApplyGrowth(0f);
    }

    // TODO: growth runs on Time.deltaTime while the object is alive. If you add
    //       day/night or sleeping, drive this off game-time / day ticks instead,
    //       otherwise crops freeze when unloaded and ignore skipped days.
    private void Update()
    {
        if (data == null || IsGrown) return;

        // Watered soil applied its boost via growthMultiplier at plant time.
        // TODO: real watering — re-water each day; unwatered days slow growth.
        growTimer += Time.deltaTime * growthMultiplier;
        Progress = data.growSeconds > 0f ? Mathf.Clamp01(growTimer / data.growSeconds) : 1f;
        ApplyGrowth(Progress);

        if (Progress >= 1f)
            IsGrown = true;
        // TODO: fire an OnGrown event here so the tile/UI can show it's ready
        //       (particle, sparkle, icon) instead of the player having to guess.
    }

    // t: 0 = just planted, 1 = fully grown.
    private void ApplyGrowth(float t)
    {
        // TODO: replace this placeholder scaling with real stages — swap the stage
        //       model/sprite from CropData.growthStages based on t. Scaling a mesh
        //       up from 0.2 is a stand-in, not the look you want.
        transform.localScale = Vector3.one * Mathf.Lerp(0.2f, 5f, t);
    }
}
