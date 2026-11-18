using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Greenhouse : MonoBehaviour
{
    public enum PurchaseMode { BrokenRestore, Locked }

    [SerializeField] private PurchaseMode mode = PurchaseMode.BrokenRestore;
    [SerializeField] private int price;
    [SerializeField] private bool owned;
    [SerializeField] private ParticleSystem purchaseFX;
    [SerializeField] private float restoreSwapDelay;
    public UnityEvent onPurchased;

    [SerializeField] private float doorOpenAngle = 100f;
    [SerializeField] private float doorDuration = 0.45f;
    [SerializeField] private string autoOpenTag = "Player";

    [SerializeField] private Transform doorL, doorR, lockObj, sign;
    [SerializeField] private GameObject[] brokenParts, intactParts;

    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip audioClip;

    [Tooltip("The raised beds inside. They can't be used until the greenhouse is restored.")]
    [SerializeField] private FarmGrid[] beds;

    public bool IsOwned => owned;
    public int Price => price;
    public bool DoorsOpen { get; private set; }

    // The merchant only stocks greenhouse seeds once this is true.
    public static bool AnyOwned { get; private set; }

    private bool playerNear;

    Quaternion closedL, closedR, openL, openR;
    Coroutine doorRoutine;

    void Reset() => AutoAssign();

    private void Awake()
    {
        if (!doorL) AutoAssign();
        closedL = doorL.localRotation;
        closedR = doorR.localRotation;
        openL = OpenRotation(doorL);
        openR = OpenRotation(doorR);
        ApplyState();
    }

    private void AutoAssign()
    {
        doorL = Find("Door_L"); doorR = Find("Door_R");
        lockObj = Find("Lock"); sign = Find("PurchaseSign");
        brokenParts = new[] { Go("Glass_Broken"), Go("Debris_Broken"), Go("DoorGlass_L_Broken"), Go("DoorGlass_R_Broken") };
        intactParts = new[] { Go("Glass_Intact"), Go("DoorGlass_L_Intact"), Go("DoorGlass_R_Intact") };
    }

    GameObject Go(string n) => Find(n)?.gameObject;

    private Transform Find(string n)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true)) if (t.name == n) return t;
        return null;
    }

    public bool TryPurchase(Func<int, bool> trySpend)
    {
        if (owned || !trySpend(price)) return false;
        owned = true;
        StartCoroutine(PurchaseRoutine());

        return true;
    }

    // For save/load: sets the state instantly, no effects.
    public void SetOwned(bool value)
    {
        owned = value;
        if (!owned) SetDoors(false, instant: true);
        ApplyState();
    }

    IEnumerator PurchaseRoutine()
    {
        if(purchaseFX) purchaseFX.Play();
        if(source != null)
        {
            source.PlayOneShot(audioClip);
        }

        if(mode == PurchaseMode.Locked && lockObj && lockObj.gameObject.activeSelf)
        {
            Vector3 start = lockObj.position, end = start - transform.up * 0.35f + transform.forward * 0.1f;
            Quaternion r0 = lockObj.rotation, r1 = r0 * Quaternion.Euler(0, 0, 70f);
            for (float t = 0; t < 1f; t += Time.deltaTime / 0.35f)
            {
                float e = t * t;
                lockObj.SetPositionAndRotation(Vector3.Lerp(start, end, e), Quaternion.Slerp(r0, r1, e));
                yield return null;
            }
            lockObj.SetPositionAndRotation(start, r0);
        }
        else if(mode == PurchaseMode.BrokenRestore && purchaseFX)
        {
            yield return new WaitForSeconds(restoreSwapDelay);
        }
        ApplyState();
        onPurchased?.Invoke();
        if (playerNear) OpenDoors();
    }

    private void ApplyState()
    {
        AnyOwned = owned;
        foreach (FarmGrid bed in beds) if (bed) bed.enabled = owned;

        bool showBroken = mode == PurchaseMode.BrokenRestore && !owned;

        foreach (var g in brokenParts) if (g) g.SetActive(showBroken);
        foreach (var g in intactParts) if (g) g.SetActive(!showBroken);
        if (lockObj) lockObj.gameObject.SetActive(mode == PurchaseMode.Locked && !owned);
        if (sign) sign.gameObject.SetActive(!owned);
    }

    public void ToogleDoors() => SetDoors(!DoorsOpen);
    public void OpenDoors() => SetDoors(true);
    public void CloseDoors() => SetDoors(false);


    private void SetDoors(bool open, bool instant = false)
    {
        if (open && !owned) return;
        DoorsOpen = open;
        if(doorRoutine != null) StopCoroutine(doorRoutine);
        if (instant || !isActiveAndEnabled)
        {
            doorL.localRotation = open ? openL : closedL;
            doorR.localRotation = open ? openR : closedR;
            return;
        }
        doorRoutine = StartCoroutine(AnimateDoors(open));
    }

    IEnumerator AnimateDoors(bool open)
    {
        Quaternion fromL = doorL.localRotation, fromR = doorR.localRotation;
        Quaternion toL = open ? openL : closedL, toR = open ? openR : closedR;
        for(float t = 0; t < 1f; t += Time.deltaTime / doorDuration)
        {
            float e = Mathf.SmoothStep(0, 1f, t);
            doorL.localRotation = Quaternion.Slerp(fromL, toL, e);
            doorR.localRotation = Quaternion.Slerp(fromR, toR, e);
            yield return null;
        }
        doorL.localRotation = toL; doorR.localRotation = toR;
    }

    private Quaternion OpenRotation(Transform door)
    {
        Vector3 up = transform.up;
        Vector3 pivot = door.position;
        Vector3 c = door.GetComponent<Renderer>().bounds.center;
        float Dist(float s)
        {
            Vector3 p = pivot + Quaternion.AngleAxis(s * 90f, up) * (c - pivot) - transform.position;
            return Vector3.ProjectOnPlane(p, up).sqrMagnitude;
        }
        float sign = Dist(1f) > Dist(-1f) ? 1f : -1f;
        Vector3 localUp = door.parent.InverseTransformDirection(up);
        return Quaternion.AngleAxis(sign * doorOpenAngle, localUp) * door.localRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(autoOpenTag)) return;
        playerNear = true;
        if (owned) OpenDoors();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(autoOpenTag)) return;
        playerNear = false;
        if (owned) CloseDoors();
    }

#if UNITY_EDITOR
    void OnValidate() { if (Application.isPlaying && doorL) ApplyState(); }
#endif
}
