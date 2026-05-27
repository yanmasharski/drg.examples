using System;
using DRG.Ads;
using DRG.Data;
using DRG.Data.Serialization;
using DRG.Utils;
using FlappyExample.Integration;
using FlappyExample.UI;
using UnityEngine;
using ILogger = DRG.Core.Logs.ILogger;

namespace FlappyExample.Game
{
	public enum FlappyGameState
	{
		Menu,
		Playing,
		GameOver
	}

	public sealed class FlappyGameController : MonoBehaviour
	{
		private const int InterstitialEveryDeaths = 3;
		private const int ReviewAfterGames = 5;
		private const float TopLimit = 4.5f;
		private const float BottomLimit = -4.5f;

		private FlappyGameServices _services;
		private BirdController _bird;
		private PipeSpawner _spawner;
		private FlappyHud _hud;

		private FlappyGameState _state = FlappyGameState.Menu;
		private int _score;
		private int _highScore;
		private int _deathCount;
		private int _flapCount;
		private bool _usedContinue;
		private bool _reviewRequested;

		public FlappyGameState State => _state;
		public int Score => _score;
		public int HighScore => _highScore;

		public event Action StateChanged;
		public event Action<int> ScoreChanged;

		public void Initialize(FlappyGameServices services)
		{
			_services = services;
			_highScore = DataStorage.current.GetInt(services.HighScoreKey).GetValue();
		}

		public void Bind(BirdController bird, PipeSpawner spawner, FlappyHud hud)
		{
			_bird = bird;
			_spawner = spawner;
			_hud = hud;

			_bird.Relay.ObstacleHit += OnObstacleHit;
			_spawner.PipeScored += OnPipeScored;

			_hud.Bind(this);
			EnterMenu();
		}

		private void OnDestroy()
		{
			if (_bird != null && _bird.Relay != null)
			{
				_bird.Relay.ObstacleHit -= OnObstacleHit;
			}

			if (_spawner != null)
			{
				_spawner.PipeScored -= OnPipeScored;
			}
		}

		private void Update()
		{
			if (WasFlapPressed())
			{
				OnFlapInput();
			}

			if (_state == FlappyGameState.Playing)
			{
				CheckBounds();
			}
		}

		private void OnFlapInput()
		{
			switch (_state)
			{
				case FlappyGameState.Menu:
					StartGame();
					break;
				case FlappyGameState.Playing:
					_bird.Flap();
					_flapCount++;
					break;
				case FlappyGameState.GameOver:
					break;
			}
		}

		public void StartGame()
		{
			_score = 0;
			_usedContinue = false;
			_spawner.ClearPipes();
			_bird.ResetToStart();
			_bird.SetSimulation(true);
			_spawner.SetActive(true);
			SetState(FlappyGameState.Playing);
			ScoreChanged?.Invoke(_score);
			_bird.Flap();
			_flapCount++;
		}

		public void RestartGame()
		{
			StartGame();
		}

		public void RequestContinue()
		{
			if (_state != FlappyGameState.GameOver || _usedContinue)
			{
				return;
			}

			if (_services.Ads == null) return;

			var rewarded = _services.Ads.GetFullscreenAd(FullscreenAdType.Rewarded, "flappy_continue");
			if (!rewarded.TryShow(OnRewardedClosed))
				_services.Logger.LogWarning(() => "[FlappyGame] Rewarded ad not ready.");
		}

		private void OnRewardedClosed(IAdImpression impression)
		{
			if (impression == null || !impression.success)
			{
				_services.Logger.Log(() => "[FlappyGame] Rewarded ad skipped or failed.");
				return;
			}

			_usedContinue = true;
			_bird.ResetToStart();
			_bird.SetSimulation(true);
			_spawner.SetActive(true);
			SetState(FlappyGameState.Playing);
			_bird.Flap();
			_flapCount++;
			_hud.Refresh();
		}

		private void OnObstacleHit()
		{
			if (_state != FlappyGameState.Playing)
			{
				return;
			}

			EndGame();
		}

		private void OnPipeScored()
		{
			if (_state != FlappyGameState.Playing)
			{
				return;
			}

			_score++;
			ScoreChanged?.Invoke(_score);
		}

		private void CheckBounds()
		{
			var y = _bird.transform.position.y;
			if (y > TopLimit || y < BottomLimit)
			{
				EndGame();
			}
		}

		private void EndGame()
		{
			_bird.SetSimulation(false);
			_spawner.SetActive(false);

			_deathCount++;
			PersistProgress();
			TryShowInterstitial();
			TryRequestReview();

			SetState(FlappyGameState.GameOver);
			_hud.Refresh();
		}

		private void EnterMenu()
		{
			_spawner.ClearPipes();
			_bird.ResetToStart();
			_bird.SetSimulation(false);
			_spawner.SetActive(false);
			SetState(FlappyGameState.Menu);
			_hud.Refresh();
		}

		private void PersistProgress()
		{
			var storage = DataStorage.current;
			var beatRecord = _score > _highScore;

			if (beatRecord)
			{
				_highScore = _score;
				storage.GetInt(_services.HighScoreKey).SetValue(_highScore);
			}

			var statsRecord = storage.GetObject(_services.StatsKey, new FlappySaveData(), _services.Serializer);
			var stats = statsRecord.GetValue() ?? new FlappySaveData();
			stats.totalGames++;
			stats.totalFlaps += _flapCount;
			stats.lastPlayedUtc = DateTime.UtcNow.ToString("O");
			statsRecord.SetValue(stats);
			storage.Save(30);

			_services.Logger.Log(() =>
				$"[FlappyGame] Game over. score={_score} best={_highScore} games={stats.totalGames}");

			if (beatRecord)
			{
				_services.AppReview?.Show(ok =>
					_services.Logger.Log(() => $"[FlappyGame] Review after record: {ok}"));
			}
		}

		private void TryShowInterstitial()
		{
			if (_deathCount % InterstitialEveryDeaths != 0) return;
			if (_services.Ads == null) return;

			_services.Ads.GetFullscreenAd(FullscreenAdType.Interstitial, "flappy_game_over").TryShow();
		}

		private void TryRequestReview()
		{
			if (_reviewRequested || _services.AppReview == null)
			{
				return;
			}

			var stats = DataStorage.current
				.GetObject(_services.StatsKey, new FlappySaveData(), _services.Serializer)
				.GetValue();

			if (stats == null || stats.totalGames < ReviewAfterGames)
			{
				return;
			}

			_reviewRequested = true;
			_services.AppReview.Show(ok =>
				_services.Logger.Log(() => $"[FlappyGame] Review after games milestone: {ok}"));
		}

		private void SetState(FlappyGameState state)
		{
			_state = state;
			StateChanged?.Invoke();
		}

		public bool CanContinue => _state == FlappyGameState.GameOver && !_usedContinue;

		private static bool WasFlapPressed()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				return true;
			}

			if (Input.GetMouseButtonDown(0))
			{
				return true;
			}

			return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
		}
	}
}
