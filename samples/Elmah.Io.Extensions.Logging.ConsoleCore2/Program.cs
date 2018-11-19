using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Elmah.Io.Extensions.Logging.ConsoleCore2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging(logging => logging.AddElmahIo(options =>
            {
                options.ApiKey = "API_KEY";
                options.LogId = new Guid("LOG_ID");
            }));

            var loggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("elmah.io");
            logger.LogInformation("Hello World");
            logger.LogInformation("A message with {type} {hostname} {application} {user} {source} {method} {version} {url} and {statusCode}",
                "custom type", "custom hostname", "custom application", "custom user", "custom source", "custom method",
                "custom version", "custom url", 500);
        }
    }
}
