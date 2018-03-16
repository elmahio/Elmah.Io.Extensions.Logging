using System;
using Microsoft.Extensions.Logging;
#if NETSTANDARD2_0
using Microsoft.Extensions.Options;
#endif

namespace Elmah.Io.Extensions.Logging
{
#if NETSTANDARD2_0
    [ProviderAlias("ElmahIo")]
#endif
    public class ElmahIoLoggerProvider : ILoggerProvider
    {
        private readonly string _apiKey;
        private readonly Guid _logId;
        private readonly ElmahIoProviderOptions _options;
#if NETSTANDARD1_1
        private readonly FilterLoggerSettings _filter;
#endif

#if NETSTANDARD2_0
        public ElmahIoLoggerProvider(IOptions<ElmahIoProviderOptions> options)
        {
            var loggerOptions = options.Value;
            _apiKey = loggerOptions.ApiKey;
            _logId = loggerOptions.LogId;
            _options = loggerOptions;
        }
#endif

        public ElmahIoLoggerProvider(string apiKey, Guid logId, ElmahIoProviderOptions options = null)
        {
#if NETSTANDARD1_1
            _filter = new FilterLoggerSettings
            {
                {"*", LogLevel.Warning}
            };
#endif
            _apiKey = apiKey;
            _logId = logId;
            _options = options ?? new ElmahIoProviderOptions();
            _options.ApiKey = apiKey;
            _options.LogId = logId;
        }

        public bool IsEnabled { get; private set; }

#if NETSTANDARD1_1
        public ElmahIoLoggerProvider(string apiKey, Guid logId, FilterLoggerSettings filter = null, ElmahIoProviderOptions options = null)
        {
            if (filter == null)
            {
                filter = new FilterLoggerSettings
                {
                    {"*", LogLevel.Warning}
                };
            }
            _filter = filter;

            _apiKey = apiKey;
            _logId = logId;
            _options = options ?? new ElmahIoProviderOptions();
            _options.ApiKey = apiKey;
            _options.LogId = logId;
        }
#endif

#if NETSTANDARD1_1
        private LogLevel FindLevel(string categoryName)
        {
            var def = LogLevel.Warning;
            foreach (var s in _filter.Switches)
            {
                if (categoryName.Contains(s.Key))
                    return s.Value;

                if (s.Key == "*")
                    def = s.Value;
            }

            return def;
        }
#endif

        public ILogger CreateLogger(string name)
        {
#if NETSTANDARD1_1
            return new ElmahIoLogger(_apiKey, _logId, FindLevel(name), _options);
#else
            return new ElmahIoLogger(_options.ApiKey, _options.LogId, _options);
#endif
        }

        public void Dispose()
        {
        }
    }
}