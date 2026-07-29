using UnityEngine;

// ReSharper disable CheckNamespace

public class MovingPlatform : MonoBehaviour {

	[Header("Movement Settings")]
	[SerializeField]
	Transform[] waypoints;

	[SerializeField] float speed = 3f;
	[SerializeField] float waitTime = 1f;

	int _index;
	bool _isWaiting;
	float _waitTimer;

	void FixedUpdate()
	{
		if (_isWaiting)
		{
			_waitTimer -= Time.fixedDeltaTime;
			if (_waitTimer <= 0)
				_isWaiting = false;
			return;
		}

		var target = waypoints[_index].position;
		if (Vector3.Distance(transform.position, target) > 0.01f)
		{
			transform.position = Vector3.MoveTowards(transform.position, target, speed*Time.fixedDeltaTime);
		}
		else
		{
			_isWaiting = true;
			_waitTimer = waitTime;
			_index = (_index + 1)%waypoints.Length;
		}
	}

	void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.CompareTag("Player")) other.transform.SetParent(transform);
	}

	void OnCollisionExit2D(Collision2D other)
	{
		if (other.gameObject.CompareTag("Player")) other?.transform?.SetParent(null);
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