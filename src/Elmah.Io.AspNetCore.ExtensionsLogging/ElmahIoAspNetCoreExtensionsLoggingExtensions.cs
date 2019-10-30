using Elmah.Io.AspNetCore.ExtensionsLogging;

namespace Microsoft.AspNetCore.Builder
{
    public static class ElmahIoAspNetCoreExtensionsLoggingExtensions
    {
        public static IApplicationBuilder UseElmahIoExtensionsLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ElmahIoExtensionsLoggingMiddleware>();
        }

    }
}
