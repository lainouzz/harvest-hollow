using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Tracks the current day and advances it. For now a debug key steps the day;
// a real day/night cycle can just call AdvanceDay() when it rolls over.
public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    // Broadcast to everything that grows on a day change. Static so crops can
    // subscribe without finding the manager (and there can be many of them).
    public static event Action<int> OnDayChanged;

    [SerializeField] private int currentDay = 1;

    [Tooltip("Debug key to advance the day until the day/night cycle exists.")]
    [SerializeField] private Key advanceKey = Key.X;

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
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard[advanceKey].wasPressedThisFrame)
            AdvanceDay();
    }

    public void AdvanceDay()
    {
        currentDay++;
        Debug.Log($"Day {currentDay}");
        OnDayChanged?.Invoke(currentDay);
    }
}
