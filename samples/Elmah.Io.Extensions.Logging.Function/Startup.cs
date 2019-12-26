using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

[assembly: FunctionsStartup(typeof(Elmah.Io.Extensions.Logging.Function.Startup))]

namespace Elmah.Io.Extensions.Logging.Function
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            builder.Services.AddLogging(logging =>
            {
                logging.AddElmahIo(o =>
                {
                    o.ApiKey = config["apiKey"];
                    o.LogId = new Guid(config["logId"]);
                });
                logging.AddFilter<ElmahIoLoggerProvider>(null, LogLevel.Warning);
            });
        }
    }
}
