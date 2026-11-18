using UnityEngine;

// The little sign on a plot that shows what's planted there. Lives on the ground-tile
// prefab; FarmGrid shows it when a seed is planted and hides it on harvest.
public class CropMarker : MonoBehaviour
{
    [Tooltip("The sign model to show/hide. Empty = this object.")]
    [SerializeField] private GameObject sign;
    [Tooltip("SpriteRenderer sitting on the sign's face.")]
    [SerializeField] private SpriteRenderer icon;

    private Transform pivot;
    private Vector3 offset;
    private Quaternion baseRotation;
    private bool captured;

    private void Awake()
    {
        if (sign == null) sign = gameObject;
        Capture();
        Hide();
    }

    // Remembers where the sign sits on the tile, so it can be turned around the tile's centre.
    private void Capture()
    {
        if (captured) return;
        captured = true;

        GroundTile tile = GetComponentInParent<GroundTile>(true);
        pivot = tile != null ? tile.transform : transform.parent;
        offset = pivot != null ? transform.position - pivot.position : Vector3.zero;
        baseRotation = transform.rotation;
    }

    // Swings the sign around the tile's centre, e.g. 180 = opposite corner, facing the other way.
    public void SetYaw(float degrees)
    {
        Capture();
        Quaternion turn = Quaternion.Euler(0f, degrees, 0f);
        transform.rotation = turn * baseRotation;
        if (pivot != null) transform.position = pivot.position + turn * offset;
    }

    public void Show(Sprite sprite)
    {
        if (icon != null)
        {
            icon.sprite = sprite;
            icon.enabled = sprite != null;
        }

        if (sign != null) sign.SetActive(true);
    }

    public void Hide()
    {
        if (sign != null) sign.SetActive(false);
    }
}
