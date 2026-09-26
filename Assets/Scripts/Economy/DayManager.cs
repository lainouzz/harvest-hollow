using System;
using System.Collections;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }
    public static event Action<int> OnDayChanged;

    // Fired when a saved day is restored. Separate from OnDayChanged on purpose:
    // loading must update the HUD without making every crop grow another day.
    public static event Action<int> OnDayLoaded;

    [SerializeField] private int currentDay = 1;
    [Tooltip("Seconds the player must stay in the sleep zone before the day advances.")]
    [SerializeField] private float daySwitchTimer = 1.5f;
    [SerializeField] private DayTransition transition;

    private float timer;
    private bool isInTrigger;
    private bool isSwitching;

    public int CurrentDay => currentDay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (!isInTrigger || isSwitching) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
            StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
        isSwitching = true;

        if (transition != null) yield return transition.FadeOut();

        AdvanceDay();

        if (SaveManager.Instance != null) SaveManager.Instance.Save();

        if (transition != null)
        {
            transition.SetDay(currentDay);
            yield return transition.Hold();
            yield return transition.FadeIn();
        }

        isInTrigger = false;
        isSwitching = false;
    }

    private void AdvanceDay()
    {
        currentDay++;
        OnDayChanged?.Invoke(currentDay);
    }

    public void SetDay(int day)
    {
        currentDay = Mathf.Max(1, day);
        OnDayLoaded?.Invoke(currentDay);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isSwitching) return;

        if (other.CompareTag("Player"))
        {
            isInTrigger = true;
            timer = daySwitchTimer;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            isInTrigger = false;
    }
}
