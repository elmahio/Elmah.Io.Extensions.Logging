using System;
using Microsoft.Extensions.Logging;

namespace Elmah.Io.Extensions.Logging
{
    public static class ElmahIoLoggerFactoryExtensions
    {
        public static ILoggerFactory AddElmahIo(this ILoggerFactory factory, string apiKey, Guid logId)
        {
            factory.AddProvider(new ElmahIoLoggerProvider(apiKey, logId));
            return factory;
        }

        public static ILoggerFactory AddElmahIoWithFilter(this ILoggerFactory factory, string apiKey, Guid logId, FilterLoggerSettings filter)
        {
            factory.AddProvider(new ElmahIoLoggerProvider(apiKey, logId, filter));
            return factory;
        }
    }
}