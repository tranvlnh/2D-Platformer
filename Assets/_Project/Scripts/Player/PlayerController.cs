using UnityEngine;
using UnityEngine.InputSystem;

// ReSharper disable CheckNamespace

public class PlayerController : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Animator animator;

    [Header("Movement Settings")] [SerializeField]
    private float speedX = 8f;

    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBuffer = 0.15f;
    private float _coyoteTimeCounter;
    private float _currentSpeedX;

    private bool _isGrounded;
    private float _jumpBufferCounter;
    private float _targetSpeedX;

    void FixedUpdate()
    {
        if (rb.linearVelocityY <= 0.1f)
        {
            _isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        }
        else
        {
            _isGrounded = false;
        }

        if (_isGrounded)
        {
            _coyoteTimeCounter = coyoteTime;
        }
        else
        {
            _coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        if (_jumpBufferCounter > 0f)
        {
            _jumpBufferCounter -= Time.fixedDeltaTime;
        }

        _currentSpeedX = Mathf.MoveTowards(_currentSpeedX, _targetSpeedX, acceleration * Time.fixedDeltaTime);
        rb.linearVelocityX = _currentSpeedX;

        if (_jumpBufferCounter > 0f && _coyoteTimeCounter > 0f)
        {
            rb.linearVelocityY = 0f;
            rb.AddForceY(jumpForce, ForceMode2D.Impulse);

            _jumpBufferCounter = 0f;
            _coyoteTimeCounter = 0f;
        }

        if (_isGrounded)
        {
            animator.Play(_currentSpeedX != 0 ? "PlayerRun" : "PlayerIdle");
        }
        else
        {
            if (rb.linearVelocityY > 0f)
                animator.Play("PlayerJump");
            else
                animator.Play("PlayerFall");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        var axis = context.ReadValue<float>();
        _targetSpeedX = axis * speedX;

        var localScale = transform.localScale;
        localScale.x = axis switch
        {
            < 0f => -1,
            > 0f => 1,
            _ => localScale.x
        };

        transform.localScale = localScale;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _jumpBufferCounter = jumpBuffer;
        }
    }
}