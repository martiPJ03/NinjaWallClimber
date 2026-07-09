using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private enum PlayerState
    {
        Idle = 0,
        Jumping = 1,
        Falling = 2,
        Sliding = 3,
        Dead = 4
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

    private Animator animator;
    private static readonly int StateParam = Animator.StringToHash("State");

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        SetState(PlayerState.Idle);
    }

    private void Update()
    {
        if (state == PlayerState.Dead) return;

        if (jumpBufferTimer > 0)
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        HandleInput();
    }

    private void FixedUpdate()
    {
        if (state == PlayerState.Dead) return;

        bool hasBufferedJump = jumpBufferTimer > 0;

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

                if (rb.linearVelocity.y < 0)
                {
                    SetState(PlayerState.Falling);
                }
                break;

            case PlayerState.Falling:
                if (hasBufferedJump && !hasDoubleJumped)
                {
                    DoubleJump();
                }

                HandleFallingPhysics();
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

    private void SetState(PlayerState newState)
    {
        // CRITICAL GUARD: If we are already Dead, do not allow switching to any other state
        if (state == PlayerState.Dead) return;

        state = newState;
        if (animator != null)
        {
            animator.SetInteger(StateParam, (int)newState);
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

        SetState(PlayerState.Jumping);
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
        // Added check so wall collisions can't resurrect a dead player object
        if (state == PlayerState.Dead) return;

        float rawVerticalVelocity = rb.linearVelocity.y;
        if (rawVerticalVelocity > 0)
        {
            currentSlideSpeed = 0;
        }
        else
        {
            float fallingSpeed = Mathf.Abs(rawVerticalVelocity);
            currentSlideSpeed = -Mathf.Clamp01(fallingSpeed / 10f) * 3;
        }
        SetState(PlayerState.Sliding);
    }

    public void OnWallExit()
    {
        if (state == PlayerState.Dead) return;

        hasDoubleJumped = false;
        if (state == PlayerState.Sliding)
        {
            SetState(PlayerState.Falling);
        }
    }

    public void Die()
    {
        Debug.Log("Player has died.");
        SetState(PlayerState.Dead);
    }
}