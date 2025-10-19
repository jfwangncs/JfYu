using Microsoft.Extensions.Logging;
using System;

namespace JfYu.RabbitMQ.Logs
{
    /// <summary>
    /// Provides logging messages for application events.
    /// </summary>
    public static partial class AppLogMessages
    {
        /// <summary>
        /// Logs an error message with an exception.
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="ex">The exception to log (required)</param>
        /// <param name="message">Error message</param>
        [LoggerMessage(EventId = 5001, Level = LogLevel.Error, Message = "{message}", SkipEnabledCheck = true)]
        public static partial void LogError(this ILogger logger, Exception ex, string message);

        /// <summary>
        /// Logs warning message.
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="message">Warning message</param>
        [LoggerMessage(EventId = 4001, Level = LogLevel.Warning, Message = "{message}", SkipEnabledCheck = true)]
        public static partial void LogWarning(this ILogger logger, string message);
    }
}
