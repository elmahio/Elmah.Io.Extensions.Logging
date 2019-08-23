﻿using System;
using Microsoft.Extensions.Logging;

namespace Elmah.Io.Extensions.Logging
{
    public static class ElmahIoLoggerFactoryExtensions
    {
        public static ILoggerFactory AddElmahIo(this ILoggerFactory factory, string apiKey, Guid logId)
        {
            factory.AddProvider(new ElmahIoLoggerProvider(apiKey, logId, null));
            return factory;
        }

        public static ILoggerFactory AddElmahIo(this ILoggerFactory factory, string apiKey, Guid logId, ElmahIoProviderOptions options)
        {
            factory.AddProvider(new ElmahIoLoggerProvider(apiKey, logId, options));
            return factory;
        }
    }
}
