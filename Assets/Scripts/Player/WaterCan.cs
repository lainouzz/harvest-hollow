using System;
using UnityEngine;

// How much water the watering can holds. Each successful watering uses one; the well
// fills it back up. Put this on the Player (an always-active object), not on the can
// model, which is switched off whenever you hold a different tool.
public class WaterCan : MonoBehaviour
{
    [SerializeField] private int capacity = 10;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip emptyCanClip;
    [SerializeField] private AudioClip refillClip;

    // Tracked as water *used* so a missing value in an old save means a full can.
    private int used;

    public event Action OnWaterChanged;

    public int Capacity => capacity;
    public int Water => capacity - used;
    public bool IsEmpty => Water <= 0;
    public bool IsFull => used <= 0;

    // For the save system.
    public int Used
    {
        get => used;
        set
        {
            used = Mathf.Clamp(value, 0, capacity);
            OnWaterChanged?.Invoke();
        }
    }

    public void Use()
    {
        if (IsEmpty) return;
        used++;
        OnWaterChanged?.Invoke();
    }

    public void Refill()
    {
        if (IsFull) return;
        used = 0;
        Play(refillClip);
        OnWaterChanged?.Invoke();
    }

    public void PlayEmptySound() => Play(emptyCanClip);

    private void Play(AudioClip clip)
    {
        if (source != null && clip != null && source.isActiveAndEnabled)
            source.PlayOneShot(clip);
    }
}
