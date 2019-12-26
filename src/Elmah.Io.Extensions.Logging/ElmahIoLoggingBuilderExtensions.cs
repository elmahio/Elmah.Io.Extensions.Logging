using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Elmah.Io.Extensions.Logging
{
    public static class ElmahIoLoggingBuilderExtensions
    {
        public static ILoggingBuilder AddElmahIo(this ILoggingBuilder builder, Action<ElmahIoProviderOptions> configure)
        {
            builder.AddElmahIo();
            builder.Services.Configure(configure);
            return builder;
        }

        public static ILoggingBuilder AddElmahIo(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, ElmahIoLoggerProvider>(services =>
            {
                var options = services.GetService<IOptions<ElmahIoProviderOptions>>();
                return new ElmahIoLoggerProvider(options);
            });
            return builder;
        }
    }
}