using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private readonly IExternalScopeProvider _externalScopeProvider;

        public ElmahIoLogger(ICanHandleMessages messageHandler, ElmahIoProviderOptions options, IExternalScopeProvider externalScopeProvider)
        {
            _messageHandler = messageHandler;
            _options = options;
            _externalScopeProvider = externalScopeProvider;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (!IsEnabled(logLevel)) return;

            var createMessage = new CreateMessage
            {
                DateTime = DateTime.UtcNow,
                Title = state.Title(formatter, exception),
                Severity = LogLevelToSeverity(logLevel).ToString(),
                Source = Source(exception),
                Hostname = Hostname(),
                User = User(),
                Type = Type(exception),
                Data = new List<Item>(),
            };

            var properties = Enumerable.Empty<KeyValuePair<string, object>>();
            if (state is IEnumerable<KeyValuePair<string, object>> stateProperties)
            {
                properties = properties.Concat(stateProperties);
            }

            if (_options.IncludeScopes)
            {
                _externalScopeProvider?.ForEachScope<object>((scope, _) =>
                {
                    if (scope == null) return;
                    if (scope is IEnumerable<KeyValuePair<string, object>> scopeProperties)
                    {
                        properties = properties.Concat(scopeProperties);
                    }
                    // Strings and primitive types are ignored for now
                    else if (!(scope is string) && !scope.GetType().IsPrimitive)
                    {
                        properties = properties.Concat(scope
                            .GetType()
                            // Only fetch public instance properties declared directly on the scope object. In time we
                            // may want to support complex inheritance structures here.
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                            // Only look at non-indexer properties which we can read from
                            .Where(p => p.CanRead && !string.IsNullOrWhiteSpace(p.Name) && p.Name != "Item" && p.GetIndexParameters().Length == 0)
                            .Select(p => new KeyValuePair<string, object>(p.Name, p.GetValue(scope)?.ToString())));
                    }
                }, null);
            }

            // Fill in fields by looking at properties provided by MEL.
            // Properties than we cannot map to elmah.io fields, are added to the Data tab.
            foreach (var stateProperty in properties)
            {
                if (stateProperty.Key == OriginalFormatPropertyKey && stateProperty.Value is string value)
                {
                    createMessage.TitleTemplate = value;
                    continue;
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

                createMessage.Data.Add(stateProperty.ToItem());
            }

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

            _options.OnMessage?.Invoke(createMessage);

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
            if (!_options.IncludeScopes || state == null) return null;
            return _externalScopeProvider?.Push(state);
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