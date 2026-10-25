using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A "+5 Potato" popup: pops in, floats up, fades out, then destroys itself.
// Put it on a small world-space canvas prefab (add a Billboard so it faces the camera).
public class PickupPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image icon;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private float riseDistance = 0.6f;
    [SerializeField] private float duration = 0.9f;

    public void Show(string text, Sprite sprite)
    {
        if (label != null) label.text = text;

        if (icon != null)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }

        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        Vector3 start = transform.position;
        Vector3 baseScale = transform.localScale;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);

            // Ease-out rise, a quick overshoot "pop" on spawn, and a fade over the last 40%.
            transform.position = start + Vector3.up * (riseDistance * (1f - (1f - k) * (1f - k)));
            transform.localScale = baseScale * (k < 0.15f ? Mathf.Lerp(0.6f, 1.1f, k / 0.15f)
                                                         : Mathf.Lerp(1.1f, 1f, (k - 0.15f) / 0.85f));
            if (group != null) group.alpha = k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f;

            yield return null;
        }

        Destroy(gameObject);
    }
}
