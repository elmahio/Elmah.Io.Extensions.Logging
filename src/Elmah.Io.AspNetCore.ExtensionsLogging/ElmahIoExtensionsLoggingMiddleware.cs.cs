using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elmah.Io.AspNetCore.ExtensionsLogging
{
    /// <summary>
    /// Middleware for enriching messages logged through Microsoft.Extensions.Logging with details from the HTTP context.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the middleware. You typically don't want to call this, but rather do this:
    /// app.UseElmahIoExtensionsLogging();
    /// </remarks>
    public class ElmahIoExtensionsLoggingMiddleware(RequestDelegate next, ILogger<ElmahIoExtensionsLoggingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ElmahIoExtensionsLoggingMiddleware> _logger = logger;

        /// <summary>
        /// Invoked on every HTTP request by ASP.NET Core. You never want to call this manually.
        /// </summary>
        public async Task Invoke(HttpContext context)
        {
            var loggerState = new Dictionary<string, object>
            {
                { "url", context.Request?.Path.Value },
                { "method", context.Request?.Method },
                { "statuscode", context.Response.StatusCode },
                { "user", context.User?.Identity?.Name },
                { "servervariables", ServerVariables(context) },
                { "cookies", Cookies(context) },
                { "form", await Form(context) },
                { "querystring", QueryString(context) },
            };
            using (_logger.BeginScope(loggerState))
            {
                await _next(context);
            }
        }

        private static Dictionary<string, string> QueryString(HttpContext context)
        {
            return context.Request?.Query?.Keys.ToDictionary(k => k, k => context.Request.Query[k].ToString());
        }

        private async Task<Dictionary<string, string>> Form(HttpContext context)
        {
            try
            {
                if (context.Request == null || !context.Request.HasFormContentType) return []; // internal logic for HasFormContentType: Content-Type header with "application/x-www-form-urlencoded" or "multipart/form-data"
#if NET8_0_OR_GREATER
                var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
                var disableFormLoggingMetadata = endpoint?.Metadata.GetMetadata<IDisableElmahIoFormLoggingMetadata>();
                if (disableFormLoggingMetadata is not null)
                {
                    if (disableFormLoggingMetadata.IncludeFormContentType)
                    {
                        return new Dictionary<string, string> { { "ContentType", context.Request.ContentType ?? "" } };
                    }
                    return [];
                }

                if (context.Request.ContentType?.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var optIntoMultipartLoggingMetadata = endpoint?.Metadata.GetMetadata<IOptIntoElmahIoMultipartBodyLoggingMetadata>();
                    if (optIntoMultipartLoggingMetadata is null)
                    {
                        return [];
                    }
                }
#else
                if (context.Request?.ContentType?.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase) == true) return [];
#endif
                var formData = await context.Request.ReadFormAsync(); // recommended over accessing Request.Form: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices?view=aspnetcore-6.0#prefer-readformasync-over-requestform
                return formData?.Keys.ToDictionary(k => k, k => context.Request.Form[k].ToString()) ?? [];
            }
            catch (Exception)
            {
                // All sorts of exceptions can happen while trying to read from Request.Form. Like:
                // - InvalidOperationException: Request not a form POST or similar
                // - InvalidDataException: Form body without a content-type or similar
                // - ConnectionResetException: More than 100 active connections or similar
                // - System.IO.IOException: Unexpected end of stream because the client disconnected or the stream has already been read elsewhere.

                // In case of an exception return an empty dictionary since we still want the middleware to run
                return [];
            }
        }

        private static Dictionary<string, string> Cookies(HttpContext context)
        {
            return context.Request?.Cookies?.Keys.ToDictionary(k => k, k => context.Request.Cookies[k].ToString());
        }

        private static Dictionary<string, string> ServerVariables(HttpContext context)
        {
            return context.Request?.Headers?.Keys.ToDictionary(k => k, k => context.Request.Headers[k].ToString());
        }
    }
}
