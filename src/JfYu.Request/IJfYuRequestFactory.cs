namespace JfYu.Request
{
    /// <summary>
    /// Factory interface for creating IJfYuRequest instances with different named HttpClient configurations.
    /// Use this when you have registered multiple HttpClient instances with different names and need to select which one to use.
    /// </summary>
    public interface IJfYuRequestFactory
    {
        /// <summary>
        /// Creates an IJfYuRequest instance using the default HttpClient (registered without specifying a name).
        /// </summary>
        /// <returns>A new IJfYuRequest instance configured with the default HttpClient.</returns>
        IJfYuRequest CreateRequest();

        /// <summary>
        /// Creates an IJfYuRequest instance using a named HttpClient configuration.
        /// </summary>
        /// <param name="httpClientName">The name of the HttpClient to use, as specified in AddJfYuHttpClient options.HttpClientName.</param>
        /// <returns>A new IJfYuRequest instance configured with the specified named HttpClient.</returns>
        /// <example>
        /// <code>
        /// // In Startup.cs or Program.cs
        /// services.AddJfYuHttpClient(options => options.HttpClientName = "ApiClient");
        /// services.AddJfYuHttpClient(options => options.HttpClientName = "ExternalApi");
        /// 
        /// // In your service
        /// public class MyService
        /// {
        ///     private readonly IJfYuRequestFactory _requestFactory;
        ///     
        ///     public MyService(IJfYuRequestFactory requestFactory)
        ///     {
        ///         _requestFactory = requestFactory;
        ///     }
        ///     
        ///     public async Task CallInternalApiAsync()
        ///     {
        ///         var request = _requestFactory.CreateRequest("ApiClient");
        ///         request.Url = "https://internal-api.com/data";
        ///         return await request.SendAsync();
        ///     }
        ///     
        ///     public async Task CallExternalApiAsync()
        ///     {
        ///         var request = _requestFactory.CreateRequest("ExternalApi");
        ///         request.Url = "https://external-api.com/data";
        ///         return await request.SendAsync();
        ///     }
        /// }
        /// </code>
        /// </example>
        IJfYuRequest CreateRequest(string httpClientName);
    }
}
