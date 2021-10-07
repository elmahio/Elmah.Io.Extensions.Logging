using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Elmah.Io.Extensions.Logging
{
    /// <summary>
    /// Extension methods for adding the elmah.io logger through ILoggingBuilder.
    /// </summary>
    public static class ElmahIoLoggingBuilderExtensions
    {
        /// <summary>
        /// Add the elmah.io logger with the specified options.
        /// </summary>
        public static ILoggingBuilder AddElmahIo(this ILoggingBuilder builder, Action<ElmahIoProviderOptions> configure)
        {
            builder.AddElmahIo();
            builder.Services.Configure(configure);
            return builder;
        }

        /// <summary>
        /// Add the elmah.io logger. This method assumes that an ElmahIoProviderOptions object is already registered.
        /// </summary>
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