using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Elmah.Io.Extensions.Logging
{
    [ProviderAlias("ElmahIo")]
    public class ElmahIoLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly ElmahIoProviderOptions _options;
        private readonly MessageQueue _messageQueue;
        private IExternalScopeProvider _scopeProvider;

        public ElmahIoLoggerProvider(IOptions<ElmahIoProviderOptions> options) : this(options.Value.ApiKey, options.Value.LogId, options.Value)
        {
        }

        public ElmahIoLoggerProvider(string apiKey, Guid logId, ElmahIoProviderOptions options = null)
        {
            _options = options ?? new ElmahIoProviderOptions();

            if (_options.BatchPostingLimit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_options.BatchPostingLimit), $"{nameof(_options.BatchPostingLimit)} must be a positive number.");
            }

            if (_options.Period <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(_options.Period), $"{nameof(_options.Period)} must be longer than zero.");
            }

            _options.ApiKey = apiKey;
            _options.LogId = logId;
            _messageQueue = new MessageQueue(_options);
            _messageQueue.Start();
        }

        public bool IsEnabled { get; private set; }

        public ILogger CreateLogger(string name)
        {
            return new ElmahIoLogger(_messageQueue, _options, _scopeProvider);
        }

        public void Dispose()
        {
            if (IsEnabled)
            {
                _messageQueue.Stop();
            }
        }

        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }
    }
}