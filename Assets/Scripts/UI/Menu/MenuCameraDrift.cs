using UnityEngine;

public class MenuCameraDrift : MonoBehaviour
{
    [SerializeField] private Transform pivot;
    [SerializeField] private float orbitSpeed = 3f;
    [SerializeField] private float bobAmount = 0.25f;
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
