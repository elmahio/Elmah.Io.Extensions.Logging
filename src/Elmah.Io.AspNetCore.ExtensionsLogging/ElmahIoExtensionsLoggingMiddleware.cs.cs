﻿using Microsoft.AspNetCore.Http;
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
                { "form", Form(context) },
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

        private static Dictionary<string, string> Form(HttpContext context)
        {
            try
            {
                return context.Request?.Form?.Keys.ToDictionary(k => k, k => context.Request.Form[k].ToString());
            }
            catch (Exception)
            {
                // All sorts of exceptions can happen while trying to read from Request.Form. Like:
                // - InvalidOperationException: Request not a form POST or similar
                // - InvalidDataException: Form body without a content-type or similar
                // - ConnectionResetException: More than 100 active connections or similar
                // - System.IO.IOException: Unexpected end of stream

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
