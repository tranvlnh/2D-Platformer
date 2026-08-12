using UnityEngine;
using UnityEngine.InputSystem;

// ReSharper disable CheckNamespace

public class PlayerController : MonoBehaviour, PlayerInputActions.IPlayerActions {

	[Header("Ground Settings")]
	[SerializeField]
	float checkRadius = 0.2f;

	[SerializeField] Transform groundCheck;
	[SerializeField] LayerMask groundLayer;

	[Header("Movement Settings")]
	[SerializeField]
	float speedX = 8f;

	[SerializeField] float jumpForce = 12f;
	[SerializeField] float acceleration = 60f;
	[SerializeField] float coyoteTime = 0.15f;
	[SerializeField] float jumpBuffer = 0.15f;
	Animator _animator;

	float _coyoteTimeCounter;
	float _currentSpeedX;
	bool _doubleJump;

	bool _isGrounded;
	float _jumpBufferCounter;


	Rigidbody2D _rb;
	SpriteRenderer _spriteRenderer;
	float _targetSpeedX;

	void Awake()
	{
		_rb = GetComponent<Rigidbody2D>();
		_animator = GetComponent<Animator>();
		_spriteRenderer = GetComponent<SpriteRenderer>();
	}

	void FixedUpdate()
	{
		if (_rb.linearVelocityY <= 0.1f)
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
			_doubleJump = true;
		}
		else
		{
			_coyoteTimeCounter -= Time.fixedDeltaTime;
		}

		if (_jumpBufferCounter > 0f)
		{
			_jumpBufferCounter -= Time.fixedDeltaTime;
		}

		_currentSpeedX = Mathf.MoveTowards(_currentSpeedX, _targetSpeedX, acceleration*Time.fixedDeltaTime);
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
		{
			_animator.Play(_currentSpeedX != 0 ? "PlayerRun" : "PlayerIdle");
		}
		else
		{
			_animator.Play(_rb.linearVelocityY > 0f ? "PlayerJump" : "PlayerFall");
		}
	}

	async void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Hazard"))
		{
			_spriteRenderer.color = Color.red;
			await Awaitable.WaitForSecondsAsync(0.1f);
			_spriteRenderer.color = Color.white;
		}
	}

	void OnDrawGizmosSelected()
	{
		if (groundCheck == null) return;
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
	}

	public void OnMove(InputAction.CallbackContext context)
	{
		var axis = context.ReadValue<float>();
		_targetSpeedX = axis*speedX;

		_spriteRenderer.flipX = axis switch{
			< 0f => true,
			> 0f => false,
			_ => _spriteRenderer.flipX
		};
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			_jumpBufferCounter = jumpBuffer;
		}
	}

	void Jump()
	{
		_rb.linearVelocityY = 0f;
		_rb.AddForceY(jumpForce, ForceMode2D.Impulse);

		_jumpBufferCounter = 0f;
		_coyoteTimeCounter = 0f;
	}
}