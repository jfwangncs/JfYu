using JfYu.Request.Enum;
using Microsoft.Extensions.Logging;
using System;

namespace JfYu.Request.Logs
{
    /// <summary>
    /// Provides logging messages for application events.
    /// </summary>
    public static partial class AppLogMessages
    { 
        /// <summary>
        /// Logs an informational message for a request.
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="requestId">The requestId</param>
        /// <param name="path">The path</param>
        /// <param name="method">The method</param>
        /// <param name="headers">The headers</param>
        /// <param name="data">The data</param>
        [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Request [RequestId:{requestId}]{path}{method}{headers}{data}", SkipEnabledCheck = true)]
        public static partial void LogRequest(this ILogger logger, string requestId, string path = "", string method = "", string headers = "", string data = "");

        /// <summary>
        /// Logs an informational message for a response.
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="requestId">The requestId</param>
        /// <param name="status">The Status</param>
        /// <param name="result">The Result</param>
        [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Response [RequestId:{requestId}]{status}{result}", SkipEnabledCheck = true)]
        public static partial void LogResponse(this ILogger logger, string requestId, string status = "", string result = "");

        /// <summary>
        /// Logs an HTTP request with optional filtering based on logging fields.
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="loggingFields">The logger fields</param>
        /// <param name="requestId">The requestId</param>
        /// <param name="path">The path</param>
        /// <param name="method">The method</param>
        /// <param name="headers">The headers</param>
        /// <param name="data">The data</param>
        public static void LogRequestWithFilter(this ILogger logger, JfYuLoggingFields loggingFields, string requestId, string path, string method, string headers, string data)
        {
            logger.LogRequest(
                requestId,
                path: (loggingFields & JfYuLoggingFields.RequestPath) != 0 ? $" Path={path}" : "",
                method: (loggingFields & JfYuLoggingFields.RequestMethod) != 0 ? $" Method={method}" : "",
                headers: (loggingFields & JfYuLoggingFields.RequestHeaders) != 0 ? $" Headers={headers}" : "",
                data: (loggingFields & JfYuLoggingFields.RequestData) != 0 ? $" Data={data}" : ""
            );
        }

        /// <summary>
        /// Logs an HTTP response with optional filtering based on logging fields.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="loggingFields">The logger fields</param>
        /// <param name="requestId">The requestId</param>
        /// <param name="status">The Status</param>
        /// <param name="result">The Result</param>
        public static void LogResponseWithFilter(this ILogger logger, JfYuLoggingFields loggingFields, string requestId, string status, string result)
        {
            logger.LogResponse(
                requestId,
                status: (loggingFields & JfYuLoggingFields.ResponseStatus) != 0 ? $" Status={status}" : "",
                result: (loggingFields & JfYuLoggingFields.Response) != 0 ? $" Result={result}" : "");
        }
    }
}