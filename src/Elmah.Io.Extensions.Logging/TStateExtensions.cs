using System;
using System.Collections.Generic;
using System.Linq;

namespace Elmah.Io.Extensions.Logging
{
    internal static class TStateExtensions
    {
        internal static string Title<TState>(this TState state, Func<TState, Exception, string> formatter, Exception exception)
        {
            if (formatter != null)
            {
                var message = formatter(state, exception);

                // User logged a formatted message. Use this.
                if (!string.IsNullOrWhiteSpace(message)) return message;
            }

            // No formatted message provided. Use the base exception message if exceptions is logged as part of this message.
            if (exception != null) return exception.GetBaseException().Message;

            // No formatted message or exception provided. Build something from the state if key values pairs of string and object.
            if (state is IEnumerable<KeyValuePair<string, object>> enumerable) return string.Join(", ", enumerable.Take(5));

            // We tried everything else. Provide a generic message to make sure this log message is still logged.
            return "Message could not be resolved";
        }
    }
}
