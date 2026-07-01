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

    [Header("Jumping config")]
    [SerializeField] private float jumpForceX = 4f;
    [SerializeField] private float jumpForceY = 6f;
    [SerializeField] private float slideSpeed = -2f;

    private Rigidbody2D rb;
    private bool isFacingRight = true;
    private PlayerState state;
    private bool shouldJump = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        state = PlayerState.Idle;
    }

    void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        // Execute the behavior of the current state and evaluate its transitions
        switch (state)
        {
            case PlayerState.Idle:
                if (shouldJump) Jump();
                break;

            case PlayerState.Jumping:
                // Transition to falling as soon as gravity starts pulling us down
                if (rb.linearVelocity.y < 0)
                {
                    state = PlayerState.Falling;
                }
                break;

            case PlayerState.Falling:
                // No specific physics behavior needed in mid-air falling, 
                // just waiting for OnCollisionEnter2D to hit a wall.
                break;

            case PlayerState.Sliding:
                if (shouldJump)
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
        // Cleaner input checking: You can only jump if you are securely attached to a wall or starting line
        if (isInputDetected() && (state == PlayerState.Idle || state == PlayerState.Sliding))
        {
            shouldJump = true;
        }
    }

    private bool isInputDetected()
    {
        return (Pointer.current != null && Pointer.current.press.wasPressedThisFrame);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(isFacingRight ? jumpForceX : -jumpForceX, jumpForceY);
        isFacingRight = !isFacingRight;

        transform.localScale = new Vector3(isFacingRight ? 1f : -1f, 1f, 1f);

        shouldJump = false;
        state = PlayerState.Jumping;
    }

    private void HandleSlidingPhysics()
    {
        rb.linearVelocity = new Vector2(0f, slideSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            state = PlayerState.Sliding;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // If the player slips off a wall ledge without jumping, drop them straight into falling
        if (collision.gameObject.CompareTag("Wall") && state == PlayerState.Sliding)
        {
            state = PlayerState.Falling;
        }
    }
}