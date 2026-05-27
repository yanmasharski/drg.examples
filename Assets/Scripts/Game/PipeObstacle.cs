using UnityEngine;

namespace FlappyExample.Game
{
	public sealed class PipeObstacle : MonoBehaviour
	{
		private float _speed;
		private float _despawnX;

		public void Initialize(float speed, float despawnX)
		{
			_speed = speed;
			_despawnX = despawnX;
		}

		private void Update()
		{
			var position = transform.position;
			position.x -= _speed * Time.deltaTime;
			transform.position = position;

			if (position.x < _despawnX)
			{
				Destroy(gameObject);
			}
		}
	}
}
