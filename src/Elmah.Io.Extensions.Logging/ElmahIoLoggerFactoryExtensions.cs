using System;
using Microsoft.Extensions.Logging;

namespace Elmah.Io.Extensions.Logging
{
    /// <summary>
    /// Extension methods for adding the elmah.io logger through ILoggerFactory.
    /// </summary>
    public static class ElmahIoLoggerFactoryExtensions
    {
        /// <summary>
        /// Add the elmah.io logger with the specified API key and log ID.
        /// </summary>
        public static ILoggerFactory AddElmahIo(this ILoggerFactory factory, string apiKey, Guid logId)
        {
            factory.AddProvider(new ElmahIoLoggerProvider(apiKey, logId, null));
            return factory;
        }

        /// <summary>
        /// Add the elmah.io logger with the specified API key, log ID, and options.
        /// </summary>
        public static ILoggerFactory AddElmahIo(this ILoggerFactory factory, string apiKey, Guid logId, ElmahIoProviderOptions options)
        {
            factory.AddProvider(new ElmahIoLoggerProvider(apiKey, logId, options));
            return factory;
        }
    }
}
