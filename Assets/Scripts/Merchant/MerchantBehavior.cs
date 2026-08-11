using UnityEngine;

// Drives the merchant between the farm and an off-map point, waiting at each end.
public class MerchantBehavior : MonoBehaviour
{
    [SerializeField] private float driveSpeed = 5f;
    [Tooltip("Seconds parked at the farm before leaving.")]
    [SerializeField] private float timerBeforeLeaving = 30f;
    [Tooltip("Seconds away before coming back.")]
    [SerializeField] private float timerBeforeArriving = 20f;

    [SerializeField] private Transform startPos;
    [SerializeField] private Transform endPos;
    [SerializeField] private GameObject CarPrefab;

    private MerchantBuySection merchantBuySection;

    private enum State { AtFarm, Leaving, Away, Arriving }
    private State state;
    private float timer;

    private void Start()
    {
        if(merchantBuySection == null) merchantBuySection = FindAnyObjectByType<MerchantBuySection>();

        transform.position = startPos.position;
        if (CarPrefab != null) CarPrefab.SetActive(true);
        EnterAtFarm();
    }

    private void Update()
    {
        switch (state)
        {
            case State.AtFarm:
                if (Countdown()) state = State.Leaving;
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

    // Moves toward target at a constant speed; returns true once it arrives.
    private bool DriveTo(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, driveSpeed * Time.deltaTime);
        return transform.position == target;
    }

    private void EnterAtFarm()
    {
        state = State.AtFarm;
        timer = timerBeforeLeaving;
        merchantBuySection.Restock();
    }

    private void EnterAway()
    {
        state = State.Away;
        timer = timerBeforeArriving;
        if (CarPrefab != null) CarPrefab.SetActive(false);
    }

    private void EnterArriving()
    {
        state = State.Arriving;
        if (CarPrefab != null) CarPrefab.SetActive(true);
    }
}
