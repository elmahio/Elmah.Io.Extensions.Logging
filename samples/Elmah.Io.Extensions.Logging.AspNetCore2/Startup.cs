using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Elmah.Io.Extensions.Logging.AspNetCore2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // If you need to decorate messages with information from the HTTP context, comment out the
            // following two lines, as well as the DecorateElmahIoMessages class later in this file.

            //services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            //services.AddSingleton<IConfigureOptions<ElmahIoProviderOptions>, DecorateElmahIoMessages>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory factory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }

        //private class DecorateElmahIoMessages : IConfigureOptions<ElmahIoProviderOptions>
        //{
        //    private readonly IHttpContextAccessor httpContextAccessor;

        //    public DecorateElmahIoMessages(IHttpContextAccessor httpContextAccessor)
        //    {
        //        this.httpContextAccessor = httpContextAccessor;
        //    }

        //    public void Configure(ElmahIoProviderOptions options)
        //    {
        //        options.OnMessage = msg =>
        //        {
        //            var context = httpContextAccessor.HttpContext;
        //            if (context == null) return;
        //            msg.User = "test";//context.User?.Identity?.Name;
        //        };

        //    }
        //}
    }
}
