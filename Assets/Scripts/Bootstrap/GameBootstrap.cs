using System;
using System.Threading.Tasks;
using DRG.Ads;
using DRG.Analytics;
using DRG.Consent;
using DRG.Core;
using DRG.Core.Logs;
using DRG.Data;
using DRG.Data.Serialization;
using DRG.Firebase;
using DRG.Firebase.Logs;
using DRG.RemoteConfig;
using DRG.Utils;
using FlappyExample.Debug;
using FlappyExample.Game;
using FlappyExample.Integration;
using FlappyExample.UI;
using UnityEngine;
using ILogger = DRG.Core.Logs.ILogger;

namespace FlappyExample.Bootstrap
{
	public sealed class GameBootstrap : MonoBehaviour
	{
		private const string HighScoreKey = "flappy.high_score";
		private const string StatsKey = "flappy.stats";

		private ILogger _logger;
		private ExampleServiceLocator _locator;
		private IAdsSystem _ads;
		private IAnalyticsGateway _analytics;
		private IAppReviewDialog _appReview;
		private IFirebaseService _firebase;
		private IRemoteConfig _remoteConfig;

		private async void Start()
		{
			_ = StaticMonoBehaviour.instance;

			_logger = new LoggerUnity(ILogger.LogLevel.Debug);
			var compositeLogger = new LoggerComposite(_logger);
			_logger = compositeLogger;

			var mainThreadDispatcher = new MainThreadDispatcherAdapter();
			var dataProvider = new DataProviderPlayerPrefs(_logger, mainThreadDispatcher);
			var debouncedExecutor = new DebouncedExecutorUnity(this, _logger);
			DataStorage.Init(dataProvider, _logger, debouncedExecutor);

			_locator = new ExampleServiceLocator();
			_locator.Register<ILogger>(_logger);

			await InitFirebaseAsync(compositeLogger);

			var analyticsComposite = new AnalyticsGatewayComposite();
			var analyticsMemory = new AnalyticsGatewayMemory();
			analyticsComposite.Add(analyticsMemory);
			AnalyticsEvent.Logger = _logger;
			_analytics = analyticsComposite;
			_locator.Register<IAnalyticsGateway>(_analytics);
			_locator.Register(analyticsMemory);

			var consent = new ConsentPlatformProxy(new ConsentPlatformGoogle(_logger), _logger);
			_locator.Register<IConsentPlatform>(consent);
			_ = ShowConsentAsync(consent);

			var adsSystem = new AdsSystem(_logger);
			adsSystem.Add(new ExampleMockFullscreenAd(_logger), FullscreenAdType.Interstitial);
			adsSystem.Add(new ExampleMockFullscreenAd(_logger), FullscreenAdType.Rewarded);
			_ads = adsSystem;
			_locator.Register<IAdsSystem>(_ads);

			_appReview = new AppReviewDialogProxy(_logger);
			_locator.Register<IAppReviewDialog>(_appReview);

			_remoteConfig = new ExampleMockRemoteConfig(_logger);
			await _remoteConfig.InitializeAsync();
			_locator.Register<IRemoteConfig>(_remoteConfig);

			var debugMenu = gameObject.GetComponent<DebugActionsMenu>() ?? gameObject.AddComponent<DebugActionsMenu>();
			debugMenu.Initialize(_locator, _logger);

			SetupCamera();
			var gameController = CreateGameController();
			gameController.Initialize(new FlappyGameServices
			{
				Logger = _logger,
				Ads = _ads,
				Analytics = _analytics,
				AppReview = _appReview,
				HighScoreKey = HighScoreKey,
				StatsKey = StatsKey,
				Serializer = new DataSerializerNewtonsoft()
			});

			_logger.Log(() => "[GameBootstrap] Flappy example ready.");
		}

		private async Task InitFirebaseAsync(LoggerComposite compositeLogger)
		{
			_firebase = new FirebaseService(_logger);
			_locator.Register<IFirebaseService>(_firebase);

			try
			{
				await _firebase.InitAsync();
				compositeLogger.Add(new LoggerCrashlytics(ILogger.LogLevel.Fatal, _firebase));
				_logger.Log(() => "[GameBootstrap] Firebase initialized.");
			}
			catch (Exception e)
			{
				_logger.LogWarning(() => $"[GameBootstrap] Firebase init failed: {e.Message}");
			}
		}

		private static async Task ShowConsentAsync(IConsentPlatform consent)
		{
			try
			{
				await consent.TryShowConsentDialogAsync();
			}
			catch (Exception)
			{
				// Consent may fail in editor without UMP setup; game still runs.
			}
		}

		private void SetupCamera()
		{
			var camera = GetComponent<Camera>();
			if (camera == null)
			{
				return;
			}

			camera.orthographic = true;
			camera.orthographicSize = 5f;
			camera.backgroundColor = new Color(0.53f, 0.81f, 0.92f);
			camera.transform.position = new Vector3(0f, 0f, -10f);
		}

		private FlappyGameController CreateGameController()
		{
			var root = new GameObject("FlappyGame");
			var controller = root.AddComponent<FlappyGameController>();

			var bird = CreateBird(root.transform);
			var spawner = CreateSpawner(root.transform);
			var hud = CreateHud();

			controller.Bind(bird, spawner, hud);
			return controller;
		}

		private static BirdController CreateBird(Transform parent)
		{
			var birdGo = new GameObject("Bird");
			birdGo.transform.SetParent(parent, false);
			birdGo.transform.position = new Vector3(-1.5f, 0f, 0f);

			var sprite = birdGo.AddComponent<SpriteRenderer>();
			sprite.sprite = CreateColoredSprite(new Color(1f, 0.85f, 0.2f));
			sprite.sortingOrder = 2;

			var collider = birdGo.AddComponent<CircleCollider2D>();
			collider.radius = 0.25f;

			var body = birdGo.AddComponent<Rigidbody2D>();
			body.gravityScale = 0f;
			body.constraints = RigidbodyConstraints2D.FreezeRotation;

			var relay = birdGo.AddComponent<BirdGameplayRelay>();
			var bird = birdGo.AddComponent<BirdController>();
			bird.Initialize(relay);
			return bird;
		}

		private static PipeSpawner CreateSpawner(Transform parent)
		{
			var spawnerGo = new GameObject("PipeSpawner");
			spawnerGo.transform.SetParent(parent, false);
			return spawnerGo.AddComponent<PipeSpawner>();
		}

		private static FlappyHud CreateHud()
		{
			var canvasGo = new GameObject("FlappyHudCanvas");
			var canvas = canvasGo.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode =
				UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
			canvasGo.GetComponent<UnityEngine.UI.CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
			canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

			return canvasGo.AddComponent<FlappyHud>();
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
