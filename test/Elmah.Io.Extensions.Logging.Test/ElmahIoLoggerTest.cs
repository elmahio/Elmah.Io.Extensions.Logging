using Elmah.Io.Client;
using Elmah.Io.Client.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Elmah.Io.Extensions.Logging.Test
{
    public class ElmahIoLoggerTest
    {
        private Mock<ICanHandleMessages> _queueMock;
        private ElmahIoLogger _logger;

        [SetUp]
        public void SetUp()
        {
            _queueMock = new Mock<ICanHandleMessages>();
            _logger = new ElmahIoLogger(_queueMock.Object, new ElmahIoProviderOptions(), null);
        }

        [Test]
        public void CanLog()
        {
            // Arrange
            CreateMessage message = null;
            _queueMock
                .Setup(x => x.AddMessage(It.IsAny<CreateMessage>()))
                .Callback<CreateMessage>((msg) =>
                {
                    message = msg;
                });

            // Act
            _logger.LogError(new ApplicationException(), "This is an error with a {property}", "PropertyValue");

            // Assert
            Assert.That(message, Is.Not.Null);
            Assert.That(message.Title, Is.EqualTo("This is an error with a PropertyValue"));
            Assert.That(message.TitleTemplate, Is.EqualTo("This is an error with a {property}"));
            Assert.That(message.Severity, Is.EqualTo("Error"));
            Assert.That(message.Type, Is.EqualTo("System.ApplicationException"));
        }

        [Test]
        public void CanLogWellKnownProperties()
        {
            // Arrange
            CreateMessage message = null;
            _queueMock
                .Setup(x => x.AddMessage(It.IsAny<CreateMessage>()))
                .Callback<CreateMessage>((msg) =>
                {
                    message = msg;
                });

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
            Assert.That(message, Is.Not.Null);
            Assert.That(message.Hostname, Is.EqualTo(hostname));
            Assert.That(message.Type, Is.EqualTo(type));
            Assert.That(message.Application, Is.EqualTo(application));
            Assert.That(message.User, Is.EqualTo(user));
            Assert.That(message.Source, Is.EqualTo(source));
            Assert.That(message.Method, Is.EqualTo(method));
            Assert.That(message.Version, Is.EqualTo(version));
            Assert.That(message.Url, Is.EqualTo(url));
            Assert.That(message.StatusCode, Is.EqualTo(statuscode));
            Assert.That(message.ServerVariables.Any(sv => sv.Key == "serverVariableKey" && sv.Value == "serverVariableValue"));
            Assert.That(message.Cookies.Any(sv => sv.Key == "cookiesKey" && sv.Value == "cookiesValue"));
            Assert.That(message.Form.Any(sv => sv.Key == "formKey" && sv.Value == "formValue"));
            Assert.That(message.QueryString.Any(sv => sv.Key == "queryStringKey" && sv.Value == "queryStringValue"));
        }
    }
}
