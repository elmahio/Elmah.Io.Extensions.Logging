using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elmah.Io.Extensions.Logging.AspNetCore21
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

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
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
