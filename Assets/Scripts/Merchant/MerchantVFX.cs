using UnityEngine;

// Plays looping exhaust/dust particles while the truck drives and stops them when it
// parks. Put this on the truck; assign particle systems that live on the truck model.
[RequireComponent(typeof(MerchantBehavior))]
public class MerchantVFX : MonoBehaviour
{
    [Tooltip("Looping systems that should run only while driving (exhaust smoke, wheel dust).")]
    [SerializeField] private ParticleSystem[] drivingEffects;

    private MerchantBehavior merchant;

    private void Awake() => merchant = GetComponent<MerchantBehavior>();

    private void OnEnable() => merchant.OnStateChanged += OnStateChanged;
    private void OnDisable() => merchant.OnStateChanged -= OnStateChanged;

    private void Start() => SetDriving(merchant.IsDriving);

    private void OnStateChanged(MerchantBehavior.State state) => SetDriving(merchant.IsDriving);

    private void SetDriving(bool driving)
    {
        foreach (ParticleSystem fx in drivingEffects)
        {
            if (fx == null) continue;

            if (driving && !fx.isPlaying) fx.Play();
            else if (!driving && fx.isPlaying) fx.Stop();   // stops emitting, lets live particles fade
        }
    }
}
