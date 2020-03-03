namespace StackExchange.Contrib.Profiling.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using StackExchange.Profiling;
    using StackExchange.Profiling.Storage;

    public class LoggingProvider : IAsyncStorage
    {
        private readonly Func<MiniProfiler, object> _formatter;
        private readonly ILogger _logger;
        private readonly LogLevel _profilingLevel;

        public LoggingProvider(ILogger logger, LogLevel profilingLevel = LogLevel.Trace,
            Func<MiniProfiler, object> formatter = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _profilingLevel = profilingLevel;
            _formatter = formatter ?? (profiler =>
            {
                var json = JsonConvert.SerializeObject(profiler);
                var obj = JsonConvert.DeserializeObject<IDictionary<string, object>>(json,
                    new RecursiveDictionaryConverter());

                return new Dictionary<string, object> {[nameof(MiniProfiler)] = obj};
            });
        }

        public void Save(MiniProfiler profiler)
        {
            if (!_logger.IsEnabled(_profilingLevel))
            {
                return;
            }

            using (_logger.BeginScope(_formatter.Invoke(MiniProfiler.Current)))
            {
                _logger.Log(_profilingLevel, MiniProfiler.Current.RenderPlainText());
            }
        }

        public MiniProfiler Load(Guid id)
        {
            return null;
        }

        public async Task SaveAsync(MiniProfiler profiler)
        {
            Save(profiler);
            await Task.CompletedTask;
        }

        public IEnumerable<Guid> List(
            int maxResults,
            DateTime? start = null,
            DateTime? finish = null,
            ListResultsOrder orderBy = ListResultsOrder.Descending)
        {
            return Enumerable.Empty<Guid>();
        }

        public Task<IEnumerable<Guid>> ListAsync(
            int maxResults,
            DateTime? start = null,
            DateTime? finish = null,
            ListResultsOrder orderBy = ListResultsOrder.Descending)
        {
            return Task.FromResult(Enumerable.Empty<Guid>());
        }

        public Task<MiniProfiler> LoadAsync(Guid id)
        {
            return Task.FromResult((MiniProfiler) null);
        }

        public void SetUnviewed(string user, Guid id)
        {
            /* no-op */
        }

        public Task SetUnviewedAsync(string user, Guid id)
        {
            return Task.CompletedTask;
        }

        public void SetViewed(string user, Guid id)
        {
            /* no-op */
        }

        public Task SetViewedAsync(string user, Guid id)
        {
            return Task.CompletedTask;
        }

        public List<Guid> GetUnviewedIds(string user)
        {
            return new List<Guid>();
        }

        public Task<List<Guid>> GetUnviewedIdsAsync(string user)
        {
            return Task.FromResult(new List<Guid>());
        }
    }
}
