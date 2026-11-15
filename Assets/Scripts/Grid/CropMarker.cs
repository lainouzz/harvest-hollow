using UnityEngine;

// The little sign on a plot that shows what's planted there. Lives on the ground-tile
// prefab; FarmGrid shows it when a seed is planted and hides it on harvest.
public class CropMarker : MonoBehaviour
{
    [Tooltip("The sign model to show/hide. Empty = this object.")]
    [SerializeField] private GameObject sign;
    [Tooltip("SpriteRenderer sitting on the sign's face.")]
    [SerializeField] private SpriteRenderer icon;

    private void Awake()
    {
        if (sign == null) sign = gameObject;
        Hide();
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
