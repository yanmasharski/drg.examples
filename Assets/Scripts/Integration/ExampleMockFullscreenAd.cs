using System;
using System.Collections;
using DRG.Ads;
using DRG.Utils;
using UnityEngine;
using ILogger = DRG.Core.Logs.ILogger;

namespace FlappyExample.Integration
{
	/// <summary>
	/// Dev mock for <see cref="IFullscreenAd"/>. Replace with drg.ads.applovin or drg.ads.ironsource in production.
	/// </summary>
	public sealed class ExampleMockFullscreenAd : IFullscreenAd
	{
		private readonly float _delaySeconds;
		private readonly ILogger _logger;
		private readonly AdReadySubject _readySubject = new AdReadySubject();

		public ExampleMockFullscreenAd(ILogger logger, float delaySeconds = 1.25f)
		{
			_logger = logger;
			_delaySeconds = delaySeconds;
		}

		public bool isReady => true;
		public DRG.Core.IObservable<bool> readyChanged => _readySubject;

		public bool TryShow(Action<IAdImpression> onClose = null)
		{
			_logger.Log(() => $"[ExampleMockFullscreenAd] Showing MockFullscreenAd");
			StaticMonoBehaviour.instance.StartCoroutine(ShowCoroutine(onClose));
			return true;
		}

		private IEnumerator ShowCoroutine(Action<IAdImpression> onClose)
		{
			yield return new WaitForSecondsRealtime(_delaySeconds);
			onClose?.Invoke(new ExampleAdImpression());
		}
	}

	public sealed class ExampleAdImpression : IAdImpression
	{
		public string provider => "example-mock";
		public bool success { get; }

		public ExampleAdImpression(bool success = true) => this.success = success;
	}
}
