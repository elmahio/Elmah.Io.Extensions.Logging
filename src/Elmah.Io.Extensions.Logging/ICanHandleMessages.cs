using Elmah.Io.Client;

namespace Elmah.Io.Extensions.Logging
{
    /// <summary>
    /// Interface for elmah.io specific implementations that can bundle log messages and store them in elmah.io.
    /// </summary>
    public interface ICanHandleMessages
    {
        /// <summary>
        /// Add a message for processing. You typically don't need to call this method but instead use one of the Log* methods on ILogger.
        /// </summary>
        /// <param name="message"></param>
        void AddMessage(CreateMessage message);

        /// <summary>
        /// Called when initializing the logger.
        /// </summary>
        void Start();

        /// <summary>
        /// Called when disposing the logger.
        /// </summary>
        void Stop();
    }
}
