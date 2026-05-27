using System;
using UnityEngine;

namespace FlappyExample.Game
{
	public sealed class BirdGameplayRelay : MonoBehaviour
	{
		public event Action ObstacleHit;

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (collision.collider.CompareTag("Obstacle"))
			{
				ObstacleHit?.Invoke();
			}
		}
	}
}
