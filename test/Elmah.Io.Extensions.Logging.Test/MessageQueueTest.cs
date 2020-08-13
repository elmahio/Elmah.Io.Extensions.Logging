using Elmah.Io.Client;
using Elmah.Io.Client.Models;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Elmah.Io.Extensions.Logging.Test
{
    public class MessageQueueTest
    {
        [Test]
        public void CanProcessMessages()
        {
            // Arrange
            var elmahIoClientMock = Substitute.For<IElmahioAPI>();
            var messagesMock = Substitute.For<IMessages>();
            elmahIoClientMock.Messages.Returns(messagesMock);

            var messageQueue = new MessageQueue(new ElmahIoProviderOptions
            {
                Period = TimeSpan.FromMilliseconds(10)
            }, elmahIoClientMock);
            messageQueue.Start();

            // Act
            messageQueue.AddMessage(new CreateMessage());

            // Assert
            Thread.Sleep(1000);
            messagesMock
                .Received()
                .CreateBulkAndNotifyAsync(Arg.Any<Guid>(), Arg.Is<IList<CreateMessage>>(messages =>
                    messages != null
                    && messages.Count == 1));
        }
    }
}
