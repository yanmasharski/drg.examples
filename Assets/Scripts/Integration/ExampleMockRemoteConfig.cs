using System;
using DRG.Data.Serialization;
using DRG.RemoteConfig;
using ILogger = DRG.Core.Logs.ILogger;

namespace FlappyExample.Integration
{
	/// <summary>
	/// Stand-in <see cref="IRemoteConfig"/> for the Flappy example.
	/// Returns hardcoded values that represent a typical remote config payload.
	/// Replace with <see cref="RemoteConfigLayered"/> wired to real providers in production.
	/// </summary>
	public sealed class ExampleMockRemoteConfig : RemoteConfigBase
	{
		private readonly ILogger _logger;

		public override DRG.Core.IObservable<DRG.Core.Unit> updated => silentUpdated;

		public override bool isLoaded => true;

		public override IDataSerializer objectSerializer => null;

		public ExampleMockRemoteConfig(ILogger logger)
		{
			_logger = logger;
		}

		public override void Initialize(Action onReady = null)
		{
			_logger?.Log(() => "[ExampleMockRemoteConfig] Mock remote config ready (no network call).");
			onReady?.Invoke();
		}

		public override bool TryGetLong(string key, out long value) { value = default; return false; }
		public override bool TryGetDouble(string key, out double value) { value = default; return false; }
		public override bool TryGetString(string key, out string value) { value = default; return false; }
		public override bool TryGetBool(string key, out bool value) { value = default; return false; }
	}
}
