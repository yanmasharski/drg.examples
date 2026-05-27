using System;
using System.Threading.Tasks;
using DRG.Ads;
using ILogger = DRG.Core.Logs.ILogger;

namespace FlappyExample.Integration
{
	/// <summary>
	/// Example <see cref="IAdsSystem"/> wiring mock fullscreen units from <see cref="FlappyExample.Bootstrap.GameBootstrap"/>.
	/// </summary>
	public sealed class ExampleAdsSystem : IAdsSystem
	{
		private readonly IFullscreenAd _interstitial;
		private readonly IFullscreenAd _rewarded;
		private readonly IFullscreenAd _unknownFullscreen;
		private readonly IRegularAd _regularNoop;
		private readonly ILogger _logger;

		public ExampleAdsSystem(IFullscreenAd interstitial, IFullscreenAd rewarded, ILogger logger)
		{
			_interstitial = interstitial;
			_rewarded = rewarded;
			_logger = logger;
			_unknownFullscreen = new ExampleNoopFullscreenAd();
			_regularNoop = new ExampleMockRegularAd();
		}

		public void Initialize(Action onReady = null)
		{
			_logger.Log(() => "[ExampleAdsSystem] Initialize (mock; ready immediately).");
			onReady?.Invoke();
		}

		public Task InitializeAsync()
		{
			_logger.Log(() => "[ExampleAdsSystem] InitializeAsync (mock; ready immediately).");
			return Task.CompletedTask;
		}

		public IFullscreenAd GetFullscreenAd(FullscreenAdType type, string placement = "")
		{
			switch (type)
			{
				case FullscreenAdType.Interstitial:
					LogFullscreen("Interstitial", placement);
					return _interstitial;
				case FullscreenAdType.Rewarded:
					LogFullscreen("Rewarded", placement);
					return _rewarded;
				default:
					_logger.LogWarning(() =>
						$"[ExampleAdsSystem] Unknown FullscreenAdType {type}, placement={placement}.");
					return _unknownFullscreen;
			}
		}

		public IRegularAd GetRegularAd(RegularAdType type, string placement = "")
		{
			if (type == RegularAdType.Unknown)
			{
				_logger.LogWarning(() =>
					$"[ExampleAdsSystem] RegularAdType.Unknown, placement={placement} (no-op mock).");
			}
			else
			{
				_logger.Log(() =>
					$"[ExampleAdsSystem] GetRegularAd {type} placement={placement} (no-op mock — not wired in sample).");
			}

			return _regularNoop;
		}

		private void LogFullscreen(string label, string placement)
		{
			if (string.IsNullOrEmpty(placement))
			{
				_logger.Log(() => $"[ExampleAdsSystem] GetFullscreenAd {label}");
			}
			else
			{
				_logger.Log(() => $"[ExampleAdsSystem] GetFullscreenAd {label} placement={placement}");
			}
		}

		private sealed class ExampleNoopFullscreenAd : IFullscreenAd
		{
			private readonly AdReadySubject _readySubject = new AdReadySubject();

			public bool isReady => false;
			public DRG.Core.IObservable<bool> readyChanged => _readySubject;

			public bool TryShow(Action<IAdImpression> onClose = null)
			{
				onClose?.Invoke(new AdImpressionEmpty());
				return false;
			}
		}

		private sealed class ExampleMockRegularAd : IRegularAd
		{
			private readonly AdReadySubject _readySubject = new AdReadySubject();

			public bool isReady => false;
			public DRG.Core.IObservable<bool> readyChanged => _readySubject;

			public bool TryShow() => false;

			public void Hide()
			{
			}
		}
	}
}
