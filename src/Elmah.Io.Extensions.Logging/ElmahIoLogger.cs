using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Elmah.Io.Client;
using Elmah.Io.Client.Models;
using Microsoft.Extensions.Logging;

namespace Elmah.Io.Extensions.Logging
{
    public class ElmahIoLogger : ILogger
    {
        private const string OriginalFormatPropertyKey = "{OriginalFormat}";
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
                Severity = LogLevelToSeverity(logLevel).ToString(),
            };

            var properties = new List<Item>();
            if (state is IEnumerable<KeyValuePair<string, object>> stateProperties)
            {
                foreach (var stateProperty in stateProperties.Where(prop => prop.Key != OriginalFormatPropertyKey))
                {
                    properties.Add(new Item { Key = stateProperty.Key, Value = stateProperty.Value?.ToString() });
                }
            }

            createMessage.Source = Source(properties, exception);
            createMessage.Hostname = Hostname(properties);
            createMessage.Application = Application(properties);
            createMessage.User = User(properties);
            createMessage.Method = Method(properties);
            createMessage.Version = Version(properties);
            createMessage.Url = Url(properties);
            createMessage.Type = Type(properties, exception);
            createMessage.StatusCode = StatusCode(properties);
            createMessage.Detail = exception?.ToString();
            createMessage.Data = properties;
            if (exception != null)
            {
                foreach (var item in exception.ToDataList())
                {
                    createMessage.Data.Add(item);
                }
            }

            _elmahioApi.Messages.CreateAndNotify(_logId, createMessage);
        }

        private int? StatusCode(List<Item> properties)
        {
            var statusCode = properties.FirstOrDefault(p => p.Key.ToLower() == "statuscode");
            if (statusCode == null || string.IsNullOrWhiteSpace(statusCode.Value)) return null;
            if (!int.TryParse(statusCode.Value.ToString(), out int code)) return null;
            return code;
        }

        private string Type(List<Item> properties, Exception exception)
        {
            var type = properties.FirstOrDefault(p => p.Key.ToLower() == "type");
            if (type != null) return type.Value?.ToString();
            return exception?.GetBaseException().GetType().Name;
        }

        private string Url(List<Item> properties)
        {
            var url = properties.FirstOrDefault(p => p.Key.ToLower() == "url");
            return url?.Value?.ToString();
        }

        private string Version(List<Item> properties)
        {
            var version = properties.FirstOrDefault(p => p.Key.ToLower() == "version");
            return version?.Value?.ToString();
        }

        private string Method(List<Item> properties)
        {
            var method = properties.FirstOrDefault(p => p.Key.ToLower() == "method");
            return method?.Value?.ToString();
        }

        private string User(List<Item> properties)
        {
            var user = properties.FirstOrDefault(p => p.Key.ToLower() == "user");
            if (user != null) return user.Value?.ToString();
#if ISTWOZERO
            return Thread.CurrentPrincipal?.Identity?.Name;
#else
            return null;
#endif
        }

        private string Application(List<Item> properties)
        {
            var application = properties.FirstOrDefault(p => p.Key.ToLower() == "application");
            return application?.Value?.ToString();
        }

        private string Hostname(List<Item> properties)
        {
            var hostname = properties.FirstOrDefault(p => p.Key.ToLower() == "hostname");
            if (hostname != null) return hostname.Value?.ToString();
#if ISTWOZERO
            return Environment.MachineName;
#else
            return null;
#endif
        }

        private string Source(List<Item> properties, Exception exception)
        {
            var source = properties.FirstOrDefault(p => p.Key.ToLower() == "source");
            if (source != null) return source.Value?.ToString();
            return exception?.GetBaseException().Source;
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