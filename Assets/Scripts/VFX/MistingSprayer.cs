using System.Collections;
using UnityEngine;

// Every so often, one of the greenhouse nozzles puffs a spray of mist: a visual hint
// that the beds water themselves. Only runs once the greenhouse is restored.
public class MistingSprayer : MonoBehaviour
{
    [SerializeField] private Greenhouse greenhouse;

    [Tooltip("The spray effects, e.g. one per nozzle or one per bed.")]
    [SerializeField] private ParticleSystem[] sprays;

    [Tooltip("Seconds between bursts (picked randomly in this range).")]
    [SerializeField] private Vector2 interval = new Vector2(6f, 14f);

    [Tooltip("How many sprays go off together in one burst.")]
    [SerializeField] private Vector2Int sprayCount = new Vector2Int(1, 3);

    [Header("Sound (optional)")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] hissClips;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    private void Awake()
    {
        if (greenhouse == null) greenhouse = GetComponentInParent<Greenhouse>();
    }

    private void OnEnable() => StartCoroutine(Loop());

    private IEnumerator Loop()
    {
        while (true)
        {
            // Scaled time, so nothing sprays while the game is paused.
            yield return new WaitForSeconds(Random.Range(interval.x, interval.y));

            if (sprays.Length == 0 || !VfxSettings.Enabled) continue;
            if (greenhouse != null && !greenhouse.IsOwned) continue;

            Burst();
        }
    }

    private void Burst()
    {
        int count = Mathf.Clamp(Random.Range(sprayCount.x, sprayCount.y + 1), 1, sprays.Length);
        int start = Random.Range(0, sprays.Length);

        // Neighbouring nozzles, starting at a random one, so a burst reads as one pipe section.
        for (int i = 0; i < count; i++)
        {
            ParticleSystem ps = sprays[(start + i) % sprays.Length];
            if (ps != null) ps.Play();
        }

        if (source != null && hissClips != null && hissClips.Length > 0)
        {
            source.pitch = Random.Range(pitchRange.x, pitchRange.y);
            source.PlayOneShot(hissClips[Random.Range(0, hissClips.Length)]);
        }
    }
}
