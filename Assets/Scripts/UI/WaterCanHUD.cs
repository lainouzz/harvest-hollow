using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A water meter for the watering-can hotbar slot. Fills/empties with the can and pulses
// a warning colour when it's empty, so you know to head to the well.
public class WaterCanHUD : MonoBehaviour
{
    [Tooltip("An Image set to Image Type: Filled. Its fill amount follows the water level.")]
    [SerializeField] private Image fill;
    [Tooltip("Optional \"7/10\" text.")]
    [SerializeField] private TMP_Text label;

    [Header("Empty warning")]
    [Tooltip("What pulses when the can is empty (e.g. the slot frame or the meter background).")]
    [SerializeField] private Graphic warning;
    [SerializeField] private Color warningColor = new Color(0.95f, 0.45f, 0.4f);
    [SerializeField] private float pulseSpeed = 4f;

    private WaterCan can;
    private Color warningRestColor;

    private void Awake()
    {
        can = FindFirstObjectByType<WaterCan>(FindObjectsInactive.Include);
        if (warning != null) warningRestColor = warning.color;
    }

    private void OnEnable()
    {
        if (can != null) can.OnWaterChanged += Refresh;
    }

    private void OnDisable()
    {
        if (can != null) can.OnWaterChanged -= Refresh;
    }

    private void Start() => Refresh();

    private void Update()
    {
        if (warning == null || can == null) return;

        if (can.IsEmpty)
        {
            float t = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
            warning.color = Color.Lerp(warningRestColor, warningColor, t);
        }
        else if (warning.color != warningRestColor)
        {
            warning.color = warningRestColor;
        }
    }

    private void Refresh()
    {
        if (can == null) return;

        if (fill != null) fill.fillAmount = can.Capacity > 0 ? (float)can.Water / can.Capacity : 0f;
        if (label != null) label.text = $"{can.Water}/{can.Capacity}";
    }
}
