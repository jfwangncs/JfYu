using Newtonsoft.Json;

namespace JfYu.WeChat.Model.Response
{
    /// <summary>
    /// Response model for access token request
    /// </summary>
    public class AccessTokenResponse
    {
        /// <summary>
        /// Access token string for API authentication
        /// </summary>
        [JsonProperty(PropertyName = "access_token")]
        public string? AccessToken { get; set; }

        /// <summary>
        /// Token expiration time in seconds (typically 7200 seconds = 2 hours)
        /// </summary>
        [JsonProperty(PropertyName = "expires_in")]
        public int Expires { get; set; }
    }
}
