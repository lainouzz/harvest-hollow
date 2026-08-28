using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

// Tracks the current day. For now a debug key steps it; a real day/night cycle
// can call AdvanceDay() instead.
public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    // Static so the many crops can subscribe without finding the manager.
    public static event Action<int> OnDayChanged;

    public Transform lightTransform;

    [SerializeField] private int currentDay = 1;
    [SerializeField] private float daySwitchTimer;
    
    private float timer;
    private bool isInTrigger;

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
        if (isInTrigger)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                AdvanceDay();
            }
        }
    }
    
    private bool Countdown()
    {
        timer -= Time.deltaTime;
        Debug.Log($"Time: {timer}"); 
        return timer <= 0f;
    }
    
    private void AdvanceDay()
    {
        currentDay++;

        Debug.Log($"Day {currentDay}");
        OnDayChanged?.Invoke(currentDay);

        isInTrigger = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInTrigger = true;
            timer = daySwitchTimer;

            Debug.Log($"Day change in {timer} seconds");
        }
    }
}
