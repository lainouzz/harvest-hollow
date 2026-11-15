using System.Collections.Generic;
using UnityEngine;

// Fades a world icon or sign out with distance from the camera, so only nearby plots
// show detail. Uses a CanvasGroup on this object if there is one (world-space canvas
// icons); otherwise fades SpriteRenderers and switches other renderers off when far.
public class DistanceFade : MonoBehaviour
{
    [SerializeField] private float showDistance = 8f;
    [Tooltip("Distance over which it fades out after Show Distance.")]
    [SerializeField] private float fadeRange = 2f;

    private Transform cam;
    private CanvasGroup group;
    private SpriteRenderer[] sprites;
    private Color[] spriteColors;
    private Renderer[] meshes;
    private float shownAlpha = -1f;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (group != null) return;

        sprites = GetComponentsInChildren<SpriteRenderer>(true);
        spriteColors = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++) spriteColors[i] = sprites[i].color;

        List<Renderer> others = new();
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
            if (r is not SpriteRenderer) others.Add(r);
        meshes = others.ToArray();
    }

    private void LateUpdate()
    {
        if (cam == null)
        {
            if (Camera.main == null) return;
            cam = Camera.main.transform;
        }

        float distance = Vector3.Distance(cam.position, transform.position);
        float alpha = 1f - Mathf.Clamp01((distance - showDistance) / Mathf.Max(0.01f, fadeRange));
        if (Mathf.Approximately(alpha, shownAlpha)) return;

        shownAlpha = alpha;
        Apply(alpha);
    }

    private void Apply(float alpha)
    {
        if (group != null)
        {
            group.alpha = alpha;
            return;
        }

        for (int i = 0; i < sprites.Length; i++)
        {
            Color c = spriteColors[i];
            c.a *= alpha;
            sprites[i].color = c;
        }

        // Opaque meshes can't fade, so they just switch off once fully faded.
        bool visible = alpha > 0.01f;
        foreach (Renderer r in meshes) r.enabled = visible;
    }
}
