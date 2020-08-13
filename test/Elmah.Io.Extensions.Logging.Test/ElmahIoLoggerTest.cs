using Elmah.Io.Client.Models;
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
        private ICanHandleMessages _queueMock;
        private ElmahIoLogger _logger;

        [SetUp]
        public void SetUp()
        {
            _queueMock = Substitute.For<ICanHandleMessages>();
            _logger = new ElmahIoLogger(_queueMock, new ElmahIoProviderOptions(), null);
        }

        [Test]
        public void CanLog()
        {
            // Arrange

            // Act
            _logger.LogError(new ApplicationException(), "This is an error with a {property}", "PropertyValue");

            // Assert
            _queueMock
                .Received()
                .AddMessage(Arg.Is<CreateMessage>(msg =>
                    msg != null
                    && msg.Title.Equals("This is an error with a PropertyValue")
                    && msg.TitleTemplate.Equals("This is an error with a {property}")
                    && msg.Severity.Equals("Error")
                    && msg.Type.Equals("System.ApplicationException")));
        }

        [Test]
        public void CanLogWellKnownProperties()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var hostname = Guid.NewGuid().ToString();
            var type = Guid.NewGuid().ToString();
            var application = Guid.NewGuid().ToString();
            var user = Guid.NewGuid().ToString();
            var source = Guid.NewGuid().ToString();
            var method = Guid.NewGuid().ToString();
            var version = Guid.NewGuid().ToString();
            var url = Guid.NewGuid().ToString();
            var statuscode = 404;
            var serverVariables = new Dictionary<string, string> { { "serverVariableKey", "serverVariableValue" } };
            var cookies = new Dictionary<string, string> { { "cookiesKey", "cookiesValue" } };
            var form = new Dictionary<string, string> { { "formKey", "formValue" } };
            var queryString = new Dictionary<string, string> { { "queryStringKey", "queryStringValue" } };

            // Act
            _logger.LogInformation("Info message {method} {version} {url} {user} {type} {statusCode} {source} {hostname} {application} {serverVariables} {cookies} {form} {queryString}",
                method,
                version,
                url,
                user,
                type,
                statuscode,
                source,
                hostname,
                application,
                serverVariables,
                cookies,
                form,
                queryString);

            // Assert
            _queueMock
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
                    && msg.ServerVariables.Any(sv => sv.Key == "serverVariableKey" && sv.Value == "serverVariableValue")
                    && msg.Cookies.Any(sv => sv.Key == "cookiesKey" && sv.Value == "cookiesValue")
                    && msg.Form.Any(sv => sv.Key == "formKey" && sv.Value == "formValue")
                    && msg.QueryString.Any(sv => sv.Key == "queryStringKey" && sv.Value == "queryStringValue")));
        }
    }
}
