using UnityEngine;

// ReSharper disable CheckNamespace

public class PatrollingEnemy : MonoBehaviour
{
    [Header("Movement Settings")] [SerializeField]
    private Transform[] waypoints;

    [SerializeField] private float speed = 3f;
    [SerializeField] private float waitTime = 1f;
    [SerializeField] private float arriveDistance = 0.05f;

    [Header("Chase Settings")] [SerializeField]
    private Transform player;

    [SerializeField] private float chaseRange = 4f;
    [SerializeField] private float loseChaseRange = 6f;

    [Header("Cliff Safety")] [SerializeField]
    private LayerMask groundLayer = 1 << 6;

    [SerializeField] private float edgeCheckForward = 0.2f;
    [SerializeField] private float edgeCheckDistance = 0.45f;

    private Animator _animator;
    private Collider2D _collider;
    private bool _hasAnimationState;
    private int _index;
    private bool _isMoving;
    private bool _isWaiting;
    private int _patrolDirection = 1;
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private EnemyState _state = EnemyState.Patrol;
    private float _waitTimer;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();

        if (player == null)
        {
            var playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
        }

        if (groundLayer.value == 0)
            groundLayer = LayerMask.GetMask("Ground");
    }

    private void FixedUpdate()
    {
        if (_rb == null) return;

        UpdateState();

        if (_state == EnemyState.Chase)
            UpdateChase();
        else
            UpdatePatrol();

        UpdateAnimation();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (waypoints != null)
            for (var i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;

                Gizmos.DrawWireSphere(waypoints[i].position, 0.1f);
                var nextIndex = (i + 1) % waypoints.Length;
                if (waypoints[nextIndex] != null)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
            }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }

    private void UpdateState()
    {
        if (player == null) return;

        var offset = (Vector2)player.position - _rb.position;
        var distanceSqr = offset.sqrMagnitude;

        if (_state == EnemyState.Patrol && distanceSqr <= chaseRange * chaseRange)
            ChangeState(EnemyState.Chase);
        else if (_state == EnemyState.Chase && distanceSqr > loseChaseRange * loseChaseRange)
            ChangeState(EnemyState.Patrol);
    }

    private void ChangeState(EnemyState newState)
    {
        if (_state == newState) return;

        _state = newState;
        _isWaiting = false;
        _waitTimer = 0f;
    }

    private void UpdatePatrol()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            StopMoving();
            return;
        }

        if (waypoints.Length == 1 || waypoints[_index] == null)
        {
            StopMoving();
            return;
        }

        if (_isWaiting)
        {
            StopMoving();
            _waitTimer -= Time.fixedDeltaTime;

            if (_waitTimer <= 0f)
            {
                _isWaiting = false;
                AdvancePatrolTarget();
            }

            return;
        }

        var target = (Vector2)waypoints[_index].position;
        if (Vector2.Distance(_rb.position, target) <= arriveDistance)
        {
            BeginPatrolWait();
            StopMoving();
            return;
        }

        var direction = Mathf.Sign(target.x - _rb.position.x);
        if (Mathf.Approximately(direction, 0f))
        {
            BeginPatrolWait();
            StopMoving();
            return;
        }

        if (!CanMoveInDirection(direction))
        {
            ReversePatrolTarget();
            BeginPatrolWait();
            StopMoving();
            return;
        }

        MoveTowards(target, direction);
    }

    private void UpdateChase()
    {
        if (!player)
        {
            ChangeState(EnemyState.Patrol);
            StopMoving();
            return;
        }

        var direction = Mathf.Sign(player.position.x - _rb.position.x);
        if (Mathf.Approximately(direction, 0f))
        {
            StopMoving();
            return;
        }

        // The enemy only chases horizontally and stops if the next step has no ground.
        if (!CanMoveInDirection(direction))
        {
            StopMoving();
            return;
        }

        var target = new Vector2(player.position.x, _rb.position.y);
        MoveTowards(target, direction);
    }

    private void MoveTowards(Vector2 target, float direction)
    {
        var newPosition = Vector2.MoveTowards(_rb.position, target, speed * Time.fixedDeltaTime);
        _rb.MovePosition(newPosition);

        _isMoving = true;
        _spriteRenderer.flipX = direction > 0f;
    }

    private bool CanMoveInDirection(float direction)
    {
        if (!_collider || groundLayer.value == 0) return true;

        var bounds = _collider.bounds;
        var checkX = direction > 0f
            ? bounds.max.x + edgeCheckForward
            : bounds.min.x - edgeCheckForward;
        var checkOrigin = new Vector2(checkX, bounds.min.y + 0.05f);

        var hit = Physics2D.Raycast(checkOrigin, Vector2.down, edgeCheckDistance, groundLayer);
        Debug.DrawRay(checkOrigin, Vector2.down * edgeCheckDistance, hit.collider ? Color.green : Color.red);

        return hit.collider;
    }

    private void BeginPatrolWait()
    {
        _isWaiting = true;
        _waitTimer = waitTime;
    }

    private void AdvancePatrolTarget()
    {
        if (waypoints is not { Length: > 1 }) return;

        _index += _patrolDirection;
        if (_index >= waypoints.Length)
        {
            _patrolDirection = -1;
            _index = waypoints.Length - 2;
        }
        else if (_index < 0)
        {
            _patrolDirection = 1;
            _index = 1;
        }
    }

    private void ReversePatrolTarget()
    {
        if (waypoints == null || waypoints.Length <= 1) return;

        _patrolDirection *= -1;
        AdvancePatrolTarget();
    }

    private void StopMoving()
    {
        _isMoving = false;
    }

    private void UpdateAnimation()
    {
        if (!_animator || _hasAnimationState == _isMoving) return;

        _animator.Play(_isMoving ? "EnemyRun" : "EnemyIdle");
        _hasAnimationState = _isMoving;
    }

    private enum EnemyState
    {
        Patrol,
        Chase
    }
}