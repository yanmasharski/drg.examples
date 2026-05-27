using UnityEngine;

namespace FlappyExample.Game
{
	public sealed class BirdController : MonoBehaviour
	{
		[SerializeField] private float gravity = 18f;
		[SerializeField] private float flapVelocity = 7f;
		[SerializeField] private float maxFallSpeed = 12f;

		private BirdGameplayRelay _relay;
		private Rigidbody2D _body;
		private bool _simulate;

		public Vector3 StartPosition { get; private set; }

		public void Initialize(BirdGameplayRelay relay)
		{
			_relay = relay;
			_body = GetComponent<Rigidbody2D>();
			StartPosition = transform.position;
		}

		public void SetSimulation(bool enabled)
		{
			_simulate = enabled;
			if (!enabled)
			{
				_body.linearVelocity = Vector2.zero;
				_body.gravityScale = 0f;
			}
			else
			{
				_body.gravityScale = 0f;
			}
		}

		public void ResetToStart()
		{
			transform.position = StartPosition;
			_body.linearVelocity = Vector2.zero;
		}

		public void Flap()
		{
			if (!_simulate)
			{
				return;
			}

			_body.linearVelocity = new Vector2(0f, flapVelocity);
		}

		private void FixedUpdate()
		{
			if (!_simulate)
			{
				return;
			}

			var velocity = _body.linearVelocity;
			velocity.y -= gravity * Time.fixedDeltaTime;
			velocity.y = Mathf.Max(velocity.y, -maxFallSpeed);
			_body.linearVelocity = velocity;
		}

		public BirdGameplayRelay Relay => _relay;
	}
}
