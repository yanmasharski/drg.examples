using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DRG.Consent;
using DRG.Core;
using DRG.Core.Logs;
using DRG.Framework;
using DRG.Utils;
using UnityEngine;
using ILogger = DRG.Core.Logs.ILogger;

public class EntryPoint : MonoBehaviour
{
	private ILogger _logger;

	private void Start()
	{
		var entryPoint = StaticMonoBehaviour.instance.gameObject.AddComponent<GameEntryPoint>();

		if (!entryPoint.serviceLocator.TryGet<ILogger>(out _logger))
		{
			_logger = new LoggerUnity(ILogger.LogLevel.Debug);
			entryPoint.serviceLocator.Register<ILogger>(_logger);
		}
	}

	public class GameEntryPoint : UnityRuntimeBridge
	{
		public override IModuleServiceLocator serviceLocator { get; } = new ModuleServiceLocator();
		protected override ILogger logger { get; } = new LoggerUnity(ILogger.LogLevel.Debug);

		protected override void SetupModules(IModuleNode root, IModuleServiceLocator locator, ILogger logger)
		{
			// Services used by the debug menu (and by the game, if needed).
			locator.Register<IAppReviewDialog>(new AppReviewDialogProxy(logger));
			locator.Register<IConsentPlatform>(new ConsentPlatformProxy(new ConsentPlatformGoogle(logger), logger));

			// Debug overlay (IMGUI) with quick actions.
			var menu = gameObject.GetComponent<DebugActionsMenu>() ?? gameObject.AddComponent<DebugActionsMenu>();
			menu.Initialize(locator, logger);
		}
	}

	/// <summary>
	/// Minimal IMGUI-based debug overlay that can invoke registered actions.
	/// Intended for examples / dev builds.
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

			// Explicit high-signal actions.
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
			for (int i = 0; i < _actions.Count; i++)
			{
				var item = _actions[i];
				if (GUILayout.Button(item.Label, GUILayout.Height(34)))
				{
					_ = InvokeSafely(item);
				}
			}
			GUILayout.EndScrollView();

			GUILayout.Space(6);
			if (!string.IsNullOrEmpty(_lastResult) && (Time.unscaledTime - _lastResultAt) < 10f)
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
			_logger.Log($"[DebugActionsMenu] {message}");
		}

		private void LogException(Exception e)
		{
			_lastResult = e.Message;
			_lastResultAt = Time.unscaledTime;
			_logger.LogException(e);
		}
	}
}
