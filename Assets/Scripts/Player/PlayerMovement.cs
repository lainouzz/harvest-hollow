using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent (typeof(CharacterController))]
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

    private void OnEnable()
    {
        inputActions.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        if (inputActions.Player.Jump.triggered)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        UpdateGrounded();
        Move();
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            Jump();
        }
        ApplyGravity();
    }

    private void UpdateGrounded()
    {
        isGrounded = controller.isGrounded;

        if(isGrounded && velocity.y < 0f)
        {
            velocity.y = 0f;
        } 
    }

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.fixedDeltaTime;
    }

    private void Move() 
    {
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();

        Vector3 move = (transform.right * input.x + transform.forward * input.y).normalized;

        Vector3 finalMove = move * moveSpeed + velocity;

        controller.Move(finalMove * Time.fixedDeltaTime);
    }

    private void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        jumpBufferCounter = 0;
        coyoteCounter = 0;
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
}
