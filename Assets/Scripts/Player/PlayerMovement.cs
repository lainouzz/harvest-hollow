using System.Collections.Generic;
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

    //----------Footsteps----------
    [SerializeField] private List<AudioClip> audioClips;
    [SerializeField] private AudioSource footstepSource;
    [Tooltip("Seconds between footsteps while walking.")]
    [SerializeField] private float stepInterval = 0.45f;

    private float stepTimer;
    private int lastStepIndex = -1;

    private Vector3 velocity = Vector3.zero;

    private GameInput inputActions;
    private CharacterController controller;

    private void Awake()
    {
        inputActions = new GameInput();
        controller = GetComponent<CharacterController>();
        if (footstepSource == null) footstepSource = GetComponent<AudioSource>();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();

    private void Update()
    {
        UpdateGrounded();
        HandleJumpTimers();

        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
            Jump();

        ApplyGravity();
        Move();
        HandleFootsteps();
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

    private void HandleFootsteps()
    {
        Vector3 horizontal = controller.velocity;
        horizontal.y = 0f;
        bool walking = isGrounded && horizontal.magnitude > 0.1f;

        if (!walking)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            PlayFootstep();
            stepTimer = stepInterval;
        }
    }

    private void PlayFootstep()
    {
        if (footstepSource == null || audioClips == null || audioClips.Count == 0) return;

        int index = Random.Range(0, audioClips.Count);

        if (audioClips.Count > 1 && index == lastStepIndex)
            index = (index + 1) % audioClips.Count;
        lastStepIndex = index;

        footstepSource.pitch = Random.Range(0.9f, 1.1f);
        footstepSource.PlayOneShot(audioClips[index]);
    }
}
