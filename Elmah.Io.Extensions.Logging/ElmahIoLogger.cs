using System;
using Elmah.Io.Client;
using Microsoft.Extensions.Logging;

namespace Elmah.Io.Extensions.Logging
{
    public class ElmahIoLogger : ILogger
     {
        private readonly IElmahioAPI _elmahioApi;
        private readonly Guid _logId;
        private readonly LogLevel _level;

        public ElmahIoLogger(string apiKey, Guid logId, LogLevel level)
        {
            _logId = logId;
            _level = level;
            _elmahioApi = ElmahioAPI.Create(apiKey);
        }
 
         public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
         {
             if (formatter == null)
             {
                 throw new ArgumentNullException(nameof(formatter));
             }
 
             if (!IsEnabled(logLevel)) return;
 
             _elmahioApi.Messages.Log(_logId, exception, LogLevelToSeverity(logLevel), formatter(state, exception));
         }
 
         public bool IsEnabled(LogLevel logLevel)
         {
            return logLevel >= _level;
         }
 
         public IDisposable BeginScope<TState>(TState state)
         {
             if (state == null)
             {
                 throw new ArgumentNullException(nameof(state));
             }
             //TODO not working with async
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