#if NETSTANDARD2_0

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Elmah.Io.Extensions.Logging
{
    public static class ElmahIoLoggingBuilderExtensions
    {
        public static ILoggingBuilder AddElmahIo(this ILoggingBuilder builder, Action<ElmahIoProviderOptions> configure)
        {
            builder.Services.AddSingleton<ILoggerProvider, ElmahIoLoggerProvider>();
            builder.Services.Configure(configure);
            return builder;
        }
    }
}

#endif