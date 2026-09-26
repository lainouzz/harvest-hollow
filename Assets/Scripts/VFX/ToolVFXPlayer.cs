using System.Collections.Generic;
using UnityEngine;

public class ToolVFXPlayer : MonoBehaviour
{
    public enum ToolVFX { Till, Water, Plant, Harvest }

    [System.Serializable]
    public class VFXEntry
    {
        public ToolVFX action;
        public ParticleSystem prefab;
        public float yOffset = 0.05f;
    }

    [SerializeField] private List<VFXEntry> effects = new();

    public void Play(ToolVFX action, Vector3 worldPos)
    {
        VFXEntry entry = Find(action);
        if (entry == null || entry.prefab == null) return;

        ParticleSystem fx = Instantiate(entry.prefab, worldPos + Vector3.up * entry.yOffset, Quaternion.Euler(-90, 0, 0));
        fx.Play();

        ParticleSystem.MainModule main = fx.main;
        Destroy(fx.gameObject, main.duration + main.startLifetime.constantMax);
    }

    private VFXEntry Find(ToolVFX action)
    {
        foreach (VFXEntry e in effects)
            if (e.action == action) return e;
        return null;
    }
}
