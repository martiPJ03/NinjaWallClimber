using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private enum PlayerState
    {
        Idle,
        Jumping,
        Falling,
        Sliding
    }

    [Header("Current State")]
    [SerializeField] private PlayerState state;


    [Header("Jumping config")]
    [SerializeField] private float jumpForceX = 4f;
    [SerializeField] private float jumpForceY = 6f;
    private bool hasDoubleJumped = false;

    [Header("Sliding config")]
    [SerializeField] private float initialSlideSpeed = -1f;
    [SerializeField] private float maxSlideSpeed = -6f;
    [SerializeField] private float slideAcceleration = 2f;
    private float currentSlideSpeed;

    [Header("Falling config")]
    [SerializeField] private float fallMultiplier = 2f;

    [Header("Input Buffering")]
    [SerializeField] private float jumpBufferTime = 0.2f;
    private float jumpBufferTimer;

    private Rigidbody2D rb;
    private bool isFacingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        state = PlayerState.Idle;
    }

    private void Update()
    {
        if (jumpBufferTimer > 0)
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        HandleInput();
    }

    private void FixedUpdate()
    {
        bool hasBufferedJump = jumpBufferTimer > 0;
        // Execute the behavior of the current state and evaluate its transitions
        switch (state)
        {
            case PlayerState.Idle:
                if (hasBufferedJump) Jump();
                break;

            case PlayerState.Jumping:
                if (hasBufferedJump && !hasDoubleJumped)
                {
                    DoubleJump();
                }
                    // Transition to falling as soon as gravity starts pulling us down
                    if (rb.linearVelocity.y < 0)
                {
                    state = PlayerState.Falling;
                }
                break;

            case PlayerState.Falling:
                if (hasBufferedJump && !hasDoubleJumped)
                {
                    DoubleJump();
                }

                HandleFallingPhysics();
                // No specific physics behavior needed in mid-air falling, 
                // just waiting for OnCollisionEnter2D to hit a wall.
                break;

            case PlayerState.Sliding:
                if (hasBufferedJump)
                {

                    Jump();
                }
                else
                {
                    HandleSlidingPhysics();
                }
                break;
        }
    }

    private void HandleInput()
    {
        if (isInputDetected())
        {
            jumpBufferTimer = jumpBufferTime;
        }
    }

    private bool isInputDetected()
    {
        return (Pointer.current != null && Pointer.current.press.wasPressedThisFrame);
    }

    private void Jump()
    {
        jumpBufferTimer = 0f;

        rb.linearVelocity = new Vector2(isFacingRight ? jumpForceX : -jumpForceX, jumpForceY);
        isFacingRight = !isFacingRight;

        transform.localScale = new Vector3(isFacingRight ? 1f : -1f, 1f, 1f);

        state = PlayerState.Jumping;
    }

    private void HandleFallingPhysics()
    {
        rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
    }

    private void HandleSlidingPhysics()
    {
        currentSlideSpeed = Mathf.MoveTowards(currentSlideSpeed, maxSlideSpeed, slideAcceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(0f, currentSlideSpeed);
    }

    private void DoubleJump()
    {
        hasDoubleJumped = true;
        jumpBufferTimer = 0f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForceY);
    }

    public void OnWallEnter()
    {
        float rawVerticalVelocity = rb.linearVelocity.y;
        if (rawVerticalVelocity > 0)
        {
            currentSlideSpeed = 0;
        } else
        {
            float fallingSpeed = Mathf.Abs(rawVerticalVelocity);
            currentSlideSpeed = -Mathf.Clamp01(fallingSpeed / 10f) * 3;
        }
        state = PlayerState.Sliding;
    }

    public void OnWallExit()
    {
        hasDoubleJumped = false;
        // If the player slips off a wall ledge without jumping, drop them straight into falling
        if (state == PlayerState.Sliding)
        {
            state = PlayerState.Falling;
        }
    }

    public void Die()
    {
        // Handle player death (e.g., reset position, reduce lives, etc.)
        Debug.Log("Player has died.");
    }
}