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

#if NET8_0_OR_GREATER
        /// <summary>
        /// Disables logging of form values from the HTTP context in the
        /// Elmah.Io.AspNetCore.ExtensionsLogging middleware. This is useful if you don't want form
        /// data to be included in the logs (i.e. for sensitive form data or files).
        /// </summary>
        public static TBuilder DisableElmahIoFormLogging<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        {
            builder.WithMetadata(new DisableElmahIoFormLoggingAttribute());
            return builder;
        }

        /// <summary>
        /// Enables logging of multipart form data from the HTTP context. By default, multipart form data
        /// is not logged because it can result in exceptions when the application also tries to consume
        /// the request body as a stream without request buffering being enabled.
        /// </summary>
        public static TBuilder EnableElmahIoMultipartBodyLogging<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        {
            builder.WithMetadata(new EnableElmahIoMultipartBodyLoggingAttribute());
            return builder;
        }
#endif
    }
}
