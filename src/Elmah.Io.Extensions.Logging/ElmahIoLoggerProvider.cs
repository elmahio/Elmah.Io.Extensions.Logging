using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Elmah.Io.Extensions.Logging
{
    /// <summary>
    /// An ILoggerProvider for registering the elmah.io logger.
    /// </summary>
    [ProviderAlias("ElmahIo")]
    public class ElmahIoLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly ElmahIoProviderOptions _options;
        private readonly ICanHandleMessages _messageQueue;
        private IExternalScopeProvider _scopeProvider;

        /// <summary>
        /// Create a new instance using the provided options.
        /// </summary>
        /// <param name="options"></param>
        public ElmahIoLoggerProvider(IOptions<ElmahIoProviderOptions> options) : this(options.Value.ApiKey, options.Value.LogId, options.Value)
        {
        }

        /// <summary>
        /// Create a new instance using the provided API key, log ID, and options.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="logId"></param>
        /// <param name="options"></param>
        public ElmahIoLoggerProvider(string apiKey, Guid logId, ElmahIoProviderOptions options = null)
        {
            _options = options ?? new ElmahIoProviderOptions();

            if (!_options.Synchronous && _options.BatchPostingLimit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_options.BatchPostingLimit), $"{nameof(_options.BatchPostingLimit)} must be a positive number.");
            }

            if (!options.Synchronous && _options.Period <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(_options.Period), $"{nameof(_options.Period)} must be longer than zero.");
            }

            _options.ApiKey = apiKey;
            _options.LogId = logId;

            if (_options.Synchronous)
                _messageQueue = new SynchronousMessageHandler(_options);
            else
                _messageQueue = new MessageQueueHandler(_options);

            _messageQueue.Start();
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string name)
        {
            return new ElmahIoLogger(name, _messageQueue, _options, _scopeProvider);
        }

        /// <summary>
        /// Dispose the internal message queue, trying to process all pending messages.
        /// </summary>
        public void Dispose()
        {
            _messageQueue?.Stop();
        }

        /// <inheritdoc/>
        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }
    }
}