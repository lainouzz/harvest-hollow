using System;
using UnityEngine;

public class MerchantBehavior : MonoBehaviour
{
    [SerializeField] private float driveSpeed = 5f;
    [Tooltip("Seconds parked at the farm before leaving.")]
    [SerializeField] private float timerBeforeLeaving = 30f;
    [Tooltip("Seconds away before coming back.")]
    [SerializeField] private float timerBeforeArriving = 20f;

    [SerializeField] private Transform startPos;
    [SerializeField] private Transform endPos;
    [SerializeField] private GameObject carPrefab;

    [SerializeField] private float wheelSpinMultiplier;

    [SerializeField] private Transform leftFrontWheel;
    [SerializeField] private Transform rightFrontWheel;
    [SerializeField] private Transform leftBackWheel;
    [SerializeField] private Transform rightBackWheel;

    private MerchantBuySection merchantBuySection;

    public enum State { AtFarm, Leaving, Away, Arriving }

    public event Action<State> OnStateChanged;
    public State CurrentState { get; private set; }

    public bool IsDriving => CurrentState == State.Leaving || CurrentState == State.Arriving;

    private float timer;

    private void Start()
    {
        if (merchantBuySection == null)
            merchantBuySection = FindFirstObjectByType<MerchantBuySection>(FindObjectsInactive.Include);

        transform.position = startPos.position;
        if (carPrefab != null) carPrefab.SetActive(true);
        EnterAtFarm();
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case State.AtFarm:
                if (Countdown()) EnterLeaving();
                break;

            case State.Leaving:
                if (DriveTo(endPos.position)) EnterAway();
                break;

            case State.Away:
                if (Countdown()) EnterArriving();
                break;

            case State.Arriving:
                if (DriveTo(startPos.position)) EnterAtFarm();
                break;
        }
    }

    private bool Countdown()
    {
        timer -= Time.deltaTime;
        return timer <= 0f;
    }

    private bool DriveTo(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, driveSpeed * Time.deltaTime);
        Vector3 distance = transform.position - target;
        
        transform.LookAt(target);
        
        leftFrontWheel.rotation = Quaternion.Euler(distance.x * wheelSpinMultiplier, 90, 0);
        rightFrontWheel.rotation = Quaternion.Euler(distance.x * wheelSpinMultiplier, 90,0);
        leftBackWheel.rotation = Quaternion.Euler(distance.x * wheelSpinMultiplier, 90, 0);
        rightBackWheel.rotation = Quaternion.Euler(distance.x * wheelSpinMultiplier, 90, 0);

        return transform.position == target;
    }

    private void EnterAtFarm()
    {
        // Restock only when the truck has just driven back (not on the first spawn).
        bool returned = CurrentState == State.Arriving;

        SetState(State.AtFarm);
        timer = timerBeforeLeaving;

        if (returned && merchantBuySection != null)
            merchantBuySection.Restock();
    }

    private void EnterLeaving()
    {
        SetState(State.Leaving);
    }

    private void EnterAway()
    {
        SetState(State.Away);
        timer = timerBeforeArriving;
        if (carPrefab != null) carPrefab.SetActive(false);
    }

    private void EnterArriving()
    {
        SetState(State.Arriving);
        if (carPrefab != null) carPrefab.SetActive(true);
    }

    private void SetState(State next)
    {
        CurrentState = next;
        OnStateChanged?.Invoke(next);
    }
}
