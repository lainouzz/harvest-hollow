using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Slowly scrolls the credits from the top when the panel opens. If the player scrolls
// themselves, it backs off and carries on from there after a short pause.
[RequireComponent(typeof(ScrollRect))]
public class CreditsScroller : MonoBehaviour
{
    [SerializeField] private float startDelay = 1.5f;
    [Tooltip("Scroll speed in canvas pixels per second.")]
    [SerializeField] private float speed = 45f;
    [SerializeField] private float endPause = 2.5f;
    [Tooltip("Jump back to the top and scroll again after reaching the end.")]
    [SerializeField] private bool loop = true;
    [Tooltip("Seconds to wait after the player scrolls before auto-scrolling again.")]
    [SerializeField] private float resumeAfter = 3f;

    private ScrollRect scroll;
    private float lastManualScroll = float.NegativeInfinity;
    private bool settingPosition;

    private void Awake()
    {
        scroll = GetComponent<ScrollRect>();
        scroll.onValueChanged.AddListener(_ =>
        {
            if (!settingPosition) lastManualScroll = Time.unscaledTime;
        });
    }

    private void OnEnable() => StartCoroutine(Run());

    private IEnumerator Run()
    {
        yield return null;                 // let the layout size the content first
        Canvas.ForceUpdateCanvases();

        while (true)
        {
            SetPosition(1f);               // 1 = top
            lastManualScroll = float.NegativeInfinity;
            yield return Wait(startDelay);

            while (scroll.verticalNormalizedPosition > 0.0001f)
            {
                float scrollable = scroll.content.rect.height - Viewport().rect.height;
                if (scrollable <= 0f) yield break;   // everything fits, nothing to scroll

                if (Time.unscaledTime - lastManualScroll > resumeAfter)
                    SetPosition(scroll.verticalNormalizedPosition - speed * Time.unscaledDeltaTime / scrollable);

                yield return null;
            }

            yield return Wait(endPause);
            if (!loop) yield break;
        }
    }

    private RectTransform Viewport() => scroll.viewport != null ? scroll.viewport : (RectTransform)scroll.transform;

    private void SetPosition(float value)
    {
        settingPosition = true;
        scroll.verticalNormalizedPosition = Mathf.Clamp01(value);
        settingPosition = false;
    }

    // Unscaled, so it still runs if the menu ever has the game paused (timeScale 0).
    private static IEnumerator Wait(float seconds)
    {
        for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime) yield return null;
    }
}
