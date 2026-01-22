using Newtonsoft.Json;

namespace JfYu.WeChat.Model.Response
{
    /// <summary>
    /// Base response model for all WeChat API responses
    /// </summary>
    public class WechatBaseResponse
    {
        /// <summary>
        /// Error code (0 indicates success, non-zero indicates error)
        /// </summary>
        [JsonProperty(PropertyName = "errcode")]
        public int ErrorCode { get; set; }

        /// <summary>
        /// Error message description
        /// </summary>
        [JsonProperty(PropertyName = "errmsg")]
        public string? ErrorMessage { get; set; }
    }
}
