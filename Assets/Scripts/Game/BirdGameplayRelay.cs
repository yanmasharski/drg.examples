using DRG.Core;
using UnityEngine;

namespace FlappyExample.Game
{
	public sealed class BirdGameplayRelay : MonoBehaviour
	{
		private readonly Observable<Unit> _obstacleHit = new();

		public IObservable<Unit> obstacleHit => _obstacleHit;

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (collision.collider.CompareTag("Obstacle"))
			{
				_obstacleHit.Notify(Unit.Value);
			}
		}
	}
}
