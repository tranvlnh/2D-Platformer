using UnityEngine;
using UnityEngine.InputSystem;

// ReSharper disable CheckNamespace

public class PlayerController : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Movement Settings")] [SerializeField]
    private float speedX = 8f;

    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBuffer = 0.15f;

    private bool _isGrounded;
    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;
    private float _targetSpeedX;
    private float _currentSpeedX;

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
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _targetSpeedX = context.ReadValue<float>() * speedX;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _jumpBufferCounter = jumpBuffer;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
    }
}