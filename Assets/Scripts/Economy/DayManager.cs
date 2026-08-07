using System;
using UnityEngine;
using UnityEngine.InputSystem;

// Tracks the current day. For now a debug key steps it; a real day/night cycle
// can call AdvanceDay() instead.
public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    // Static so the many crops can subscribe without finding the manager.
    public static event Action<int> OnDayChanged;

    [SerializeField] private int currentDay = 1;
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
