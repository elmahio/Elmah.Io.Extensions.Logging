using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Elmah.Io.Extensions.Logging.AspNetCore21
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureLogging((ctx, logging) =>
                {
                    // Use the following to configure API key and log ID in appsettings.json
                    //logging.Services.Configure<ElmahIoProviderOptions>(ctx.Configuration.GetSection("ElmahIo"));

                    // Use the following line to configure elmah.io provider settings like log level from appsettings.json
                    //logging.AddConfiguration(ctx.Configuration.GetSection("Logging"));

                    // If everything is configured in appsettings.json, call the AddElmahIo overload without an options action
                    //logging.AddElmahIo();

                    logging
                        .AddElmahIo(options =>
                        {
                            options.ApiKey = "API_KEY";
                            options.LogId = new Guid("LOG_ID");

                            // Additional options can be configured like this:
                            //options.OnMessage = msg =>
                            //{
                            //    msg.Version = "2.0.0";
                            //};

                            // Remove comment on the following line to log through a proxy (in this case Fiddler).
                            //options.WebProxy = new WebProxy("localhost", 8888);
                        });

                    // The elmah.io provider can log any log level, but we recommend only to log warning and up
                    logging.AddFilter<ElmahIoLoggerProvider>(null, LogLevel.Warning);
                });
    }
}
