using System;

namespace JfYu.Request.Enum
{
    /// <summary>
    /// Specifies which HTTP request/response fields should be included in logging output.
    /// This is a flags enum allowing bitwise combinations (e.g., RequestPath | Response).
    /// Used with LogFilter configuration in service registration.
    /// </summary>
    [Flags]
    public enum JfYuLoggingFields
    {
        /// <summary>
        /// No logging. Disables all request/response logging output.
        /// </summary>
        None = 0,

        /// <summary>
        /// Logs the request URL path and query string (e.g., "/api/users?page=1").
        /// </summary>
        RequestPath = 1,

        /// <summary>
        /// Logs the HTTP request method (GET, POST, PUT, DELETE, PATCH, etc.).
        /// </summary>
        RequestMethod = 2,

        /// <summary>
        /// Logs all request headers including custom headers, authorization, and user-agent.
        /// Useful for debugging header-related issues but may expose sensitive tokens.
        /// </summary>
        RequestHeaders = 4,

        /// <summary>
        /// Logs the request body/payload data (JSON, XML, form data, etc.).
        /// May expose sensitive information - consider using RequestFilter to sanitize.
        /// </summary>
        RequestData = 8,

        /// <summary>
        /// Logs the HTTP response status code (200, 404, 500, etc.).
        /// </summary>
        ResponseStatus = 16,

        /// <summary>
        /// Logs the response body/content returned by the server.
        /// May be large for binary downloads or large JSON responses.
        /// </summary>
        Response = 32,

        /// <summary>
        /// Logs all request-related fields: path, method, headers, and data.
        /// Equivalent to RequestPath | RequestMethod | RequestHeaders | RequestData.
        /// </summary>
        RequestAll = RequestPath | RequestMethod | RequestHeaders | RequestData,

        /// <summary>
        /// Logs all response-related fields: content and status code.
        /// Equivalent to Response | ResponseStatus.
        /// </summary>
        ResponseAll = Response | ResponseStatus,

        /// <summary>
        /// Logs all available fields for both request and response.
        /// Equivalent to RequestPath | RequestMethod | RequestHeaders | RequestData | Response | ResponseStatus.
        /// Use with caution in production as it may expose sensitive data and generate large logs.
        /// </summary>
        All = RequestPath | RequestMethod | RequestHeaders | RequestData | Response | ResponseStatus
    }
}