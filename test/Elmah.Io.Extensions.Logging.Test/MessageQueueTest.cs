using Elmah.Io.Client;
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
            var messagesClientMock = Substitute.For<IMessagesClient>();
            elmahIoClientMock.Messages.Returns(messagesClientMock);

            var messageQueue = new MessageQueueHandler(new ElmahIoProviderOptions
            {
                Period = TimeSpan.FromMilliseconds(10)
            }, elmahIoClientMock);
            messageQueue.Start();

            // Act
            messageQueue.AddMessage(new CreateMessage());

            // Assert
            Thread.Sleep(1000);
            messagesClientMock
                .Received()
                .CreateBulkAndNotifyAsync(Arg.Any<Guid>(), Arg.Is<IList<CreateMessage>>(messages =>
                    messages != null
                    && messages.Count == 1), Arg.Any<CancellationToken>());
        }
    }
}
