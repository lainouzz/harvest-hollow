using UnityEngine;
using TMPro;

// Shows the current day in the HUD. Put it on the DayCount text object (or assign
// the text). Updates whenever the day advances.
public class DayHUD : MonoBehaviour
{
    [Tooltip("The day text. Empty = a TMP_Text on this object.")]
    [SerializeField] private TMP_Text dayText;

    [Tooltip("{0} = the day number.")]
    [SerializeField] private string format = "Day {0}";

    private void Awake()
    {
        if (dayText == null) dayText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        DayManager.OnDayChanged += UpdateDisplay;
        ShowCurrentDay();
    }

    private void OnDisable()
    {
        DayManager.OnDayChanged -= UpdateDisplay;
    }

    private void Start()
    {
        // Covers the case where the manager wasn't ready yet in OnEnable.
        ShowCurrentDay();
    }

    private void ShowCurrentDay()
    {
        if (DayManager.Instance != null)
            UpdateDisplay(DayManager.Instance.CurrentDay);
    }

    private void UpdateDisplay(int day)
    {
        if (dayText != null)
            dayText.text = string.Format(format, day);
    }
}
