using Elmah.Io.Client;
using System;
using System.Net;

namespace Elmah.Io.Extensions.Logging
{
    /// <summary>
    /// Options object containing all available options for the elmah.io provider for Microsoft.Extensions.Logging.
    /// </summary>
    public class ElmahIoProviderOptions
    {
        /// <summary>
        /// Specify an elmah.io API key with permission to create messages.
        /// </summary>
        public string ApiKey { get; set; }
        /// <summary>
        /// Specify which log to store messages in.
        /// </summary>
        public Guid LogId { get; set; }
        /// <summary>
        /// Specify a filter function to be called before logging each message. If a filter returns true, the message isn't logged.
        /// </summary>
        public Func<CreateMessage, bool> OnFilter { get; set; }
        /// <summary>
        /// An application name to put on all error messages.
        /// </summary>
        public string Application { get; set; }
        /// <summary>
        /// Specify an action to be called on all (not filtered) messages. Use this to decorate log messages with custom properties.
        /// </summary>
        public Action<CreateMessage> OnMessage { get; set; }
        /// <summary>
        /// Specify an action to be called on all (not filtered) messages if communication with the elmah.io API fails.
        /// </summary>
        public Action<CreateMessage, Exception> OnError { get; set; }
        /// <summary>
        /// Specify the interval between storing messages to elmah.io. As default messages are stored every 2 seconds.
        /// </summary>
        public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(2);
        /// <summary>
        /// Specify the batch size to store messages to elmah.io in. As default messages are stored in batches of 50. If you log large messages,
        /// you may want to lower this number.
        /// </summary>
        public int BatchPostingLimit { get; set; } = 50;
        /// <summary>
        /// Specify the size of the queue storing messages to be logged. As default the queue size is 1,000. If you log a lot of messages in your
        /// application, you can increase the queue size.
        /// </summary>
        public int BackgroundQueueSize { get; set; } = 1000;
        /// <summary>
        /// If you don't have outgoing internet connection from the server hosting your application, you can log through a web proxy.
        /// </summary>
        public IWebProxy WebProxy { get; set; }
        /// <summary>
        /// Enable additional properties added manually and/or through ASP.NET Core, Elmah.Io.AspNetCore.ExtensionsLogging, and similar.
        /// </summary>
        public bool IncludeScopes { get; set; } = true;
    }
}
