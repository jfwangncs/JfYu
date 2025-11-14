using JfYu.Request.Enum;
using System;

namespace JfYu.Request.Logs
{
    /// <summary>
    /// Configuration for controlling HTTP request/response logging behavior.
    /// Allows filtering sensitive data and selecting which fields to log.
    /// </summary>
    public class LogFilter
    {
        /// <summary>
        /// Gets or sets which HTTP request/response fields should be logged.
        /// Default value is <see cref="JfYuLoggingFields.All"/>.
        /// Use bitwise flags to combine multiple fields (e.g., RequestPath | Response).
        /// </summary>
        /// <example>
        /// // Log only request path and response status
        /// LoggingFields = JfYuLoggingFields.RequestPath | JfYuLoggingFields.ResponseStatus;
        /// </example>
        public JfYuLoggingFields LoggingFields { get; set; } = JfYuLoggingFields.All;

        /// <summary>
        /// Gets or sets a function to transform or sanitize the request log before writing.
        /// Default implementation returns the input unchanged.
        /// Use this to remove sensitive data like passwords or API keys from logs.
        /// </summary>
        /// <remarks>
        /// The function receives the raw request log string and should return the filtered version.
        /// If the function throws an exception, the error is logged but does not stop the request.
        /// </remarks>
        /// <example>
        /// RequestFilter = log => log.Replace("password=secret", "password=***");
        /// </example>
        public Func<string, string> RequestFilter { get; set; } = q => q;

        /// <summary>
        /// Gets or sets a function to transform or sanitize the response log before writing.
        /// Default implementation returns the input unchanged.
        /// Use this to remove sensitive data like tokens or personal information from logs.
        /// </summary>
        /// <remarks>
        /// The function receives the raw response log string and should return the filtered version.
        /// If the function throws an exception, the error is logged but does not stop the response processing.
        /// </remarks>
        /// <example>
        /// ResponseFilter = log => Regex.Replace(log, "\"token\":\"[^\"]+\"", "\"token\":\"***\"");
        /// </example>
        public Func<string, string> ResponseFilter { get; set; } = q => q;
    }
}