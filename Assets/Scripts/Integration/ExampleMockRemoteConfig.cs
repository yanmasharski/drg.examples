using System;
using System.Threading.Tasks;
using DRG.RemoteConfig;
using ILogger = DRG.Core.Logs.ILogger;

namespace FlappyExample.Integration
{
    /// <summary>
    /// Stand-in <see cref="IRemoteConfig"/> for the Flappy example.
    /// Returns hardcoded values that represent a typical remote config payload.
    /// Replace with <see cref="RemoteConfigComposite"/> wired to real providers in production.
    /// </summary>
    public sealed class ExampleMockRemoteConfig : IRemoteConfig
    {
        private readonly ILogger _logger;

#pragma warning disable 67
        public event Action Updated;
#pragma warning restore 67

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

        public long   GetLong  (string key, long   defaultValue = default) => defaultValue;
        public double GetDouble(string key, double defaultValue = default) => defaultValue;
        public string GetString(string key, string defaultValue = "") => defaultValue;
        public bool   GetBool  (string key, bool   defaultValue = false) => defaultValue;
        public T      GetObject<T>(string key, T   defaultValue = default) => defaultValue;
    }
}
