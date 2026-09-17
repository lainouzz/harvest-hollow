using UnityEngine;

// Reacts to the merchant's state: loops an engine sound while driving and honks the
// horn on arrival and departure. Put this on the truck (alongside MerchantBehavior).
[RequireComponent(typeof(MerchantBehavior))]
public class MerchantSFX : MonoBehaviour
{
    [Header("Engine")]
    [Tooltip("Looping engine source. Set Spatial Blend to 3D so it swells as you approach.")]
    [SerializeField] private AudioSource engineSource;

    [Header("Horn (one-shots)")]
    [SerializeField] private AudioSource hornSource;
    [SerializeField] private AudioClip arriveHorn;
    [SerializeField] private AudioClip leaveHorn;

    private MerchantBehavior merchant;

    private void Awake() => merchant = GetComponent<MerchantBehavior>();

    private void OnEnable() => merchant.OnStateChanged += OnStateChanged;
    private void OnDisable() => merchant.OnStateChanged -= OnStateChanged;

    private void Start() => SetEngine(merchant.IsDriving);   // match whatever state we start in

    private void OnStateChanged(MerchantBehavior.State state)
    {
        switch (state)
        {
            case MerchantBehavior.State.Arriving:
                SetEngine(true);
                Honk(arriveHorn);
                break;

            case MerchantBehavior.State.Leaving:
                SetEngine(true);
                Honk(leaveHorn);
                break;

            case MerchantBehavior.State.AtFarm:
            case MerchantBehavior.State.Away:
                SetEngine(false);
                break;
        }
    }

    private void SetEngine(bool on)
    {
        if (engineSource == null) return;

        if (on && !engineSource.isPlaying) engineSource.Play();
        else if (!on && engineSource.isPlaying) engineSource.Stop();
    }

    private void Honk(AudioClip clip)
    {
        if (hornSource != null && clip != null)
            hornSource.PlayOneShot(clip);
    }
}
