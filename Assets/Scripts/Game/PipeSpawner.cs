using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FlappyExample.Game
{
	public sealed class PipeSpawner : MonoBehaviour
	{
		public event Action PipeScored;
		[SerializeField] private float spawnInterval = 2.2f;
		[SerializeField] private float pipeSpeed = 3.5f;
		[SerializeField] private float gapSize = 3.2f;
		[SerializeField] private float spawnX = 8f;
		[SerializeField] private float despawnX = -10f;
		[SerializeField] private float minY = -2f;
		[SerializeField] private float maxY = 2f;
		[SerializeField] private Vector2 pipeSize = new(1.2f, 8f);

		private Sprite _pipeSprite;
		private float _timer;
		private bool _spawning;

		public void SetActive(bool active)
		{
			_spawning = active;
			if (!active)
			{
				_timer = 0f;
			}
		}

		public void ClearPipes()
		{
			for (var i = transform.childCount - 1; i >= 0; i--)
			{
				Destroy(transform.GetChild(i).gameObject);
			}
		}

		private void Awake()
		{
			_pipeSprite = CreateColoredSprite(new Color(0.2f, 0.7f, 0.3f));
		}

		private void Update()
		{
			if (!_spawning)
			{
				return;
			}

			_timer += Time.deltaTime;
			if (_timer < spawnInterval)
			{
				return;
			}

			_timer = 0f;
			SpawnPair();
		}

		private void SpawnPair()
		{
			var centerY = Random.Range(minY, maxY);
			var pairRoot = new GameObject("PipePair");
			pairRoot.transform.SetParent(transform, false);
			pairRoot.transform.position = new Vector3(spawnX, centerY, 0f);
			pairRoot.AddComponent<PipeObstacle>().Initialize(pipeSpeed, despawnX);

			CreatePipeSegment(pairRoot.transform, "TopPipe", gapSize * 0.5f + pipeSize.y * 0.5f, pipeSize);
			CreatePipeSegment(pairRoot.transform, "BottomPipe", -(gapSize * 0.5f + pipeSize.y * 0.5f), pipeSize);
			CreateScoreZone(pairRoot.transform);
		}

		private void CreatePipeSegment(Transform parent, string name, float localY, Vector2 size)
		{
			var pipe = new GameObject(name);
			pipe.tag = "Obstacle";
			pipe.transform.SetParent(parent, false);
			pipe.transform.localPosition = new Vector3(0f, localY, 0f);
			pipe.transform.localScale = new Vector3(size.x, size.y, 1f);

			var renderer = pipe.AddComponent<SpriteRenderer>();
			renderer.sprite = _pipeSprite;
			renderer.sortingOrder = 1;

			var collider = pipe.AddComponent<BoxCollider2D>();
			collider.size = Vector2.one;
		}

		private void CreateScoreZone(Transform parent)
		{
			var zone = new GameObject("ScoreZone");
			zone.tag = "ScoreZone";
			zone.transform.SetParent(parent, false);
			zone.transform.localPosition = Vector3.zero;

			var collider = zone.AddComponent<BoxCollider2D>();
			collider.isTrigger = true;
			collider.size = new Vector2(0.5f, 4f);

			var scoreZone = zone.AddComponent<PipeScoreZone>();
			scoreZone.Scored += () => PipeScored?.Invoke();
		}

		private static Sprite CreateColoredSprite(Color color)
		{
			var texture = new Texture2D(1, 1);
			texture.SetPixel(0, 0, color);
			texture.Apply();
			return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
		}
	}
}
