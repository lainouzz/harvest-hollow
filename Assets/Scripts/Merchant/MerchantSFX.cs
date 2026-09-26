using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MerchantBehavior))]
public class MerchantSFX : MonoBehaviour
{
    [Header("Sources (set Output + 3D settings here, clips go below)")]
    [SerializeField] private AudioSource idleSource;
    [SerializeField] private AudioSource driveSource;
    [SerializeField] private AudioSource oneShotSource;

    [Header("Engine clips")]
    [SerializeField] private AudioClip idleLoop;
    [SerializeField] private AudioClip driveStart;
    [SerializeField] private AudioClip driveLoop;

    [Header("Horn (optional)")]
    [SerializeField] private AudioClip arriveHorn;
    [SerializeField] private AudioClip leaveHorn;

    [Header("Mix")]
    [SerializeField, Range(0f, 1f)] private float idleVolume = 0.6f;
    [SerializeField, Range(0f, 1f)] private float driveVolume = 1f;
    [SerializeField] private float crossfadeTime = 0.5f;
    [Tooltip("Start the drive loop this many seconds before the drive-start clip ends, so they overlap.")]
    [SerializeField] private float startOverlap = 0.3f;

    private MerchantBehavior merchant;
    private Coroutine idleFade;
    private Coroutine driveFade;
    private Coroutine pendingDriveLoop;

    private void Awake()
    {
        merchant = GetComponent<MerchantBehavior>();
        SetupLoop(idleSource, idleLoop);
        SetupLoop(driveSource, driveLoop);
    }

    private void OnEnable() => merchant.OnStateChanged += OnStateChanged;
    private void OnDisable() => merchant.OnStateChanged -= OnStateChanged;

    private void Start() => ApplyState(merchant.CurrentState, fromEvent: false);

    private void OnStateChanged(MerchantBehavior.State state) => ApplyState(state, fromEvent: true);

    private void ApplyState(MerchantBehavior.State state, bool fromEvent)
    {
        switch (state)
        {
            case MerchantBehavior.State.AtFarm:
                CancelPendingDriveLoop();
                FadeTo(idleSource, idleVolume, ref idleFade);
                FadeTo(driveSource, 0f, ref driveFade);
                break;

            case MerchantBehavior.State.Leaving:
                if (fromEvent) PlayOneShot(leaveHorn);
                FadeTo(idleSource, 0f, ref idleFade);

                if (fromEvent && driveStart != null)
                {
                    PlayOneShot(driveStart);
                    CancelPendingDriveLoop();
                    pendingDriveLoop = StartCoroutine(StartDriveLoopAfter(driveStart.length - startOverlap));
                }
                else
                {
                    FadeTo(driveSource, driveVolume, ref driveFade);
                }
                break;

            case MerchantBehavior.State.Arriving:
                if (fromEvent) PlayOneShot(arriveHorn);
                CancelPendingDriveLoop();
                FadeTo(idleSource, 0f, ref idleFade);
                FadeTo(driveSource, driveVolume, ref driveFade);
                break;

            case MerchantBehavior.State.Away:
                CancelPendingDriveLoop();
                FadeTo(idleSource, 0f, ref idleFade);
                FadeTo(driveSource, 0f, ref driveFade);
                break;
        }
    }

    private IEnumerator StartDriveLoopAfter(float delay)
    {
        yield return new WaitForSeconds(Mathf.Max(0f, delay));
        pendingDriveLoop = null;
        FadeTo(driveSource, driveVolume, ref driveFade);
    }

    private void CancelPendingDriveLoop()
    {
        if (pendingDriveLoop == null) return;
        StopCoroutine(pendingDriveLoop);
        pendingDriveLoop = null;
    }

    private void FadeTo(AudioSource source, float target, ref Coroutine running)
    {
        if (source == null) return;
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(Fade(source, target));
    }

    private IEnumerator Fade(AudioSource source, float target)
    {
        if (target > 0f && !source.isPlaying)
        {
            source.volume = 0f;
            source.Play();
        }

        float start = source.volume;
        float t = 0f;
        while (t < crossfadeTime)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(start, target, t / crossfadeTime);
            yield return null;
        }

        source.volume = target;
        if (target <= 0f) source.Stop();
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (oneShotSource != null && clip != null)
            oneShotSource.PlayOneShot(clip);
    }

    private static void SetupLoop(AudioSource source, AudioClip clip)
    {
        if (source == null) return;

        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0f;
        source.Stop();
    }
}
