using System;

namespace JfYu.Request.Enum
{
    /// <summary>
    /// Specifies the fields to be included in the logging.
    /// </summary>
    [Flags]
    public enum JfYuLoggingFields
    {
        /// <summary>
        /// No logging.
        /// </summary>
        None = 0,

        /// <summary>
        /// Log the request path.
        /// </summary>
        RequestPath = 1,

        /// <summary>
        /// Log the request method.
        /// </summary>
        RequestMethod = 2,

        /// <summary>
        /// Log the request headers.
        /// </summary>
        RequestHeaders = 4,

        /// <summary>
        /// Log the request data.
        /// </summary>
        RequestData = 8,

        /// <summary>
        /// Log the response status.
        /// </summary>
        ResponseStatus = 16,

        /// <summary>
        /// Log the response content.
        /// </summary>
        Response = 32,

        /// <summary>
        /// Log all request-related fields.
        /// </summary>
        RequestAll = RequestPath | RequestMethod | RequestHeaders | RequestData,

        /// <summary>
        /// Log all response-related fields.
        /// </summary>
        ResponseAll = Response | ResponseStatus,

        /// <summary>
        /// Log all fields.
        /// </summary>
        All = RequestPath | RequestMethod | RequestHeaders | RequestData | Response | ResponseStatus
    }
}