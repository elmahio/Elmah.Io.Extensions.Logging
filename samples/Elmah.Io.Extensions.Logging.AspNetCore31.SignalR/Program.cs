using Elmah.Io.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Elmah.Io.Extensions.Logging.AspNetCore31.SignalR
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureLogging((ctx, logging) =>
                    {
                        logging.AddElmahIo(options =>
                        {
                            options.ApiKey = "API_KEY";
                            options.LogId = new Guid("LOG_ID");
                        });
                        logging.AddFilter<ElmahIoLoggerProvider>(null, LogLevel.Warning);
                    });
                });
    }
}
