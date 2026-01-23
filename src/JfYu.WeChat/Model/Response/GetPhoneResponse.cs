using Newtonsoft.Json;

namespace JfYu.WeChat.Model.Response
{
    /// <summary>
    /// Response model for phone number retrieval
    /// </summary>
    public class GetPhoneResponse : WechatBaseResponse
    {
        /// <summary>
        /// User's phone information
        /// </summary>
        [JsonProperty(PropertyName = "phone_info")]
        public PhoneInfo? PhoneInfo { get; set; }
    }

    /// <summary>
    /// Detailed phone number information
    /// </summary>
    public class PhoneInfo
    {
        /// <summary>
        /// Complete phone number with country code (e.g., +86 138****1234)
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Phone number without country code
        /// </summary>
        public string? PurePhoneNumber { get; set; }

        /// <summary>
        /// Country calling code (e.g., 86 for China)
        /// </summary>
        public string? CountryCode { get; set; }

        /// <summary>
        /// Security watermark information
        /// </summary>
        public WaterMark? Watermark { get; set; }
    }

    /// <summary>
    /// Watermark for data security verification
    /// </summary>
    public class WaterMark
    {
        /// <summary>
        /// Mini Program AppId
        /// </summary>
        public string? Appid { get; set; }

        /// <summary>
        /// Timestamp of data acquisition
        /// </summary>
        public long Timestamp { get; set; }
    }
}
