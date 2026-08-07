using UnityEngine;
using TMPro;

// Shows the current day in the HUD; updates when the day advances.
public class DayHUD : MonoBehaviour
{
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
        ShowCurrentDay();   // in case the manager wasn't ready in OnEnable
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
