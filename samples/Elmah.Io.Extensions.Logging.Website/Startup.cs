using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Elmah.Io.Extensions.Logging.Website
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddElmahIo("API_KEY", new Guid("LOG_ID"));

            // The following example show two things:
            // 1. As default, the Microsoft.Extensions.Logging implementation for elmah.io, only logs warnings, errors and criticals.
            //    To log other log levels, create a FilterLoggerSettings as shown in the following example.
            // 2. To hook into the pipeline of logging a message to elmah.io, implement the OnMessage and OnError actions
            //loggerFactory.AddElmahIo(
            //    "API_KEY",
            //    new Guid("LOG_ID"),
            //    new FilterLoggerSettings
            //    {
            //        {"elmah.io", LogLevel.Information}
            //    },
            //    new ElmahIoProviderOptions
            //    {
            //        OnMessage = msg =>
            //        {
            //            msg.Version = "1.0.0";
            //        }
            //    });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
