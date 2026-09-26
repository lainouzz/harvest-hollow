using TMPro;
using UnityEngine;

// Floats an icon over every plot you can buy next, showing its price.
[RequireComponent(typeof(FarmGrid))]
public class PurchasableSlotIcons : MonoBehaviour
{
    [Tooltip("Icon prefab. Put a Billboard on it so it faces the camera. If it has a " +
             "TMP text anywhere inside, the price is written there.")]
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private float height = 0.8f;
    [Tooltip("{0} = the price, e.g. \"[B] {0}g\".")]
    [SerializeField] private string priceFormat = "{0}";

    private FarmGrid grid;
    private FarmExpansion expansion;
    private Transform root;

    private void Awake()
    {
        grid = GetComponent<FarmGrid>();
        expansion = GetComponent<FarmExpansion>();
    }

    private void OnEnable() => grid.OnGridChanged += Rebuild;
    private void OnDisable() => grid.OnGridChanged -= Rebuild;
    private void Start() => Rebuild();

    private void Rebuild()
    {
        if (root != null) Destroy(root.gameObject);
        if (iconPrefab == null) return;

        root = new GameObject("PurchasableIcons").transform;
        root.SetParent(transform, worldPositionStays: false);

        string price = expansion != null ? string.Format(priceFormat, expansion.NextSlotCost) : null;

        for (int x = 0; x < grid.Width; ++x)
        {
            for (int z = 0; z < grid.Height; ++z)
            {
                Vector2Int cell = new Vector2Int(x, z);
                if (!grid.IsPurchasable(cell)) continue;

                Vector3 pos = grid.GridToWorld(cell) + Vector3.up * height;
                GameObject icon = Instantiate(iconPrefab, pos, Quaternion.identity, root);

                if (price == null) continue;
                TMP_Text label = icon.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = price;
            }
        }
    }
}
