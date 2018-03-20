using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Elmah.Io.Extensions.Logging;
using System;
using Microsoft.Extensions.Logging;

namespace Elmah.Io.Extensions.LoggingAspNetCore2WithEfCore2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging(logging =>
                {
                    logging.AddElmahIo(options =>
                    {
                        options.ApiKey = "API_KEY";
                        options.LogId = new Guid("LOG_ID");
                    });
                    logging.AddFilter<ElmahIoLoggerProvider>(null, LogLevel.Warning);
                })
                .Build();
    }
}
