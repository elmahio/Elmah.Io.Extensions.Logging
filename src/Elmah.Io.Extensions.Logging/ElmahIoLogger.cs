using System;
using Elmah.Io.Client;
using Elmah.Io.Client.Models;
using Microsoft.Extensions.Logging;

namespace Elmah.Io.Extensions.Logging
{
    public class ElmahIoLogger : ILogger
    {
        private readonly IElmahioAPI _elmahioApi;
        private readonly Guid _logId;
#if NETSTANDARD1_1
        private readonly LogLevel _level;
#endif
        private readonly ElmahIoProviderOptions _options;

        public ElmahIoLogger(string apiKey, Guid logId, ElmahIoProviderOptions options)
        {
            _logId = logId;
            _elmahioApi = ElmahioAPI.Create(apiKey);
            _elmahioApi.Messages.OnMessage += (sender, args) => options.OnMessage?.Invoke(args.Message);
            _elmahioApi.Messages.OnMessageFail += (sender, args) => options.OnError?.Invoke(args.Message, args.Error);
            _options = options;
        }

#if NETSTANDARD1_1
        public ElmahIoLogger(string apiKey, Guid logId, LogLevel level, ElmahIoProviderOptions options)
        {
            _logId = logId;
            _level = level;
            _elmahioApi = ElmahioAPI.Create(apiKey);
            _elmahioApi.Messages.OnMessage += (sender, args) => options.OnMessage?.Invoke(args.Message);
            _elmahioApi.Messages.OnMessageFail += (sender, args) => options.OnError?.Invoke(args.Message, args.Error);
            _options = options;
        }
#endif

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (!IsEnabled(logLevel)) return;

            var message = formatter(state, exception);
            var createMessage = new CreateMessage
            {
                DateTime = DateTime.UtcNow,
                Title = message,
                Severity = LogLevelToSeverity(logLevel).ToString()
            };
            if (exception != null)
            {
                createMessage.Detail = exception.ToString();
                createMessage.Data = exception.ToDataList();
                createMessage.Type = exception.GetBaseException().GetType().Name;
                createMessage.Source = exception.GetBaseException().Source;
            }

            _elmahioApi.Messages.CreateAndNotify(_logId, createMessage);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
#if NETSTANDARD1_1
            return logLevel >= _level;
#else
            return true;
#endif
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        private Severity LogLevelToSeverity(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                    return Severity.Fatal;
                case LogLevel.Debug:
                    return Severity.Debug;
                case LogLevel.Error:
                    return Severity.Error;
                case LogLevel.Information:
                    return Severity.Information;
                case LogLevel.Trace:
                    return Severity.Verbose;
                case LogLevel.Warning:
                    return Severity.Warning;
                default:
                    return Severity.Information;
            }
        }
    }
}