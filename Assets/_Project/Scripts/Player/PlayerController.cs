using System;
using UnityEngine;
using UnityEngine.InputSystem;

// ReSharper disable CheckNamespace

public class PlayerController : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    [Header("Ground Settings")] [SerializeField]
    private float checkRadius = 0.2f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Movement Settings")] [SerializeField]
    private float speedX = 8f;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int currentAmmo = 10;

    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBuffer = 0.15f;
    private Animator _animator;

    private float _coyoteTimeCounter;
    private float _currentSpeedX;
    private bool _doubleJump;

    private bool _isGrounded;
    private float _jumpBufferCounter;


    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private float _targetSpeedX;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        OnPlayerShoot += (remainingAmmo, shootDirection) =>
        {
            Debug.Log($"<color=yellow>[BẮN]</color> Đã bắn 1 viên! Đạn còn lại: <b>{remainingAmmo}</b>");

            _currentSpeedX = -shootDirection.x * 5f;
        };
    }

    private void FixedUpdate()
    {
        if (_rb.linearVelocityY <= 0.1f)
            _isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        else
            _isGrounded = false;

        if (_isGrounded)
        {
            _coyoteTimeCounter = coyoteTime;
            _doubleJump = true;
        }
        else
        {
            _coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        if (_jumpBufferCounter > 0f) _jumpBufferCounter -= Time.fixedDeltaTime;

        _currentSpeedX = Mathf.MoveTowards(_currentSpeedX, _targetSpeedX, acceleration * Time.fixedDeltaTime);
        _rb.linearVelocityX = _currentSpeedX;

        if (_jumpBufferCounter > 0f && _coyoteTimeCounter > 0f)
        {
            Jump();
        }
        else if (_jumpBufferCounter > 0 && _doubleJump)
        {
            Jump();
            _doubleJump = false;
        }

        if (_isGrounded)
            _animator.Play(_currentSpeedX != 0 ? "PlayerRun" : "PlayerIdle");
        else
            _animator.Play(_rb.linearVelocityY > 0f ? "PlayerJump" : "PlayerFall");
    }

    private async void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Hazard"))
        {
            _spriteRenderer.color = Color.red;
            await Awaitable.WaitForSecondsAsync(0.1f);
            _spriteRenderer.color = Color.white;
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

        _spriteRenderer.flipX = axis switch
        {
            < 0f => true,
            > 0f => false,
            _ => _spriteRenderer.flipX
        };
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started) _jumpBufferCounter = jumpBuffer;
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (currentAmmo <= 0)
            {
                Debug.Log("<color=red>Hết đạn! Cần nạp đạn (Reload).</color>");
                return;
            }

            if (bulletPrefab == null) return;

            Instantiate(bulletPrefab, transform.position, transform.rotation);

            currentAmmo--;

            Vector2 shootDirection = transform.right;
            OnPlayerShoot?.Invoke(currentAmmo, shootDirection);
        }
    }

    public static event Action<int, Vector2> OnPlayerShoot;

    private void Jump()
    {
        _rb.linearVelocityY = 0f;
        _rb.AddForceY(jumpForce, ForceMode2D.Impulse);

        _jumpBufferCounter = 0f;
        _coyoteTimeCounter = 0f;
    }
}