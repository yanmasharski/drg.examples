using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DRG.Ads;
using DRG.Analytics;
using DRG.Consent;
using DRG.Core;
using DRG.Firebase;
using DRG.Utils;
using UnityEngine;
using ILogger = DRG.Core.Logs.ILogger;

namespace FlappyExample.Debug
{
	/// <summary>
	/// Minimal IMGUI overlay for package integration smoke tests.
	/// </summary>
	public sealed class DebugActionsMenu : MonoBehaviour
	{
		private sealed class ActionItem
		{
			public string Label;
			public Func<Task> InvokeAsync;
		}

		private readonly List<ActionItem> _actions = new();
		private IServiceLocator _locator;
		private ILogger _logger;

		private Vector2 _scroll;
		private Rect _windowRect = new Rect(16, 16, 420, 520);
		private string _lastResult = "";
		private float _lastResultAt = -999f;

		public void Initialize(IServiceLocator locator, ILogger logger)
		{
			_locator = locator;
			_logger = logger;
			_actions.Clear();

			AddAction("Rate us (AppReviewDialog.Show)", () =>
			{
				if (!locator.TryGet<IAppReviewDialog>(out var review))
				{
					LogResult("AppReviewDialog not registered");
					return;
				}

				review.Show(ok => LogResult($"Rate us completed: {ok}"));
			});

			AddAction("Consent (TryShowConsentDialogAsync)", async () =>
			{
				if (!locator.TryGet<IConsentPlatform>(out var consent))
				{
					LogResult("ConsentPlatform not registered");
					return;
				}

				var ok = await consent.TryShowConsentDialogAsync();
				LogResult($"Consent completed: {ok}; state={consent.state}");
			});

			AddAction("Log test exception", () =>
			{
				_logger.LogException(() => new Exception("Flappy example test exception"));
				LogResult("Exception logged.");
			});

			AddAction("Firebase state", () =>
			{
				if (!locator.TryGet<IFirebaseService>(out var firebase))
				{
					LogResult("FirebaseService not registered");
					return;
				}

				LogResult($"Firebase state: {firebase.InitializationState}");
			});

			AddAction("Ads: show interstitial", () =>
			{
				if (!locator.TryGet<IAdsSystem>(out var ads))
				{
					LogResult("AdsSystem not registered");
					return;
				}

				var ad = ads.GetFullscreenAd(FullscreenAdType.Interstitial, "debug");
				LogResult($"Interstitial ready={ad.isReady}");
				ad.TryShow(imp => LogResult($"Interstitial closed: success={imp?.success}"));
			});

			AddAction("Ads: show rewarded", () =>
			{
				if (!locator.TryGet<IAdsSystem>(out var ads))
				{
					LogResult("AdsSystem not registered");
					return;
				}

				var ad = ads.GetFullscreenAd(FullscreenAdType.Rewarded, "debug");
				LogResult($"Rewarded ready={ad.isReady}");
				ad.TryShow(imp => LogResult($"Rewarded closed: success={imp?.success}"));
			});

			AddAction("Ads: readiness", () =>
			{
				if (!locator.TryGet<IAdsSystem>(out var ads))
				{
					LogResult("AdsSystem not registered");
					return;
				}

				var intAd = ads.GetFullscreenAd(FullscreenAdType.Interstitial);
				var rvAd = ads.GetFullscreenAd(FullscreenAdType.Rewarded);
				LogResult($"Int ready={intAd.isReady}  RV ready={rvAd.isReady}");
			});

			AddAction("Analytics: recent events", () =>
			{
				if (!locator.TryGet<AnalyticsGatewayMemory>(out var memory))
				{
					LogResult("AnalyticsGatewayMemory not registered");
					return;
				}

				var events = memory.Events;
				if (events.Count == 0)
				{
					LogResult("Analytics: no events tracked yet");
					return;
				}

				var last = events[events.Count - 1];
				LogResult($"Analytics: {events.Count} total. Last: {last.Name}");
			});
		}

		private void OnGUI()
		{
			_windowRect = GUI.Window(GetInstanceID(), _windowRect, DrawWindow, "DRG Debug Menu");
		}

		private void DrawWindow(int windowId)
		{
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Rebuild", GUILayout.Width(90)))
			{
				try
				{
					Initialize(_locator, _logger);
					LogResult("Menu rebuilt.");
				}
				catch (Exception e)
				{
					LogException(e);
				}
			}

			GUILayout.FlexibleSpace();
			GUILayout.Label("Tap action to invoke");
			GUILayout.EndHorizontal();

			GUILayout.Space(6);

			_scroll = GUILayout.BeginScrollView(_scroll);
			for (var i = 0; i < _actions.Count; i++)
			{
				var item = _actions[i];
				if (GUILayout.Button(item.Label, GUILayout.Height(34)))
				{
					_ = InvokeSafely(item);
				}
			}

			GUILayout.EndScrollView();

			GUILayout.Space(6);
			if (!string.IsNullOrEmpty(_lastResult) && Time.unscaledTime - _lastResultAt < 10f)
			{
				GUILayout.Label(_lastResult);
			}

			GUILayout.EndVertical();
		}

		private async Task InvokeSafely(ActionItem item)
		{
			try
			{
				LogResult($"Invoking: {item.Label}");
				await item.InvokeAsync();
			}
			catch (Exception e)
			{
				LogException(e);
			}
		}

		private void AddAction(string label, Action action)
		{
			_actions.Add(new ActionItem
			{
				Label = label,
				InvokeAsync = () =>
				{
					action();
					return Task.CompletedTask;
				}
			});
		}

		private void AddAction(string label, Func<Task> actionAsync)
		{
			_actions.Add(new ActionItem { Label = label, InvokeAsync = actionAsync });
		}

		private void LogResult(string message)
		{
			_lastResult = message;
			_lastResultAt = Time.unscaledTime;
			_logger.Log(() => $"[DebugActionsMenu] {message}");
		}

		private void LogException(Exception e)
		{
			_lastResult = e.Message;
			_lastResultAt = Time.unscaledTime;
			_logger.LogException(() => e);
		}
	}
}
