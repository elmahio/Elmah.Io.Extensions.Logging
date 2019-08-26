using System;
using System.Collections.Generic;
using System.Linq;
using Elmah.Io.Client;
using Elmah.Io.Client.Models;
using Microsoft.Extensions.Logging;

namespace Elmah.Io.Extensions.Logging
{
    public class ElmahIoLogger : ILogger
    {
        private const string OriginalFormatPropertyKey = "{OriginalFormat}";
        private readonly ElmahIoProviderOptions _options;
        private readonly ICanHandleMessages _messageHandler;

        public ElmahIoLogger(ICanHandleMessages messageHandler, ElmahIoProviderOptions options)
        {
            _messageHandler = messageHandler;
            _options = options;
        }

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
                Data = new List<Item>(),
            };

            if (state is IEnumerable<KeyValuePair<string, object>> stateProperties)
            {
                // Fill in fields by looking at properties provided by MEL.
                // Properties than we cannot map to elmah.io fields, are added to the Data tab.
                foreach (var stateProperty in stateProperties)
                {
                    if (stateProperty.Key == OriginalFormatPropertyKey)
                    {
                        if (stateProperty.Value is string value)
                        {
                            createMessage.TitleTemplate = value;
                        }
                    }
                    else if (stateProperty.IsStatusCode(out int? statusCode)) createMessage.StatusCode = statusCode.Value;
                    else if (stateProperty.IsApplication(out string application)) createMessage.Application = application;
                    else if (stateProperty.IsSource(out string source)) createMessage.Source = source;
                    else if (stateProperty.IsHostname(out string hostname)) createMessage.Hostname = hostname;
                    else if (stateProperty.IsUser(out string user)) createMessage.User = user;
                    else if (stateProperty.IsMethod(out string method)) createMessage.Method = method;
                    else if (stateProperty.IsVersion(out string version)) createMessage.Version = version;
                    else if (stateProperty.IsUrl(out string url)) createMessage.Url = url;
                    else if (stateProperty.IsType(out string type)) createMessage.Type = type;
                    else if (stateProperty.IsServerVariables(out List<Item> serverVariables)) createMessage.ServerVariables = serverVariables;
                    else if (stateProperty.IsCookies(out List<Item> cookies)) createMessage.Cookies = cookies;
                    else if (stateProperty.IsForm(out List<Item> form)) createMessage.Form = form;
                    else if (stateProperty.IsQueryString(out List<Item> queryString)) createMessage.QueryString = queryString;
                    else createMessage.Data.Add(stateProperty.ToItem());
                }
            }

            // Fill in as many blanks as we can by looking at environment variables, etc.
            if (string.IsNullOrWhiteSpace(createMessage.Source)) createMessage.Source = Source(exception);
            if (string.IsNullOrWhiteSpace(createMessage.Hostname)) createMessage.Hostname = Hostname();
            if (string.IsNullOrWhiteSpace(createMessage.User)) createMessage.User = User();
            if (string.IsNullOrWhiteSpace(createMessage.Type)) createMessage.Type = Type(exception);

            if (exception != null)
            {
                createMessage.Detail = exception.ToString();
                foreach (var item in exception.ToDataList())
                {
                    createMessage.Data.Add(item);
                }
            }

            if (_options.OnFilter != null && _options.OnFilter(createMessage))
            {
                return;
            }

            _messageHandler.AddMessage(createMessage);
        }

        private string Type(Exception exception)
        {
            return exception?.GetBaseException().GetType().FullName;
        }

        private string User()
        {
            return System.Threading.Thread.CurrentPrincipal?.Identity?.Name;
        }

        private string Hostname()
        {
            return Environment.MachineName;
        }

        private string Source(Exception exception)
        {
            return exception?.GetBaseException().Source;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
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