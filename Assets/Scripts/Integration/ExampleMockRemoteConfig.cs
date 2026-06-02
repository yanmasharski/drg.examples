using System;
using System.Threading.Tasks;
using DRG.RemoteConfig;
using ILogger = DRG.Core.Logs.ILogger;

namespace FlappyExample.Integration
{
	/// <summary>
	/// Stand-in <see cref="IRemoteConfig"/> for the Flappy example.
	/// Returns hardcoded values that represent a typical remote config payload.
	/// Replace with <see cref="RemoteConfigLayered"/> wired to real providers in production.
	/// </summary>
	public sealed class ExampleMockRemoteConfig : IRemoteConfig
	{
		private readonly ILogger _logger;

		private static readonly DRG.Core.Observable<DRG.Core.Unit> UpdatedSilent = new();

		public DRG.Core.IObservable<DRG.Core.Unit> updated => UpdatedSilent;

		public bool IsLoaded => true;

		public ExampleMockRemoteConfig(ILogger logger)
		{
			_logger = logger;
		}

		public void Initialize(Action onReady = null)
		{
			_logger?.Log(() => "[ExampleMockRemoteConfig] Mock remote config ready (no network call).");
			onReady?.Invoke();
		}

		public Task InitializeAsync()
		{
			var tcs = new TaskCompletionSource<bool>();
			Initialize(() => tcs.TrySetResult(true));
			return tcs.Task;
		}

		public bool TryGetLong(string key, out long value) { value = default; return false; }
		public bool TryGetDouble(string key, out double value) { value = default; return false; }
		public bool TryGetString(string key, out string value) { value = default; return false; }
		public bool TryGetBool(string key, out bool value) { value = default; return false; }
		public bool TryGetObject<T>(string key, out T value) { value = default; return false; }
	}
}
