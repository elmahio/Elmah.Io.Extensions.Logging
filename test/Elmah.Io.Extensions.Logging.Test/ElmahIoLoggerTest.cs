using Elmah.Io.Client;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elmah.Io.Extensions.Logging.Test
{
    public class ElmahIoLoggerTest
    {
        IExternalScopeProvider scopeProvider;
        private ICanHandleMessages queueMock;
        private ElmahIoLogger logger;

        [SetUp]
        public void SetUp()
        {
            queueMock = Substitute.For<ICanHandleMessages>();
            scopeProvider = Substitute.For<IExternalScopeProvider>();
            logger = new ElmahIoLogger("TestLogger", queueMock, new ElmahIoProviderOptions(), scopeProvider);
        }

        [Test]
        public void CanLog()
        {
            // Arrange

            // Act
            logger.LogError(new ApplicationException(), "This is an error with a {property}", "PropertyValue");

            // Assert
            queueMock
                .Received()
                .AddMessage(Arg.Is<CreateMessage>(msg =>
                    msg != null
                    && msg.Title.Equals("This is an error with a PropertyValue")
                    && msg.TitleTemplate.Equals("This is an error with a {property}")
                    && msg.Severity.Equals("Error")
                    && msg.Type.Equals("System.ApplicationException")
                    && msg.Category.Equals("TestLogger")));
        }

        [Test]
        public void CanLogWithNoScope()
        {
            // Arrange
            scopeProvider
                .When(x => x.ForEachScope(Arg.Any<Action<object, object>>(), Arg.Any<object>()))
                .Do(x => x.Arg<Action<object, object>>().Invoke(null, null));

            // Act
            logger.LogInformation("msg");

            // Assert
            queueMock
                .Received()
                .AddMessage(Arg.Is<CreateMessage>(msg => msg.Data != null && !msg.Data.Any()));
        }

        [Test]
        public void CanLogWithEnumerableScope()
        {
            // Arrange
            scopeProvider
                .When(x => x.ForEachScope(Arg.Any<Action<object, object>>(), Arg.Any<object>()))
                .Do(x => x.Arg<Action<object, object>>().Invoke(new List<KeyValuePair<string, object>> { new("Key", "Value") }, null));

            // Act
            logger.LogInformation("msg");

            // Assert
            queueMock
                .Received()
                .AddMessage(Arg.Is<CreateMessage>(msg => msg.Data != null && msg.Data.Any(d => d.Key == "Key" && d.Value == "Value")));
        }

        [Test]
        public void CanLogWithStringScope()
        {
            // Arrange
            scopeProvider
                .When(x => x.ForEachScope(Arg.Any<Action<object, object>>(), Arg.Any<object>()))
                .Do(x => x.Arg<Action<object, object>>().Invoke("Scope", null));

            // Act
            logger.LogInformation("msg");

            // Assert
            queueMock
                .Received()
                .AddMessage(Arg.Is<CreateMessage>(msg => msg.Data != null && !msg.Data.Any()));
        }


        [Test]
        public void CanLogWithObjectScope()
        {
            // Arrange
            scopeProvider
                .When(x => x.ForEachScope(Arg.Any<Action<object, object>>(), Arg.Any<object>()))
                .Do(x => x.Arg<Action<object, object>>().Invoke(new { Hello = "World" }, null));

            // Act
            logger.LogInformation("msg");

            // Assert
            queueMock
                .Received()
                .AddMessage(Arg.Is<CreateMessage>(msg => msg.Data != null && msg.Data.Any(d => d.Key == "Hello" && d.Value == "World")));
        }

        [Test]
        public void CanLogWellKnownProperties()
        {
            // Arrange
            var hostname = Guid.NewGuid().ToString();
            var type = Guid.NewGuid().ToString();
            var application = Guid.NewGuid().ToString();
            var user = Guid.NewGuid().ToString();
            var source = Guid.NewGuid().ToString();
            var method = Guid.NewGuid().ToString();
            var version = Guid.NewGuid().ToString();
            var url = Guid.NewGuid().ToString();
            var statuscode = 404;
            var correlationId = Guid.NewGuid().ToString();
            var category = Guid.NewGuid().ToString();
            var detail = Guid.NewGuid().ToString();
            var serverVariables = new Dictionary<string, string> { { "serverVariableKey", "serverVariableValue" } };
            var cookies = new Dictionary<string, string> { { "cookiesKey", "cookiesValue" } };
            var form = new Dictionary<string, string> { { "formKey", "formValue" } };
            var queryString = new Dictionary<string, string> { { "queryStringKey", "queryStringValue" } };

            // Act
            logger.LogInformation("Info message {method} {version} {url} {user} {type} {statusCode} {source} {hostname} {application} {correlationId} {category} {detail} {serverVariables} {cookies} {form} {queryString}",
                method,
                version,
                url,
                user,
                type,
                statuscode,
                source,
                hostname,
                application,
                correlationId,
                category,
                detail,
                serverVariables,
                cookies,
                form,
                queryString);

            // Assert
            queueMock
                .Received()
                .AddMessage(Arg.Is<CreateMessage>(msg =>
                    msg != null
                    && msg.Hostname.Equals(hostname)
                    && msg.Type.Equals(type)
                    && msg.Application.Equals(application)
                    && msg.User.Equals(user)
                    && msg.Source.Equals(source)
                    && msg.Method.Equals(method)
                    && msg.Version.Equals(version)
                    && msg.Url.Equals(url)
                    && msg.StatusCode == statuscode
                    && msg.CorrelationId.Equals(correlationId)
                    && msg.Category.Equals(category)
                    && msg.Detail.Equals(detail)
                    && msg.ServerVariables.Any(sv => sv.Key == "serverVariableKey" && sv.Value == "serverVariableValue")
                    && msg.Cookies.Any(sv => sv.Key == "cookiesKey" && sv.Value == "cookiesValue")
                    && msg.Form.Any(sv => sv.Key == "formKey" && sv.Value == "formValue")
                    && msg.QueryString.Any(sv => sv.Key == "queryStringKey" && sv.Value == "queryStringValue")));
        }

        /// <summary>
        /// Test that exception passed to the logger overrides detail property.
        /// </summary>
        [Test]
        public void CanLogExceptionOverDetailProperty()
        {
            // Arrange
            var detail = Guid.NewGuid().ToString();
            var message = Guid.NewGuid().ToString();

            // Act
            logger.LogInformation(new Exception(message), "Info message {detail}", detail);

            // Assert
            queueMock
                .Received()
                .AddMessage(Arg.Is<CreateMessage>(msg =>
                    msg != null
                    && msg.Detail.Contains(message)
                    && !msg.Detail.Contains(detail)));
        }

        [Test]
        public void CanLogEventId()
        {
            // Arrange

            // Act
            logger.LogError(new EventId(42, "Answer"), "A message");

            // Assert
            queueMock
                .Received()
                .AddMessage(Arg.Is<CreateMessage>(msg =>
                    msg != null
                    && msg.Data.Any(d => d.Key == "EventId" && d.Value == "42")
                    && msg.Data.Any(d => d.Key == "EventName" && d.Value == "Answer")));

        }
    }
}