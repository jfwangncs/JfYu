using Newtonsoft.Json;

namespace JfYu.WeChat.Model.Response
{
    /// <summary>
    /// Response model for user login authentication
    /// </summary>
    public class WechatLoginResponse : WechatBaseResponse
    {
        /// <summary>
        /// User's unique identifier within the Mini Program
        /// </summary>
        public string? OpenId { get; set; }

        /// <summary>
        /// User's unique identifier across all WeChat apps (if bound)
        /// </summary>
        public string? UnionId { get; set; }

        /// <summary>
        /// Session key for encrypted data decryption
        /// </summary>
        [JsonProperty(PropertyName = "session_key")]
        public string? SessionKey { get; set; }
    }
}
