using UnityEngine;

[RequireComponent(typeof(MerchantBehavior))]
public class MerchantVFX : MonoBehaviour
{
    [Tooltip("Looping systems that should run only while driving (exhaust smoke, wheel dust).")]
    [SerializeField] private ParticleSystem[] drivingEffects;

    private MerchantBehavior merchant;

    private void Awake() => merchant = GetComponent<MerchantBehavior>();

    private void OnEnable()
    {
        merchant.OnStateChanged += OnStateChanged;
        VfxSettings.OnChanged += OnVfxChanged;
    }

    private void OnDisable()
    {
        merchant.OnStateChanged -= OnStateChanged;
        VfxSettings.OnChanged -= OnVfxChanged;
    }

    private void Start() => Refresh();

    private void OnStateChanged(MerchantBehavior.State state) => Refresh();
    private void OnVfxChanged(bool enabled) => Refresh();

    private void Refresh()
    {
        bool play = merchant.IsDriving && VfxSettings.Enabled;

        foreach (ParticleSystem fx in drivingEffects)
        {
            if (fx == null) continue;

            if (play && !fx.isPlaying) fx.Play();
            else if (!play && fx.isPlaying) fx.Stop();   // stops emitting, lets live particles fade
        }
    }
}
