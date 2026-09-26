using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLook : MonoBehaviour
{
    public Transform camera;
    public Transform playerBody;

    [Range(1f, 10f)]
    [SerializeField]private float sensX;
    [Range(1f, 10f)]
    [SerializeField]private float sensY;
    private float verticalRot = 0f;

    private float mouseX;
    private float mouseY;

    public bool isInShopUI;

    private GameInput inputActions;

    private void Awake()
    {
        inputActions = new GameInput();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isInShopUI = false;
    }

    void Update()
    {
        Vector2 lookRotation = inputActions.Player.Look.ReadValue<Vector2>();

        if (!isInShopUI)
        {
            LookX(lookRotation.x);
            LookY(lookRotation.y);
        }
    }

    void LookX(float lookX)
    {
        mouseX = lookX * sensX * Time.deltaTime;
        playerBody.Rotate(Vector3.up * mouseX);
    }

    void LookY(float lookY)
    {
        mouseY = lookY * sensY * Time.deltaTime;

        verticalRot -= mouseY;
        verticalRot = Mathf.Clamp(verticalRot, -85f, 85f);

        camera.localRotation = Quaternion.Euler(verticalRot, 0f, 0f);
    }
}
