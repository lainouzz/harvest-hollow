using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }
    public static event Action<int> OnDayChanged;

    // Fired when a saved day is restored. Separate from OnDayChanged on purpose:
    // loading must update the HUD without making every crop grow another day.
    public static event Action<int> OnDayLoaded;

    [SerializeField] private int currentDay = 1;
    [SerializeField] private DayTransition transition;
    [SerializeField] private DaySummaryUI summary;
    [SerializeField] private DayStats stats;
    [SerializeField] private MerchantBehavior merchantBehavior;
    [SerializeField] private TMP_Text promptText;

    private GameInput input;
    private PlayerLook playerLook;
    private bool playerInZone;
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
        input = new GameInput();

        if (promptText != null) promptText.gameObject.SetActive(false);
        if (merchantBehavior == null) merchantBehavior = FindAnyObjectByType<MerchantBehavior>();
        if (stats == null) stats = FindAnyObjectByType<DayStats>();
        if (summary == null) summary = FindFirstObjectByType<DaySummaryUI>(FindObjectsInactive.Include);
        playerLook = FindAnyObjectByType<PlayerLook>();
    }

    private void OnEnable() => input.Enable();
    private void OnDisable() => input.Disable();

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (isSwitching) return;

        if (summary != null && summary.IsOpen)
        {
            if (input.Player.Interact.WasPressedThisFrame())
                StartCoroutine(SleepRoutine());
            else if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                StartCoroutine(CloseSummary());
            return;
        }

        if (!playerInZone || !input.Player.Interact.WasPressedThisFrame()) return;
        if (Time.timeScale == 0f || (playerLook != null && playerLook.isInShopUI)) return;

        if (summary != null && stats != null)
            OpenSummary();
        else
            StartCoroutine(SleepRoutine());   // no summary set up: sleep straight away
    }

    private void OpenSummary()
    {
        SetPaused(true);
        if (promptText != null) promptText.gameObject.SetActive(false);
        summary.Show(currentDay, stats);
    }

    // Un-pauses a frame later, so the same Esc press doesn't also open the pause menu.
    private IEnumerator CloseSummary()
    {
        summary.Hide();
        yield return null;

        SetPaused(false);
        if (playerInZone && promptText != null) promptText.gameObject.SetActive(true);
    }

    private IEnumerator SleepRoutine()
    {
        isSwitching = true;
        SetPaused(true);
        if (promptText != null) promptText.gameObject.SetActive(false);

        if (transition != null) yield return transition.FadeOut();

        // Everything below happens behind the black screen.
        if (summary != null) summary.Hide();
        if (merchantBehavior != null) merchantBehavior.transform.position = merchantBehavior.endPos.position;

        AdvanceDay();

        if (SaveManager.Instance != null) SaveManager.Instance.Save();

        if (transition != null)
        {
            transition.SetDay(currentDay);
            yield return transition.Hold();
            yield return transition.FadeIn();
        }

        SetPaused(false);
        if (merchantBehavior != null) merchantBehavior.EnterArriving();
        if (playerInZone && promptText != null) promptText.gameObject.SetActive(true);
        isSwitching = false;
    }

    private void SetPaused(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;
        if (playerLook != null) playerLook.isInShopUI = paused;
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
        if (!other.CompareTag("Player")) return;

        playerInZone = true;
        if (!isSwitching && promptText != null) promptText.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInZone = false;
        if (promptText != null) promptText.gameObject.SetActive(false);
    }
}
