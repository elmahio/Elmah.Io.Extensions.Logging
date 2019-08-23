using Elmah.Io.Client;
using Elmah.Io.Client.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elmah.Io.Extensions.Logging.Test
{
    public class ElmahIoLoggerTest
    {
        [Test]
        public void CanLog()
        {
            // Arrange
            var queueMock = new Mock<ICanHandleMessages>();

            CreateMessage message = null;
            queueMock
                .Setup(x => x.AddMessage(It.IsAny<CreateMessage>()))
                .Callback<CreateMessage>((msg) =>
                {
                    message = msg;
                });
            var logger = new ElmahIoLogger(queueMock.Object, new ElmahIoProviderOptions());

            // Act
            logger.LogError("This is an error from {User} with {Cookies} and {StatusCode}",
                "John Doe", new Dictionary<string, string> { { "Key", "Value" } }, 500);

            // Assert
            Assert.That(message, Is.Not.Null);
            Assert.That(message.Title, Is.EqualTo("This is an error from John Doe with [Key, Value] and 500"));
            Assert.That(message.User, Is.EqualTo("John Doe"));
            Assert.That(message.Cookies, Is.Not.Null);
            Assert.That(message.Cookies.Any(c => c.Key == "Key" && c.Value == "Value"));
            Assert.That(message.StatusCode, Is.Not.Null.And.EqualTo(500));
        }
    }
}
