using System;
using Microsoft.Extensions.Logging;

namespace Elmah.Io.Extensions.Logging
{
    public class ElmahIoLoggerProvider : ILoggerProvider
    {
        private readonly string _apiKey;
        private readonly Guid _logId;
        private readonly FilterLoggerSettings _filter;

        public ElmahIoLoggerProvider(string apiKey, Guid logId, FilterLoggerSettings filter = null)
        {
            _apiKey = apiKey;
            _logId = logId;

            if (filter == null)
            {
                filter = new FilterLoggerSettings
                {
                    {"*", LogLevel.Trace}
                };
            }
            _filter = filter;
        }

        private LogLevel FindLevel(string categoryName)
        {
            var def = LogLevel.Debug;
            foreach (var s in _filter.Switches)
            {
                if (categoryName.Contains(s.Key))
                    return s.Value;

                if (s.Key == "*")
                    def = s.Value;
            }

            return def;
        }

        public ILogger CreateLogger(string name)
        {
            return new ElmahIoLogger(_apiKey, _logId, FindLevel(name));
        }

        public void Dispose()
        {
        }
    }
}