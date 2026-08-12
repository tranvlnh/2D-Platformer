using UnityEngine;

// ReSharper disable CheckNamespace

public class PatrollingEnemy : MonoBehaviour {
	[Header("Movement Settings")]
	[SerializeField]
	Transform[] waypoints;

	[SerializeField] float speed = 3f;
	[SerializeField] float waitTime = 1f;

	Animator _animator;
	int _index;
	bool _isWaiting;
	Rigidbody2D _rb;
	SpriteRenderer _spriteRenderer;
	float _waitTimer;

	void Awake()
	{
		_animator = GetComponent<Animator>();
		_spriteRenderer = GetComponent<SpriteRenderer>();
		_rb = GetComponent<Rigidbody2D>();
	}

	void FixedUpdate()
	{
		if (_isWaiting)
		{
			_waitTimer -= Time.fixedDeltaTime;
			if (_waitTimer <= 0)
				_isWaiting = false;
			return;
		}

		var target = (Vector2)waypoints[_index].position;
		if (Vector2.Distance(_rb.position, target) > 0.05f)
		{
			var newPos = Vector2.MoveTowards(_rb.position, target, speed*Time.fixedDeltaTime);
			_rb.MovePosition(newPos);

			_animator.Play("EnemyRun");
			if (target.x > transform.position.x)
				_spriteRenderer.flipX = true;
			else if (target.x < transform.position.x) _spriteRenderer.flipX = false;
		}
		else
		{
			_isWaiting = true;
			_waitTimer = waitTime;
			_index = (_index + 1)%waypoints.Length;
			_animator.Play("EnemyIdle");
		}
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		for (var i = 0; i < waypoints.Length; i++)
		{
			if (waypoints[i] == null) continue;
			Gizmos.DrawWireSphere(waypoints[i].position, 0.1f);
			var nextWaypoint = waypoints[(i + 1)%waypoints.Length];
			if (nextWaypoint != null) Gizmos.DrawLine(waypoints[i].position, nextWaypoint.position);
		}
	}
}