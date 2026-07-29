using UnityEngine;

// ReSharper disable CheckNamespace

public class Coin : MonoBehaviour {

	[Header("Coin Configuration")]
	[SerializeField]
	CoinType coinType = CoinType.Bronze;

	[SerializeField] int scoreValue = 1;

	[Header("Sine Wave Settings")]
	[SerializeField]
	float amplitude = 0.2f;

	[SerializeField] float frequency = 3f;

	Animator _animator;
	Vector3 _startPos;

	void Awake()
	{
		_animator = GetComponent<Animator>();
		_startPos = transform.position;
		switch (coinType)
		{
			case CoinType.Bronze:
				_animator.Play("CoinBronze");
				break;
			case CoinType.Silver:
				_animator.Play("CoinSilver");
				break;
			default:
			case CoinType.Gold:
				_animator.Play("CoinGold");
				break;
		}
	}

	void Update()
	{
		transform.position = _startPos + Vector3.up*(Mathf.Sin(Time.time*frequency + transform.position.x)*amplitude);
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			Destroy(gameObject);
		}
	}
}