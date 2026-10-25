using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// The end-of-day summary card shown before sleeping. Uses unscaled time because the
// game is paused while it's open.
public class DaySummaryUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float fadeDuration = 0.2f;

    [Header("Texts")]
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private string titleFormat = "Day {0} complete";
    [SerializeField] private TMP_Text earnedLabel;
    [SerializeField] private TMP_Text spentLabel;
    [SerializeField] private TMP_Text profitLabel;
    [SerializeField] private TMP_Text tomorrowLabel;

    [Header("Harvested")]
    [SerializeField] private Transform chipContainer;
    [SerializeField] private HarvestChip chipTemplate;
    [Tooltip("Shown instead of the chips when nothing was harvested today (optional).")]
    [SerializeField] private GameObject nothingHarvested;

    [Header("Colors")]
    [SerializeField] private Color gainColor = new Color(0.62f, 0.83f, 0.54f);
    [SerializeField] private Color lossColor = new Color(0.95f, 0.64f, 0.54f);

    private readonly List<HarvestChip> chips = new();

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        if (chipTemplate != null) chipTemplate.gameObject.SetActive(false);
        SetVisible(0f);
    }

    public void Show(int day, DayStats stats)
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (titleLabel != null) titleLabel.text = string.Format(titleFormat, day);

        BuildChips(stats.Harvested);

        if (earnedLabel != null) earnedLabel.text = $"+{stats.Earned}g";
        if (spentLabel != null) spentLabel.text = $"-{stats.Spent}g";

        if (profitLabel != null)
        {
            int profit = stats.Profit;
            profitLabel.text = profit >= 0 ? $"+{profit}g" : $"{profit}g";
            profitLabel.color = profit >= 0 ? gainColor : lossColor;
        }

        if (tomorrowLabel != null)
        {
            int ready = stats.CropsReadyTomorrow;
            tomorrowLabel.text = ready == 0 ? "Your crops are still growing"
                : ready == 1 ? "1 crop will be ready to harvest tomorrow"
                : $"{ready} crops will be ready to harvest tomorrow";
        }

        IsOpen = true;
        StopAllCoroutines();
        StartCoroutine(Fade(1f));
    }

    public void Hide()
    {
        IsOpen = false;
        StopAllCoroutines();
        SetVisible(0f);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void BuildChips(IReadOnlyDictionary<InventoryItem, int> harvested)
    {
        foreach (HarvestChip chip in chips)
            if (chip != null) Destroy(chip.gameObject);
        chips.Clear();

        if (nothingHarvested != null) nothingHarvested.SetActive(harvested.Count == 0);
        if (chipTemplate == null || chipContainer == null) return;

        foreach (KeyValuePair<InventoryItem, int> entry in harvested)
        {
            HarvestChip chip = Instantiate(chipTemplate, chipContainer);
            chip.gameObject.SetActive(true);
            chip.Set(entry.Key, entry.Value);
            chips.Add(chip);
        }
    }

    private IEnumerator Fade(float to)
    {
        float from = group != null ? group.alpha : to;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            SetVisible(Mathf.Lerp(from, to, t / fadeDuration));
            yield return null;
        }
        SetVisible(to);
    }

    private void SetVisible(float alpha)
    {
        if (group == null) return;

        group.alpha = alpha;
        group.blocksRaycasts = alpha > 0.5f;
    }
}
