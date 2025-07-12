using Elmah.Io.AspNetCore.ExtensionsLogging;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <summary>
    /// Helper for installing the Elmah.Io.AspNetCore.ExtensionsLogging middleware.
    /// </summary>
    public static class ElmahIoAspNetCoreExtensionsLoggingExtensions
    {
        /// <summary>
        /// Install the Elmah.Io.AspNetCore.ExtensionsLogging middleware to enrich all messages logged
        /// through Microsoft.Extensions.Logging with details from the HTTP context. These information
        /// will show up in the correct fields on elmah.io.
        /// </summary>
        public static IApplicationBuilder UseElmahIoExtensionsLogging(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ElmahIoExtensionsLoggingMiddleware>();
        }
    }
}
