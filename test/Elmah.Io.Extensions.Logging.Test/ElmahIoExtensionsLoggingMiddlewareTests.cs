using Elmah.Io.AspNetCore.ExtensionsLogging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Elmah.Io.Extensions.Logging.Test
{
    public class ElmahIoExtensionsLoggingMiddlewareTests
    {
        private ILogger<ElmahIoExtensionsLoggingMiddleware> logger;
        private RequestDelegate next;
        private ElmahIoExtensionsLoggingMiddleware middleware;

        [SetUp]
        public void Setup()
        {
            logger = Substitute.For<ILogger<ElmahIoExtensionsLoggingMiddleware>>();
            next = Substitute.For<RequestDelegate>();
            middleware = new ElmahIoExtensionsLoggingMiddleware(next, logger);
        }

        [Test]
        public async Task InvokeShouldNotLogFormWhenDisableAttributeIsPresent()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.Request.ContentType = "application/x-www-form-urlencoded";

            context.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "Password", "ShouldNotBeLogged" }
            });

            var metadata = new EndpointMetadataCollection(new DisableElmahIoFormLoggingAttribute());
            var endpoint = new Endpoint(c => Task.CompletedTask, metadata, "TestEndpoint");

            var feature = Substitute.For<IEndpointFeature>();
            feature.Endpoint.Returns(endpoint);
            context.Features.Set(feature);

            // Act
            await middleware.Invoke(context);

            // Assert
            logger.Received(1).BeginScope(Arg.Is<Dictionary<string, object>>(dict =>
                dict.ContainsKey("form") && ((Dictionary<string, string>)dict["form"]).Count == 0
            ));
        }

        [Test]
        public async Task InvokeShouldLogMultipartWhenEnableAttributeIsPresent()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Method = "POST";
            context.Request.ContentType = "multipart/form-data; boundary=something";

            context.Request.Form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "HiddenKey", "SecretValue" }
            });

            var metadata = new EndpointMetadataCollection(new EnableElmahIoMultipartBodyLoggingAttribute());
            var endpoint = new Endpoint(c => Task.CompletedTask, metadata, "TestEndpoint");
            var feature = Substitute.For<IEndpointFeature>();
            feature.Endpoint.Returns(endpoint);
            context.Features.Set(feature);

            // Act
            await middleware.Invoke(context);

            // Assert
            logger.Received(1).BeginScope(Arg.Is<Dictionary<string, object>>(dict =>
                ((Dictionary<string, string>)dict["form"])["HiddenKey"] == "SecretValue"
            ));
        }

        [Test]
        public async Task InvokeShouldHandleExceptionInFormReading()
        {
            // Arrange
            var context = Substitute.For<HttpContext>();
            context.Request.HasFormContentType.Returns(true);
            context.Request.ReadFormAsync().ThrowsAsync(new IOException("Connection reset"));

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await middleware.Invoke(context));

            logger.Received(1).BeginScope(Arg.Is<Dictionary<string, object>>(dict =>
                ((Dictionary<string, string>)dict["form"]).Count == 0
            ));
        }
    }
}
