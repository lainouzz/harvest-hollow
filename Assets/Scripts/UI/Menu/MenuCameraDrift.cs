using UnityEngine;

// Slowly orbits (and gently bobs) the menu camera around the farm so the backdrop
// feels alive instead of being a frozen shot.
public class MenuCameraDrift : MonoBehaviour
{
    [SerializeField] private Transform pivot;          // point to orbit around (farm center)
    [SerializeField] private float orbitSpeed = 3f;    // degrees per second
    [SerializeField] private float bobAmount = 0.25f;  // vertical sway height
    [SerializeField] private float bobSpeed = 0.5f;

    private float startY;

    private void Start()
    {
        startY = transform.position.y;
    }

    private void Update()
    {
        if (pivot != null)
            transform.RotateAround(pivot.position, Vector3.up, orbitSpeed * Time.deltaTime);

        Vector3 pos = transform.position;
        pos.y = startY + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = pos;

        if (pivot != null)
            transform.LookAt(pivot);
    }
}
