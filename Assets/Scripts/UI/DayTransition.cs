using System.Collections;
using TMPro;
using UnityEngine;

public class DayTransition : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TMP_Text dayLabel;
    [SerializeField] private float fadeDuration = 0.6f;
    [SerializeField] private float holdDuration = 1.2f;

    private void Awake()
    {
        if (group != null)
        {
            group.alpha = 0f;
            group.blocksRaycasts = false;
        }
    }

    public IEnumerator FadeOut() => Fade(0f, 1f);
    public IEnumerator FadeIn() => Fade(1f, 0f);

    public WaitForSeconds Hold() => new WaitForSeconds(holdDuration);

    public void SetDay(int day)
    {
        if (dayLabel != null) dayLabel.text = $"Day {day}";
    }

    private IEnumerator Fade(float from, float to)
    {
        if (group == null) yield break;

        group.blocksRaycasts = true;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        group.alpha = to;
        group.blocksRaycasts = to > 0.5f;
    }
}
