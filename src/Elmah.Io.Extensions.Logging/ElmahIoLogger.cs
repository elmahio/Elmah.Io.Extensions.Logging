using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Elmah.Io.Client;
using Microsoft.Extensions.Logging;

namespace Elmah.Io.Extensions.Logging
{
    /// <summary>
    /// Implementation of Microsoft.Extensions.Logging's ILogger interface that log messages to elmah.io.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the logger. You typically don't want to call this constructor but rather call the AddElmahIo method.
    /// </remarks>
    public class ElmahIoLogger(string name, ICanHandleMessages messageHandler, ElmahIoProviderOptions options, IExternalScopeProvider externalScopeProvider) : ILogger
    {
        private const string OriginalFormatPropertyKey = "{OriginalFormat}";
        private readonly ElmahIoProviderOptions _options = options;
        private readonly string _name = name;
        private readonly ICanHandleMessages _messageHandler = messageHandler;
        private readonly IExternalScopeProvider _externalScopeProvider = externalScopeProvider;

        /// <summary>
        /// Tell the logger to store a message in elmah.io. The message is added to an internal queue and stored asynchronous.
        /// </summary>
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
                Application = _options.Application,
                Data = [],
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
                    else if (scope is not string && !scope.GetType().IsPrimitive)
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
                if (stateProperty.Key == OriginalFormatPropertyKey && stateProperty.Value is string value) createMessage.TitleTemplate = value;
                else if (stateProperty.IsStatusCode(out int? statusCode)) createMessage.StatusCode = statusCode.Value;
                else if (stateProperty.IsApplication(out string application)) createMessage.Application = application;
                else if (stateProperty.IsSource(out string source)) createMessage.Source = source;
                else if (stateProperty.IsHostname(out string hostname)) createMessage.Hostname = hostname;
                else if (stateProperty.IsUser(out string user)) createMessage.User = user;
                else if (stateProperty.IsMethod(out string method)) createMessage.Method = method;
                else if (stateProperty.IsVersion(out string version)) createMessage.Version = version;
                else if (stateProperty.IsUrl(out string url)) createMessage.Url = url;
                else if (stateProperty.IsType(out string type)) createMessage.Type = type;
                else if (stateProperty.IsCorrelationId(out string correlationId)) createMessage.CorrelationId = correlationId;
                else if (stateProperty.IsCategory(out string category)) createMessage.Category = category;
                else if (stateProperty.IsServerVariables(out List<Item> serverVariables)) createMessage.ServerVariables = serverVariables;
                else if (stateProperty.IsCookies(out List<Item> cookies)) createMessage.Cookies = cookies;
                else if (stateProperty.IsForm(out List<Item> form)) createMessage.Form = form;
                else if (stateProperty.IsQueryString(out List<Item> queryString)) createMessage.QueryString = queryString;
                else createMessage.Data.Add(stateProperty.ToItem());
            }

            // If a property named 'category' was not found in the log message, set the category to the name of the logger.
            if (string.IsNullOrWhiteSpace(createMessage.Category)) createMessage.Category = _name;

            // Store potential EventId in data
            if (eventId != default)
            {
                createMessage.Data.Add(new Item("EventId", eventId.Id.ToString()));
                if (!string.IsNullOrWhiteSpace(eventId.Name)) createMessage.Data.Add(new Item("EventName", eventId.Name));
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

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
        {
            if (!_options.IncludeScopes || state.Equals(default(TState))) return null;
            return _externalScopeProvider?.Push(state);
        }

        private static string Type(Exception exception)
        {
            return exception?.GetBaseException().GetType().FullName;
        }

        private static string User()
        {
            return System.Threading.Thread.CurrentPrincipal?.Identity?.Name;
        }

        private static string Hostname()
        {
            return Environment.MachineName;
        }

        private static string Source(Exception exception)
        {
            return exception?.GetBaseException().Source;
        }

        private static Severity LogLevelToSeverity(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Critical => Severity.Fatal,
                LogLevel.Debug => Severity.Debug,
                LogLevel.Error => Severity.Error,
                LogLevel.Information => Severity.Information,
                LogLevel.Trace => Severity.Verbose,
                LogLevel.Warning => Severity.Warning,
                _ => Severity.Information,
            };
        }
    }
}