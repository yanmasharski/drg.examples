using System;
using UnityEngine;

namespace FlappyExample.Game
{
	public sealed class PipeScoreZone : MonoBehaviour
	{
		public event Action Scored;

		private bool _scored;

		private void OnTriggerEnter2D(Collider2D other)
		{
			if (_scored)
			{
				return;
			}

			if (!other.GetComponent<BirdController>())
			{
				return;
			}

			_scored = true;
			Scored?.Invoke();
		}
	}
}
