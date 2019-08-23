using Elmah.Io.Client;
using Elmah.Io.Client.Models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Elmah.Io.Extensions.Logging.Test
{
    public class MessageQueueTest
    {
        [Test]
        public void CanProcessMessages()
        {
            // Arrange
            var elmahIoClientMock = new Mock<IElmahioAPI>();
            var messagesMock = new Mock<IMessages>();
            elmahIoClientMock.Setup(x => x.Messages).Returns(messagesMock.Object);
            IList<CreateMessage> messages = null;
            messagesMock
                .Setup(x => x.CreateBulkAndNotifyAsync(It.IsAny<Guid>(), It.IsAny<IList<CreateMessage>>()))
                .Callback<Guid, IList<CreateMessage>>((logId, msgs) =>
                {
                    messages = msgs;
                });

            var messageQueue = new MessageQueue(new ElmahIoProviderOptions
            {
                Period = TimeSpan.FromMilliseconds(10)
            }, elmahIoClientMock.Object);
            messageQueue.Start();

            // Act
            messageQueue.AddMessage(new CreateMessage());

            // Assert
            Thread.Sleep(1000);
            Assert.That(messages, Is.Not.Null);
            Assert.That(messages.Count, Is.EqualTo(1));
        }
    }
}
