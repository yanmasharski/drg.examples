using DRG.Core;
using UnityEngine;

namespace FlappyExample.Game
{
	public sealed class PipeScoreZone : MonoBehaviour
	{
		private readonly Observable<Unit> _scored = new();
		private bool _scoredOnce;

		public IObservable<Unit> scored => _scored;

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (_scoredOnce)
			{
				return;
			}

			if (!other.GetComponent<BirdController>())
			{
				return;
			}

			_scoredOnce = true;
			_scored.Notify(Unit.Value);
		}
	}
}
