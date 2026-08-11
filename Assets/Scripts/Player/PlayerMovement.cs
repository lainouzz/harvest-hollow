using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    //----------Movement VARIABLES----------
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float gravity;
    [SerializeField]
    private float groundCheckRadius;

    //----------JUMP VARIABLES----------
    [SerializeField]
    private float jumpHeight;
    [SerializeField]
    private bool isGrounded;

    private float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;
    private float coyoteTime = 0.3f;
    private float coyoteCounter;

    //----------Refences----------
    [SerializeField]
    private Transform groundPos;
    [SerializeField]
    private LayerMask layerMask;

    private Vector3 velocity = Vector3.zero;

    private GameInput inputActions;
    private CharacterController controller;

    private void Awake()
    {
        inputActions = new GameInput();
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    // CharacterController has no physics interpolation, so moving it in FixedUpdate
    // makes it step at the physics rate and jitter. Drive it in Update instead.
    private void Update()
    {
        UpdateGrounded();
        HandleJumpTimers();

        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
            Jump();

        ApplyGravity();
        Move();
    }

    private void UpdateGrounded()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0f)
            velocity.y = 0f;
    }

    private void HandleJumpTimers()
    {
        coyoteCounter = isGrounded ? coyoteTime : coyoteCounter - Time.deltaTime;

        jumpBufferCounter = inputActions.Player.Jump.triggered
            ? jumpBufferTime
            : jumpBufferCounter - Time.deltaTime;
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
    }

    private void Move()
    {
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();

        Vector3 move = (transform.right * input.x + transform.forward * input.y).normalized;
        Vector3 finalMove = move * moveSpeed + velocity;

        controller.Move(finalMove * Time.deltaTime);
    }

    private void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        jumpBufferCounter = 0;
        coyoteCounter = 0;
    }
}
