using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Elmah.Io.Extensions.Logging
{
    [ProviderAlias("ElmahIo")]
    public class ElmahIoLoggerProvider : ILoggerProvider
    {
        private readonly ElmahIoProviderOptions _options;
        private readonly MessageQueue _messageQueue;

        public ElmahIoLoggerProvider(IOptions<ElmahIoProviderOptions> options) : this(options.Value.ApiKey, options.Value.LogId, options.Value)
        {
        }

        public ElmahIoLoggerProvider(string apiKey, Guid logId, ElmahIoProviderOptions options = null)
        {
            if (options.BatchPostingLimit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(options.BatchPostingLimit), $"{nameof(options.BatchPostingLimit)} must be a positive number.");
            }

            if (options.Period <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(options.Period), $"{nameof(options.Period)} must be longer than zero.");
            }

            _options = options ?? new ElmahIoProviderOptions();
            _options.ApiKey = apiKey;
            _options.LogId = logId;
            _messageQueue = new MessageQueue(_options);
            _messageQueue.Start();
        }

        public bool IsEnabled { get; private set; }

        public ILogger CreateLogger(string name)
        {
            return new ElmahIoLogger(_messageQueue, _options);
        }

        public void Dispose()
        {
            if (IsEnabled)
            {
                _messageQueue.Stop();
            }
        }
    }
}