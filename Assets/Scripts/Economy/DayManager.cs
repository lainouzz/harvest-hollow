using System;
using System.Collections;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }
    public static event Action<int> OnDayChanged;

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

        AdvanceDay();   // crops grow while the screen is black

        // TODO: autosave here — the new day, wallet, inventory and grid are all settled
        //       at this point. Call your save system once it exists (see SaveManager).

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
        // Walking out of the zone cancels a pending switch.
        if (other.CompareTag("Player"))
            isInTrigger = false;
    }
}
