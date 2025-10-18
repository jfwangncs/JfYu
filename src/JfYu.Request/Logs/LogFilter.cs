using JfYu.Request.Enum;
using System;

namespace JfYu.Request.Logs
{
    /// <summary>
    /// Represents a filter for logging requests and responses.
    /// </summary>
    public class LogFilter
    {
        /// <summary>
        /// Gets or sets the logging fields to be included in the log.
        /// </summary>
        public JfYuLoggingFields LoggingFields { get; set; } = JfYuLoggingFields.All;

        /// <summary>
        /// Gets or sets the function to filter the request log.
        /// </summary>
        public Func<string, string> RequestFilter { get; set; } = q => q;

        /// <summary>
        /// Gets or sets the function to filter the response log.
        /// </summary>
        public Func<string, string> ResponseFilter { get; set; } = q => q;
    }
}