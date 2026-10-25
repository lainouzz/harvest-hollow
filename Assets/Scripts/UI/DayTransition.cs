using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DayTransition : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TMP_Text dayLabel;
    [SerializeField] private float fadeDuration = 0.6f;
    [SerializeField] private float holdDuration = 1.2f;

    [Header("Tips")]
    [SerializeField] private TMP_Text tipLabel;
    [SerializeField] private string tipPrefix = "Tip: ";
    [SerializeField, TextArea(1, 3)] private List<string> tips = new()
    {
        "Water tilled soil before you plant. Seeds in wet soil grow faster and give more.",
        "Soil dries out after a couple of days, so check your fields each morning.",
        "Your harvest is also your seeds. Keep a few to replant!",
        "The merchant's stock changes every day. Check back for rare seeds.",
        "Selling pays half an item's price. Planting it might earn you more.",
        "Pricier crops grow slower, but you harvest less often for about the same gold.",
        "Wheat is the fastest crop, great for quick coins early on.",
        "Each new plot costs a little more than the last.",
        "Press X on a fence to open a path through it.",
        "Your game saves every time you sleep.",
    };

    [Header("Wake-up sound")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("One is picked at random each morning (e.g. rooster, dove + bird chirps).")]
    [SerializeField] private List<AudioClip> wakeUpClips = new();

    [Header("Saving")]
    [SerializeField] private TMP_Text savingLabel;
    [SerializeField] private string savingText = "Saving";
    [SerializeField] private float dotInterval = 0.35f;

    private int lastTip = -1;
    private int lastWakeUpClip = -1;
    private int shownDots = -1;

    private void Awake()
    {
        if (group != null)
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        if (savingLabel == null || group == null || group.alpha <= 0f) return;

        int dots = (int)(Time.unscaledTime / dotInterval) % 4;
        if (dots == shownDots) return;
        shownDots = dots;

        // The unused dots are drawn invisible so "Saving" doesn't shift sideways as they appear.
        savingLabel.text = savingText + new string('.', dots) + "<alpha=#00>" + new string('.', 3 - dots);
    }

    public IEnumerator FadeOut()
    {
        PickTip();

        // Show the day that's ending while the screen fades in; SetDay then ticks it to the new one.
        if (DayManager.Instance != null) SetDay(DayManager.Instance.CurrentDay);

        return Fade(0f, 1f);
    }

    public IEnumerator FadeIn()
    {
        PlayWakeUpSound();
        return Fade(1f, 0f);
    }

    private void PlayWakeUpSound()
    {
        if (audioSource == null || wakeUpClips.Count == 0) return;

        int index = Random.Range(0, wakeUpClips.Count);
        if (wakeUpClips.Count > 1 && index == lastWakeUpClip) index = (index + 1) % wakeUpClips.Count;
        lastWakeUpClip = index;

        if (wakeUpClips[index] != null) audioSource.PlayOneShot(wakeUpClips[index]);
    }

    public WaitForSecondsRealtime Hold() => new WaitForSecondsRealtime(holdDuration);

    public void SetDay(int day)
    {
        if (dayLabel != null) dayLabel.text = $"Day {day}";
    }

    private void PickTip()
    {
        if (tipLabel == null || tips.Count == 0) return;

        int index = Random.Range(0, tips.Count);
        if (tips.Count > 1 && index == lastTip) index = (index + 1) % tips.Count;
        lastTip = index;

        tipLabel.text = tipPrefix + tips[index];
    }

    private IEnumerator Fade(float from, float to)
    {
        if (group == null) yield break;

        group.blocksRaycasts = true;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;   // runs while the game is paused for sleep
            group.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        group.alpha = to;
        group.blocksRaycasts = to > 0.5f;
    }
}
