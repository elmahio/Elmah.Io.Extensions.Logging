using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Elmah.Io.Extensions.Logging.ConsoleCore31
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging(logging => logging.AddElmahIo(options =>
            {
                options.ApiKey = "API_KEY";
                options.LogId = new Guid("LOG_ID");

                // Control the message queue with the following properties

                //options.BatchPostingLimit = 20;
                //options.BackgroundQueueSize = 200;
                //options.Period = TimeSpan.FromSeconds(1);
            }));

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("elmah.io");

                // Simple information message
                logger.LogInformation("Hello World");

                // By using reserved names in the log message, core fields on the log message can be set
                logger.LogInformation("A message with {type} {hostname} {application} {user} {source} {method} {version} {url} {statusCode} and {serverVariables}",
                    "custom type", "custom hostname", "custom application", "custom user", "custom source", "custom method",
                    "custom version", "custom url", 500, new Dictionary<string, object> { { "UserAgent", "Me" } });

                // Scopes can be used to both decorate core fields on the log message and add additional properties to Data
                using (logger.BeginScope(new { StatusCode = 500, Method = "GET", Foo = "Bar" }))
                {
                    logger.LogInformation("A message inside a logging scope");
                }
            }
        }
    }
}
